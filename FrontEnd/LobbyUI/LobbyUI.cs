using Godot;
using System;
using System.Dynamic;

public partial class LobbyUI : Control
{

    enum Status {
        SERVER,
        CLIENT,
        INACTIVE,
    }

    Status myStatus = Status.INACTIVE;

    Control myClientUI;

    Control myServerUI;

    Server MyServer;
    Client MyClient;

    

    Button connectserverbutton;
    Button connectclientbutton;
    Button packetbutton;

    Button joinLobbyButton;
    LineEdit LobbyValueEdit;

    LineEdit IPLineEdit;
    LineEdit PortLineEdit;

    LineEdit ServerLineEdit;
    RichTextLabel LobbyValueLabel;
    Button CopyLabelValue;
    Button DisconnectClientButton;
    Button StartLobbyButton;
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

        IPLineEdit = GetNode<LineEdit>("%IPAddressEdit");
        PortLineEdit = GetNode<LineEdit>("%PortEdit");

        myClientUI = GetNode<Control>("%ClientUI");
        myServerUI = GetNode<Control>("%ServerUI");
        LobbyValueLabel = GetNode<RichTextLabel>("%LobbyValue");
        ServerLineEdit = GetNode<LineEdit>("%ServerPortLineEdit");
        DisconnectClientButton = GetNode<Button>("%DisconnectClient");

        CopyLabelValue = GetNode<Button>("%CopyLobbyValue");
        StartLobbyButton = GetNode<Button>("%StartLobby");

        MyClient.LobbyValueRecieved += (string value) => { GD.Print( "hi", value) ; LobbyValueLabel.Text = value;};

        ConnectAllSignals();
    }

    private void ConnectAllSignals(){
        connectclientbutton.Pressed += onConnectClientPressed;
        connectserverbutton.Pressed += onServerButtonPressed;
        packetbutton.Pressed += onPacketButtonPressed;
        joinLobbyButton.Pressed += onJoinLobbyButtonPressed;
        CopyLabelValue.Pressed += onCopyLabelValueButtonPressed;
        StartLobbyButton.Pressed += OnStartButtonPressed;
        DisconnectClientButton.Pressed += OnDisconnectButtonPressed;
        MyClient.GameStarted += OnGameStarted;
        
    }


    private void DisconnectAllSignals(){
        connectclientbutton.Pressed -= onConnectClientPressed;
        connectserverbutton.Pressed -= onServerButtonPressed;
        packetbutton.Pressed -= onPacketButtonPressed;
        joinLobbyButton.Pressed -= onJoinLobbyButtonPressed;
        CopyLabelValue.Pressed -= onCopyLabelValueButtonPressed;
        StartLobbyButton.Pressed -= OnStartButtonPressed;
        DisconnectClientButton.Pressed -= OnDisconnectButtonPressed;
        MyClient.GameStarted -= OnGameStarted;
        
    }

    private void OnGameStarted()
    {
        Visible = false;
        GetViewport().GuiReleaseFocus();
        MouseFilter = MouseFilterEnum.Stop;
        //DisconnectAllSignals();
        myClientUI.Visible = false;
        myServerUI.Visible = false;
        
    }


    private void OnDisconnectButtonPressed()
    {
        MyClient.Disconnect();
    }


    private void OnStartButtonPressed()
    {
        MyClient.AttemptStartGame();

    }


    private void onCopyLabelValueButtonPressed()
    {
        DisplayServer.ClipboardSet(LobbyValueLabel.Text);

    }


    private void SetStatus(Status newStatus){
        switch(newStatus){
            case Status.SERVER:
                myClientUI.Visible = false;
                myServerUI.Visible = true;
                connectclientbutton.Visible=false;
                connectserverbutton.Visible=false;
                break;
            case Status.CLIENT:
                myServerUI.Visible = false;
                myClientUI.Visible = true;
                connectclientbutton.Visible=false;
                connectserverbutton.Visible=false;
                break;
            case Status.INACTIVE:
                myServerUI.Visible = false;
                myClientUI.Visible = false;
                connectclientbutton.Visible=true;
                connectserverbutton.Visible=true;
                break;
        }

    }

    private void onJoinLobbyButtonPressed()
    {
        MyClient.JoinLobby(LobbyValueEdit.Text);
    }


    private void onServerButtonPressed()
    {
        try{
            int port = ServerLineEdit.Text.ToInt();
            if (port == 0){
                port = Server.defaultPort;
            }
            MyServer.CreateServer(port);
        } catch (Exception){
            MyServer.CreateServer();
        } 
    }


    private void onPacketButtonPressed()
    {
        MyClient.Rpc("Ping");
    }


    private void onConnectClientPressed()
    {
        MyClient.ConnectToServer(IPLineEdit.Text, PortLineEdit.Text);
    }

}
