using Godot;
using System;
using System.Collections.Generic;

public partial class SampleGameRenderer : Node2D
{
    [Export]
    SampleGameBackend MyBackend;

    Node2D PlayersPath;
    
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
        if (Client.instance.IsMultiplayerAuthority()){
            //Client.instance.Rpc("SynchronizeAuthority",[this.GetPath(),true]);
        }
        Client.instance.SynchronizeAuthority(GetPath(),true);

    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (!Client.active){
            return;
        }

        if (IsMultiplayerAuthority()){
            SampleGameState state = MyBackend.GetGameState();
            renderGameState(state);
        }
    }

    private void renderGameState(SampleGameState state){
        var players = state.Players;
        foreach (var kvp in players){
            if (myPlayerRenderers.ContainsKey(kvp.Key)){
                myPlayerRenderers[kvp.Key].Position = kvp.Value.position;
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
        PlayersPath.AddChild(newRendererScene);
        myPlayerRenderers[id] = newRendererScene;

    }


}
