using Godot;
using System;
using System.Collections.Generic;

public partial class SampleGameRenderer : Node2D
{
    [Export]
    SampleGameBackend MyBackend;

    Node2D PlayersPath;
    [Export]
    MultiplayerSpawner MySpawner;
    
    private Dictionary<int, Node2D> myPlayerRenderers = new Dictionary<int, Node2D>();

    private PackedScene playerRendererScene = GD.Load<PackedScene>("res://SampleGame/Renderer/CharacterRenderer.tscn");
    //private PlayerReferences;
    public override void _Ready()
    {
        base._Ready();
        //Client.instance.GameStarted += OnGameStarted;
        PlayersPath = GetNode<Node2D>("%Players");
        Setup();
    }   

    private void Setup()
    {

        
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (!Client.active){
            return;
        }

        SampleGameState state = MyBackend.GetGameState();
        renderGameState(state);
        
    }
    private void renderGameState(SampleGameState state){
        var players = state.Players;
        foreach (var kvp in players){
            if (myPlayerRenderers.ContainsKey(kvp.Key)){
                myPlayerRenderers[kvp.Key].Position = myPlayerRenderers[kvp.Key].Position.Lerp(players[kvp.Key].position,.8f);
                myPlayerRenderers[kvp.Key].Rotation = Mathf.LerpAngle(myPlayerRenderers[kvp.Key].Rotation, players[kvp.Key].rotation,.8f);
            
            }else{
                instantiatePlayerRenderer(kvp.Key,kvp.Value);
            }
        }
    }
    private void instantiatePlayerRenderer(int id, PlayerState playerState){
        if (myPlayerRenderers.ContainsKey(id)){
            GD.PushError("already contains key ",id);
            return;
        }

        Node2D newRendererScene = playerRendererScene.Instantiate<Node2D>();
        newRendererScene.Position = playerState.position;
        newRendererScene.Name = id.ToString();
        PlayersPath.AddChild(newRendererScene,true);
        myPlayerRenderers[id] = newRendererScene;

        Client.instance.SynchronizeAuthority(newRendererScene.GetPath(),false);


    }


}
