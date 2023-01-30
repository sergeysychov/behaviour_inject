using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourInject.Test
{
	public class TestHierarchyContext : HierarchyContext
	{
		private Context _context;

		public override Context GetContext()
		{
			if (_context == null)
			{
				_context = Context.CreateLocal()
					.SetParentContext("test")
					.RegisterDependency(new PrecomposeDependency("local"));
			}
			return _context;
		}
	}
}
