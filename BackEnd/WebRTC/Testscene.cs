using Godot;
using System;

public partial class Testscene : Control
{
    Server MyServer;
    Client MyClient;

    Button connectserverbutton;
    Button connectclientbutton;
    Button packetbutton;

    Button joinLobbyButton;
    LineEdit LobbyValueEdit;

    public override void _Ready()
    {
        base._Ready();
        MyServer = GetNode<Server>("Server");
        MyClient = GetNode<Client>("Client");
        LobbyValueEdit = GetNode<LineEdit>("%LobbyValueEdit");

        connectclientbutton = GetNode<Button>("%ConnectClient");
        connectserverbutton = GetNode<Button>("%ConnectServer");
        joinLobbyButton = GetNode<Button>("%JoinLobby");
        packetbutton = GetNode<Button>("%PacketTest");

        connectclientbutton.Pressed += onConnectClientPressed;
        connectserverbutton.Pressed += onServerButtonPressed;
        packetbutton.Pressed += onPacketButtonPressed;
        joinLobbyButton.Pressed += onJoinLobbyButtonPressed;


    }

    private void onJoinLobbyButtonPressed()
    {
        // MyClient.JoinLobby(LobbyValueEdit.Text);
    }


    private void onServerButtonPressed()
    {
        // MyServer.StartServer();
    }


    private void onPacketButtonPressed()
    {
        // MyClient.SendTestPacket();
    }


    private void onConnectClientPressed()
    {
        // MyClient.StartClient("");
    }

}
