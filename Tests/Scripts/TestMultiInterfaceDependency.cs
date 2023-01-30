using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourInject;
using BehaviourInject.Test;
using UnityEngine;

namespace BehaviourInject.Test
{
	public class TestMultiInterfaceDependency : MonoBehaviour
	{
		public void Start()
		{
			Context context = Context.Create()
				.RegisterTypeAsMultiple<MultipleInterfaceDependency>(typeof(InterfaceA), typeof(InterfaceB))
				.RegisterType<Recipient>()
				.CreateAll();
			
			context.EventManager.DispatchEvent(new Event());

			InterfaceA dependency = context.Resolve<InterfaceA>();
			Assert.True(dependency.AlreadyNotified, "multi-interface dependency received event");
			
			context.Destroy();
		}


		private interface InterfaceA { bool AlreadyNotified { get; } }
		private interface InterfaceB {}

		private class MultipleInterfaceDependency : InterfaceA, InterfaceB
		{
			public bool AlreadyNotified { get; set; }
			
			[InjectEvent]
			public void Handle(TestMultiInterfaceDependency.Event e)
			{
				Assert.False(AlreadyNotified, "event for multi-interface dependency received only once");
				AlreadyNotified = true;
			}
		}

		private class Recipient
		{
			
			public Recipient(InterfaceA a, InterfaceB b)
			{
				Assert.Equals(a, b, "multiple type dependency");
			}
		}


		public class Event { }
	}
}