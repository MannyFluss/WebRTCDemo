using Godot;
using System;

public partial class BackendInputParser : Node
{

    private SampleGameBackend MyBackend;
    public override void _Ready()
    {
        base._Ready();
        if (!Client.instance.IsMultiplayerAuthority()){
            return;
        }
        MyBackend = GetParent<SampleGameBackend>();
        Client.instance.InputPackedRecieved += OnInputPacketRecieved;
        Client.instance.InputPackedRecievedUnreliable += OnInputPacketRecieved;
    }

    private void OnInputPacketRecieved(int senderID, NetworkInputPacket packet)
    {
        GD.Print($"recieved packet from {senderID} : {packet}");

        if (packet.type == InputType.FLICK){
            MyBackend.pingPlayerBall(senderID);
        }
        if (packet.type == InputType.GAME_PRESSED){
            MyBackend.PlayerStartMoving(senderID);
        }
        if (packet.type == InputType.GAME_HELD){
            MyBackend.PlayerMoving(senderID,(Vector2)packet.vec1);
        }
        if (packet.type == InputType.GAME_RELEASED){
            MyBackend.PlayerStopMoving(senderID);
        }
    }

}
