using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourInject;

public class EventBenchReceiver : MonoBehaviour {

	public EventReceiver<MyEvent> receiver { get; private set; }
	
	void Awake()
	{
		receiver = new EventReceiver<MyEvent>();
		receiver.OnEvent += HandleEvent;
	}

	//[InjectEvent]
	public void HandleEvent(MyEvent evt)
	{
	}
}
