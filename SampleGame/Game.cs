using Godot;
using System;

public partial class Game : Node2D
{
    

    public void Setup(){
        Client.instance.GameStarted += StartGame;
    }

    public void StartGame(){
        
    }

}
