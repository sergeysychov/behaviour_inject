﻿using UnityEngine;
using System.Collections;
using BehaviourInject;

public class TestBeahviour2 : MonoBehaviour {

    [Inject]
    public DataModel Model { get; set; }

    [Inject]
    public IReader Reader { get; set; }


	void Start () {
        Debug.Log(gameObject.name + " TB2");
        Debug.Log(Model.Data);
        Debug.Log(Reader.Read());
	}
	
	[InjectEvent]
	public void HandleEvent(MyEvent evnt)
	{
		Debug.Log("catch event " + evnt._index);
	}

	[InjectEvent]
	private void HandleEvent2(IMyEvent evnt)
	{
		Debug.Log("catch IMyEvent " + evnt.Num());
	}

	[InjectEvent]
	public void HandleEvent3(InternEvent evnt)
	{
		Debug.Log("catch event3 ");
	}

	public class InternEvent {
	}
}
