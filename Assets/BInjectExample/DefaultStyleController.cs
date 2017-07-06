using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourInject.Example
{
	public class DefaultStyleController : MonoBehaviour
	{
		[Inject]
		private TileStyle _style;
		[Inject]
		private IEventDispatcher _eventDispatcher;

		public void OnClick()
		{
			var @event = new StyleChangedEvent {
				Style = _style
			};

			_eventDispatcher.DispatchEvent(@event);
		}
	}
}
