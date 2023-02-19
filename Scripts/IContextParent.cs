using System;

namespace BehaviourInject.Internal
{
	public interface IContextParent
	{
		bool TryResolve(Type resolvingType, out object dependency);
		event Action OnContextDestroyed;
		EventManager EventManager { get; }
	}

	internal class ParentContextStub : IContextParent
	{
		public static readonly ParentContextStub STUB = new ParentContextStub();

		public event Action OnContextDestroyed;

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
