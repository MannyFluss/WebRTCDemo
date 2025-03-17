using Godot;
using System;

public partial class ClientInputManager : Node
{
    private enum InputState{
        NONE,
        PRESSED,
    }

    InputState myState = InputState.NONE;

    Vector2 LastInputVector = Vector2.Zero;

    public override void _Ready()
    {
        base._Ready();

    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (Input.IsActionJustPressed("game_click")){
            //to do later input differentiation between mouse + touchscreen
            if (true){
                Client.SendInputToAuthority(
                    new NetworkInputPacket(InputType.GAME_PRESSED,null)
                );
                LastInputVector = GetViewport().GetMousePosition();
            }
        }
        if (Input.IsActionPressed("game_click")){
            //to do later input differentiation between mouse + touchscreen
            if (true){
                Vector2 mouseDelta = GetViewport().GetMousePosition() - LastInputVector;
                
                Client.SendInputToAuthority(
                    new NetworkInputPacket(InputType.GAME_HELD,mouseDelta)
                );
                LastInputVector = GetViewport().GetMousePosition();
            }
        }
        if (Input.IsActionJustReleased("game_click")){
            //to do later input differentiation between mouse + touchscreen
            if (true){
                Client.SendInputToAuthority(
                    new NetworkInputPacket(InputType.GAME_RELEASED,null)
                );
                LastInputVector = GetViewport().GetMousePosition();
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.IsActionPressed("ui_up")){
            Client.SendInputToAuthority(new NetworkInputPacket(InputType.FLICK));
        }
    }

}
