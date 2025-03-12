using Godot;
using System;
using Godot.Collections;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

public partial class Client : Node
{

    enum State {
        DISCONNECTED,
        IN_LOBBY,
        IN_ACTIVE_SESSION,
        
    }
    State myState = State.DISCONNECTED;
    int myId = -1;
    int hostId = -2;
    string lobbyValue = "";


    public static Client instance = null;

    public static bool active 
    {
        get
        {
            if (Client.instance != null && Client.instance.myState == State.IN_ACTIVE_SESSION){
                return true;
            } else {return false;}
        }
        set
        {
            GD.PushError("you cannot set this property.");
            return;
        }
    }

    const string defaultPort = "8976";
    //74.111.121.13
    const string DefaultIpAdrress = "ws://127.0.0.1";
    [Export]
    bool debug=true;
    public event Action<string> debugTextEmit;
    public event Action<string> LobbyValueRecieved;

    public event Action<int, NetworkInputPacket> InputPackedRecieved;
    public event Action<int, NetworkInputPacket> InputPackedRecievedUnreliable;


    public event Action GameStarted;

    private int AuthorityPEERID = -1;

    WebSocketMultiplayerPeer peer = new WebSocketMultiplayerPeer();
    WebRtcMultiplayerPeer rtcPeer = new WebRtcMultiplayerPeer();

    public override void _Ready()
    {
        base._Ready();
        if (Client.instance == null){
            Client.instance = this;
        } else {
            GD.PushError("Client singleton instance already exists");
        }
        Multiplayer.ConnectedToServer += OnConnectedToRPCServer;
        Multiplayer.PeerConnected += OnRTCPeerConnected;
        Multiplayer.PeerDisconnected += OnRTCPeerDisconnected;

    }

    private void OnRTCPeerDisconnected(long id)
    {
        debugTextEmit.Invoke($"RTC peer disconnected : {id}");
        debugTextEmit.Invoke($"{id} , {GetMultiplayerAuthority()}");

        if (id == GetMultiplayerAuthority()){
            //later on this can transfer the host to a different user
            debugTextEmit.Invoke($"host disconnected, ending session.");
            Disconnect();
            return;
        }

        if (IsMultiplayerAuthority()){
            debugTextEmit.Invoke($"I am the authority and disconnected, this should disconnect all other peers");
        }
        //CallDeferred("PrintPeers");


    }


    private void OnRTCPeerConnected(long id)
    {
        GD.Print("RTC peer connected");
        debugTextEmit.Invoke($"RTC peer connected : {id}");
        if (hostId == myId){
            debugTextEmit.Invoke($"only the host should see this");
            SetMultiplayerAuthority(Multiplayer.GetUniqueId());
            Rpc("SetAuthority", [Multiplayer.GetUniqueId(),true]);
            //i want this to be finished before calling print peers
        }

    }



    private void PrintPeers(){
        debugTextEmit.Invoke($"");
        foreach (var kvp in rtcPeer.GetPeers()){
            debugTextEmit.Invoke($"peer {kvp.Key} ");
        }
        debugTextEmit.Invoke($"{Multiplayer.GetUniqueId()} (my id)");
        debugTextEmit.Invoke($"{GetMultiplayerAuthority()} (authority)");

        debugTextEmit.Invoke($"current peers");
        
    }

    private void OnConnectedToRPCServer()
    {

        GD.Print("connected to RTC server");
        debugTextEmit.Invoke("RTC server connected");
    }



    public void ConnectToServer(string ipAdrress = DefaultIpAdrress, string port = defaultPort){

        if (ipAdrress==""){ipAdrress = DefaultIpAdrress;}
        if (port==""){port = defaultPort;}


        Error err = peer.CreateClient(ipAdrress+":"+port);
        debugTextEmit?.Invoke($"Client connection to server {ipAdrress} with result: {err.ToString()}");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        
        if (peer==null){
            return;
        }

        peer.Poll();
        if (peer.GetAvailablePacketCount() > 0){
            Byte[] packet = peer.GetPacket();
            if (packet!=null){
                string packetString = packet.GetStringFromUtf8();
                NetworkPacket deserializedPacket = JsonSerializer.Deserialize<NetworkPacket>(packetString);
                //debugTextEmit?.Invoke($"Client {myId} received packet {packetString}");
                parsePacket(deserializedPacket);
            }
        }

    }

    
    public void JoinLobby(string LobbyID){
        NetworkPacket newPacket = new NetworkPacket(){
            Id = myId,
            Message = Message.JOIN_LOBBY,
            LobbyValue = LobbyID
        };
        peer.PutPacket(JsonSerializer.Serialize(newPacket).ToUtf8Buffer());
    }
 
