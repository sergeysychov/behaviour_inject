using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourInject.Test
{
	public class TestRecipient : MonoBehaviour
	{
		public string _contextKeyword;

		public IEvent evt1;
		public TestEvent evt2;

		[Inject]
		public IDependency Idependency { get; private set; }
		[Inject]
		public PrecomposeDependency _fieldInjected;
		private AutocomposeDependency _methodInjected;
		[Inject]
		public AutocomposeDependency PropertyInjected { get; private set; }

		[InjectEvent(Inherit = true)]
		public Action<IEvent> OnIEvent;

		[InjectEvent]
		public Action<TestEvent> EmptyEvent;

		[Inject]
		public void Init(AutocomposeDependency dep)
		{
			_methodInjected = dep;
		}

		private void Awake()
		{
			OnIEvent += Handle;
		}

		void Start()
		{
			Assert.NotNull(_fieldInjected, " field inject");
			string keyword = _fieldInjected.Keyword;
			Assert.NotNull(Idependency, keyword + " parent and interface");
			Assert.NotNull(_methodInjected, keyword + " method inject");
			Assert.NotNull(PropertyInjected, keyword + " property inject");
			Assert.Equals(_contextKeyword, _fieldInjected.Keyword, keyword + " compare contexts");
			_methodInjected.Run();
			Idependency.Run();
		}


		public void Handle(IEvent evt)
		{
			evt1 = evt;
		}

		[InjectEvent]
		public void Handle(TestEvent evt)
		{
			evt2 = evt;
		}
	}
}