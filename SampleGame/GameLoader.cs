using Godot;
using System;
using System.Threading.Tasks;

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
            Client.instance.Rpc("SynchronizeAuthority",[MySpawner.GetPath(),true]);
            Client.instance.Rpc("SynchronizeAuthority",[this.GetPath(),true]);
        }

        if (IsMultiplayerAuthority()){
            GD.Print("Server is spawning the object.");
            Game toAdd = GD.Load<PackedScene>("res://SampleGame/Game.tscn").Instantiate<Game>();

            AddChild(toAdd);


        }
    }

}
