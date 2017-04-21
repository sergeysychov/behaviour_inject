using System;
using System.Reflection;

namespace BehaviourInject.Internal
{
	public class BlindEventHandler
	{
		public MethodInfo Method { get; private set; }
		public Type EventType { get; private set; }
		private object[] _invocationParameters = new object[1];

		public BlindEventHandler(MethodInfo method, Type eventType)
		{
			Method = method;
			EventType = eventType;
		}


		public bool IsSuitableForEvent(Type dispatchedType)
		{
			return EventType.IsAssignableFrom(dispatchedType);
		}


		public void Invoke(object target, object evnt)
		{
			_invocationParameters[0] = evnt;
			Method.Invoke(target, _invocationParameters);
		}
	}
}
