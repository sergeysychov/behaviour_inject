using System;

namespace BehaviourInject
{
    public interface DependencyFactory<T>
    {
        T Create();
    }
}


namespace BehaviourInject.Internal
{
	public class DependencyFactoryFacade<T> : IFactoryFacade
	{
		private DependencyFactory<T> _factory;
		public DependencyFactoryFacade(DependencyFactory<T> factory)
		{
			_factory = factory;
		}

		public object Create()
		{
			return _factory.Create();
		}
	}


	public interface IFactoryFacade
	{
		object Create();
	}
}
