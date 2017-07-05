using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace BehaviourInject.Internal
{
	public class ChooseContextDrawerAttribute : PropertyAttribute
	{ }


	[CustomEditor(typeof(Injector))]
	public class ChooseContextProperyDrawer : Editor
	{
		private Settings _settings;
		//private Injector _target;
		private bool _isDropped;
		private int[] _optiopnsIndexes = null;
		private List<string> _contexts;

		private SerializedProperty _choosenContext;

		void OnEnable()
		{
			_settings = Settings.Load();
			_contexts = new List<string>(_settings.ContextNames);
			//_target = (Injector)target;
			_choosenContext = serializedObject.FindProperty("_contextIndex"); ;
		}


		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			string[] contextNames = _settings.ContextNames;
			int contextIndex = _choosenContext.intValue;
			if (contextIndex >= contextNames.Length)
			{
				contextIndex = 0;
				_isDropped = true;
			}

			int index = EditorGUILayout.IntPopup("Context", contextIndex, contextNames, GetOptionValues(_settings));
			_choosenContext.intValue = index;

			if (_isDropped)
			{
				EditorGUILayout.HelpBox("Context index exceeded. Returned to " + contextNames[0], MessageType.Warning);
			}

			serializedObject.ApplyModifiedProperties();
		}


		private int FindIndexOf(string contextName, Settings settings)
		{
			string[] options = settings.ContextNames;
			for (int i = 0; i < options.Length; i++)
				if (options[i] == contextName)
					return i;
			return -1;
		}


		private int[] GetOptionValues(Settings settings)
		{
			string[] options = settings.ContextNames;
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
