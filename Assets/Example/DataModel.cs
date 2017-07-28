using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BehaviourInject;

public class DataModel
{
    public string Data { get; private set; }

    public DataModel(string data)
    {
        Data = data;
    }

	/*[InjectEvent]
	public void EventHandler(IMyEvent evt)
	{
		Debug.Log("Event in model " + evt.Num());
	}*/
}
