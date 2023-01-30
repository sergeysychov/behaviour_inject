using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourInject;
using BehaviourInject.Test;

namespace TestIgonreNamespace
{
	public class Ignored : MonoBehaviour
	{
		[Inject]
		public IDependency Dependency { get; private set; }

		void Start()
		{
			Assert.IsNull(Dependency, "ignore namespace");
		}
	}
}