using System;
using System.Reflection;

namespace BehaviourInject.Internal
{
	public interface IMemberInjection
	{
		void Inject(object target, Context context);
	}


	public class AbstractInjecton {

		private Mode _mode;

		public AbstractInjecton(MemberInfo info)
		{
			_mode = AttributeUtils.IsMarked<CreateAttribute>(info) ? Mode.Create : Mode.Inject;
		}

		protected object GetDependency(Type type, Context context)
		{
			if (_mode == Mode.Create)
			{
				return context.AutocomposeDependency(type, 0);
			}
			else
			{
				return context.Resolve(type);
			}
		}

		private enum Mode
		{
			Create,
			Inject
		}
	}


	public class PropertyInjection : AbstractInjecton, IMemberInjection
	{
		private PropertyInfo _propertyInfo;
		private Type _dependencyType;

		public PropertyInjection(PropertyInfo info) : base(info)
		{
			_propertyInfo = info;
			_dependencyType = info.PropertyType;
		}
		
		public void Inject(object target, Context context)
		{
			object dependency = GetDependency(_dependencyType, context);

			if (_propertyInfo.GetValue(target, null) != null)
				throw new BehaviourInjectException(String.Format("Property {0} to inject is already contains something!", _propertyInfo.Name));
			_propertyInfo.SetValue(target, dependency, null);
		}
	}

	public class FieldInjection : AbstractInjecton, IMemberInjection
	{
		private FieldInfo _fieldInfo;
		private Type _dependencyType;

		public FieldInjection(FieldInfo info)
			: base(info)
		{
			_fieldInfo = info;
			_dependencyType = info.FieldType;
		}
		
		public void Inject(object target, Context context)
		{
			object dependency = GetDependency(_dependencyType, context);

			if (_fieldInfo.GetValue(target) != null)
				throw new BehaviourInjectException(String.Format("Field {0} to inject is already contains something!", _fieldInfo.Name));
			_fieldInfo.SetValue(target, dependency);
		}
	}


	public class MethodInjection : AbstractInjecton, IMemberInjection
	{
		private MethodInfo _methodInfo;

		private Type[] _dependencyTypes;
		private object[] _invokationArguments;

		public MethodInjection(MethodInfo info)
			: base(info)
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
				_invokationArguments[i] = GetDependency(dependencyType, context);
			}

			_methodInfo.Invoke(target, _invokationArguments);
		}
	}
}
