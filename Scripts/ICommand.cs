using System;
using System.Collections.Generic;

namespace BehaviourInject
{
	public interface ICommand
	{
		void Execute();
	}
}

namespace BehaviourInject.Internal
{
	public class CommandEntry
	{
		public Type EventType { get; private set; }
		public List<Type> CommandTypes { get; private set; }

		public CommandEntry(Type evt)
		{
			EventType = evt;
			CommandTypes = new List<Type>();
		}
	}
}