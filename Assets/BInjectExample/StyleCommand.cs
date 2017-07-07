using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourInject.Example
{
	public class StyleCommand : ICommand
	{
		private TileStyle _originalStyle;
		private TileStyle _newStyle;

		public StyleCommand(TileStyle original)
		{
			_originalStyle = original;
		}


		[InjectEvent]
		public void SetEvent(StyleChangedEvent evt)
		{
			_newStyle = evt.Style;
		}


		public void Execute()
		{
			UnityEngine.Debug.LogFormat("Set {0} style. Original was {1}", _newStyle.Name, _originalStyle.Name);
		}
	}
}
