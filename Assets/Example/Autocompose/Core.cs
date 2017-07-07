using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Core
{
    private Connection _connection;

    public Core(Connection connection, MyData data)
    {
        _connection = connection;
    }


    public string GetData()
    {
        return _connection.Read();
    }
}


public class MyData
{

}
