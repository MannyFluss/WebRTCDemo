using Godot;
using System;

public partial class Game : Node2D
{
    [Export]
    Node2D SpawnPath;



    public override void _Ready()
    {
        base._Ready();
        Setup();
    }
    public void Setup(){
    }

    public void StartGame(){
        return;
    }

}
