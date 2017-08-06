using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BehaviourInject.Test
{
	public class TestEventRunner : MonoBehaviour
	{
		public TestRecipient _recipient;

		[Inject]
		private IEventDispatcher _dispatcher;
		[Inject]
		private PrecomposeDependency _dependency;

		private void Start()
		{
			var evt = new TestEvent();
			_dispatcher.DispatchEvent(evt);

			Assert.NotNull(_dependency.RecievedEvt, "dep event not null");
			Assert.Equals(_dependency.RecievedEvt, evt, "dep event recieved");
			Assert.Equals(_recipient.evt1, evt, "receiver ok");
			Assert.Equals(_recipient.evt2, evt, "beh event recieved");
		}
	}
}
