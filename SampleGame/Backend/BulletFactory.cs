using System;
using System.Collections.Generic;
using System.Data;
using Godot;


interface IBulletInterface {
    public float length {set;get;}
    public float speed {set;get;}
    public Projectile projectileReference {set;get;}
    public Vector2 UpdatePosition(Vector2 previousPosition);
}

struct SimpleBullet : IBulletInterface
{
    public float length { get;set;}
    public float speed {get;set;}
    public Projectile projectileReference {set;get;}
    public Vector2 direction {set;get;}
    public Vector2 UpdatePosition(Vector2 previousPosition)
    {
        previousPosition += direction * speed;
        return previousPosition;
    }
    public SimpleBullet(Projectile _projectile, Vector2 _direction,float _length = 5f, float _speed = 100f){
        projectileReference = _projectile;
        direction=_direction.Normalized();
        length=_length;
        speed=_speed;
    }
}

public partial class BulletFactory : Node2D {

    [Export]
    SampleGameBackend MyGameBackend;

    List<IBulletInterface> MyInterfaces;
    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        foreach(IBulletInterface bullet in MyInterfaces){
            bullet.projectileReference.position = bullet.UpdatePosition(bullet.projectileReference.position);
        }
    }

    public void FireSimpleBullet(Vector2 position, Vector2 direction){
        Projectile newProjectile = new Projectile(position);
        int registeredProjectile = MyGameBackend.RegisterNewProjectile(newProjectile);
        SimpleBullet movementBehavior = new SimpleBullet(newProjectile,direction);
        //callback to delete this
        GetTree().CreateTimer(movementBehavior.length).Timeout += () =>{
            MyGameBackend.DeleteRegisteredProjectile(registeredProjectile);
        };
    }



}