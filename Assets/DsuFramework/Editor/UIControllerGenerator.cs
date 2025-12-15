/// @file   UIControllerGenerator.cs
/// @date   20251215_jintaeks
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using System.IO;

namespace Dsu.Framework
{
    public static class UIControllerGenerator
    {
        private const string defaultClassName = "NewUIController";
        private const string baseClassName = "UIControllerBase";
        private const string namespaceName = "Dsu.Framework";

        [MenuItem("Assets/DsuFramework/Generate UIController Script")]
        private static void CreateUIControllerScript()
        {
            string folderPath = GetSelectedPathOrFallback();
            string filePath = Path.Combine(folderPath, defaultClassName + ".cs");

            var action = ScriptableObject.CreateInstance<CreateUIControllerAssetAction>();
            action.baseClassName = baseClassName;
            action.namespaceName = namespaceName;

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

        private class CreateUIControllerAssetAction : EndNameEditAction
        {
            public string baseClassName;
            public string namespaceName;

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                // Ensure the file name is unique
                pathName = GetUniqueFilePath(pathName);

                string className = Path.GetFileNameWithoutExtension(pathName);
                string script = GenerateScript(className);
                File.WriteAllText(pathName, script);
                AssetDatabase.ImportAsset(pathName);
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(pathName);
                ProjectWindowUtil.ShowCreatedAsset(obj);
            }

            private string GenerateScript(string className)
            {
                return $@"using UnityEngine;
using Dsu.Framework;

public class {className} : {baseClassName}
{{
    protected override void UpdateData(int groupId)
    {{
        // TODO: Implement your UI update logic here
        // read data from your RuntimeGameDataManager
    }}
}}";
            }

            // Generates a unique file name if one already exists
            private string GetUniqueFilePath(string path)
            {
                string directory = Path.GetDirectoryName(path);
                string baseFileName = Path.GetFileNameWithoutExtension(path);
                string extension = Path.GetExtension(path);

                int count = 1;
                string uniquePath = path;

                while (File.Exists(uniquePath)) {
                    uniquePath = Path.Combine(directory, $"{baseFileName} ({count}){extension}");
                    count++;
                }

                return uniquePath;
            }
        }
    }
}
#endif
