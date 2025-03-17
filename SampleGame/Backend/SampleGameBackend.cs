using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;


public struct PlayerState {
    public Vector2 position {set;get;}
    public float rotation {set;get;}


    public PlayerState(Vector2 _position, float _rotation){
        position = _position;
        rotation = _rotation;
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
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        Converters = { new Vector2Converter() }
    };
    private PackedScene PlayerScene = GD.Load<PackedScene>("res://SampleGame/Backend/BackendCharacter/Character.tscn");
    public Dictionary<int,Character> MyPlayers = new Dictionary<int,Character>();
    private Node2D PlayersSpawnPath;
    private SampleGameState MyState = new SampleGameState();
    public override void _Ready()
    {
        base._Ready();
        PlayersSpawnPath = GetNode<Node2D>("%Players");
        Client.instance.SynchronizeAuthority(GetPath(),false);
        if (!IsMultiplayerAuthority()){
            return;
        }
        CallDeferred("Setup");

    }
    public void Setup(){
        List<int> allIds = [.. Multiplayer.GetPeers(), Multiplayer.GetUniqueId()];

        foreach (int id in allIds){
            GD.Print(id, "gotten");
            
        }
        GD.Print(Multiplayer.GetUniqueId(), "my id");


        foreach (int peerId in allIds){
            Character newPlayer = PlayerScene.Instantiate<Character>();
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
            string stateData =  JsonSerializer.Serialize(GetGameState(),JsonOptions);
            Rpc("replicateGameStates", stateData);
        }
    }

    [Rpc (MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferChannel = 0, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    private void replicateGameStates(string data){
        SampleGameState newState = JsonSerializer.Deserialize<SampleGameState>(data,JsonOptions);
        MyState = newState;
    }

    private void updateGameState(){
        Dictionary<int,PlayerState> toAdd = new Dictionary<int, PlayerState>(); 

        foreach (var kvp in MyPlayers){
            toAdd[kvp.Key] = new PlayerState(kvp.Value.Position, kvp.Value.Rotation);
            //GD.Print(kvp.Value.Position);
        }

        MyState = new SampleGameState(toAdd);

        //add in some sort of rpc to send this out to all the other peers
    }

    public void pingPlayerBall(int playerID){
        if (MyPlayers.ContainsKey(playerID)){
            MyPlayers[playerID].Flick();
        } else {
            GD.PushError("game state does not contain ", playerID);
        }
    }

    public void PlayerStartMoving(int playerID){

    }
    public void PlayerMoving(int playerID, Vector2 moveBy){
        if (MyPlayers.ContainsKey(playerID)){
            MyPlayers[playerID].Position += moveBy;
            if (moveBy.Length() > 3){
                MyPlayers[playerID].Rotation = moveBy.Angle();
            } else {
                MyPlayers[playerID].Rotation = MyPlayers[playerID].Rotation;
            }
                

        } else {
            GD.PushError("game state does not contain ", playerID);
        }  
    }
    public void PlayerStopMoving(int playerID){
        
    }

    public SampleGameState GetGameState(){
        return MyState;
    }

}
