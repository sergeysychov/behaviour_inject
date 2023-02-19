using System;
using System.Collections;
using System.Collections.Generic;
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
			_precomposed = new PrecomposeDependency("test1");

			Assert.False(Context.Exists("test"), "Context not exist check.");
			
			_contextTest = Context.Create("test")
				.RegisterDependency(_precomposed)
				.RegisterType<AutocomposeDependency>()
				.RegisterTypeAs<DependencyImpl, IDependency>();
			
			Assert.True(Context.Exists("test"), "Context exist check.");

			_contextBase = Context.Create("base")
				.SetParentContext("test")
				.RegisterDependency(new PrecomposeDependency("base"))
				.RegisterType<AutocomposeDependency>()
				.RegisterDependency(_recipient);
		}


		private IEnumerator Start()
		{
			yield return new WaitForSeconds(0.5f);
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
		

		[InjectEvent(Inherit = true)]
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