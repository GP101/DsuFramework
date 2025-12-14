#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using System.IO;

namespace Dsu.Framework
{
    public static class GameplayEventScriptGenerator
    {
        private const string defaultFileName = "NewGameplayEvents";

        [MenuItem("Assets/DsuFramework/Generate GameplayEvent Script")]
        private static void CreateGameplayEventScript()
        {
            string folderPath = GetSelectedPathOrFallback();
            string filePath = Path.Combine(folderPath, defaultFileName + ".cs");

            var action = ScriptableObject.CreateInstance<CreateGameplayEventScriptAction>();
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

        private class CreateGameplayEventScriptAction : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                string className = Path.GetFileNameWithoutExtension(pathName);
                string scriptContent = GenerateScript(className);
                File.WriteAllText(pathName, scriptContent);
                AssetDatabase.ImportAsset(pathName);
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(pathName);
                ProjectWindowUtil.ShowCreatedAsset(asset);
            }

            private string GenerateScript(string className)
            {
                return $@"using UnityEngine;
using Dsu.Framework;

public static class {className}
{{
    public enum EventId
    {{
        NullGameplayEvent = 0,
        CustomGameplayEventFromHere,
    }}

    public static readonly GameplayEvent CustomGameplayEventFromHere = new GameplayEvent(EventId.CustomGameplayEventFromHere);
}}";
            }
        }
    }
}
#endif
