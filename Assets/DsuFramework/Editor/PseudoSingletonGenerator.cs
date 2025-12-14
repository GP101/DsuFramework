#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using System.IO;

namespace Dsu.Framework
{
    public static class PseudoSingletonGenerator
    {
        private const string defaultClassName = "NewPseudoSingleton";
        private const string menuPath = "Assets/DsuFramework/Generate PseudoSingleton Script";

        [MenuItem(menuPath)]
        private static void CreateScript()
        {
            string folderPath = GetSelectedPathOrFallback();
            string filePath = Path.Combine(folderPath, defaultClassName + ".cs");

            var action = ScriptableObject.CreateInstance<CreatePseudoSingletonAssetAction>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                action,
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

        private class CreatePseudoSingletonAssetAction : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                string className = Path.GetFileNameWithoutExtension(pathName);
                string script = GenerateScript(className);

                if (File.Exists(pathName)) {
                    Debug.LogWarning("File already exists: " + pathName);
                    return;
                }

                File.WriteAllText(pathName, script);
                AssetDatabase.ImportAsset(pathName);
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(pathName);
                ProjectWindowUtil.ShowCreatedAsset(obj);
            }

            private string GenerateScript(string className)
            {
                return $@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class {className} : MonoBehaviour
{{
    // all data members and methods are static
    // Create an empty game object and attach this script for every scene
    // We don't need to use like {className}.Instance.myIntData
    // We can use like {className}.myIntData
    static private int myIntData = 0;
    static public string myStringData = ""Hello World"";

    void Start()
    {{
        // Because this is a component of a game object, we can initialize the data here
        int i = {className}.GetIntDate();
    }}

    public static int GetIntDate()
    {{
        return myIntData;
    }}

    public static void SetIntDate(int value)
    {{
        myIntData = value;
    }}
}}";
            }
        }
    }
}
#endif
