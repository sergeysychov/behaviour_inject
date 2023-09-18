using BehaviourInject.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BehaviourInject.Test
{
	public class TestInitiator : MonoBehaviour
	{
		public GameObject _testRunner;

		[SerializeField]
		private EventDuplicationRecipient _recipient;

		private Context _contextTest;
		private Context _contextBase;
		private PrecomposeDependency _precomposed;

		private void Awake()
		{
			EventManager.DeclareEvent<IEvent>();
            EventManager.DeclareEvent<TestEvent>();

            _precomposed = new PrecomposeDependency("test1");

			Assert.False(Context.Exists("test"), "Context not exist check.");

			_contextTest = Context.Create("test")
				.RegisterSingleton(_precomposed)
				.RegisterSingleton<AutocomposeDependency>()
				.RegisterSingletonAs<DependencyImpl, IDependency>()
                .CompleteRegistration();

			Assert.True(Context.Exists("test"), "Context exist check.");

			_contextBase = Context.CreateChild("base", "test")
				.RegisterSingleton(new PrecomposeDependency("base"))
				.RegisterSingleton<AutocomposeDependency>()
				.RegisterSingleton(_recipient)
                .CompleteRegistration();

		}


        private IEnumerator Start()
		{
			yield return new WaitForSeconds(2.5f);
#if BINJECT_DIAGNOSTICS
			Debug.Log(Diagnostics.BinjectDiagnostics.GetDiagnosticStirng());
#endif
			Destroy(gameObject);
		}


		private void OnDestroy()
		{
			_contextTest.Destroy();
			_contextBase.Destroy();
			Assert.True(_precomposed.Disposed, "dispose");
			Assert.False(Context.Exists("test"), "Context not exist after destroy check.");

			_testRunner.SetActive(true);
		}
	}

	public interface IDependency {
		void Run();
	}


	public class DependencyImpl : IDependency
	{
		private AutocomposeDependency _dep;

		public DependencyImpl(AutocomposeDependency dep)
		{
			_dep = dep;
		}

		public void Run()
		{
			Assert.NotNull(_dep, "interfaced autocompose");
		}
	}

	public class PrecomposeDependency : IDisposable
	{
		public IEvent RecievedEvt { get; private set; }
		public string Keyword { get; private set; }
		public bool Disposed { get; private set; }

		public PrecomposeDependency(string keyword)
		{
			Keyword = keyword;
		}
		

		[InjectEvent(HandleAllDerived = true)]
		public void Handle(IEvent evt)
		{
			RecievedEvt = evt;
		}

		public void Dispose()
		{
			Disposed = true;
		}
	}

	public class AutocomposeDependency
	{
		private PrecomposeDependency _dep;

		public AutocomposeDependency(PrecomposeDependency dep)
		{
			_dep = dep;
		}

		public void Run()
		{
			Assert.NotNull(_dep, "autocompose");
		}
	}
	

	public class IEvent
	{ }


	public class TestEvent : IEvent
	{ }
}