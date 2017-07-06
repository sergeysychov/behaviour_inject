using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourInject.Example
{
	public class TileStyle
	{
		public Color StyleColor { get; private set; }
		public string Name { get; private set; }

		public TileStyle(Color color, string name)
		{
			StyleColor = color;
			Name = name;
		}
	}
}
