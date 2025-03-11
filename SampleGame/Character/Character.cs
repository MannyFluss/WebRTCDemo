using Godot;
using System;

public partial class Character : Node2D
{
    float speed = 10;
    public override void _Input(InputEvent @event)
    {
        if (!Client.instance.IsMultiplayerAuthority()){
            return;
        }

        base._Input(@event);
        if (@event.IsActionPressed("ui_up")){
            Position += Vector2.Down * speed; 
        }

        if (@event.IsActionPressed("ui_down")){
            Position += Vector2.Up * speed; 
        }

        if (@event.IsActionPressed("ui_left")){
            Position += Vector2.Right * speed; 
        }
        if (@event.IsActionPressed("ui_right")){
            Position += Vector2.Left * speed; 
        }

    }
}
