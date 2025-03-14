using Godot;
using System;

public partial class CharacterRenderer : Node2D
{

    public override void _Ready()
    {
        base._Ready();
        Label mylabel = GetNode<Label>("Label");
        mylabel.Text = Name;
    }



}
