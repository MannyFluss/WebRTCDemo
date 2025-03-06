// using Godot;
// using System;


// struct ServerSettings {

//     public string IPADDRESS {set;get;}
//     public int PORT {set;get;}
//     public int MAX_CLIENTS {set;get;}

//     public ServerSettings(){
//         IPADDRESS = "127.0.0.1";
//         PORT = 9888;
//         MAX_CLIENTS = 8; 
//     }
// }

// public enum OnlineState {
//     NONE,
//     CLIENT,
//     SERVER,

// }

// public partial class Server : Node
// {
//     public OnlineState MyState = OnlineState.NONE;
    
//     ServerSettings MySettings = new ServerSettings();


//     public override void _Ready()
//     {
//         base._Ready();
//         if (OS.HasFeature("server")){
//             CallDeferred("StartServer");
//         }
//         if (OS.HasFeature("client")){
//             CallDeferred("StartClient");
//         }

//     }

//     public void StartClient(){
//         if (MyState != OnlineState.NONE){
//             GD.PushError("this instance is already a client or server");
//             return;
//         }

//         var peer = new ENetMultiplayerPeer();
//         var error = peer.CreateClient(MySettings.IPADDRESS,MySettings.PORT);
//         if (error != Error.Ok){
//             GD.PushError("error creating client "+ error.ToString());
//             return;
//         }

//         Multiplayer.MultiplayerPeer = peer;
//         GD.Print("client created " + Multiplayer.GetUniqueId().ToString() );
//         MyState = OnlineState.CLIENT;

//     }

//     public void StartServer(){
//         if (MyState != OnlineState.NONE){
//             GD.PushError("this instance is already a client or server");
//             return;
//         }
        
//         var peer = new ENetMultiplayerPeer();
//         var error = peer.CreateServer(MySettings.PORT, MySettings.MAX_CLIENTS);
//         if (error != Error.Ok){
//             GD.PushError("error creating server "+ error.ToString());
//             return;
//         }

//         GD.Print("Server created");
//         Multiplayer.MultiplayerPeer = peer;
//         MyState = OnlineState.SERVER;
//     }

//     public void TerminateConnection(){
//         MyState = OnlineState.NONE;
//         Multiplayer.MultiplayerPeer = null;

//     }

//     private void SetupSignals(){
//         if (MyState == OnlineState.CLIENT 
//             || MyState == OnlineState.SERVER){
//                 Multiplayer.PeerConnected += PeerConnected;
//                 Multiplayer.PeerDisconnected += PeerDisconnected;
//                 Multiplayer.ConnectedToServer += ConnectedToServer; 
//                 Multiplayer.ConnectionFailed += ConnectionFailed;
//         }

//     }

//     //client only
//     private void ConnectionFailed()
//     {
//         GD.PushError("failed to connect to server");
//     }

//     //client only
//     private void ConnectedToServer()
//     {
//         GD.Print("Connected to server");

//     }

//     //runs on all peers
//     private void PeerDisconnected(long id)
//     {
//         GD.Print(id);
//     }
//     //runs on all peers
//     private void PeerConnected(long PeerID){
//         GD.Print(PeerID);
//     }



// }


