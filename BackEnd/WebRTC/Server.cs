using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

public partial class Server : Node
{

    WebSocketMultiplayerPeer peer = new WebSocketMultiplayerPeer();

    //it is a little odd that users are stored as network packets, but it should work.
    Dictionary<int, NetworkPacket> Users = new Dictionary<int, NetworkPacket>();
    Dictionary<string, Lobby> Lobbies = new Dictionary<string, Lobby>();
    string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private const float lobbyMaxWaitTime = 15f * 60;

    public const int defaultPort = 8976;
    [Export]
    bool debug = true;

    public event Action<string> debugTextEmit;

    public override void _Ready()
    {
        base._Ready(); 
        peer.PeerConnected += OnPeerConnected;
        peer.PeerDisconnected += OnPeerDisconnected;
        
        foreach (string arg in OS.GetCmdlineArgs()){
            switch (arg){
                case "-server":
                    CreateServer();
                    debugTextEmit += (string value)=> { GD.Print(value);};
                    break;
            }

        }

    }

    public void CreateServer(int port = defaultPort){

        peer.CreateServer(port);
        debugTextEmit.Invoke($"server created at port {port}");

    }


    private void OnPeerConnected(long _id)
    {
        NetworkPacket NewUser = new NetworkPacket(){
            Id=(int)_id,
            Message=Message.ID,
        };

        Users[(int)_id] = NewUser;
        debugTextEmit.Invoke($"peer is attempting to connect with new id {_id}");

        SendPacketToPeer((int)_id,NewUser);


    }
    private void OnPeerDisconnected(long id)
    {
        Users.Remove((int)id);
    }

    private void SendPacketToPeer(int id, NetworkPacket packet){
        peer.GetPeer(id).PutPacket(JsonSerializer.Serialize(packet).ToUtf8Buffer());
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
                //debugTextEmit.Invoke($"server recieved packet from  {deserializedPacket.Id} {packetString}");
                parsePacket(deserializedPacket);
            }

        }
    }

 
    private void parsePacket(NetworkPacket packet){
        switch(packet.Message){
            case Message.NONE:
                GD.PushError("Packet had no message");
                break;
            case Message.JOIN_LOBBY:
                JoinLobby(packet);
                break;
            case Message.DELETE_LOBBY:
                DeleteLobby(packet.LobbyValue);
                
                break;
        }

        if (packet.Message == Message.OFFER || packet.Message == Message.ANSWER
             || packet.Message == Message.CANDIDATE){
                debugTextEmit.Invoke($"source id is {packet.OrgPeer} Message Data {packet.Data}");
                //this isnt working peer is null?
                SendPacketToPeer(packet.Peer,packet);
             }

    }

    private void JoinLobby(NetworkPacket packet){

        string CurrentLobbyValue = packet.LobbyValue;
        //no lobby has been created we need to create a lobby
        if (CurrentLobbyValue == ""){
            CurrentLobbyValue = GenerateRandomString();
            Lobbies[CurrentLobbyValue] = new Lobby(packet.Id);         
        }

        // client sends packet to server, attempting to join lobby, adds themselves to lobby,
        // messages all other users that they connected
        // then messages itself that all other users also exist ok makes sense

        //created at the request of the client attempting to join our lobby
        User newUser = Lobbies[CurrentLobbyValue].AddPlayer(packet.Id,packet.Name);

        foreach (int userID in Lobbies[CurrentLobbyValue].Users.Keys){
            
            //tells other users that this client just connected
            NetworkPacket Packet1 = new NetworkPacket(){
                Message = Message.USER_CONNECTED,
                Id = packet.Id,
            };
            SendPacketToPeer(userID,Packet1);

            //tells our client that just requested all of the other players that exist in this lobby already
            NetworkPacket Packet2 = new NetworkPacket(){
                Message = Message.USER_CONNECTED,
                Id = userID,
            };
            SendPacketToPeer(packet.Id,Packet2);
        }

        NetworkPacket LobbyInfoPacket = new NetworkPacket(){
            Message=Message.LOBBY_CONNECTED,
            Id = packet.Id,
            UserData = JsonSerializer.Serialize(Lobbies[CurrentLobbyValue].Users),
            HostId = Lobbies[CurrentLobbyValue].HostId,
            LobbyValue = CurrentLobbyValue,

        };
        debugTextEmit.Invoke($"user joined lobby... \n Updated Lobby Info {CurrentLobbyValue}: {Lobbies[CurrentLobbyValue].StringUsers()}");

        //asynchronous call to delete the lobby after a certain amount of time.    
        GetTree().CreateTimer(lobbyMaxWaitTime).Timeout += () => DeleteLobby(CurrentLobbyValue);

        SendPacketToPeer(packet.Id,LobbyInfoPacket);

    }

    private void DeleteLobby(string id){
        if (Lobbies.ContainsKey(id)){
            Lobbies.Remove(id);
            debugTextEmit.Invoke($"Lobby has been deleted {id}. sesion has started, or {lobbyMaxWaitTime/60} mins have passed");
        }
        PrintLobbies();

    }

    private void PrintLobbies(){
        foreach (var kvp in Lobbies){
            debugTextEmit.Invoke($"Lobby: {kvp.Key}, with {kvp.Value.Users.Count} users");
        }
        debugTextEmit.Invoke($"Current Lobbies:");

    }

    private string GenerateRandomString(){
        string res = "";
        for(int i=0;i<32;i++){
            res += Characters[(int)(GD.Randi() % Characters.Length)];
        }
        return res;
    }

}
