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
			var simpleDep = new SimpleDependency();

			var context = Context.Create()
				.RegisterDependency(simpleDep)
				.RegisterType<AutocomposedDependency>()
				.RegisterType<DependencyInherited>()
				.RegisterDependencyAs<IDependencyImpl, IDependency>(new IDependencyImpl())
				.RegisterTypeAs<IDependencyAutoImpl, IDependencyAuto>();

			Assert.NotNull(context.TestResolve<SimpleDependency>(), "resolve simple");
			AutocomposedDependency auto = context.TestResolve<AutocomposedDependency>();
			Assert.NotNull(auto, "resolve auto");
			Assert.NotNull(auto.ConstructorInjected, "auto constructor injected");
			Assert.NotNull(auto.PropertyInjected, "auto property injected");
			Assert.NotNull(auto.FieldInjected, "auto field injected");
			Assert.NotNull(auto.Created, "local creation");
			Assert.NotNull(auto.Created._dependency, "local creation dep not null");
			Assert.Equals(simpleDep, auto.FieldInjected, "auto field injected equals");
			Assert.NotNull(auto.InitInjected, "auto Init injected");

			DependencyInherited inherit = context.TestResolve<DependencyInherited>();
			Assert.NotNull(inherit, "resolve child");
			Assert.NotNull(inherit.AdditionalChildDep, "child own dep");
			Assert.NotNull(inherit.PropertyInjected, "child inherited property dep");
			Assert.NotNull(inherit.FieldInjected, "child inherited field dep");
			Assert.NotNull(inherit.InitInjected, "child inherited method dep");
			Assert.Equals(inherit.InjCount, 1, "child injection called only once");

			Assert.NotNull(context.TestResolve<IDependency>(), "resolve interface");
			Assert.NotNull(context.TestResolve<IDependencyAuto>(), "resolve auto interface");

			context.Destroy();
		}
		

		private class SimpleDependency
		{ }

		private class AutocomposedDependency
		{
			public SimpleDependency ConstructorInjected;
			[Inject]
			public virtual SimpleDependency PropertyInjected { get; set; }
			[Inject]
			public SimpleDependency _fieldInjected;
			public SimpleDependency FieldInjected { get { return _fieldInjected; } }
			public SimpleDependency InitInjected;

			[Create]
			public CreatedDependency Created;

			public int InjCount = 0;

			public AutocomposedDependency(SimpleDependency foo)
			{
				ConstructorInjected = foo;
			}

			[Inject]
			public void Init(SimpleDependency foo)
			{
				InitInjected = foo;
			}
			
			// [Inject]
			// public void TestException(SimpleDependency foo)
			// {
			// 	throw new ArgumentException("Unhandled exception in Inject target method.");
			// }

			[Inject]
			public virtual void CheckTwice(SimpleDependency foo)
			{
				InjCount++;
			}
		}


		private class DependencyInherited : AutocomposedDependency
		{
			[Inject]
			public SimpleDependency AdditionalChildDep { get; set; }

			public DependencyInherited(SimpleDependency dep) : base (dep)
			{ }

			
			/*[Inject]
			public override void CheckTwice(SimpleDependency foo)
			{
				InjCount++;
			}*/
		}


		private class CreatedDependency
		{
			[Inject]
			public SimpleDependency _dependency;
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
