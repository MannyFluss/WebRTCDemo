using Godot;
using System;

public partial class Character : RigidBody2D
{
    float strength = 700;
    public void Flick(){

        var amt = new Vector2(GD.RandRange(-1,1),GD.RandRange(-1,0)) * strength;

        ApplyImpulse(amt);
    }
}
