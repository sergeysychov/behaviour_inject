using System;
using System.Reflection;

namespace BehaviourInject.Internal
{
	public interface IBehaviourInjection
	{
		void Inject(object target, Context context);
	}


	public class PropertyInjection : IBehaviourInjection
	{
		private PropertyInfo _propertyInfo;
		private Type _dependencyType;

		public PropertyInjection(PropertyInfo info)
		{
			_propertyInfo = info;
			_dependencyType = info.PropertyType;
		}
		
		public void Inject(object target, Context context)
		{
			object dependency = context.Resolve(_dependencyType);
			if (_propertyInfo.GetValue(target, null) != null)
				throw new BehaviourInjectException(String.Format("Property {0} to inject is already contains something!", _propertyInfo.Name));
			_propertyInfo.SetValue(target, dependency, null);
		}
	}

	public class FieldInjection : IBehaviourInjection
	{
		private FieldInfo _fieldInfo;
		private Type _dependencyType;

		public FieldInjection(FieldInfo info)
		{
			_fieldInfo = info;
			_dependencyType = info.FieldType;
		}
		
		public void Inject(object target, Context context)
		{
			object dependency = context.Resolve(_dependencyType);
			if (_fieldInfo.GetValue(target) != null)
				throw new BehaviourInjectException(String.Format("Field {0} to inject is already contains something!", _fieldInfo.Name));
			_fieldInfo.SetValue(target, dependency);
		}
	}


	public class MethodInjection : IBehaviourInjection
	{
		private MethodInfo _methodInfo;

		private Type[] _dependencyTypes;
		private object[] _invokationArguments;

		public MethodInjection(MethodInfo info)
		{
			_methodInfo = info;
			ParameterInfo[] arguments = _methodInfo.GetParameters();
			int argumentCount = arguments.Length;
			_dependencyTypes = new Type[argumentCount];
			_invokationArguments = new object[argumentCount];
			for (int i = 0; i < argumentCount; i++)
			{
				_dependencyTypes[i] = arguments[i].ParameterType;
			}
		}

		public void Inject(object target, Context context)
		{
			for (int i = 0; i < _dependencyTypes.Length; i++)
			{
				Type dependencyType = _dependencyTypes[i];
				_invokationArguments[i] = context.Resolve(dependencyType);
			}

			_methodInfo.Invoke(target, _invokationArguments);
		}
	}
}
