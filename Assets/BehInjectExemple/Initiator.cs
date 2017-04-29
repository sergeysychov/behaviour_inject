using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourInject.Example
{
	public class Initiator : MonoBehaviour
	{
		private void Awake()
		{
			new Context()
				.RegisterDependency(new TileStyle(Color.red, "RED"));

			new Context("green")
				.SetParentContext(Context.DEFAULT)
				.RegisterDependency(new TileStyle(Color.green, "GREEN"));

			new Context("blue")
				.SetParentContext(Context.DEFAULT)
				.RegisterDependency(new TileStyle(Color.blue, "BLUE"));
		}
	}
}
