﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace BehaviourInject.Internal
{
	public class ReflectionCache
	{
		private static ReflectionCache _instance;

		private Dictionary<Type, IMemberInjection[]> _behavioirInjections;
		private Dictionary<Type, BlindEventHandler[]> _blindEvents;

		public ReflectionCache()
		{
			_behavioirInjections = new Dictionary<Type, IMemberInjection[]>();
			_blindEvents = new Dictionary<Type, BlindEventHandler[]>();
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
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			PropertyInfo[] properties = type.GetProperties(flags);
			for (int i = 0; i < properties.Length; i++)
			{
				PropertyInfo property = properties[i];
				if (IsInjectable(property))
					injections.Add(new PropertyInjection(property));
			}

			FieldInfo[] fields = type.GetFields(flags);
			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo field = fields[i];
				if (IsInjectable(field))
					injections.Add(new FieldInjection(field));
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


		public BlindEventHandler[] GetEventHandlers(Type target)
		{
			BlindEventHandler[] handlers;
			if (!_blindEvents.TryGetValue(target, out handlers))
			{
				handlers = GenerateEventHandlers(target);
				_blindEvents[target] = handlers;
			}

			return handlers;
		}


		private BlindEventHandler[] GenerateEventHandlers(Type target)
		{
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			List<BlindEventHandler> events = new List<BlindEventHandler>();
			MethodInfo[] methods = target.GetMethods(flags);

			foreach (MethodInfo methodInfo in methods)
			{
				if (IsNotBlindEvent(methodInfo))
					continue;

				ParameterInfo[] parameters = methodInfo.GetParameters();
				int parametersCount = parameters.Length;
				if (parametersCount != 1)
					throw new BehaviourInjectException(target.FullName + "." + methodInfo.Name + ": Injected event handlers can not have more than one argument!");

				Type eventType = parameters[0].ParameterType;
				if(eventType.IsValueType)
					throw new BehaviourInjectException(target.FullName + "." + methodInfo.Name + ": Injected event can not be a value type!");

				events.Add(new BlindEventHandler(methodInfo, eventType));
			}

			return events.ToArray();
		}

		private bool IsInjectable(MemberInfo member)
		{
			return ContainsAttribute(member, typeof(InjectAttribute));
		}

		private bool IsNotBlindEvent(MethodInfo member)
		{
			return !ContainsAttribute(member, typeof(InjectEventAttribute));
		}


		private bool ContainsAttribute(MemberInfo member, Type attributeType)
		{
			object[] attributes = member.GetCustomAttributes(attributeType, true);
			return attributes.Length > 0;
		}


		public static IMemberInjection[] GetInjections(Type type)
		{
			if (_instance == null)
				_instance = new ReflectionCache();

			return _instance.GetInjectionsFor(type);
		}

		public static BlindEventHandler[] GetEventHandlersFor(Type target)
		{
			if (_instance == null)
				_instance = new ReflectionCache();

			return _instance.GetEventHandlers(target);
		}
	}
}
