using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;


public struct User{
    public int id {set;get;}
    public string name {set;get;}
    public int index {set;get;}
    public User(int _id, string _name, int _index){
        id=_id;
        name=_name;
        index=_index;
    }
}

public class Lobby{
    public int HostId {set;get;}
    public Dictionary<int,User> Users {set;get;}

    public Lobby(int _HostId){
        HostId=_HostId;
        Users = new Dictionary<int, User>();
    }

    public User AddPlayer(int id, string name){
        Users[id] = new User(id,name,Users.Count+1);
        return Users[id];
    }

    public string StringUsers(){
        string res = "";
        foreach (int i in Users.Keys){
            res += i.ToString()+"\n";
        } 
        return res;
    }

}

public enum Message {
    ID,
    JOIN,
    USER_CONNECTED,
    USER_DISCONNECTED,
    CANDIDATE,
    OFFER,
    ANSWER,
    CHECKIN,
    LOBBY_CONNECTED,
    JOIN_LOBBY,
    DELETE_LOBBY,
    NONE,
}
public struct NetworkPacket
{
    public Message Message { get; set; }
    public string Data { get; set; }
    public int Id { get; set; }
    public int HostId { get; set; }
    //will be a serialized value of a dictionary
    public string UserData { get; set; }
    public string LobbyValue { get; set; }
    public string Name {get;set;}
    public int Peer { get; set; }
    public int OrgPeer { get; set; }
    public string Mid { get; set; }
    public long Index { get; set; }
    public string Sdp { get; set; }

    public NetworkPacket(
        Message message = Message.NONE,
        string data = null,
        string name = null,
        int id = 0,
        int hostId = 0,
        string userData = null,
        string lobbyValue = null,
        int peer = 0,
        int orgPeer = 0,
        string mid = null,
        long index = 0,
        string sdp = null)
    {
        Message = message;
        Data = data;
        Id = id;
        HostId = hostId;
        UserData = userData;
        LobbyValue = lobbyValue;
        Peer = peer;
        OrgPeer = orgPeer;
        Mid = mid;
        Index = index;
        Sdp = sdp;
        Name = name;
    }
}

public enum InputType{
    NONE,
    FLICK,
    GAME_PRESSED,
    GAME_HELD,
    GAME_RELEASED,

}


//for now the packets will just hold any combination of data possible in nullable form
public struct NetworkInputPacket
{
    public InputType type {get; set;}
    public Vector2? vec1 {get;set;}
    public NetworkInputPacket(
        InputType _type = InputType.NONE
    ){
        type = _type;
    }
    public NetworkInputPacket(InputType _type, Vector2? _vec1){
        type = _type;
        vec1 = _vec1;
    }
}