    public void Disconnect(){
        if (myState == State.DISCONNECTED){
            debugTextEmit?.Invoke($"not currently connected. (nothing happens)");
        } else {
            debugTextEmit?.Invoke($"shutting down client");

            // Gracefully close the WebSocket connection
            if (peer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected) {
                peer.Close();
                // Poll to process the close event
                peer.Poll();
            }

            // Gracefully close all WebRTC connections
            foreach (var kvp in rtcPeer.GetPeers()) {
                int peerId = (int)kvp.Key;
                if (rtcPeer.HasPeer(peerId)) {
                    WebRtcPeerConnection connection = (WebRtcPeerConnection)rtcPeer.GetPeer(peerId)["connection"];
                    connection.Close(); 
                    // Properly close the WebRTC connection
                }
            }

            Multiplayer.MultiplayerPeer = null;
            rtcPeer.Close();
            
            peer = new WebSocketMultiplayerPeer();
            rtcPeer = new WebRtcMultiplayerPeer();

            myId = -1;
            hostId = -2;
        }
    }
    private void parsePacket(NetworkPacket packet){


        switch(packet.Message){
            case Message.NONE:
                GD.PushError("Packet had no message");
                break;
            case Message.ID:
                myId = packet.Id;
                debugTextEmit?.Invoke($"Client id is {myId}");

                setupMesh(myId);

                break;

            case Message.USER_CONNECTED:
                CreatePeer(packet.Id);

                break;

            case Message.LOBBY_CONNECTED:
                hostId = packet.HostId;
                lobbyValue = packet.LobbyValue;
                debugTextEmit?.Invoke($"My Lobby Key: {packet.LobbyValue}");
                LobbyValueRecieved?.Invoke(packet.LobbyValue);
                myState = State.IN_LOBBY;

                break;

            case Message.CANDIDATE:
                if (rtcPeer.HasPeer(packet.OrgPeer)){
                    debugTextEmit.Invoke($"Got candidate: {packet.OrgPeer} + my id is {myId}");
                    WebRtcPeerConnection connection = (WebRtcPeerConnection)rtcPeer.GetPeer(packet.OrgPeer)["connection"];
                    Error err = connection.AddIceCandidate(packet.Mid,(int)packet.Index,packet.Sdp);

                    GD.Print("candidate ", rtcPeer, " ", connection , " ", err.ToString());


                }
                break;
            case Message.OFFER:
                if (rtcPeer.HasPeer(packet.OrgPeer)){
                    WebRtcPeerConnection connection = (WebRtcPeerConnection)rtcPeer.GetPeer(packet.OrgPeer)["connection"];
                    Error err = connection.SetRemoteDescription("offer",packet.Data);
                    GD.Print("offer ", rtcPeer, " ", connection , " ", err.ToString());

                    debugTextEmit.Invoke($"Got candidate: {packet.OrgPeer} + my id is {myId}");

                }
                break;
            case Message.ANSWER:
                if (rtcPeer.HasPeer(packet.OrgPeer)){
                    WebRtcPeerConnection connection = (WebRtcPeerConnection)rtcPeer.GetPeer(packet.OrgPeer)["connection"];
                    Error err = connection.SetRemoteDescription("answer",packet.Data);
                    
                    GD.Print("answer ", rtcPeer, " ", connection , " ", err.ToString());
                }
                break;           
        }
    }

    private void setupMesh(int id)
    {
        //hooks up the rpc connections
        rtcPeer.CreateMesh(id);
        Multiplayer.MultiplayerPeer = rtcPeer;
    }


    private void CreatePeer(int id){
        // you dont bind to yourself
        if (id != myId){
            WebRtcPeerConnection peer = new WebRtcPeerConnection();
            
            Dictionary iceServersDict = new Dictionary
            {
                { "urls", new Godot.Collections.Array { "stun:stun.l.google.com:19302" } }
            };

            Dictionary initializeDict = new Dictionary
            {
                { "iceServers", new Godot.Collections.Array { iceServersDict } }
            };

            peer.Initialize(initializeDict);

            peer.Connect("session_description_created", Callable.From((string type, string sdp) => OnSessionDescriptionCreated(id, type, sdp)));
            peer.Connect("ice_candidate_created", Callable.From((string media, long index, string name) => OnIceCandidateCreated(id, media, index, name)));
            
            debugTextEmit?.Invoke($"binding id + {id}, my id is {myId}");
            rtcPeer.AddPeer(peer,id);

            if (id < rtcPeer.GetUniqueId()){
                peer.CreateOffer();
            }

            // if (myId < rtcPeer.GetUniqueId()){
            //     peer.CreateOffer();
            //     GD.Print("LALALALAL");
            // } 

        }


    }


