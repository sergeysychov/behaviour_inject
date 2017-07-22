using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourInject.Example
{
	//another examble of dependency recipient
	public class DefaultStyleController : MonoBehaviour
	{
		//Injected dependency (TileStyle here) is resolved by Injector on the same gameObject
		//Injector uses specified Context to resolve dependency
		[Inject]
		private TileStyle _style;

		//IEventDispatcher might be resolved from any context and represents 
		//tool for propagating events
		[Inject]
		private IEventDispatcher _eventDispatcher;

		public void OnClick()
		{
			var @event = new StyleChangedEvent {
				Style = _style
			};

			//here event is being fired to context and than to all of it's dependencies 
			//and gameobjects that contains Injector
			_eventDispatcher.DispatchEvent(@event);
		}
	}
}
