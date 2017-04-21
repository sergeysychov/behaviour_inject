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


public class MyEvent
{
	public int _index;
}
