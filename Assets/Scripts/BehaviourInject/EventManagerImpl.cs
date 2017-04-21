using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourInject.Internal
{
	public class EventManagerImpl : IEventManager
	{
		public event Action<object> EventInjectors;

		public void DispatchEvent(object evnt)
		{
			EventInjectors(evnt);
		}
	}
}
