using Godot;
using System;

public partial class GameLoader : Node
{
    [Export]
    MultiplayerSpawner MySpawner;
    public override void _Ready()
    {
        base._Ready();
        Client.instance.GameStarted += StartGame;

    }

    private void StartGame()
    {
        if (Client.instance.IsMultiplayerAuthority()){
            GD.Print("set authority");
            Client.instance.Rpc("SynchronizeAuthority",[this.GetPath()]);
            
        }


        if (IsMultiplayerAuthority()){
            GD.Print("Server is spawning the object.");
            Game toAdd = GD.Load<PackedScene>("res://SampleGame/Game.tscn").Instantiate<Game>();

            AddChild(toAdd);


        }
    }

}
