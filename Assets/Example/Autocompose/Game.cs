using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Game
{
    private int _id;
    private string _data;

    public Game(int id, string data)
    {
        _id = id;
        _data = data;
    }


    public override string ToString()
    {
        return _data + " with id " + _id;
    }
}
