using Godot;
using System;

public partial class BackendInputParser : Node
{
    public override void _Ready()
    {
        base._Ready();
        if (!Client.instance.IsMultiplayerAuthority()){
            QueueFree();
        }
        Client.instance.InputPackedRecieved += OnInputPacketRecieved;
    }

    private void OnInputPacketRecieved(int senderID, NetworkInputPacket packet)
    {
        GD.Print($"recieved packet from {senderID} : {packet}");
    }

}
