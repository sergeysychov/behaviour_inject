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

#if BINJECT_DIAGNOSTICS
using BehaviourInject.Diagnostics;
#endif

namespace BehaviourInject
{
    //Do not use this class anywhere!

    public class Context : IContextParent, EventTransmitter
    {
		public const string DEFAULT = "default";
		private const int MAX_HIERARCHY_DEPTH = 32;

        private readonly Dictionary<Type, IDependency> _dependencies;
		private readonly List<IDependency> _listedDependencies;
		private readonly Stack<Type> _compositionStack;

		private readonly List<CommandEntry> _commands;
		private readonly Dictionary<Type, CommandEntry> _commandsByEvent;

		private readonly string _name;
		private readonly bool _isGlobal;
		private IContextParent _parentContext = ParentContextStub.STUB;

		public bool IsDestroyed { get; private set; }
		public EventManager EventManager { get; private set; }
		public event Action OnContextDestroyed;


		public static Context Create()
		{
			return Create(Context.DEFAULT);
		}

		public static Context Create(string name)
		{
			Context context = new Context(name, true);
			ContextRegistry.RegisterContext(name, context);
			return context;
		}

		public static Context CreateLocal()
		{
			return new Context("___local_context", false);
		}
		
		
		public static bool Exists(string contextName)
		{
			return ContextRegistry.Contains(contextName);
		}


		//[Obsolete("Use Context.Create() instead")]
        private Context(string name, bool isGlobal = true)
		{
			_name = name;
			_isGlobal = isGlobal;
			_dependencies = new Dictionary<Type, IDependency>(32);
			_listedDependencies = new List<IDependency>(32);
			_compositionStack = new Stack<Type>(16);

			_commands = new List<CommandEntry>(32);
			_commandsByEvent = new Dictionary<Type, CommandEntry>(32);
			
			EventManager = new EventManager();
			EventManager.AddTransmitter(this);
			RegisterDependency<IEventDispatcher>(EventManager);
			RegisterDependency<IInstantiator>(new LocalInstantiator(this));

			_parentContext = ParentContextStub.STUB;
		}


		public Context SetParentContext(string parentName)
		{
			if (_name == parentName)
				throw new ContextCreationException("Scopes can not be cycled: " + _name + " - " + parentName);

			_parentContext.OnContextDestroyed -= HandleParentDestroyed;

			EventManager.ClearParent();

			Context parentContext = ContextRegistry.GetContext(parentName);
			_parentContext = parentContext;
			_parentContext.OnContextDestroyed += HandleParentDestroyed;
			EventManager.SetParent(_parentContext.EventManager);

			return this;
		}


		public void TransmitEvent(object evnt)
		{
			int count = _listedDependencies.Count;
			for (int i = 0; i < count; i++)
				_listedDependencies[i].AlreadyNotified = false;
			
			Type eventType = evnt.GetType();
			for (int i = 0; i < _listedDependencies.Count; i++)
			{
				IDependency dependency = _listedDependencies[i];
				if (dependency.IsSingle 
				    && ReflectionCache.GetEventHandlersFor(dependency.DependencyType).Length > 0
					&& !dependency.AlreadyNotified)
				{
					object target = dependency.Resolve(this);
					InjectEventTo(target, eventType, evnt);
					dependency.AlreadyNotified = true;
				}
			}


			for (int i = 0; i < _commands.Count; i++)
			{
				CommandEntry commandEntry = _commands[i];
				bool isSuitable = commandEntry.EventType.IsAssignableFrom(eventType);
				if (isSuitable)
					ExecuteCommands(commandEntry.CommandTypes, evnt);
			}
		}


		private void InjectEventTo(object recipient, Type eventType, object evt)
		{
			Type recipientType = recipient.GetType();
			IEventHandler[] handlers = ReflectionCache.GetEventHandlersFor(recipientType);
			for (int i = 0; i < handlers.Length; i++)
			{
				IEventHandler handler = handlers[i];
				try
				{
					if (handler.IsSuitableForEvent(eventType))
						handler.Invoke(recipient, evt);
				} catch(Exception e)
				{
					UnityEngine.Debug.LogException(e);
				}
			}
		}


		private void ExecuteCommands(List<Type> commands, object evt)
		{
			Type eventType = evt.GetType();
			for (int i = 0; i < commands.Count; i++)
			{
				Type commandType = commands[i];
				ICommand command = (ICommand)AutocomposeDependency(commandType);
				InjectEventTo(command, eventType, evt);
				
				try
				{
					command.Execute();
				}
				catch (Exception e)
				{
					UnityEngine.Debug.LogException(e);
				}
			}
		}


		private void HandleParentDestroyed()
		{
			Destroy();
		}


		public Context RegisterDependency<T>(T dependency) {
            RegisterDependencyAs<T, T>(dependency);
			return this;
        }


