using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourInject.Test
{
	public class EventDuplicationRecipient : MonoBehaviour
	{
		public int EventCounter { get; set; }

		[InjectEvent]
		public void HandleEvent(TestEvent evt)
		{
			EventCounter++;
		}
	}
}