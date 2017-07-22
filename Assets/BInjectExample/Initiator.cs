using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourInject.Example
{
	//This script is used to create contexts
	//It has to be stated in Script Execution Order as very first
	public class Initiator : MonoBehaviour
	{
		private void Awake()
		{
			//here we create three different example contexts

			new Context()
				//registering new TileStyle object as dependecy to be requested by other
				//classes and gameobjects
				.RegisterDependency(new TileStyle(Color.red, "RED"));

			//other context might be choosen by gameobject Injector
			new Context("green")

				//context inheritence allow to request dependencies from parent
				//if they are not found in current context.
				//Also events from parent are propagated to child context
				.SetParentContext(Context.DEFAULT)
				//register dependency. For it has same type as in base context, it will
				//'override' it
				.RegisterDependency(new TileStyle(Color.green, "GREEN"))

				//command represents reaction to event
				//Object of command type is being created and executed on each event occurance
				.RegisterCommand<StyleChangedEvent, StyleCommand>();

			new Context("blue")
				.SetParentContext(Context.DEFAULT)
				.RegisterDependency(new TileStyle(Color.blue, "BLUE"));
		}
	}
}
