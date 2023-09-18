using System;

namespace BehaviourInject.Internal
{
	public interface IContextParent
	{
		bool TryResolve(Type resolvingType, out object dependency);
		event Action OnContextDestroyed;
	}

	internal class ParentContextStub : IContextParent
	{
		public static readonly ParentContextStub STUB = new ParentContextStub();

		public event Action OnContextDestroyed;

		public bool TryResolve(Type resolvingType, out object dependency)
		{
			dependency = null;
			return false;
		}
	}
}
