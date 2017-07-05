using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BehaviourInject.Internal
{

	[CreateAssetMenu(fileName = "BInjectSettings", menuName = "Create BInSettings")]
	public class Settings : ScriptableObject {

		private const string SETTINGS_PATH = "BInjectSettings";

		private static Settings _instance;

		public static string[] GetContextNames()
		{
			if(_instance == null)
			{
				_instance = Load();
			}
			return _instance.ContextNames;
		}


		public static Settings Load()
		{
			Settings settings = Resources.Load<Settings>(SETTINGS_PATH);
			return settings;
		}

		public string[] ContextNames = { Context.DEFAULT };
	}
}