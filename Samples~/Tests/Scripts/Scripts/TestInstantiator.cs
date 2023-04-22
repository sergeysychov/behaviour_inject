using System.Collections;
using System.Collections.Generic;
using BehaviourInject;
using BehaviourInject.Test;
using UnityEngine;

namespace BehaviourInject.Test
{
	public class TestInstantiator : MonoBehaviour
	{
		private IInstantiator _instantiator;

		[Inject]
		public void Init(IInstantiator instantiator)
		{
			_instantiator = instantiator;
		}
	
	
		private void Start()
		{
			MyCustomComposition a = _instantiator.New<MyCustomComposition>();
			MyCustomComposition b = _instantiator.New<MyCustomComposition>();

			AssertComposition(a);
			AssertComposition(b);
			Assert.NotEquals(a, b, "instantiator  compositions are not equal");

			Addition1 one = new Addition1();
			Addition2 two = new Addition2();
			CustomCompositionWithAdditions c = 
				_instantiator.New<CustomCompositionWithAdditions>(one, two);
			Assert.True(c.Addition1 == one, "instantiator addition");
			Assert.True(c.Addition2 == two, "instantiator addition");
		}

	
		private void AssertComposition(MyCustomComposition c)
		{
			Assert.NotNull(c, "instantiator composition not null");
			Assert.NotNull(c.Dependency1, "instantiator  composition dependency not null");
			Assert.NotNull(c.Dependency2, "instantiator  composition dependency not null");
		}
	
	
		public class MyCustomComposition
		{
			public AutocomposeDependency Dependency1;
			public PrecomposeDependency Dependency2;

			public MyCustomComposition(AutocomposeDependency a, PrecomposeDependency b)
			{
				Dependency1 = a;
				Dependency2 = b;
			}
		}
	
		public class CustomCompositionWithAdditions
		{
			public AutocomposeDependency Dependency1;
			public PrecomposeDependency Dependency2;
			public Addition1 Addition1;
			public Addition2 Addition2;

			public CustomCompositionWithAdditions(AutocomposeDependency a, PrecomposeDependency b, 
				Addition1 addition1, Addition2 addition2)
			{
				Dependency1 = a;
				Dependency2 = b;
				Addition1 = addition1;
				Addition2 = addition2;
			}
		}
		
		
		public class Addition1
		{
			
		}
		
		public class Addition2
		{
			
		}
	}
}
