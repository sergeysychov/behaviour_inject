using System;
using System.Collections.Generic;
using System.Reflection;
#if BINJECT_DIAGNOSTICS
using BehaviourInject.Diagnostics;
#endif

namespace BehaviourInject.Internal
{
	internal class ReflectionCache
	{
		private static IMemberInjection[] EMPTY_MEMBER_COLLECTION = new IMemberInjection[0];

		private static ReflectionCache _instance;
		private string[] _excludedNamespaces;

		private Dictionary<Type, IMemberInjection[]> _behavioirInjections;
		private Dictionary<Type, IEventHandler[]> _blindEvents;

		public ReflectionCache()
		{
			Settings settings = Settings.Load();
			_excludedNamespaces = settings.ExcludedNames;
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

#if BINJECT_DIAGNOSTICS
				BinjectDiagnostics.CachedTypes++;
				BinjectDiagnostics.CachedInjections += injections.Length;
#endif
			}

			return injections;
		}


		private IMemberInjection[] GenerateInjectionsFor(Type type)
		{
			List<IMemberInjection> injections = new List<IMemberInjection>();

			if (IsIgnoredType(type))
				return EMPTY_MEMBER_COLLECTION;

			BindingFlags flags =
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.DeclaredOnly;

			Type target = type;

			while (!IsIgnoredType(target))
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

				MethodInfo[] methods = target.GetMethods(flags);
				for (int i = 0; i < methods.Length; i++)
				{
					MethodInfo method = methods[i];
					if (IsInjectable(method))
						injections.Add(new MethodInjection(method));
				}

				target = target.BaseType;
			}

			return injections.ToArray();
		}


		private bool IsInjectable(MemberInfo member)
		{
			return AttributeUtils.IsMarked<InjectAttribute>(member)
				|| AttributeUtils.IsMarked<CreateAttribute>(member);
		}


		public IEventHandler[] GetEventHandlers(Type target)
		{
			IEventHandler[] handlers;
			if (!_blindEvents.TryGetValue(target, out handlers))
			{
				handlers = GenerateEventHandlers(target);
				_blindEvents[target] = handlers;

#if BINJECT_DIAGNOSTICS
				BinjectDiagnostics.CachedEventTargets++;
				BinjectDiagnostics.CachedEventHandlers += handlers.Length;
#endif
			}

			return handlers;
		}


		private IEventHandler[] GenerateEventHandlers(Type target)
		{
			BindingFlags flags =
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.DeclaredOnly;

			List<IEventHandler> events = new List<IEventHandler>();

			while (!IsIgnoredType(target))
			{
				MethodInfo[] methods = target.GetMethods(flags);

				foreach (MethodInfo methodInfo in methods)
				{
					if (IsIgnoredMember(methodInfo)
						|| !AttributeUtils.IsMarked<InjectEventAttribute>(methodInfo))
						continue;

					ParameterInfo[] parameters = methodInfo.GetParameters();
					if (parameters.Length != 1)
						throw new BehaviourInjectException(target.FullName + "." + methodInfo.Name + ": Injected event handlers can not have more than one argument!");

					Type eventType = parameters[0].ParameterType;
					if (eventType.IsValueType)
						throw new BehaviourInjectException(target.FullName + "." + methodInfo.Name + ": Injected event can not be a value type!");

					events.Add(new MethodEventHandler(methodInfo, eventType));
				}

				FieldInfo[] fields = target.GetFields(flags);
				Type demandedFieldType = typeof(MulticastDelegate);
				foreach (FieldInfo fieldInfo in fields)
				{
					Type fieldType = fieldInfo.FieldType;

					if (IsIgnoredMember(fieldInfo)
						|| !AttributeUtils.IsMarked<InjectEventAttribute>(fieldInfo)
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
				target = target.BaseType;
			}

			return events.ToArray();
		}


		private bool IsIgnoredMember(MemberInfo info)
		{
			string fullname = info.DeclaringType.FullName;
			return IsIgnored(fullname);
		}

		private bool IsIgnoredType(Type type)
		{
			return IsIgnored(type.FullName);
		}

		private bool IsIgnored(string fullName)
		{
			if(String.IsNullOrEmpty(fullName))
				return false;

			int length = _excludedNamespaces.Length;
			for (int i = 0; i < length; i++)
			{
				//UnityEngine.Debug.Log("full name <color=green>" + fullName + "</color> starts with <color=teal>" + _excludedNamespaces[i] + "</color> = " + fullName.StartsWith(_excludedNamespaces[i], StringComparison.InvariantCultureIgnoreCase));
				if (fullName.StartsWith(_excludedNamespaces[i], StringComparison.Ordinal))
					return true;
			}

			return false;
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
