using Godot;
using System;

public partial class SampleGameRenderer : Node2D
{
    [Export]
    SampleGameBackend MyBackend;
    [Export]
    Sprite2D sprite1;
    [Export]
    Sprite2D sprite2;
    
    public override void _Ready()
    {
        base._Ready();
        //Client.instance.GameStarted += OnGameStarted;
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
            sprite1.Position = state.position1;
            sprite2.Position = state.position2;
        }
    }
}
