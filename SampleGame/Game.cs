using Godot;
using System;

public partial class Game : Node2D
{
    [Export]
    Node2D SpawnPath;

    PackedScene MyScene = GD.Load<PackedScene>("res://SampleGame/Character/Character.tscn");


    public override void _Ready()
    {
        base._Ready();
        Setup();
    }
    public void Setup(){
    }

    public void StartGame(){
        return;
        if (Client.instance.IsMultiplayerAuthority() || true){
            foreach (int peer in Multiplayer.GetPeers()){
                Node instance = MyScene.Instantiate();
                instance.Name = peer.ToString();
                instance.SetMultiplayerAuthority(peer);
                SpawnPath.AddChild(instance,true);
            }
        }
    }

}
