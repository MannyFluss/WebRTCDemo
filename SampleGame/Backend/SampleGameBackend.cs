using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;



public class PlayerState {
    public enum InputState{
        ACTIVE,
        INACTIVE,
    };
    public Vector2 position {set;get;}
    public float rotation {set;get;}
    public InputState MyInputState {set;get;}
    public int health {set;get;}

    public PlayerState(Vector2 _position, float _rotation){
        position = _position;
        rotation = _rotation;
        //add options
        health = 3;
        MyInputState = InputState.ACTIVE;
    }
}
public class Projectile{
    public Vector2 position {set;get;}
    public Projectile(Vector2 _position){
        position=_position;
    }
}

//sent over the wire
public struct SampleGameState {
    public Dictionary<int,PlayerState> Players {set;get;}
    public List<Vector2> DefaultProjectilePositions {set;get;}
    public SampleGameState(Dictionary<int,PlayerState> _Players = null, List<Vector2> _projectiles = null){
        Players = _Players ?? new Dictionary<int,PlayerState>();
        DefaultProjectilePositions = _projectiles ?? new List<Vector2>(); 
    }
}

public class SampleGameAuthorityGameState{
    public Dictionary<int, Projectile> Projectiles;
    public SampleGameAuthorityGameState(){
        Projectiles = new Dictionary<int, Projectile>();
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
    private SampleGameAuthorityGameState MyAuthorityState = new SampleGameAuthorityGameState();
    public event Action<int> PlayerHit;

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
        PlayerHit += OnPlayerHit;

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
        List<Vector2> bullets = new List<Vector2>();
        foreach (var item in MyAuthorityState.Projectiles.Values)
        { 
            bullets.Add(item.position);  
        }

        foreach (var kvp in MyPlayers){
            PlayerState current = new PlayerState(kvp.Value.Position, kvp.Value.Rotation);
            
            toAdd[kvp.Key] = current;
            ProccessPlayer(current,kvp.Key);
        }
        MyState = new SampleGameState(toAdd,bullets);

        //add in some sort of rpc to send this out to all the other peers
    }

    //side effect players gets updated
    private void OnPlayerHit(int obj)
    {
        PlayerState playerHit = MyState.Players[obj];
        playerHit.health -= 1;
    }

    private void ProccessPlayer(PlayerState curr, int playerid){
        foreach (Vector2 projectile in MyState.DefaultProjectilePositions){
            //add setting here later
            if (projectile.DistanceSquaredTo(curr.position) < 5 * 5){
                PlayerHit.Invoke(playerid);
            }
        }
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

    public int RegisterNewProjectile(Projectile _projectile){
        MyAuthorityState.Projectiles[_projectile.GetHashCode()] = _projectile;
        return _projectile.GetHashCode();
    }

    public void DeleteRegisteredProjectile(int id){
        if (MyAuthorityState.Projectiles.ContainsKey(id)){
            MyAuthorityState.Projectiles.Remove(id);
        }
    }
    public SampleGameState GetGameState(){
        return MyState;
    }

}
