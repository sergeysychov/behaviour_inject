using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourInject.Internal
{
	public interface IContextParent
	{
		bool TryResolve(Type resolvingType, out object dependency);
		EventManager EventManager { get; }
	}

	public class ParentContextStub : IContextParent
	{
		public static readonly ParentContextStub STUB = new ParentContextStub();
		
		public EventManager EventManager { get; private set; }

		public ParentContextStub()
		{
			EventManager = new EventManager();
		}

		public bool TryResolve(Type resolvingType, out object dependency)
		{
			dependency = null;
			return false;
		}
	}
}
