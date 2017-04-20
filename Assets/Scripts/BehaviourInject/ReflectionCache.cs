using System;
using System.Collections.Generic;
using System.Reflection;

namespace BehaviourInject.Internal
{
	public class ReflectionCache
	{
		private static ReflectionCache _instance;

		private Dictionary<Type, IBehaviourInjection[]> _behavioirInjections;

		public ReflectionCache()
		{
			_behavioirInjections = new Dictionary<Type, IBehaviourInjection[]>();
		}


		public IBehaviourInjection[] GetInjectionsFor(Type type)
		{
			if (!_behavioirInjections.ContainsKey(type))
				_behavioirInjections[type] = GenerateInjectionsFor(type);

			return _behavioirInjections[type];
		}


		private IBehaviourInjection[] GenerateInjectionsFor(Type type)
		{
			List<IBehaviourInjection> injections = new List<IBehaviourInjection>();

			PropertyInfo[] properties = type.GetProperties();
			for (int i = 0; i < properties.Length; i++)
			{
				PropertyInfo property = properties[i];
				if (IsInjectable(property))
					injections.Add(new PropertyInjection(property));
			}

			FieldInfo[] fields = type.GetFields();
			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo field = fields[i];
				if (IsInjectable(field))
					injections.Add(new FieldInjection(field));
			}

			return injections.ToArray();
		}


		private bool IsInjectable(MemberInfo property)
		{
			object[] attributes = property.GetCustomAttributes(typeof(InjectAttribute), true);
			return attributes.Length > 0;
		}


		public static IBehaviourInjection[] GetInjections(Type type)
		{
			if (_instance == null)
				_instance = new ReflectionCache();

			return _instance.GetInjectionsFor(type);
		}
	}
}
