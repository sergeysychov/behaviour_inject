using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourInject.Test
{
	public class EventDuplicationSibling : MonoBehaviour
	{
		public TestEvent Evt { get; private set; }


		[InjectEvent]
		public void HandleEvent(TestEvent evt)
		{
			Evt = evt;
		}
	}
}