        public Context RegisterDependencyAs<T, IT>(T dependency) where T : IT
        {
            ThrowIfNull(dependency, "dependency");

			UnityEngine.MonoBehaviour monobeh = dependency as UnityEngine.MonoBehaviour;
			if (monobeh != null)
				TrySuppressInjectorEvents(monobeh);

            Type dependencyType = typeof(IT);
			InsertDependency(dependencyType, new SingleDependency(dependency));
			return this;
        }


		//this method fixes bug, wich caused doubling event for components that are both dependency and recipient
		private void TrySuppressInjectorEvents(UnityEngine.MonoBehaviour behaviour)
		{
			Injector injector = behaviour.GetComponent<Injector>();
			if (injector != null)
				injector.SuppressEventsFor(behaviour);
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

			SingleDependency selfDependency = new SingleDependency(factory);
			InsertDependency(factoryType, selfDependency);
			FactoryDependency<T> factoryDependency = new FactoryDependency<T>(selfDependency);
			InsertDependency(dependencyType, factoryDependency);

			return this;
        }


        public Context RegisterFactory<T, FactoryT>() where FactoryT : DependencyFactory<T>
        {
            Type factoryType = typeof(FactoryT);
            Type dependencyType = typeof(T);
			var selfDependency = new SingleAutocomposeDependency(factoryType);
			InsertDependency(factoryType, selfDependency);
			FactoryDependency<T> factoryDependency = new FactoryDependency<T>(selfDependency);
			InsertDependency(dependencyType, factoryDependency);

			return this;
        }


        public Context RegisterType<T>()
        {
            Type dependencyType = typeof(T);
			InsertDependency(dependencyType, new SingleAutocomposeDependency(dependencyType));
			return this;
        }


		public Context RegisterTypeAs<T, IT>() where T : IT
		{
			Type dependencyType = typeof(IT);
			Type concreteType = typeof(T);
			InsertDependency(dependencyType, new SingleAutocomposeDependency(concreteType));
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
			IDependency dependency = new SingleAutocomposeDependency(concreteType);
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


		public Context RegisterCommand<EventT, CommandT>() where CommandT : ICommand
		{
			Type eventType = typeof(EventT);
			Type commandType = typeof(CommandT);

			if (!_commandsByEvent.ContainsKey(eventType))
			{
				var newEntry = new CommandEntry(eventType);
				_commandsByEvent.Add(eventType, newEntry);
				_commands.Add(newEntry);
#if BINJECT_DIAGNOSTICS
				BinjectDiagnostics.CommandsCount++;
#endif
			}

			CommandEntry entry = _commandsByEvent[eventType];
			List<Type> commands = entry.CommandTypes;
			if (commands.Contains(commandType))
				throw new ContextCreationException(String.Format("Context {0} already contains command {1} on event {2}", _name, commandType, eventType));

			commands.Add(commandType);
			return this;
		}


		public object Resolve(Type resolvingType)
		{
			object dependency;

			if(! TryResolve(resolvingType, out dependency))
				throw new BehaviourInjectException(
					String.Format("Can not resolve. Type {0} not registered in {1} context!", resolvingType.FullName, _name));

			return dependency;
		}


	    public T Resolve<T>()
	    {
		    return (T) Resolve(typeof(T));
	    }
		

		public bool TryResolve(Type resolvingType, out object dependency)
		{ return TryResolve(resolvingType, out dependency, 0); }


		private bool TryResolve(Type resolvingType, out object dependency, int hierarchyDepthCount)
		{
			if (IsDestroyed)
				throw new BehaviourInjectException(String.Format("Can not resolve {0}. Context {1} is destroyed.", resolvingType, _name));

			if (hierarchyDepthCount > MAX_HIERARCHY_DEPTH)
				throw new BehaviourInjectException(String.Format("You have reached maximum hierarchy depth ({0}). Probably recursive dependencies are occured in {1}", MAX_HIERARCHY_DEPTH, resolvingType.FullName));
			
			hierarchyDepthCount++;

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


		public object AutocomposeDependency(Type resolvingType)
		{
			return AutocomposeDependency(resolvingType, Array.Empty<object>());
		}
		
		
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


		public Context CreateAll()
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

			DisposeDependencies();

#if BINJECT_DIAGNOSTICS
			BinjectDiagnostics.DependenciesCount -= _listedDependencies.Count;
			BinjectDiagnostics.RecipientCount -= _listedDependencies.Count;
			BinjectDiagnostics.CommandsCount -= _commands.Count;
#endif

			_parentContext.OnContextDestroyed -= HandleParentDestroyed;
			EventManager.ClearParent();
			EventManager.RemoveTransmitter(this);

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
					UnityEngine.Debug.LogError("During context dispose: " + e.Message + "\r\n" + e.StackTrace);
				}
			}
		}
	}
}
