#if BINJECT_DIAGNOSTICS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourInject.Diagnostics
{
	internal static class BinjectDiagnostics
	{
		public static bool IsEnabled;
		public static int ContextCount;
		public static int RecipientCount;
		public static int DependenciesCount;
		public static int CommandsCount;
		public static int InjectorsCount;
		public static int CachedTypes;
		public static int CachedInjections;
		public static int CachedEventTargets;
		public static int CachedEventHandlers;


		public static string GetDiagnosticStirng()
		{
			return string.Format(
@"Binject Diagnostics:
	ContextCount {0}
	DependenciesCount {1}
	RecipientCount {2}
	CommandsCount {3}
	InjectorsCount {4}
	CachedTypes {5}
	CachedInjections {6}
	CachedEventTargets {7}
	CachedEventHandlers {8}
",
				BinjectDiagnostics.ContextCount,
				BinjectDiagnostics.DependenciesCount,
				BinjectDiagnostics.RecipientCount,
				BinjectDiagnostics.CommandsCount,
				BinjectDiagnostics.InjectorsCount,
				BinjectDiagnostics.CachedTypes,
				BinjectDiagnostics.CachedInjections,
				BinjectDiagnostics.CachedEventTargets,
				BinjectDiagnostics.CachedEventHandlers
			);
		}
	}
}
#endif
