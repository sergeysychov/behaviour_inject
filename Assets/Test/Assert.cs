using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BehaviourInject.Test
{
	public static class Assert
	{
		public static void True(bool val, string message = "")
		{
			if (val)
			{
				LogGreen("Success " + message);
			}
			else
			{
				LogRed("Fail " + message);
			}
		}


		public static void False(bool val, string message = "")
		{
			True(!val, message);
		}

		public static void NotNull(object val, string message = "")
		{
			True(val != null, message);
		}
		public static void Equals(object val, object val2, string message = "")
		{
			True(val.Equals(val2), message);
		}
		public static void NotEquals(object val, object val2, string message = "")
		{
			True(!val.Equals(val2), message);
		}

		public static void NotReached(string message)
		{
			LogRed("Fail " + message);
		}

		public static void Exception(Exception e, string message = "")
		{
			LogGreen("Seccess got exeption " + e.GetType() + "; " + message);
		}


		private static void LogGreen(string msg)
		{
			LogColored(msg, "green");
		}

		private static void LogRed(string msg)
		{
			LogColored(msg, "red");
		}


		private static void LogColored(string msg, string color)
		{
			Debug.Log("<color=" + color + ">" + msg + "</color>");
		}
	}
}
