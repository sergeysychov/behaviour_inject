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
			
			context.Destroy();
		}


		private interface InterfaceA {}
		private interface InterfaceB {}
		private class MultipleInterfaceDependency : InterfaceA, InterfaceB { }

		private class Recipient
		{
			public Recipient(InterfaceA a, InterfaceB b)
			{
				Assert.Equals(a, b, "multiple type dependency");
			}
		}
		
	}
}