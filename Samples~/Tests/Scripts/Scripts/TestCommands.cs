using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BehaviourInject.Test
{
	public class TestCommands : MonoBehaviour
	{
		private void Start()
		{
			var dependency = new CommandDependency();
			var evt = new Event();

			var context = Context.Create()
				.RegisterDependency(dependency)
				.RegisterCommand<Event, Command>();

			IEventDispatcher dispatcher = (IEventDispatcher)context.Resolve(typeof(IEventDispatcher));
			dispatcher.DispatchEvent(evt);

			Assert.True(dependency.Executed, "command executed");
			Assert.NotNull(dependency.WithEvent, "command event setter");

			context.Destroy();
		}


		private class Command : ICommand
		{
			private Event _evt;
			private CommandDependency _dep;

			public Command(CommandDependency dep)
			{
				_dep = dep;
			}

			[InjectEvent]
			public void Receive(Event evt)
			{
				_evt = evt;
			}

			public void Execute()
			{
				_dep.Executed = true;
				_dep.WithEvent = _evt;
			}
		}


		private class Event
		{
		}


		private class CommandDependency
		{
			public bool Executed;
			public Event WithEvent;
		}
	}
}
