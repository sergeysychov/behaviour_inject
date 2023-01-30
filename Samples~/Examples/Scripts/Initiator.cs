using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourInject.Example
{
	//This script is used to create contexts
	//It has to be stated in Script Execution Order as very first
	public class Initiator : MonoBehaviour
	{
		private Context _context;

		private void Awake()
		{
			//here we create three different example contexts

			_context = Context.Create()
				//registering new TileStyle object as dependecy to be requested by other
				//classes and gameobjects
				.RegisterDependency(new TileStyle(Color.red, "RED"));

			//other context might be choosen by gameobject Injector
			Context.Create("green")

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

			Context.Create("blue")
				.SetParentContext(Context.DEFAULT)
				.RegisterDependency(new TileStyle(Color.blue, "BLUE"));
		}


		private void OnDestroy()
		{
			//You may destroy any context at any time. In this case all child contexts also will be destroyed
			//And all dependent gameObjects will be destroyed as well
			_context.Destroy();
		}

		
		//try it yourself calling this method via button
		public void Destroy()
		{
			Destroy(gameObject);
		}
	}
}
