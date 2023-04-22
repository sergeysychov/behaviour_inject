using System;
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
			return (T) _containerContext.AutocomposeDependency(typeof(T));
		}
		
		public T New<T>(params object[] additions)
		{
			return (T) _containerContext.AutocomposeDependency(typeof(T), additions);
		}
		
		public object New(Type t)
		{
			return _containerContext.AutocomposeDependency(t);
		}
	}


	public interface IInstantiator
	{
		T New<T>();
		T New<T>(params object[] additions);
		object New(Type t);
	}
}