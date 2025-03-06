using Godot;
using System;
using Godot.Collections;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Mail;

public partial class Client : Node
{
    int myId = 0;
    int hostId = 0;
    string lobbyValue = "";

    string ipAdrress = "ws://127.0.0.1:8976";

    [Export]
    bool debug=true;

    public event Action<string> debugTextEmit;

    WebSocketMultiplayerPeer peer = new WebSocketMultiplayerPeer();
    WebRtcMultiplayerPeer rtcPeer = new WebRtcMultiplayerPeer();

    public override void _Ready()
    {
        base._Ready();
        Multiplayer.ConnectedToServer += OnConnectedToRPCServer;
        Multiplayer.PeerConnected += OnRTCPeerConnected;
        Multiplayer.PeerDisconnected += OnRTCPeerDisconnected;
 
    }

    private void OnRTCPeerDisconnected(long id)
    {
        debugTextEmit.Invoke($"RTC peer disconnected : {id}");

    }


    private void OnRTCPeerConnected(long id)
    {
        GD.Print("RTC peer connected");
        debugTextEmit.Invoke($"RTC peer connected : {id}");

    }


    private void OnConnectedToRPCServer()
    {
        GD.Print("connected to RTC server");
        debugTextEmit.Invoke("RTC server connected");
    }


    public void ConnectToServer(){
        peer.CreateClient(ipAdrress);
        debugTextEmit?.Invoke($"Client connection to server {ipAdrress}");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
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

            if (hostId != myId){
                peer.CreateOffer();
            }

            // if (myId < rtcPeer.GetUniqueId()){
            //     peer.CreateOffer();
            //     GD.Print("LALALALAL");
            // }

        }


    }


    [Rpc (MultiplayerApi.RpcMode.AnyPeer)]
    public void Ping(){
        GD.Print("ping from " + Multiplayer.GetRemoteSenderId());
        debugTextEmit.Invoke($"ping from  + {Multiplayer.GetRemoteSenderId()}");
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
