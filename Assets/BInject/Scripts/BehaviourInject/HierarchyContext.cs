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
		private int _context;

		public virtual Context GetContext()
		{
			return ContextRegistry.GetContext(_context);
		}
	}
}
