#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using System.IO;

namespace Dsu.Framework
{
    public static class GameplayManagerScriptGenerator
    {
        private const string defaultFileName = "NewGameplayManager";

        [MenuItem("Assets/DsuFramework/Generate GameplayManager Script")]
        private static void CreateGameplayManagerScript()
        {
            string folderPath = GetSelectedPathOrFallback();
            string filePath = Path.Combine(folderPath, defaultFileName + ".cs");

            var action = ScriptableObject.CreateInstance<CreateGameplayManagerScriptAction>();
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

        private class CreateGameplayManagerScriptAction : EndNameEditAction
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

public class {className} : DsuGameplayManagerBase
{{
    void Start()
    {{
        // How to use the {className} in other component
        // step1. declare GameplayEventHandler variable in the component
        //   private event GameplayEventHandler gameplayHandler = null;
        // step2. register the event in the Start method
        //   {className} gameplayManager = FindObjectOfType<{className}>();
        //   gameplayManager?.RegisterEvent(ref gameplayHandler);
    }}

    void Update()
    {{
        // Per-frame update logic here
    }}

    public override void OnGameplayEvent(object sender, GameplayEventArgsBase args)
    {{
        GameObject gameObject = sender as GameObject;
        //if (args.Event == GameplayEvents.CustomGameplayEventFromHere)
        //{{
        //    OnCustomGameplayEvent(gameObject, args);
        //}}
    }}

    //private void OnCustomGameplayEvent(GameObject sender, GameplayEventArgsBase args)
    //{{
    //    CollisionEventArgs collArgs = args as CollisionEventArgs;
    //    Debug.Log($""Custom gameplay event received from: {{sender.name}}"");
    //}}
}}";
            }
        }
    }
}
#endif
