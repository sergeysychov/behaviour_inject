using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if BINJECT_DIAGNOSTICS
using BehaviourInject.Diagnostics;
#endif

namespace BehaviourInject.Test
{
	public class DiagnosticsBehaviour : MonoBehaviour
	{

		IEnumerator Start()
		{
			yield return new WaitForSeconds(3f);
#if BINJECT_DIAGNOSTICS
		Debug.Log(BinjectDiagnostics.GetDiagnosticStirng());
#endif
		}
	}
}