using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;


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
    private PackedScene PlayerScene = GD.Load<PackedScene>("res://SampleGame/Backend/BackendCharacter/Character.tscn");
    public Dictionary<int,Node2D> MyPlayers = new Dictionary<int,Node2D>();
    private Node2D PlayersSpawnPath;
    private SampleGameState MyState;
    public override void _Ready()
    {

        Client.instance.SynchronizeAuthority(GetPath(),false);
        if (!IsMultiplayerAuthority()){
            return;
        }

        PlayersSpawnPath = GetNode<Node2D>("%Players");
        base._Ready();
        foreach (int peerId in Multiplayer.GetPeers().Append(Multiplayer.GetUniqueId())){
            GD.Print(peerId);
            Node2D newPlayer = PlayerScene.Instantiate<Node2D>();
            newPlayer.Position = new Vector2(500,100);
            MyPlayers[peerId] = newPlayer;
            PlayersSpawnPath.AddChild(newPlayer,true);
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (IsMultiplayerAuthority()){
            updateGameState();
            string stateData =  JsonSerializer.Serialize(GetGameState());
            Rpc("replicateGameStates", stateData);
        }
    }

    [Rpc (MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    private void replicateGameStates(string data){
        SampleGameState newState = JsonSerializer.Deserialize<SampleGameState>(data);
        MyState = newState;
    }

    private void updateGameState(){
        Dictionary<int,PlayerState> toAdd = new Dictionary<int, PlayerState>(); 

        foreach (var kvp in MyPlayers){
            toAdd[kvp.Key] = new PlayerState(kvp.Value.Position);

        }

        MyState = new SampleGameState(toAdd);

        //add in some sort of rpc to send this out to all the other peers
    }


    public SampleGameState GetGameState(){
        return MyState;
    }

}
