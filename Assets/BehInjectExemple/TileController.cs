using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BehaviourInject.Example
{
	public class TileController : MonoBehaviour
	{
		[SerializeField]
		private Image _image;
		[SerializeField]
		private Text _title;

		[Inject]
		private TileStyle _style;
		
		void Start()
		{
			SetupStyle(_style);
		}


		private void SetupStyle(TileStyle style)
		{
			_image.color = style.StyleColor;
			_title.text = style.Name;
		}


		public void OnClickHandler()
		{
			SetupStyle(_style);
		}

		[InjectEvent]
		public void OnStyleChangedHandler(StyleChangedEvent @event)
		{
			SetupStyle(@event.Style);
		}
	}
}
