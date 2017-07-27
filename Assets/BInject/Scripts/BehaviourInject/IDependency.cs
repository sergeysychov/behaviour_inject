using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourInject.Internal
{
	public interface IDependency
	{
		object Resolve(Context context, int depth);
		void Dispose();
		bool IsSingle { get; }
	}


	public class SingleDependency : IDependency
	{
		private object _dependency;
		public SingleDependency(object dependency)
		{
			if (dependency == null)
				throw new ContextCreationException("Dependency is null");
			_dependency = dependency;
		}

		public object Resolve(Context context, int depth)
		{
			return _dependency;
		}

		public void Dispose()
		{
			IDisposable disposable = _dependency as IDisposable;
			if (disposable != null)
				disposable.Dispose();
		}


		public bool IsSingle
		{
			get { return true; }
		}
	}


	public class SingleAutocomposeDependency : IDependency
	{
		private object _dependency;
		private Type _type;
		public SingleAutocomposeDependency(Type type)
		{
			if (type == null)
				throw new ContextCreationException("Dependency is null");
			_type = type;
		}

		public object Resolve(Context context, int depth)
		{
			if (_dependency == null)
				_dependency = context.AutocomposeDependency(_type, depth);
			return _dependency;
		}

		public void Dispose()
		{
			IDisposable disposable = _dependency as IDisposable;
			if (disposable != null)
				disposable.Dispose();
		}

		public bool IsSingle
		{
			get { return true; }
		}
	}


	public class FactoryDependency<T> : IDependency
	{
		private IDependency _factorySingle;

		public FactoryDependency(IDependency factory)
		{
			_factorySingle = factory;
		}

		public object Resolve(Context context, int depth)
		{
			return ((DependencyFactory<T>)_factorySingle.Resolve(context, depth)).Create();
		}


		public void Dispose()
		{
			_factorySingle.Dispose();
		}

		public bool IsSingle
		{
			get { return false; }
		}
	}
}
