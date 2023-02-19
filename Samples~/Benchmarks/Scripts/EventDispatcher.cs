using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourInject;
using UnityEngine;

public class EventDispatcher : MonoBehaviour {

	[Inject]
	public IEventDispatcher EventManager { get; private set; }
	
	IEnumerator Start () {

		yield return new WaitForSeconds(2f);

		Debug.Log("Dispatch event");
		EventManager.DispatchEvent(new MyEvent { _index = 2 });
	}
}


public class MyEvent : IMyEvent
{
	public int _index;

	public int Num()
	{
		return _index;
	}
}

public interface IMyEvent
{
	int Num();
}
