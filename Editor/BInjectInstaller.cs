using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace BehaviourInject.Internal
{
    public class BInjectInstaller
    {
        [DidReloadScripts]
        private static void CreateAssetWhenReady()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += CreateAssetWhenReady;
                return;
            }
 
            EditorApplication.delayCall += CreateAssetNow;
        }

        private static void CreateAssetNow()
        {
            Settings settings = Resources.Load<Settings>(Settings.SETTINGS_PATH);
            if (settings == null)
            {
                string resourcesParentFolder = "Assets";
                string folderPath = resourcesParentFolder + "/Resources";
                Debug.Log("[Behaviour Inject]: No settings found. Create from template at " + folderPath);
                string templateName = Settings.SETTINGS_PATH + "Template";
                Settings templateSettings = Resources.Load<Settings>(templateName);
                if (templateSettings == null)
                {
                    Debug.LogError("Can not find settings template. Original package folder is probably corrupted.");
                    return;
                }

                string path = AssetDatabase.GetAssetPath(templateSettings);
                if(!AssetDatabase.IsValidFolder(folderPath))
                    AssetDatabase.CreateFolder(resourcesParentFolder, "Resources");
                AssetDatabase.CopyAsset(path, folderPath + "/" + Settings.SETTINGS_PATH + ".asset");
                AssetDatabase.Refresh();
            }
        }
    }
}