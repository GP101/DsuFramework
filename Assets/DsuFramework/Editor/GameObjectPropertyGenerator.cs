#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using System.IO;

namespace Dsu.Framework
{
    public static class GameObjectPropertyGenerator
    {
        private const string defaultClassName = "NewGameObjectProperty";

        [MenuItem("Assets/DsuFramework/Generate GameObjectProperty Script")]
        private static void CreateGameObjectPropertyScript()
        {
            string folderPath = GetSelectedPathOrFallback();
            string filePath = Path.Combine(folderPath, defaultClassName + ".cs");

            var action = ScriptableObject.CreateInstance<CreateGameObjectPropertyAssetAction>();

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

        private class CreateGameObjectPropertyAssetAction : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                string className = Path.GetFileNameWithoutExtension(pathName);
                //string dataClassName = className + "Data";
                string script = GenerateScript(className);
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
using Dsu.Framework;

namespace Dsu.Framework
{{
    public partial class GameObjectPropertyData
    {{
        //public int customProperty;

        //public override void Reset()
        //{{
        //    isMovable = false;
        //    customProperty = 0;
        //}}
    }}

    public static partial class DsuGameObjectExtensions
    {{
        //public static int CustomProperty(this GameObject go)
        //{{
        //    _AddKey(go);
        //    return _objectDictionary[go.transform].propertyData.customProperty;
        //}}
    }}
}}";
            }
        }
    }
}
#endif
