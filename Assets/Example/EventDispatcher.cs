using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourInject;
using UnityEngine;

public class EventDispatcher : MonoBehaviour {

	[Inject]
	private IEventDispatcher _eventManager;
	
	IEnumerator Start () {

		yield return new WaitForSeconds(2f);

		Debug.Log("Dispatch event");
		_eventManager.DispatchEvent(new MyEvent { _index = 2 });
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
