using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BehaviourInject.Example
{
	//this is our example of monobehaviour that uses dependency from context
	public class TileController : MonoBehaviour
	{
		[SerializeField]
		private Image _image;
		[SerializeField]
		private Text _title;

		//Injected dependency (TileStyle here) is resolved by Injector on the same gameObject
		//Injector uses specified Context to resolve dependency
		[Inject]
		public TileStyle Style { get; private set; }
		
		void Start()
		{
			SetupStyle(Style);
		}


		private void SetupStyle(TileStyle style)
		{
			_image.color = style.StyleColor;
			_title.text = style.Name;
		}


		public void OnClickHandler()
		{
			SetupStyle(Style);
		}

		//event handler. No need to subscribe or unsubscribe.
		//it is specefied just by type of event argument, witch is StyleChangedEvent here
		[InjectEvent]
		public void OnStyleChangedHandler(StyleChangedEvent @event)
		{
			SetupStyle(@event.Style);
		}
	}
}
