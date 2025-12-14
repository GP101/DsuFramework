#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using System.IO;

namespace Dsu.Framework
{
    public static class RuntimeGameDataManagerGenerator
    {
        private const string defaultClassName = "NewRuntimeGameDataManager";
        private const string baseClassName = "RuntimeGameDataManagerBase";
        private const string namespaceName = "Dsu.Framework";

        [MenuItem("Assets/DsuFramework/Generate RuntimeGameDataManager Script")]
        private static void CreateRuntimeGameDataManagerScript()
        {
            string folderPath = GetSelectedPathOrFallback();
            string filePath = Path.Combine(folderPath, defaultClassName + ".cs");
            string absolutePath = Path.GetFullPath(filePath);

            var scriptAsset = ScriptableObject.CreateInstance<CreateScriptAssetAction>();
            scriptAsset.className = defaultClassName;

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                scriptAsset,
                filePath,
                EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D,
                null
            );
        }

        private static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets)) {
                path = AssetDatabase.GetAssetPath(obj);
                if (File.Exists(path))
                    path = Path.GetDirectoryName(path);
                break;
            }
            return path;
        }

        private class CreateScriptAssetAction : EndNameEditAction
        {
            public string className;

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                string fileName = Path.GetFileNameWithoutExtension(pathName);
                string scriptContent = GenerateScript(fileName); // Use entered name
                File.WriteAllText(pathName, scriptContent);
                AssetDatabase.ImportAsset(pathName);
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(pathName);
                ProjectWindowUtil.ShowCreatedAsset(obj);
            }

            private string GenerateScript(string name)
            {
                return $@"using UnityEngine;
using Dsu.Framework;

public class {name} : {baseClassName}
{{
    static public {name} instance = null;
    // TODO: Add your custom fields and methods here
    //private static int count = 0;
    //public static int Count
    //{{
    //    get {{ return count; }}
    //    set {{ _UpdateDataStamp(); count = value; }}
    //}}
    void Awake()
    {{
        if (instance == null) {{
            instance = this;
        }}
        // Initialize your variables here
        //count = 0;
    }}
}}";
            }
        }
    }
}
#endif
