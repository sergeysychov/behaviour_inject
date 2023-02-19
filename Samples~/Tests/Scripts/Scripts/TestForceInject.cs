using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourInject;
using BehaviourInject.Test;
using UnityEngine;

namespace BehaviourInject.Test
{
	public class TestForceInject : MonoBehaviour
	{
		void Start()
		{
			Context context = Context.Create()
				.RegisterType<Dependency>()
				.CreateAll();

			GameObject gameObject = new GameObject("test_force_inject_holder");
			gameObject.AddComponent<Injector>();

			ForceInjectRecipient recipient = gameObject.AddComponent<ForceInjectRecipient>();
			recipient.ForceInject();
			Assert.NotNull(recipient.D, "force inject dependency at delayed component");
			context.EventManager.TransmitEvent(new Event());
			Assert.NotNull(recipient.E, "inject event at delayed component");
			context.Destroy();
		}


		public class ForceInjectRecipient : MonoBehaviour
		{
			public Dependency D { get; private set; }
			public Event E { get; private set; }

			[Inject]
			public void Init(Dependency d)
			{
				D = d;
			}


			[InjectEvent]
			public void Handle(Event e)
			{
				E = e;
			}
		}


		public class Dependency
		{
		}

		public class Event
		{
		}
	}
}