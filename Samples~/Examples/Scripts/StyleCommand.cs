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

		//command may resolve any dependency represented in context
		public StyleCommand(TileStyle original)
		{
			_originalStyle = original;
		}

		//also via [InjectEvent] command may obtain event (only) that triggered that command
		[InjectEvent]
		public void SetEvent(StyleChangedEvent evt)
		{
			_newStyle = evt.Style;
		}

		//whatever things to do by this command
		public void Execute()
		{
			UnityEngine.Debug.LogFormat("Set {0} style. Original was {1}", _newStyle.Name, _originalStyle.Name);
		}
	}
}
