using Godot;
using System;

public partial class DebugTextPrinter : RichTextLabel
{
    [Export]
    Client MyClient = null;

    [Export]
    Server MyServer = null;


    public override void _Ready()
    {
        base._Ready();

        // MyClient = GetParentOrNull<Client>();
        // MyServer = GetParentOrNull<Server>();
        if (MyClient!=null){
            MyClient.debugTextEmit += OnDebugTextEmitted;
        }
        if (MyServer!=null){
            MyServer.debugTextEmit += OnDebugTextEmitted;
        }
        

    }

    private void OnDebugTextEmitted(string obj)
    {
        Text = Text.Insert(0,obj + "\n");
    }
}

