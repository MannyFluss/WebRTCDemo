// using Godot;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Data.Common;
// using System.Diagnostics;
// using System.Text.Json;


// public partial class WEBrtcServer : Node
// {


// 	const string DebugIP = "ws://127.0.0.1:8915";

//     const string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";


// 	WebSocketMultiplayerPeer peer = new WebSocketMultiplayerPeer();

//     private Dictionary<long,UserData> Users = new Dictionary<long,UserData>();

//     private Dictionary<string, Lobby> Lobbies = new Dictionary<string,Lobby>();
//     public override void _Ready()
//     {
//         peer.PeerConnected += OnPeerConnected;
//         peer.PeerDisconnected += OnPeerDisonnected;
//     }

//     private void OnPeerDisonnected(long id)
//     {

//     }


//     private void OnPeerConnected(long id)
//     {
//         Users[id] = new UserData(id, Message.id);


//         SendPacketToPeer(id,Users[id]);


//     }

//     public void JoinLobby(LobbyRequestPacket user){
//         string result = "";
//         //if no lobby exists yet
//         if (user.LobbyValue==""){
//             user.LobbyValue = GenerateRandomString();
//             Lobbies[user.LobbyValue] = new Lobby(user.id);
//             GD.Print("new lobby was created!");
//             GD.Print(user.LobbyValue);

//         }
//         LobbyEntry player = Lobbies[user.LobbyValue].AddPlayer(user.id,user.LobbyValue);

//         foreach (long p in Lobbies[user.LobbyValue].players.Keys){
//             var packet = new LobbyPlayerInfoPacket(Message.lobby,JsonSerializer.Serialize(Lobbies[user.LobbyValue].players));
//             SendPacketToPeer(p,packet);

//         }



//         JoinLobbyResults message = new JoinLobbyResults(
//             user.id,
//             Message.userConnected,
//             Lobbies[user.LobbyValue].HostID,
//             Lobbies[user.LobbyValue].players[user.id].id

//         );
//         GD.Print(JsonSerializer.Serialize(message).ToUtf8Buffer());
//         SendPacketToPeer(user.id,message);
    
//     }

//     void SendPacketToPeer(long targetPlayerID, object data){
//         peer.GetPeer((int)targetPlayerID).PutPacket(JsonSerializer.Serialize(data).ToUtf8Buffer());
//     }

//     private string GenerateRandomString(){
//         string res = "";
//         for(int i=0;i<32;i++){
//             res += characters[(int)(GD.Randi() % characters.Length)];
//         }
//         return res;
//     }

//     public void StartServer(){
//         GD.Print("server started");
// 		peer.CreateServer(8915);
// 	}

//     public override void _Process(double delta)
//     {
//         base._Process(delta);
//         peer.Poll();
//         if (peer.GetAvailablePacketCount() > 0){
//             var packet = peer.GetPacket();
//             if (packet != null){
//                 var dataString = packet.GetStringFromUtf8();
//                 var data = Json.ParseString(dataString);

//                 LobbyRequestPacket possiblePacket = JsonSerializer.Deserialize<LobbyRequestPacket>(dataString);
                
//                 if (possiblePacket.message==Message.lobby){
//                     JoinLobby(possiblePacket);
//                 }


//                 GD.Print(data);

//             }


//         }
//     }


// }
