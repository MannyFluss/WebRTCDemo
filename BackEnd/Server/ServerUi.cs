// using Godot;
// using System;

// public partial class ServerUi : Control
// {
//     private Server MyServer;

//     private RichTextLabel TextOutput;
//     private RichTextLabel StateLabel;
//     private RichTextLabel IDLabel;

//     private Button HostButton;
//     private Button ClientButton;
//     private Button DisconnectButton;




//     public override void _Ready()
//     {
//         base._Ready();
//         MyServer = GetParent<Server>();
//         TextOutput = GetNode<RichTextLabel>("%TextOutput");
//         StateLabel = GetNode<RichTextLabel>("%CurrentState");
//         IDLabel = GetNode<RichTextLabel>("%CurrentID");

//         HostButton = GetNode<Button>("%HostButton");
//         ClientButton = GetNode<Button>("%ClientButton");
//         DisconnectButton = GetNode<Button>("%DisconnectButton");

//         HostButton.Pressed += OnHostButtonPressed;
//         ClientButton.Pressed += OnClientButtonPressed;
//         DisconnectButton.Pressed += OnDisconnectButtonPressed;

//     }

//     private void OnDisconnectButtonPressed()
//     {
//         MyServer.TerminateConnection();
//     }
//     private void OnClientButtonPressed()
//     {
//         MyServer.StartClient();
//     }
//     private void OnHostButtonPressed()
//     {
//         MyServer.StartServer();
//     }

//     public override void _Process(double delta)
//     {
//         base._Process(delta);
//         StateLabel.Text = MyServer.MyState.ToString();
//         IDLabel.Text = Multiplayer.MultiplayerPeer?.GetUniqueId().ToString() ?? "N/A";
        
        
//     }





// }
