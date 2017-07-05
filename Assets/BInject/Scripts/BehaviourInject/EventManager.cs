using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourInject.Internal
{
	public class EventManager : IEventDispatcher
	{
		public event Action<object> EventInjectors;

		private EventManager _parent;

		public void DispatchEvent(object evnt)
		{
			if (evnt == null)
				throw new BehaviourInjectException("Dispatched event can not be null");
			Type eventType = evnt.GetType();

			if(eventType.IsValueType)
				throw new BehaviourInjectException("Dispatched event can not be value type");
			
			EventInjectors(evnt);
		}


		public void SetParent(EventManager manager)
		{
			_parent = manager;
			_parent.EventInjectors += DispatchEvent;
		}


		public void ClearParent()
		{
			if (_parent != null)
				_parent.EventInjectors -= DispatchEvent;
			_parent = null;
		}
	}
}
