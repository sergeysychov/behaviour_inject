using System;
using System.Reflection;

namespace BehaviourInject.Internal
{
	public interface IBehaviourInjection
	{
		void Inject(object target, object val);
		Type DependencyType { get; }
	}


	public class PropertyInjection : IBehaviourInjection
	{
		private PropertyInfo _propertyInfo;

		public PropertyInjection(PropertyInfo info)
		{
			_propertyInfo = info;
		}

		public Type DependencyType { get { return _propertyInfo.PropertyType; } }

		public void Inject(object target, object val)
		{
			if (_propertyInfo.GetValue(target, null) != null)
				throw new BehaviourInjectException(String.Format("Property {0} to inject is already contains something!", _propertyInfo.Name));
			_propertyInfo.SetValue(target, val, null);
		}
	}

	public class FieldInjection : IBehaviourInjection
	{
		private FieldInfo _fieldInfo;

		public FieldInjection(FieldInfo info)
		{
			_fieldInfo = info;
		}

		public Type DependencyType { get { return _fieldInfo.FieldType; } }

		public void Inject(object target, object val)
		{
			if(_fieldInfo.GetValue(target) != null)
				throw new BehaviourInjectException(String.Format("Field {0} to inject is already contains something!", _fieldInfo.Name));

			_fieldInfo.SetValue(target, val);
		}
	}
}
