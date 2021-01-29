using System;
using System.Collections.Generic;
using System.Reflection;

namespace BehaviourInject.Internal
{
	public interface IMemberInjection
	{
		void Inject(object target, Context context);
	}
	

	public class AbstractInjecton {

		protected ResolveMode GetResolveMode(ICustomAttributeProvider attributeProvider)
		{
			if (AttributeUtils.IsMarked<CreateAttribute>(attributeProvider))
				return ResolveMode.Create;
			if (AttributeUtils.IsMarked<InjectAttribute>(attributeProvider))
				return ResolveMode.Inject;
			return ResolveMode.Unspecified;
		}

		
		protected object GetDependency(Type type, Context context, ResolveMode resolveMode)
		{
			if (resolveMode == ResolveMode.Create)
			{
				return context.AutocomposeDependency(type, 0);
			}
			return context.Resolve(type);
		}


		protected enum ResolveMode
		{
			Inject,
			Create,
			Unspecified
		}
	}


	public class PropertyInjection : AbstractInjecton, IMemberInjection
	{
		private readonly PropertyInfo _propertyInfo;
		private readonly Type _dependencyType;
		private readonly ResolveMode _resolveMode;

		public PropertyInjection(PropertyInfo info)
		{
			_propertyInfo = info;
			_dependencyType = info.PropertyType;
			_resolveMode = GetResolveMode(_propertyInfo);
		}
		
		public void Inject(object target, Context context)
		{
			object dependency = GetDependency(_dependencyType, context, _resolveMode);

			if (_propertyInfo.GetValue(target, null) != null)
				throw new BehaviourInjectException(String.Format("Property {0} to inject is already contains something!", _propertyInfo.Name));
			_propertyInfo.SetValue(target, dependency, null);
		}
	}

	public class FieldInjection : AbstractInjecton, IMemberInjection
	{
		private readonly FieldInfo _fieldInfo;
		private readonly Type _dependencyType;
		private readonly ResolveMode _resolveMode;

		public FieldInjection(FieldInfo info)
		{
			_fieldInfo = info;
			_dependencyType = info.FieldType;
			_resolveMode = GetResolveMode(_fieldInfo);
		}
		
		public void Inject(object target, Context context)
		{
			object dependency = GetDependency(_dependencyType, context, _resolveMode);

			if (_fieldInfo.GetValue(target) != null)
				throw new BehaviourInjectException(String.Format("Field {0} to inject is already contains something!", _fieldInfo.Name));
			_fieldInfo.SetValue(target, dependency);
		}
	}


	public class MethodInjection : AbstractInjecton, IMemberInjection
	{
		private MethodInfo _methodInfo;

		private Type[] _dependencyTypes;
		private ResolveMode[] _resolveModes;
		private object[] _invokationArguments;

		public MethodInjection(MethodInfo info)
		{
			_methodInfo = info;
			ResolveMode methodResolveMode = GetResolveMode(_methodInfo);
			ParameterInfo[] arguments = _methodInfo.GetParameters();
			int argumentCount = arguments.Length;
			
			_dependencyTypes = new Type[argumentCount];
			_invokationArguments = new object[argumentCount];
			_resolveModes = new ResolveMode[argumentCount];
			
			for (int i = 0; i < argumentCount; i++)
			{
				ParameterInfo argument = arguments[i];
				_dependencyTypes[i] = argument.ParameterType;
				ResolveMode resolveMode = GetResolveMode(argument);
				if (resolveMode == ResolveMode.Unspecified) resolveMode = methodResolveMode;
				_resolveModes[i] = resolveMode;
			}
		}

		public void Inject(object target, Context context)
		{
			for (int i = 0; i < _dependencyTypes.Length; i++)
			{
				Type dependencyType = _dependencyTypes[i];
				ResolveMode resolveMode = _resolveModes[i];
				_invokationArguments[i] = GetDependency(dependencyType, context, resolveMode);
			}

			_methodInfo.Invoke(target, _invokationArguments);
		}
	}
}
