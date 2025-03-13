using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;


public struct PlayerState {
    public Vector2 position {set;get;}

    public PlayerState(Vector2 _position){
        position = _position;
    }
}
public struct SampleGameState {

    public Dictionary<int,PlayerState> Players {set;get;}

    public SampleGameState(Dictionary<int,PlayerState> _Players = null){
        Players = _Players ?? new Dictionary<int,PlayerState>(); 
    }
}


public partial class SampleGameBackend : Node2D
{
    private PackedScene PlayerScene = GD.Load<PackedScene>("res://SampleGame/Character/Character.tscn");
    private Dictionary<int,Node2D> MyPlayers = new Dictionary<int,Node2D>();
    private Node2D PlayersSpawnPath;
    public override void _Ready()
    {
        PlayersSpawnPath = GetNode<Node2D>("%Players");
        base._Ready();
        foreach (int peerId in Multiplayer.GetPeers().Append(Multiplayer.GetUniqueId())){
            GD.Print(peerId);
            Node2D newPlayer = PlayerScene.Instantiate<Node2D>();
            newPlayer.Position = new Vector2(500,100);
            MyPlayers[peerId] = newPlayer;
            PlayersSpawnPath.AddChild(newPlayer);

        }
    }




    public SampleGameState GetGameState(){
        Dictionary<int,PlayerState> toAdd = new Dictionary<int, PlayerState>(); 

        foreach (var kvp in MyPlayers){
            toAdd[kvp.Key] = new PlayerState(kvp.Value.Position);

        }

        return new SampleGameState(toAdd);

    }

}
