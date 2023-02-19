using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourInject;

namespace BehaviourInject.Test
{
	public class TestRecipientSuccessor : TestRecipientAncestor
	{
		[InjectEvent]
		public void SuccessorEvent(TestEvent evt)
		{

		}
	}
}