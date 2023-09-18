using System;

namespace BehaviourInject
{
	public interface IEventRegistry
	{
        void Subscribe(Type eventType, Delegate handler, bool handleAllDerived);
        void Subscribe(Type eventType, Delegate handler, bool handleAllDerived, Type[] filter);

        void Unsubscribe(Type eventType, Delegate handler);
        void Unsubscribe(Type eventType, Delegate handler, Type[] filter);

        void UnsubscribeTarget(object target);
    }
}
