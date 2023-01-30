using System;
using System.Reflection;

namespace BehaviourInject.Internal
{
	public interface IEventHandler
	{
		bool IsSuitableForEvent(Type dispatchedType);
		void Invoke(object target, object evnt);
	}


	public abstract class AbstractEventHandler
	{
		private Type _eventType;
		private bool _inherit;

		public AbstractEventHandler(MemberInfo member, Type eventType)
		{
			_eventType = eventType;

			InjectEventAttribute attribute;
			if (AttributeUtils.TryGetAttribute(member, out attribute))
				_inherit = attribute.Inherit;
		}


		public bool IsSuitableForEvent(Type dispatchedType)
		{
			if (_inherit)
				return _eventType.IsAssignableFrom(dispatchedType);
			else
				return _eventType == dispatchedType;
		}
	}

	public class MethodEventHandler : AbstractEventHandler, IEventHandler
	{
		private MethodInfo _method;
		private object[] _invocationParameters = new object[1];

		public MethodEventHandler(MethodInfo method, Type eventType) : base(method, eventType)
		{
			_method = method;
		}

		public void Invoke(object target, object evnt)
		{
			_invocationParameters[0] = evnt;

			try
			{
				_method.Invoke(target, _invocationParameters);
			}
			finally
			{
				_invocationParameters[0] = null;
			}
		}
	}


	public class DelegateEventHandler : AbstractEventHandler, IEventHandler
	{
		private FieldInfo _delegateField;
		private object[] _invocationParameters = new object[1];

		public DelegateEventHandler(FieldInfo delegateField, Type eventType)
			: base(delegateField, eventType)
		{
			_delegateField = delegateField;
		}

		public void Invoke(object target, object evnt)
		{
			MulticastDelegate receiver = (MulticastDelegate)_delegateField.GetValue(target);
			if (receiver == null)
				return;

			_invocationParameters[0] = evnt;

			try
			{
				Delegate[] delegates = receiver.GetInvocationList();
				for (int i = 0; i < delegates.Length; i++)
				{
					Delegate d = delegates[i];
					d.Method.Invoke(d.Target, _invocationParameters);
				}
			}
			finally
			{
				_invocationParameters[0] = null;
			}
		}
	}
}
