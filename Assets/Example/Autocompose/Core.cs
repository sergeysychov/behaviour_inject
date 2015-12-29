using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Core
{
    private Connection _connection;

    public Core(Connection connection)
    {
        _connection = connection;
    }


    public string GetData()
    {
        return _connection.Read();
    }
}
