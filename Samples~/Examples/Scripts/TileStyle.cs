using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourInject.Example
{
	public class TileStyle : IDisposable
	{
		public Color StyleColor { get; private set; }
		public string Name { get; private set; }

		public TileStyle(Color color, string name)
		{
			StyleColor = color;
			Name = name;
		}

		public void Dispose()
		{
			Debug.Log("Disposing style " + Name);
		}
	}
}
