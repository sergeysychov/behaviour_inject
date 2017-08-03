using System;
using System.Reflection;

namespace BehaviourInject.Internal
{
	public interface IEventHandler
	{
		bool IsSuitableForEvent(Type dispatchedType);
		void Invoke(object target, object evnt);
	}

	public class MethodEventHandler : IEventHandler
	{
		private MethodInfo _method;
		private Type _eventType;
		private object[] _invocationParameters = new object[1];

		public MethodEventHandler(MethodInfo method, Type eventType)
		{
			_method = method;
			_eventType = eventType;
		}


		public bool IsSuitableForEvent(Type dispatchedType)
		{
			return _eventType.IsAssignableFrom(dispatchedType);
		}


		public void Invoke(object target, object evnt)
		{
			_invocationParameters[0] = evnt;
			_method.Invoke(target, _invocationParameters);
		}
	}


	public class ReceiverEventHandler : IEventHandler
	{
		private PropertyInfo _receiverMember;
		private Type _eventType;

		public ReceiverEventHandler(PropertyInfo receiverMember, Type eventType)
		{
			_receiverMember = receiverMember;
			_eventType = eventType;
		}

		public bool IsSuitableForEvent(Type dispatchedType)
		{
			return _eventType.IsAssignableFrom(dispatchedType);
		}

		public void Invoke(object target, object evnt)
		{
			object receiver = _receiverMember.GetValue(target, null);
			if (receiver == null)
				return;
			((IReceiver)receiver).Receive(evnt);
		}
	}
}
