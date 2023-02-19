using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourInject.Test
{
	public static class TestUtils
	{
		public static T TestResolve<T>(this Context context)
		{
			return (T)context.Resolve(typeof(T));
		}
	}
}
