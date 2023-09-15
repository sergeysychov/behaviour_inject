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
using System.Text;
using BehaviourInject.Internal;
using System.Linq;
using System.Diagnostics;

#if BINJECT_DIAGNOSTICS
using BehaviourInject.Diagnostics;
#endif

namespace BehaviourInject
{
    //Do not use this class anywhere!

    public class Context : IContextParent, IEventDispatcher, IEventRegistry
    {
		public const string DEFAULT = "default";

        private readonly Dictionary<Type, IDependency> _dependencies;
		private readonly List<IDependency> _listedDependencies;
		private readonly Stack<Type> _compositionStack;

		private readonly string _name;
		private readonly bool _isGlobal;
		private IContextParent _parentContext = ParentContextStub.STUB;
		private HashSet<Context> _children;
		
		public bool IsDestroyed { get; private set; }
		public EventManager EventManager { [DebuggerStepThrough] get; private set; }

		public event Action OnContextDestroyed;

		public static Context Create() => Create(Context.DEFAULT);

		public static Context Create(string name) => InternalCreate(name, parent: null, overrideEventManager: false);

        public static Context CreateChild(string name)
            => InternalCreate(name, parent: ContextRegistry.GetContext(Context.DEFAULT), overrideEventManager: false);

        public static Context CreateChild(string name, string parent)
            => InternalCreate(name, parent: ContextRegistry.GetContext(parent), overrideEventManager: false);

        public static Context CreateChild(string name, string parent, bool overrideEventManager)
            => InternalCreate(name, parent: ContextRegistry.GetContext(parent), overrideEventManager: overrideEventManager);

        public static Context CreateChild(string name, Context parent) => InternalCreate(name, parent: parent, overrideEventManager: false);
        public static Context CreateChild(string name, Context parent, bool overrideEventManager) => InternalCreate(name, parent: parent, overrideEventManager: overrideEventManager);

        private static Context InternalCreate(string name, Context parent, bool overrideEventManager)
        {
            ContextRegistry.ValidateContextName(name);

            Context context = new Context(name, isGlobal: true, parent: parent, overrideEventManager: overrideEventManager);
            ContextRegistry.RegisterContext(name, context);

            return context;
        }


        public static Context CreateLocal() => new Context("___local_context", false, parent: null, overrideEventManager: true);
		
		public static bool Exists(string contextName) => ContextRegistry.Contains(contextName);

        private Context(string name, bool isGlobal, Context parent, bool overrideEventManager)
		{
			_name = name;
			_isGlobal = isGlobal;
			_dependencies = new Dictionary<Type, IDependency>(32);
			_listedDependencies = new List<IDependency>(32);
			_compositionStack = new Stack<Type>(16);

			_children = new HashSet<Context>();

            RegisterSingleton<IEventDispatcher>(this);
            RegisterSingleton<IEventRegistry>(this);

            RegisterSingleton<IInstantiator>(new LocalInstantiator(this));

			if (parent is not null && !overrideEventManager)
			{
				EventManager = parent.EventManager;
			}
			else
			{
                EventManager = new EventManager();
				if (parent is not null)
				{ 
					parent.EventManager.AttachDispatcher(this);
				}
            }

            _parentContext = ParentContextStub.STUB;
            if (parent is not null)
			{
				SetParentContext(parent);
			}
			
        }

		private void ValidateParent(Context toValidate)
		{
			if (object.ReferenceEquals(toValidate, this))
			{
				throw new ContextCreationException($"Scopes can not be cycled: {_name}");
			}
			if (_parentContext is Context parent)
			{
				parent.ValidateParent(toValidate);
			}
        }

		public Context SetParentContext(string parentName)
        {
            if (_name == parentName)
                throw new ContextCreationException($"Scopes can not be cycled: {_name} - {parentName}");

            Context newParentContext = ContextRegistry.GetContext(parentName);

            return SetParentContext(newParentContext);
        }

        private Context SetParentContext(Context parent)
        {
            parent.ValidateParent(this);

            _parentContext.OnContextDestroyed -= HandleParentDestroyed;
            if (_parentContext is Context oldParent)
            {
                oldParent._children.Remove(this);
				oldParent.EventManager.DetachDispatcher(EventManager);
            }

            _parentContext = parent;
            parent._children.Add(this);
            _parentContext.OnContextDestroyed += HandleParentDestroyed;

            return this;
        }

        private void HandleParentDestroyed()
		{
			Destroy();
		}

		public Context RegisterSingleton<T>(T instance) {
            RegisterDependencyAs<T, T>(instance);

			return this;
        }

        public Context RegisterDependencyAs<T, IT>(T dependency) where T : IT
        {
            ThrowIfNull(dependency, "dependency");

            Type dependencyType = typeof(IT);
			InsertDependency(dependencyType, new SingletonDependency(dependency, this));

			return this;
        }

