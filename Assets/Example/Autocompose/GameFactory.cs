using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BehaviourInject;
using UnityEngine;

public class GameFactory : DependencyFactory
{
    private Connection _connection;


    public GameFactory(Connection connection)
    {
        _connection = connection;
    }


    public object Create()
    {
        Debug.Log("create game from factory. Connected = " + _connection.Connected);
        
        if (_connection.Connected)
            return new Game(1, "connected game");
        else
            return null;
    }
}
