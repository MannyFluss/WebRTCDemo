using Godot;
using System;


public struct SampleGameState {

    public Vector2 position1 {set;get;}
    public Vector2 position2 {set;get;}

    public SampleGameState(Vector2 _position1, Vector2 _position2){
        position1=_position1;
        position2=_position2;
    }
}


public partial class SampleGameBackend : Node2D
{
    [Export]
    Node2D ball1;
    [Export]
    Node2D ball2;

    public override void _Ready()
    {
        base._Ready();
        if (!Client.instance.IsMultiplayerAuthority()){
            QueueFree();
        }
    }

    public SampleGameState GetGameState(){
        return new SampleGameState(ball1.Position,ball2.Position);
    }

}
