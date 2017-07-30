using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BehaviourInject.Test
{
	public class TestContextResolving : MonoBehaviour
	{
		private void Start()
		{
			var context = CreateFilledContext();

			Assert.NotNull(context.TestResolve<SimpleDependency>(), "resolve simple");
			AutocomposedDependency auto = context.TestResolve<AutocomposedDependency>();
			Assert.NotNull(auto, "resolve auto");
			Assert.NotNull(auto.ConstructorInjected, "auto constructor injected");
			Assert.NotNull(auto.PropertyInjected, "auto property injected");
			Assert.NotNull(auto.FieldInjected, "auto field injected");
			Assert.NotNull(auto.InitInjected, "auto Init injected");

			Assert.NotNull(context.TestResolve<IDependency>(), "resolve interface");
			Assert.NotNull(context.TestResolve<IDependencyAuto>(), "resolve auto interface");

			context.Destroy();
		}


		private Context CreateFilledContext()
		{
			return Context.Create()
				.RegisterDependency(new SimpleDependency())
				.RegisterType<AutocomposedDependency>()
				.RegisterDependencyAs<IDependencyImpl, IDependency>(new IDependencyImpl())
				.RegisterTypeAs<IDependencyAutoImpl, IDependencyAuto>();
		}

		private class SimpleDependency
		{ }

		private class AutocomposedDependency
		{
			public SimpleDependency ConstructorInjected;
			[Inject]
			public SimpleDependency PropertyInjected { get; private set; }
			[Inject]
			private SimpleDependency _fieldInjected;
			public SimpleDependency FieldInjected { get { return _fieldInjected; } }
			public SimpleDependency InitInjected;

			public AutocomposedDependency(SimpleDependency foo)
			{
				ConstructorInjected = foo;
			}

			[Inject]
			public void Init(SimpleDependency foo)
			{
				InitInjected = foo;
			}
		}

		private interface IDependency
		{
		}

		private class IDependencyImpl : IDependency
		{

		}

		private interface IDependencyAuto
		{
		}

		private class IDependencyAutoImpl : IDependencyAuto
		{

		}
	}
}
