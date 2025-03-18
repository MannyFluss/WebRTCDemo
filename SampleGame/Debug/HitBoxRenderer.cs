using Godot;

public partial class HitBoxRenderer : Node2D {

    [Export]
    SampleGameBackend backend;

    [Export]
    bool enabled = false;
    public override void _Ready()
    {
        base._Ready();

    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        QueueRedraw();

        
    }
    public override void _Draw()
    {

        base._Draw();
        if (!enabled){
            return;
        }
        foreach (Vector2 projectile in backend.GetGameState().DefaultProjectilePositions){
            DrawCircle(projectile,5,new Color(1,0,0,1));
        }
    }

}