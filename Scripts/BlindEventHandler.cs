using System;
using System.Reflection;

namespace BehaviourInject.Internal
{
	public interface IEventBinder
	{
		void Bind(object target, Context context);
	}

	public abstract class AbstractEventBinder
	{
		protected Type _eventType;
		protected Type[] _handleDerived;
		protected bool _handleAllDerived;

        public AbstractEventBinder(MemberInfo member, Type eventType)
		{
			_eventType = eventType;
			 
			if (AttributeUtils.TryGetAttribute(member, out InjectEventAttribute attribute))
			{
				_handleDerived = attribute.DerivedFilter;
				_handleAllDerived = attribute.HandleAllDerived;
			}
		}
	}

	public class MethodEventBinder : AbstractEventBinder, IEventBinder
	{
		private MethodInfo _method;

		public MethodEventBinder(MethodInfo method, Type eventType) : base(method, eventType)
		{
			_method = method;
		}

        public void Bind(object target, Context context)
        {
			Type delegateType = EventManager.GetDelegateType(_eventType);
			Delegate handler = _method.CreateDelegate(delegateType, target);

			context.Subscribe(_eventType, handler, _handleAllDerived, _handleDerived);
        }
    }

	public class DelegateEventBinder : AbstractEventBinder, IEventBinder
	{
		private FieldInfo _delegateField;

		public DelegateEventBinder(FieldInfo delegateField, Type eventType)
			: base(delegateField, eventType)
		{
			_delegateField = delegateField;
		}

        public void Bind(object target, Context context)
        {
            Delegate handlers = (Delegate)_delegateField.GetValue(target);
			
			if (handlers is null) return;

			foreach (Delegate handler in handlers.GetInvocationList())
			{
				context.Subscribe(_eventType, handler, _handleAllDerived, _handleDerived);
			}
        }
	}
}
