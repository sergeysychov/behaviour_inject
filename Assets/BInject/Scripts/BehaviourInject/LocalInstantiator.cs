using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourInject
{
	public class LocalInstantiator : IInstantiator
	{
		private Context _containerContext;
		
		public LocalInstantiator(Context container)
		{
			_containerContext = container;
		}
		
		public T New<T>()
		{
			return (T) _containerContext.AutocomposeDependency(typeof(T), 0);
		}
	}


	public interface IInstantiator
	{
		T New<T>();
	}
}