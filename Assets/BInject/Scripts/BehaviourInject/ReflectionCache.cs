using System;
using System.Collections.Generic;
using System.Reflection;

namespace BehaviourInject.Internal
{
	public class ReflectionCache
	{
		private static ReflectionCache _instance;

		private Dictionary<Type, IMemberInjection[]> _behavioirInjections;
		private Dictionary<Type, IEventHandler[]> _blindEvents;

		public ReflectionCache()
		{
			_behavioirInjections = new Dictionary<Type, IMemberInjection[]>();
			_blindEvents = new Dictionary<Type, IEventHandler[]>();
		}


		public IMemberInjection[] GetInjectionsFor(Type type)
		{
			IMemberInjection[] injections;
			if (!_behavioirInjections.TryGetValue(type, out injections))
			{
				injections = GenerateInjectionsFor(type);
				_behavioirInjections[type] = injections;
			}

			return injections;
		}


		private IMemberInjection[] GenerateInjectionsFor(Type type)
		{
			List<IMemberInjection> injections = new List<IMemberInjection>();
			BindingFlags flags =
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			Type target = type;

			while (!IsSystemOrEngine(target.Namespace))
			{
				PropertyInfo[] properties = target.GetProperties(flags);
				for (int i = 0; i < properties.Length; i++)
				{
					PropertyInfo property = properties[i];
					if (IsInjectable(property))
						injections.Add(new PropertyInjection(property));
				}

				FieldInfo[] fields = target.GetFields(flags);
				for (int i = 0; i < fields.Length; i++)
				{
					FieldInfo field = fields[i];
					if (IsInjectable(field))
						injections.Add(new FieldInjection(field));
				}

				target = target.BaseType;
			}

			MethodInfo[] methods = type.GetMethods();
			for (int i = 0; i < methods.Length; i++)
			{
				MethodInfo method = methods[i];
				if (IsInjectable(method))
					injections.Add(new MethodInjection(method));
			}

			return injections.ToArray();
		}


		private bool IsInjectable(MemberInfo member)
		{
			return AttributeUtils.IsMarked<InjectAttribute>(member);
		}


		public IEventHandler[] GetEventHandlers(Type target)
		{
			IEventHandler[] handlers;
			if (!_blindEvents.TryGetValue(target, out handlers))
			{
				handlers = GenerateEventHandlers(target);
				_blindEvents[target] = handlers;
			}

			return handlers;
		}


		private IEventHandler[] GenerateEventHandlers(Type target)
		{
			BindingFlags flags = 
				BindingFlags.Instance | 
				BindingFlags.Public | 
				BindingFlags.NonPublic;

			List<IEventHandler> events = new List<IEventHandler>();

			MethodInfo[] methods = target.GetMethods(flags);

			foreach (MethodInfo methodInfo in methods)
			{
				if (IsSystemOrEngine(methodInfo) 
					|| !AttributeUtils.IsMarked<InjectEventAttribute>(methodInfo))
					continue;

				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length != 1)
					throw new BehaviourInjectException(target.FullName + "." + methodInfo.Name + ": Injected event handlers can not have more than one argument!");

				Type eventType = parameters[0].ParameterType;
				if(eventType.IsValueType)
					throw new BehaviourInjectException(target.FullName + "." + methodInfo.Name + ": Injected event can not be a value type!");

				events.Add(new MethodEventHandler(methodInfo, eventType));
			}

			FieldInfo[] fields = target.GetFields(flags);
			Type demandedFieldType = typeof(MulticastDelegate);
			foreach (FieldInfo fieldInfo in fields)
			{
				Type fieldType = fieldInfo.FieldType;

				if (IsSystemOrEngine(fieldInfo)
					||!AttributeUtils.IsMarked<InjectEventAttribute>(fieldInfo)
					|| !demandedFieldType.IsAssignableFrom(fieldType))
					continue;

				MethodInfo invokeMethod = fieldType.GetMethod("Invoke");
				ParameterInfo[] parameters = invokeMethod.GetParameters();
				if (parameters.Length != 1)
					throw new BehaviourInjectException(target.FullName + "." + fieldInfo.Name 
						+ " delegate should have only one argument");

				Type eventType = parameters[0].ParameterType;
				events.Add(new DelegateEventHandler(fieldInfo, eventType));
			}

			return events.ToArray();
		}


		private bool IsSystemOrEngine(MemberInfo info)
		{
			string @namespace = info.DeclaringType.Namespace;
			return IsSystemOrEngine(@namespace);
		}

		private bool IsSystemOrEngine(string @namespace)
		{
			return 
				!String.IsNullOrEmpty(@namespace) &&
				( @namespace.Contains("System") 
				|| @namespace.Contains("UnityEngine") );
		}


		public static IMemberInjection[] GetInjections(Type type)
		{
			if (_instance == null)
				_instance = new ReflectionCache();

			return _instance.GetInjectionsFor(type);
		}

		public static IEventHandler[] GetEventHandlersFor(Type target)
		{
			if (_instance == null)
				_instance = new ReflectionCache();

			return _instance.GetEventHandlers(target);
		}
	}
}
