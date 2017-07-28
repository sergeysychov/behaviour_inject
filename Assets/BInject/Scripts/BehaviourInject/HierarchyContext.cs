using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BehaviourInject.Internal;

namespace BehaviourInject
{
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
