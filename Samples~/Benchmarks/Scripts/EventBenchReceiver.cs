using System;
using UnityEngine;
using BehaviourInject;

public class EventBenchReceiver : MonoBehaviour {

	[InjectEvent]
	public Action<MyEvent> receiver;
	
	void Awake()
	{
		receiver += HandleEvent;
	}

	//[InjectEvent]
	public void HandleEvent(MyEvent evt)
	{
	}
}
