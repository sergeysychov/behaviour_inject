using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourInject.Diagnostics
{
	public static class BinjectDiagnostics
	{
		public static bool IsEnabled;
		public static int ContextCount;
		public static int RecipientCount;
		public static int DependenciesCount;
		public static int CommandsCount;
		public static int InjectorsCount;
		public static int CachedTypes;
		public static int CachedInjections;
		public static int CachedEventHandlers;
		public static int CachedEventTargets;
	}
}
