using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BehaviourInject.Internal;

namespace BehaviourInject
{
	[DisallowMultipleComponent]
	public class HierarchyContext : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		private int _contextIndex;

		private Context _context;

		public virtual Context GetContext()
		{
			if (_context == null)
			{
				_context = ContextRegistry.GetContext(_contextIndex);
				_context.OnContextDestroyed += OnContextDestroy;
			}

			return _context;
		}


		public virtual void SwitchContext(int index)
		{
			if (_contextIndex != index)
			{
				if (_context != null)
				{
					_context.OnContextDestroyed -= OnContextDestroy;
					_context = null;
				}
				_contextIndex = index;
				_context = ContextRegistry.GetContext(_contextIndex);
				_context.OnContextDestroyed += OnContextDestroy;
			}
		}

		public virtual void SwitchContext(string contextName)
		{
			Settings settings = Settings.Load();
			int index = settings.FindIndexOf(contextName);
			SwitchContext(index);
		}


		private void OnContextDestroy()
		{
			_context.OnContextDestroyed -= OnContextDestroy;
			Destroy(gameObject);
		}


		private void OnDestroy()
		{
			if(_context != null)
				_context.OnContextDestroyed -= OnContextDestroy;
		}
	}
}
