/*
The MIT License (MIT)

Copyright (c) 2015 Sergey Sychov

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Reflection;
using System.Collections.Generic;
using BehaviourInject.Internal;

namespace BehaviourInject
{
    //Do not use this class anywhere!

    public class Context : IContextParent
    {
		public const string DEFAULT = "default";

        private Dictionary<Type, object> _dependencies;
		private List<object> _listedDependencies;
        private Dictionary<Type, DependencyFactory> _factories;
        private HashSet<Type> _autoCompositionTypes;
        private string _name;
		private IContextParent _parentContext = ParentContextStub.STUB;

		public EventManager EventManager { get; private set; }


        public Context() : this(DEFAULT)
        { }


        public Context(string name)
		{
			_name = name;
			ContextRegistry.RegisterContext(name, this);
			_dependencies = new Dictionary<Type, object>();
			_listedDependencies = new List<object>();
			_factories = new Dictionary<Type, DependencyFactory>();
			_autoCompositionTypes = new HashSet<Type>();
			
			EventManager = new EventManager();
			EventManager.EventInjectors += OnBlindEventHandler;
			RegisterDependency<IEventDispatcher>(EventManager);

			_parentContext = ParentContextStub.STUB;
		}


		public Context SetParentContext(string parentName)
		{
			if (_name == parentName)
				throw new BehaviourInjectException("Scopes can not be cycled: " + _name + " - " + parentName);

			EventManager.ClearParent();

			Context parentContext = ContextRegistry.GetContext(parentName);
			_parentContext = parentContext;
			EventManager.SetParent(_parentContext.EventManager);

			return this;
		}


		private void OnBlindEventHandler(object evnt)
		{
			for (int i = 0; i < _listedDependencies.Count; i++)
			{
				object dependency = _listedDependencies[i];
				Type dependencyType = dependency.GetType();
				BlindEventHandler[] handlers = ReflectionCache.GetEventHandlersFor(dependencyType);
				foreach (BlindEventHandler handler in handlers)
					if (handler.IsSuitableForEvent(evnt.GetType()))
						handler.Invoke(dependency, evnt);
			}
		}


		public Context RegisterDependency<T>(T dependency) {
            RegisterDependencyAs<T, T>(dependency);
			return this;
        }


        public Context RegisterDependencyAs<T, IT>(T dependency) where T : IT
        {
            ThrowIfNull(dependency, "dependency");
            Type dependencyType = typeof(IT);
            ThrowIfRegistered(dependencyType);
			InsertDependency(dependencyType, dependency);
			return this;
        }


		private void InsertDependency(Type type, object dependency)
		{
			_dependencies.Add(type, dependency);
			_listedDependencies.Add(dependency);
		}


        private void ThrowIfNull(object target, string argName)
        {
            if (target == null)
                throw new BehaviourInjectException(argName + " is null");
        }


        private void ThrowIfRegistered(Type dependencyType)
        {
            if (_dependencies.ContainsKey(dependencyType) || _factories.ContainsKey(dependencyType) || _autoCompositionTypes.Contains(dependencyType))
                throw new BehaviourInjectException(dependencyType.FullName + " is already registered in this context");
        }


        public Context RegisterFactory<T>(DependencyFactory factory)
        {
            ThrowIfNull(factory, "factory");
            Type dependencyType = typeof(T);
            ThrowIfRegistered(dependencyType);
            _factories.Add(dependencyType, factory);
			return this;
        }


        public Context RegisterFactory<T, FactoryT>()
        {
            Type factoryType = typeof(FactoryT);
            Type dependencyType = typeof(T);
            ThrowIfRegistered(dependencyType);
            RegisterType<FactoryT>();
            DependencyFactory factory = (DependencyFactory)Resolve(factoryType);
            _factories.Add(dependencyType, factory);
			return this;
        }


        public Context RegisterType<T>()
        {
            Type dependencyType = typeof(T);
            ThrowIfRegistered(dependencyType);
            _autoCompositionTypes.Add(dependencyType);
			return this;
        }


		public bool TryResolve(Type resolvingType, out object dependency)
		{
			object parentDependency;
			if (_dependencies.ContainsKey(resolvingType))
				dependency = _dependencies[resolvingType];
			else if (_factories.ContainsKey(resolvingType))
				dependency = _factories[resolvingType].Create();
			else if (_autoCompositionTypes.Contains(resolvingType))
				dependency = AutocomposeDependency(resolvingType);
			else if (_parentContext.TryResolve(resolvingType, out parentDependency))
				dependency = parentDependency;
			else
			{
				dependency = null;
				return false;
			}

			return true;
		}

		public object Resolve(Type resolvingType)
        {
			object dependency;

			if(! TryResolve(resolvingType, out dependency))
				throw new BehaviourInjectException(String.Format("Can not resolve. Type {0} not registered in this context!", resolvingType.FullName));

			return dependency;
        }


        private object AutocomposeDependency(Type resolvingType)
        {
            ConstructorInfo constructor = FindAppropriateConstructor(resolvingType);
            ParameterInfo[] parameters = constructor.GetParameters();
            object[] arguments = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                Type argumentType = parameter.ParameterType;

                if (argumentType == resolvingType)
                    throw new BehaviourInjectException("Cyclic dependency occured: " + argumentType.FullName);

                arguments[i] = Resolve(argumentType);
            }

            object result = constructor.Invoke(arguments);
			InsertDependency(resolvingType, result);

            return result;
        }


        private ConstructorInfo FindAppropriateConstructor(Type resolvingType)
        {
            ConstructorInfo[] constructors = resolvingType.GetConstructors();

            ConstructorInfo constructorWithLeastArguments = null;
            int leastParameters = Int32.MaxValue;

            for (int i = 0; i < constructors.Length; i++)
            {
                ConstructorInfo constructor = constructors[i];

                if (HasAttribute(constructor))
                    return constructor;

                int parametersLength = constructor.GetParameters().Length;
                if (parametersLength < leastParameters)
                {
                    constructorWithLeastArguments = constructor;
                    leastParameters = parametersLength;
                }
            }

            return constructorWithLeastArguments;
        }


        private bool HasAttribute(MemberInfo constructor)
        {
            object[] attributes = constructor.GetCustomAttributes(typeof(InjectAttribute), true);
            return attributes.Length > 0;
        }

        
        public void Destroy()
        {
			EventManager.ClearParent();
            ContextRegistry.UnregisterContext(_name);
        }
	}
}
