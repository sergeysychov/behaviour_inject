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


public class MyEvent
{
	public int _index;
}
