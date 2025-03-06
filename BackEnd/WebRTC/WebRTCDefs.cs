// using Godot;
// using System;
// using System.Collections.Generic;
// using System.Text.Json;

// public enum Message{
// 		id,
// 		join,
// 		userConnected,
// 		userDisconnected,
// 		lobby,
// 		candidate,
// 		offer,
// 		answer,
// 		checkIn,
// 	}
// struct UserData{


//     public long id {set;get;}

//     public Message message {set;get;}
//     public UserData(long _id, Message _message){
//         id = _id;
//         message = _message;
//     }
// }
// public class NetworkPacket
// {
//     public string Message { get; set; }
//     public string Data { get; set; }
//     public long ID { get; set; }


//     public NetworkPacket(string message, string data, long id)
//     {
//         Message = message;
//         Data = data;
//         ID = id;
//     }

//     public byte[] ToBytes()
//     {
//         string jsonString = JsonSerializer.Serialize(this);
//         return System.Text.Encoding.UTF8.GetBytes(jsonString);
//     }

//     public static NetworkPacket FromBytes(byte[] data)
//     {
//         string jsonString = System.Text.Encoding.UTF8.GetString(data);
//         return JsonSerializer.Deserialize<NetworkPacket>(jsonString);
//     }
// }

// public class Lobby{
//     public long HostID;
    
//     public Dictionary<long,LobbyEntry> players = new Dictionary<long,LobbyEntry>();
//     public Lobby(long id){
//         HostID = id;
//     }

//     public LobbyEntry AddPlayer(long _id, string _name){
//         players[_id] = new LobbyEntry(_name,_id,players.Count+1); 
//         return players[_id];
//     }

// }

// public struct LobbyEntry {
//     public string name {set;get;}
//     public long id {set;get;}
//     public int index {set;get;}

//     public LobbyEntry(string _name, long _id, int _index){
//         name=_name;
//         id=_id;
//         index=_index;
//     }
// }

// public struct LobbyRequestPacket {
//     public long id {set;get;}
//     public Message message {set;get;}
//     public string LobbyValue {set;get;}
//     public LobbyRequestPacket(long _id, Message _message, string _LobbyValue){
//             id=_id;
//             message=_message;
//             LobbyValue=_LobbyValue;
//     }
    
// }

// public struct JoinLobbyResults {
//     public long id {set;get;}
//     public Message message {set;get;}
//     public long host {set;get;}
//     public long player {set;get;}

//     public JoinLobbyResults(long _id, Message _message, long _host, long _player){
//             id=_id;
//             message=_message;
//             host=_host;
//             player=_player;
//     }
    
// }

// public class LobbyPlayerInfoPacket{
//     public Message message;
    
//     public string players;
//     public LobbyPlayerInfoPacket(Message _message, string _players ){
//         message = _message;
//         players = _players;
//     }

// }