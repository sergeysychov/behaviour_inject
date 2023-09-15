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
        Type DependencyType { get; }
    }


    public class SingletonDependency : IDependency
    {
        private readonly object _dependency;
        private readonly Context _context;

        public SingletonDependency(object dependency, Context context)
        {
            if (dependency == null)
                throw new ContextCreationException("Dependency is null");
            _dependency = dependency;

            if (context is not null)
            {
                _context = context;
                _context.AutoSubscribeToEvents(_dependency);
            }
        }

        public object Resolve(Context context)
        {
            return _dependency;
        }

        public void Dispose()
        {
            if (_context is not null)
            {
                _context.UnsubscribeTarget(_dependency);
            }

            IDisposable disposable = _dependency as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        public Type DependencyType => _dependency.GetType();
    }

    public class SingletonAutocomposeDependency : IDependency
    {
        private object _dependency;
        private Type _type;

        public SingletonAutocomposeDependency(Type type)
        {
            if (type == null)
                throw new ContextCreationException("Dependency is null");
            _type = type;
        }

        public object Resolve(Context context)
        {
            if (_dependency == null)
            {
                _dependency = context.AutocomposeDependency(_type);
                context.AutoSubscribeToEvents(_dependency);
            }

            return _dependency;
        }

        public void Dispose()
        {
            IDisposable disposable = _dependency as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        public Type DependencyType => _type;
    }


    public class FactoryDependency<T> : IDependency
    {
        private IDependency _factorySingle;

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

        public Type DependencyType => typeof(T);
    }


    public class FactoryMethodDependency<T1, T2> : IDependency
        where T1 : class
        where T2 : class
    {
        private IDependency _argumentDependency;
        private Func<T1, T2> _func;

        public FactoryMethodDependency(IDependency argumentDependency, Func<T1, T2> func)
        {
            _argumentDependency = argumentDependency;
            _func = func;
        }

        public object Resolve(Context context)
        {
            return _func((T1)_argumentDependency.Resolve(context));
        }

        public void Dispose()
        {
            _argumentDependency.Dispose();
        }

        public Type DependencyType => typeof(T2);
    }
}