		private void InsertDependency(Type type, IDependency dependency)
		{
			ThrowIfRegistered(type);
			_dependencies.Add(type, dependency);
			_listedDependencies.Add(dependency);
#if BINJECT_DIAGNOSTICS
			BinjectDiagnostics.DependenciesCount++;
			BinjectDiagnostics.RecipientCount++;
#endif
		}

		private void ThrowIfNull(object target, string argName)
        {
            if (target == null)
                throw new BehaviourInjectException(argName + " is null");
        }

        private void ThrowIfRegistered(Type dependencyType)
        {
            if (_dependencies.ContainsKey(dependencyType))
                throw new ContextCreationException(dependencyType.FullName + " is already registered in this context");
        }

        public Context RegisterFactory<T>(DependencyFactory<T> factory)
        {
            ThrowIfNull(factory, "factory");
            Type dependencyType = typeof(T);
			Type factoryType = factory.GetType();

			SingletonDependency selfDependency = new SingletonDependency(factory, context: this);
			InsertDependency(factoryType, selfDependency);
			FactoryDependency<T> factoryDependency = new FactoryDependency<T>(selfDependency);
			InsertDependency(dependencyType, factoryDependency);

			return this;
        }

        public Context RegisterFactory<T, FactoryT>() where FactoryT : DependencyFactory<T>
        {
            Type factoryType = typeof(FactoryT);
            Type dependencyType = typeof(T);
			var selfDependency = new SingletonAutocomposeDependency(factoryType);
			InsertDependency(factoryType, selfDependency);
			FactoryDependency<T> factoryDependency = new FactoryDependency<T>(selfDependency);
			InsertDependency(dependencyType, factoryDependency);

			return this;
        }

        public Context RegisterSingleton<T>()
        {
            Type dependencyType = typeof(T);
			InsertDependency(dependencyType, new SingletonAutocomposeDependency(dependencyType));
			return this;
        }
		public Context RegisterSingletonAs<T, IT>() where T : IT
		{
			Type dependencyType = typeof(IT);
			Type concreteType = typeof(T);
			InsertDependency(dependencyType, new SingletonAutocomposeDependency(concreteType));
			return this;
		}

		public Context RegisterAsFunction<T1, T2>(Func<T1, T2> func)
			where T1 : class
			where T2 : class
		{
			Type argumentType = typeof(T1);
			Type dependencyType = typeof(T2);
			if (!_dependencies.TryGetValue(argumentType, out IDependency argumentDependency))
				throw new BehaviourInjectException("No argument type registered: " + argumentType.FullName);
			var factoryDependency = new FactoryMethodDependency<T1, T2>(argumentDependency, func);
			InsertDependency(dependencyType, factoryDependency);
			return this;
		}
		
		public Context RegisterTypeAsMultiple<T>(params Type[] types)
		{
			Type concreteType = typeof(T);
			IDependency dependency = new SingletonAutocomposeDependency(concreteType);
			int length = types.Length;
			
			if(length == 0)
				throw new BehaviourInjectException("register types array shouldn't be empty");
			
			for (int i = 0; i < length; i++)
			{
				Type dependencyType = types[i];
				ThrowIfNotAncestor(concreteType, dependencyType);
				InsertDependency(dependencyType, dependency);	
			}
			return this;
		}

		private void ThrowIfNotAncestor(Type descent, Type ancestor)
		{
			if (!ancestor.IsAssignableFrom(descent))
				throw new BehaviourInjectException("Can not register " + descent.FullName + " as " + ancestor.FullName + ": no ancestry");
		}

		public object Resolve(Type resolvingType)
		{
			object dependency;

			if(!InternalResolve(resolvingType, out dependency))
				throw new BehaviourInjectException(
					String.Format("Can not resolve. Type {0} not registered in {1} context!", resolvingType.FullName, _name));

			return dependency;
		}

		public T Resolve<T>() => (T)Resolve(typeof(T));

		public bool TryResolve(Type resolvingType, out object dependency) => InternalResolve(resolvingType, 
			out dependency);

		private bool InternalResolve(Type resolvingType,
			out object dependency)
		{
            if (IsDestroyed)
                throw new BehaviourInjectException(String.Format("Can not resolve {0}. Context {1} is destroyed.", resolvingType, _name));

            object parentDependency;
            if (_dependencies.ContainsKey(resolvingType))
                dependency = _dependencies[resolvingType].Resolve(this);
            else if (_parentContext.TryResolve(resolvingType, out parentDependency))
                dependency = parentDependency;
            else
            {
                dependency = null;
                return false;
            }

            return true;
        }

		public object AutocomposeDependency(Type resolvingType) => AutocomposeDependency(resolvingType, Array.Empty<object>());
		
		public object AutocomposeDependency(Type resolvingType, object[] additions)
        {
	        CheckAgainstCompositionStack(resolvingType);
	        _compositionStack.Push(resolvingType);
	        
            ConstructorInfo constructor = FindAppropriateConstructor(resolvingType);
            ParameterInfo[] parameters = constructor.GetParameters();
            object[] arguments = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                Type argumentType = parameter.ParameterType;