    [Rpc (MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void Ping(){
        GD.Print("ping from " + Multiplayer.GetRemoteSenderId());
        debugTextEmit.Invoke($"ping from  + {Multiplayer.GetRemoteSenderId()}");

    }

    

    
    public void AttemptStartGame(){
        if (myState != State.IN_LOBBY){
            debugTextEmit.Invoke($"you are not in lobby, aborting. Current state {myState.ToString()}");
            return;
        }

        if (hostId == myId){
            debugTextEmit.Invoke($"I should be the authority");
            NetworkPacket message = new NetworkPacket(){
                Message = Message.DELETE_LOBBY,
                LobbyValue = lobbyValue,
                Id = myId,
            };
            

            peer.PutPacket(JsonSerializer.Serialize(message).ToUtf8Buffer());

            Rpc("StartGame");
        } else {
            debugTextEmit.Invoke($"you are not the host, you cannot start the game");
        }
    }

    [Rpc (MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SetAuthority(int id, bool print_callback = false){
        SetMultiplayerAuthority(id);
        AuthorityPEERID = id;
        if (print_callback){
            debugTextEmit.Invoke($"new authority : {id} , {GetMultiplayerAuthority()}");
            PrintPeers();
        }
    }


    [Rpc (MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SynchronizeAuthority(NodePath TargetNode, bool recursive){

        Node target = GetNodeOrNull<Node>(TargetNode);
        if (target==null){
            GD.PushError("big synchronize error");
            return;
        }
        target.SetMultiplayerAuthority(AuthorityPEERID, recursive);

    }

    [Rpc (MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartGame(){
        if (IsMultiplayerAuthority()){
            debugTextEmit.Invoke($"this instance is the multiplayer authority");

        }
        debugTextEmit.Invoke($"Starting gameplay session");
        myState = State.IN_ACTIVE_SESSION;
        GameStarted.Invoke();
    }

    [Rpc (MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferChannel = 1, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void _SendInputToAuthority(byte[] serializedData){
        if (Client.instance.IsMultiplayerAuthority()){
            NetworkInputPacket packetRecieved = JsonSerializer.Deserialize<NetworkInputPacket>(serializedData.GetStringFromUtf8());
            Client.instance.InputPackedRecieved.Invoke(Multiplayer.GetRemoteSenderId(),packetRecieved);
        }
    }
    static public void SendInputToAuthority(NetworkInputPacket packetToSend){
        byte[] data = JsonSerializer.Serialize(packetToSend).ToUtf8Buffer();
        Client.instance.Rpc("_SendInputToAuthority",[data]);
    }

    [Rpc (MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferChannel = 1, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    private void _SendInputToAuthorityUnreliable(byte[] serializedData){
        if (Client.instance.IsMultiplayerAuthority()){

        }

    }

    static public void SendInputToAuthorityUnreliable(NetworkInputPacket packetToSend){
        throw new NotImplementedException();
    }




    //ice candidate candidate
    private void OnIceCandidateCreated(int id, string midName, long indexName, string sdpName)
    {
        NetworkPacket message = new NetworkPacket(){
            Peer=id,
            OrgPeer = myId,
            Message = Message.CANDIDATE,
            Mid = midName,
            Index = indexName,
            Sdp = sdpName,
            LobbyValue = this.lobbyValue
        };

        peer.PutPacket(JsonSerializer.Serialize(message).ToUtf8Buffer());  

    }

    //offer created
    private void OnSessionDescriptionCreated(int id, string type, string sdp)
    {
        //we need to make sure our rtc actually has the peer we are trying to connect with
        if (!rtcPeer.HasPeer(id)){
            return;
        }
        WebRtcPeerConnection connection = (WebRtcPeerConnection)rtcPeer.GetPeer(id)["connection"];
        connection.SetLocalDescription(type,sdp);

        if (type=="offer"){
            sendOffer(id,sdp);
        }else{
            sendAnswer(id,sdp);
        }
    }

    private void sendAnswer(int id, string sdp)
    {
        NetworkPacket message = new NetworkPacket(){
            Peer=id,
            OrgPeer = myId,
            Message = Message.ANSWER,
            Data = sdp,
            LobbyValue = this.lobbyValue
        };
        peer.PutPacket(JsonSerializer.Serialize(message).ToUtf8Buffer());
    }

    private void sendOffer(int id, string sdp)
    {
        NetworkPacket message = new NetworkPacket(){
            Peer=id,
            OrgPeer = myId,
            Message = Message.OFFER,
            Data = sdp,
            LobbyValue = this.lobbyValue
        };
        peer.PutPacket(JsonSerializer.Serialize(message).ToUtf8Buffer());
    }
}
