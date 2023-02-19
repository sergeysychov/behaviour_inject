using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourInject.Test
{
	public class TestRecipientAncestor : MonoBehaviour
	{
		public TestEvent _methodEvt;
		public TestEvent _fieldEvt;

		[InjectEvent]
		public Action<TestEvent> OnTestEvent;

		void Awake()
		{
			OnTestEvent += FieldEventHandler;
		}


		private void FieldEventHandler(TestEvent evt)
		{
			_fieldEvt = evt;
		}


		[InjectEvent]
		public void AncestorEvent(TestEvent evt)
		{
			_methodEvt = evt;
		}
	}
}