                object dependency;
                if (AttributeUtils.IsMarked<CreateAttribute>(parameter))
                {
	                dependency = AutocomposeDependency(argumentType);
                }
                else
                {
	                if (!TryResolve(argumentType, out dependency)
	                    && !TryFindAmongAdditions(argumentType, additions, out dependency))
	                {
		                throw new BehaviourInjectException(String.Format(
			                "Could not resolve {0} for {2} in context {1}. Probably it's not registered",
			                argumentType.FullName, _name, resolvingType.FullName));
	                }
                }

				arguments[i] = dependency;
            }

            object result = constructor.Invoke(arguments);

			IMemberInjection[] injections = ReflectionCache.GetInjections(resolvingType);
			for (int i = 0; i < injections.Length; i++)
				injections[i].Inject(result, this);

			_compositionStack.Pop();
			
            return result;
        }

		private bool TryFindAmongAdditions(Type wantedType, object[] additions, out object dependency)
		{
			dependency = null;
			if (additions.Length == 0) return false;
			foreach (object addition in additions)
			{
				if (addition.GetType() == wantedType)
				{
					dependency = addition;
					return true;
				}
			}
			return false;
		}


		private void CheckAgainstCompositionStack(Type type)
		{
			if (_compositionStack.Contains(type))
			{
				StringBuilder text = new StringBuilder(128);
				text.Append("Cycled dependency occured:");
				foreach (Type stackedType in _compositionStack)
					text.Append(stackedType.Name).Append("->");
				text.Append(type.Name);
				throw new BehaviourInjectException(text.ToString());
			}
		}

        private ConstructorInfo FindAppropriateConstructor(Type resolvingType)
        {
            ConstructorInfo[] constructors = resolvingType.GetConstructors();

            ConstructorInfo constructorWithLeastArguments = null;
            int leastParameters = Int32.MaxValue;

            for (int i = 0; i < constructors.Length; i++)
            {
                ConstructorInfo constructor = constructors[i];

                if (AttributeUtils.IsMarked<InjectAttribute>(constructor))
                    return constructor;

                int parametersLength = constructor.GetParameters().Length;
                if (parametersLength < leastParameters)
                {
                    constructorWithLeastArguments = constructor;
                    leastParameters = parametersLength;
                }
            }

			if (constructorWithLeastArguments == null)
				throw new BehaviourInjectException("Can not find constructor for type " + resolvingType.FullName);

            return constructorWithLeastArguments;
        }

		public Context CompleteRegistration()
		{
			foreach (IDependency dependency in _listedDependencies)
			{
				dependency.Resolve(this);
			}

			return this;
		}
        
        public void Destroy()
		{
			if (IsDestroyed)
				return;

			if(_isGlobal)
				ContextRegistry.UnregisterContext(_name);

			_children.Clear();

			DisposeDependencies();

#if BINJECT_DIAGNOSTICS
			BinjectDiagnostics.DependenciesCount -= _listedDependencies.Count;
			BinjectDiagnostics.RecipientCount -= _listedDependencies.Count;
#endif

            _parentContext.OnContextDestroyed -= HandleParentDestroyed;
			EventManager.Dispose();

			if (OnContextDestroyed != null)
				OnContextDestroyed();

			IsDestroyed = true;
        }

		private void DisposeDependencies()
		{
			for (int i = 0; i < _listedDependencies.Count; i++)
			{
				try
				{
					_listedDependencies[i].Dispose();
				}
				catch (Exception e)
				{
					UnityEngine.Debug.LogError($"During context dispose: {e.Message}\r\n{e.StackTrace}");
				}
			}
		}

		#region IEventDispatcher

		public void DispatchEvent<TEvent>(TEvent @event) => EventManager.DispatchEvent(@event);

		#endregion

        public void AutoSubscribeToEvents(object target)
        {
            Type targetType = target.GetType();
            IEventBinder[] binders = ReflectionCache.GetEventBinderFor(targetType);
            foreach (IEventBinder binder in binders)
            {
                try
                {
                    binder.Bind(target, this);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

		#region IEventRegistry

		public void Subscribe(Type eventType, Delegate handler, bool handleAllDerived)
		{
			EventManager.Subscribe(eventType: eventType, handler: handler, handleAllDerived: handleAllDerived, handleDerived: null);
		}
        	
        public void Unsubscribe(Type eventType, Delegate handler)
        	=> EventManager.Unsubscribe(eventType: eventType, handler: handler, filter: null);

		public void Subscribe(Type eventType, Delegate handler, bool handleAllDerived, Type[] filter)
		{
			EventManager.Subscribe(eventType: eventType, handler: handler, handleAllDerived: handleAllDerived, handleDerived: filter);
		}
        
		public void Unsubscribe(Type eventType, Delegate handler, Type[] filter)
			=> EventManager.Unsubscribe(eventType: eventType, handler: handler, filter: filter);
		
		public void UnsubscribeTarget(object target) => EventManager.UnsubscribeTarget(target);
		#endregion
    }
}
