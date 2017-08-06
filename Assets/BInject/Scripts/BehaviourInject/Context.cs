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
		private const int MAX_HIERARCHY_DEPTH = 32;

        private Dictionary<Type, IDependency> _dependencies;
		private List<IDependency> _listedDependencies;

		private List<CommandEntry> _commands;
		private Dictionary<Type, CommandEntry> _commandsByEvent;


        private string _name;
		private IContextParent _parentContext = ParentContextStub.STUB;
		private bool _isGlobal;

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


		//[Obsolete("Use Context.Create() instead")]
        private Context(string name, bool isGlobal = true)
		{
			_name = name;
			_isGlobal = isGlobal;
			_dependencies = new Dictionary<Type, IDependency>(32);
			_listedDependencies = new List<IDependency>(32);

			_commands = new List<CommandEntry>(32);
			_commandsByEvent = new Dictionary<Type, CommandEntry>(32);
			
			EventManager = new EventManager();
			EventManager.EventInjectors += OnBlindEventHandler;
			RegisterDependency<IEventDispatcher>(EventManager);

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


		private void OnBlindEventHandler(object evnt)
		{
			Type eventType = evnt.GetType();
			for (int i = 0; i < _listedDependencies.Count; i++)
			{
				IDependency dependency = _listedDependencies[i];
				if (dependency.IsSingle)
				{
					object target = dependency.Resolve(this, 0);
					InjectEventTo(target, eventType, evnt);
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
				if (handler.IsSuitableForEvent(eventType))
					handler.Invoke(recipient, evt);
			}
		}


		private void ExecuteCommands(List<Type> commands, object evt)
		{
			Type eventType = evt.GetType();
			for (int i = 0; i < commands.Count; i++)
			{
				Type commandType = commands[i];
				ICommand command = (ICommand)AutocomposeDependency(commandType, 0);
				InjectEventTo(command, eventType, evt);
				command.Execute();
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
            Type dependencyType = typeof(IT);
			InsertDependency(dependencyType, new SingleDependency(dependency));
			return this;
        }


		private void InsertDependency(Type type, IDependency dependency)
		{
			ThrowIfRegistered(type);
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
			ThrowIfRegistered(dependencyType);
			InsertDependency(dependencyType, new SingleAutocomposeDependency(concreteType));
			return this;
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
				throw new BehaviourInjectException(String.Format("Can not resolve. Type {0} not registered in this context!", resolvingType.FullName));

			return dependency;
		}
		

		public bool TryResolve(Type resolvingType, out object dependency)
		{ return TryResolve(resolvingType, out dependency, 0); }


		private bool TryResolve(Type resolvingType, out object dependency, int hierarchyDepthCount)
		{
			if (IsDestroyed)
				throw new BehaviourInjectException(String.Format("Can not resolve {0}. Context {1} is destroyed.", resolvingType, _name));

			if (hierarchyDepthCount > MAX_HIERARCHY_DEPTH)
				throw new BehaviourInjectException(String.Format("You have reached maximum hierarchy depth ({0}). Probably recursive dependencies are occured in {1}", MAX_HIERARCHY_DEPTH, resolvingType.FullName));
			else
				hierarchyDepthCount++;

			object parentDependency;
			if (_dependencies.ContainsKey(resolvingType))
				dependency = _dependencies[resolvingType].Resolve(this, hierarchyDepthCount);
			else if (_parentContext.TryResolve(resolvingType, out parentDependency))
				dependency = parentDependency;
			else
			{
				dependency = null;
				return false;
			}

			return true;
		}


		public object AutocomposeDependency(Type resolvingType, int hierarchyDepthCount)
        {
            ConstructorInfo constructor = FindAppropriateConstructor(resolvingType);
            ParameterInfo[] parameters = constructor.GetParameters();
            object[] arguments = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                Type argumentType = parameter.ParameterType;

				object dependency;
				if (!TryResolve(argumentType, out dependency, hierarchyDepthCount))
					throw new BehaviourInjectException(String.Format("Could not resolve {0} in context {1}. Probably it's not registered", argumentType.FullName, _name));

				arguments[i] = dependency;
            }

            object result = constructor.Invoke(arguments);

			IMemberInjection[] injections = ReflectionCache.GetInjections(resolvingType);
			for (int i = 0; i < injections.Length; i++)
				injections[i].Inject(result, this);

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

                if (AttributeUtils.IsMarked<InjectAttribute>(constructor))
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


		public Context CreateAll()
		{
			foreach (IDependency dependency in _listedDependencies)
			{
				dependency.Resolve(this, 0);
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

			_parentContext.OnContextDestroyed -= HandleParentDestroyed;
			EventManager.ClearParent();
			EventManager.EventInjectors -= OnBlindEventHandler;

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
