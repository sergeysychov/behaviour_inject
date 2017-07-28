using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BehaviourInject.Internal
{
#if UNITY_EDITOR
	[CustomEditor(typeof(Injector))]
	public class ChooseContextProperyDrawer : Editor
	{
		private Settings _settings;
		//private Injector _target;
		private bool _isDropped;
		private int[] _optiopnsIndexes = null;
		private List<string> _contexts;

		private SerializedProperty _choosenContext;
		private SerializedProperty _useHierarchy;

		void OnEnable()
		{
			_settings = Settings.Load();
			_contexts = new List<string>(_settings.ContextNames);
			//_target = (Injector)target;
			_choosenContext = serializedObject.FindProperty("_contextIndex"); ;
			_useHierarchy = serializedObject.FindProperty("_useHierarchy"); ;
		}


		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			bool useHierarchy = _useHierarchy.boolValue;
			_useHierarchy.boolValue = EditorGUILayout.Toggle("Use hierarchy", useHierarchy);

			if (!useHierarchy)
			{
				string[] contextNames = _settings.ContextNames;
				int contextIndex = _choosenContext.intValue;
				if (contextIndex >= contextNames.Length)
				{
					contextIndex = 0;
					_isDropped = true;
				}

				int index = EditorGUILayout.IntPopup("Context", contextIndex, contextNames, _settings.GetOptionValues());
				_choosenContext.intValue = index;

				if (_isDropped)
				{
					EditorGUILayout.HelpBox("Context index exceeded. Returned to " + contextNames[0], MessageType.Warning);
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}

	[CustomEditor(typeof(HierarchyContext))]
	public class HierarchyContextProperyDrawer : Editor
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
			_choosenContext = serializedObject.FindProperty("_contextIndex");
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

			int index = EditorGUILayout.IntPopup("Context", contextIndex, contextNames, _settings.GetOptionValues());
			_choosenContext.intValue = index;

			if (_isDropped)
			{
				EditorGUILayout.HelpBox("Context index exceeded. Returned to " + contextNames[0], MessageType.Warning);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
#endif
}
