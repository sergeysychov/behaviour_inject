using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BehaviourInject.Test
{
	public class TestFactory : MonoBehaviour
	{
		private void Start()
		{
			var context = Context.Create()
				.RegisterFactory<Dependency, Factory>();

			Factory factory = (Factory)context.Resolve(typeof(Factory));
			Assert.NotNull(factory, "factory resolved self");

			Dependency dependency = (Dependency)context.Resolve(typeof(Dependency));
			Assert.NotNull(dependency, "factored resolved dependency 1");

			dependency = (Dependency)context.Resolve(typeof(Dependency));
			Assert.NotNull(dependency, "factored resolved dependency 2 ");
			Assert.Equals(dependency.Count, 2, "factory creation count");

			context.Destroy();
		}


		private class Factory : DependencyFactory<Dependency>
		{
			public int _count;
			public Dependency Create()
			{
				_count++;
				return new Dependency() { Count = _count };
			}
		}


		private class Dependency
		{
			public int Count;
		}
	}
}
