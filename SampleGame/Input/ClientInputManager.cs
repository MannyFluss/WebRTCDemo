using Godot;
using System;

public partial class ClientInputManager : Node
{
    
    public override void _Ready()
    {
        base._Ready();

    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.IsActionPressed("ui_up")){
            Client.SendInputToAuthority(new NetworkInputPacket(InputType.FLICK));
        }
    }

}
