// using Godot;
// using Godot.Collections;
// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Text.Json;

// public partial class WEBrtcClient : Node
// {

// 	const string DebugIP = "ws://127.0.0.1:8915";
// 	// enum Message{
// 	// 	id,
// 	// 	join,
// 	// 	userConnected,
// 	// 	userDisconnected,
// 	// 	lobby,
// 	// 	candidate,
// 	// 	offer,
// 	// 	answer,
// 	// 	checkIn,
// 	// }

// 	WebSocketMultiplayerPeer peer = new WebSocketMultiplayerPeer();

//     long id = 0;

//     //this is so bad
//     public override void _Process(double delta)
//     {
//         base._Process(delta);
//         peer.Poll();
//         if (peer.GetAvailablePacketCount() > 0){
//             var packet = peer.GetPacket();
//             if (packet != null){
//                 var dataString = packet.GetStringFromUtf8();
//                 InterpretMessage(dataString);
                


//                 UserData DataPacket = JsonSerializer.Deserialize<UserData>(dataString);

//                 JoinLobbyResults Packres = JsonSerializer.Deserialize<JoinLobbyResults>(dataString);
//                 if (Packres.message == Message.userConnected){
//                     createPeer(Packres.id);
//                 }
//                 LobbyPlayerInfoPacket lobbyplayers = JsonSerializer.Deserialize<LobbyPlayerInfoPacket>(dataString);
//                 if (lobbyplayers.message == Message.lobby){
//                     //ehhh
//                 }


//             }
//         }
//     }


//     private void createPeer(long id){


//     }

//     private void InterpretMessage(string dataString){
//         UserData? DataPacket = JsonSerializer.Deserialize<UserData>(dataString);
//         if (DataPacket!=null){
//             GD.Print("my id is ", DataPacket?.id);
//         }
//         JoinLobbyResults? joinLobbyPacket = JsonSerializer.Deserialize<JoinLobbyResults>(dataString);
//         if (joinLobbyPacket!=null){
//             createPeer((long)joinLobbyPacket?.id);
//         }
//         LobbyPlayerInfoPacket? lobbyPacketRecieved = JsonSerializer.Deserialize<LobbyPlayerInfoPacket>(dataString);
        
//     }



//     public void StartClient(string ip){
// 		GD.Print("started client");
// 		peer.CreateClient(DebugIP);

// 	}

//     public void SendTestPacket(){
//         NetworkPacket message = new NetworkPacket("hello", "goodbye", 123);
//         byte[] messageBytes = message.ToBytes();
//         peer.PutPacket(messageBytes);
//     }

//     public void JoinLobby(string LobbyValue){
//         LobbyRequestPacket NewPacket = new LobbyRequestPacket(id,Message.lobby,LobbyValue);
//         peer.PutPacket(JsonSerializer.Serialize(NewPacket).ToUtf8Buffer());

//     }

// }
