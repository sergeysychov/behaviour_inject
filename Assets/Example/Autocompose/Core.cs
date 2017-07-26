using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Core
{
    private Connection _connection;
	[BehaviourInject.Inject]
	private MyData _myData;

    public Core(Connection connection)
    {
        _connection = connection;
    }


    public string GetData()
    {
		UnityEngine.Debug.Log(_myData);
        return _connection.Read();
    }
}


public class MyData
{

}
