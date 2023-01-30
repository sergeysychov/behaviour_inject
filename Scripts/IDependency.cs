using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourInject.Internal
{
	public interface IDependency
	{
		object Resolve(Context context);
		void Dispose();
		bool IsSingle { get; }
		bool AlreadyNotified { get; set; }
		Type DependencyType { get; }
	}


	public class SingleDependency : IDependency
	{
		private object _dependency;

		public bool AlreadyNotified { get; set; }

		public SingleDependency(object dependency)
		{
			if (dependency == null)
				throw new ContextCreationException("Dependency is null");
			_dependency = dependency;
		}

		public object Resolve(Context context)
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

		public Type DependencyType
		{
			get { return _dependency.GetType(); }
		}
	}

	public class SingleAutocomposeDependency : IDependency
	{
		private object _dependency;
		private Type _type;
		
		public bool AlreadyNotified { get; set; }
		
		public SingleAutocomposeDependency(Type type)
		{
			if (type == null)
				throw new ContextCreationException("Dependency is null");
			_type = type;
		}

		public object Resolve(Context context)
		{
			if (_dependency == null)
				_dependency = context.AutocomposeDependency(_type);
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

		public Type DependencyType
		{
			get { return _type; }
		}
	}


	public class FactoryDependency<T> : IDependency
	{
		private IDependency _factorySingle;

		public bool AlreadyNotified { get; set; }
		
		public FactoryDependency(IDependency factory)
		{
			_factorySingle = factory;
		}

		public object Resolve(Context context)
		{
			return ((DependencyFactory<T>)_factorySingle.Resolve(context)).Create();
		}


		public void Dispose()
		{
			_factorySingle.Dispose();
		}

		public bool IsSingle
		{
			get { return false; }
		}
		
		public Type DependencyType
		{
			get { return typeof(T); }
		}
	}


	public class FactoryMethodDependency<T1, T2> : IDependency
		where T1 : class
		where T2 : class
	{
	private IDependency _argumentDependency;
	private Func<T1, T2> _func;

	public bool AlreadyNotified { get; set; }

	public FactoryMethodDependency(IDependency argumentDependency, Func<T1, T2> func)
	{
		_argumentDependency = argumentDependency;
		_func = func;
	}

	public object Resolve(Context context)
	{
		return _func((T1) _argumentDependency.Resolve(context));
	}


	public void Dispose()
	{
		_argumentDependency.Dispose();
	}

	public bool IsSingle
	{
		get { return false; }
	}

	public Type DependencyType
	{
		get { return typeof(T2); }
	}
	}
}
