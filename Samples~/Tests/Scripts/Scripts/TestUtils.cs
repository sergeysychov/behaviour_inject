using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BehaviourInject.Test
{
	public static class TestUtils
	{
		[DebuggerStepThrough]
		public static T TestResolve<T>(this Context context) => (T)context.Resolve(typeof(T));
	}
}
