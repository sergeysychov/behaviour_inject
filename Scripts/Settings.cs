using UnityEngine;

namespace BehaviourInject.Internal
{

	[CreateAssetMenu(fileName = "BInjectSettings", menuName = "BInject/Create Settings")]
	public class Settings : ScriptableObject {

		public const string SETTINGS_PATH = "BInjectSettings";

		private static Settings _instance;

		[Tooltip("List of contexts")]
		[SerializeField]
		public string[] ContextNames = { };

		[Tooltip("If member or monobeh type has FullName that begins like that, it will be ignored by injectors and events. Use it to optimize injection time.")]
		[SerializeField]
		public string[] ExcludedNames = { "System", "UnityEngine" };

		private int[] _optiopnsIndexes;

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


		public int FindIndexOf(string contextName)
		{
			string[] options = ContextNames;
			for (int i = 0; i < options.Length; i++)
				if (options[i] == contextName)
					return i;
			return -1;
		}


		public int[] GetOptionValues()
		{
			string[] options = ContextNames;
			int length = options.Length;
			if (_optiopnsIndexes == null || length != _optiopnsIndexes.Length)
			{
				_optiopnsIndexes = new int[length];
				for (int i = 0; i < length; i++)
					_optiopnsIndexes[i] = i;
			}
			return _optiopnsIndexes;
		}
	}
}
