using UnityEditor;

namespace BehaviourInject.Internal
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(HierarchyContext))]
    public class HierarchyContextProperyDrawer : Editor
    {
        private Settings _settings;
        private bool _isDropped;

        private SerializedProperty _choosenContext;

        void OnEnable()
        {
            _settings = Settings.Load();
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
}