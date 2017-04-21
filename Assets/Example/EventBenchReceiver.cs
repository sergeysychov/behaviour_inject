using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourInject;

public class EventBenchReceiver : MonoBehaviour {

	[InjectEvent]
	public void HandleEven(MyEvent evt)
	{
	}
}
