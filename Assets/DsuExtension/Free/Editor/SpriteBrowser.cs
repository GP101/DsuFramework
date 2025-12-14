#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.U2D;

namespace Dsu.Extension
{
    public class SpriteBrowser : EditorWindow
    {
        private Vector2 scrollPos;
        private Dictionary<string, List<Texture2D>> folderToSprites = new();
        private Dictionary<string, bool> foldoutStates = new();
        private bool riggedOnly = false;

        private string searchQuery = ""; // 검색어 입력

        private string[] asyncGuids;
        private int asyncIndex = 0;
        private bool isSearching = false;

        [MenuItem("Tools/Sprite Browser")]
        public static void ShowWindow()
        {
            GetWindow<SpriteBrowser>("Sprite Browser");
        }

        private void OnGUI()
        {
            GUILayout.Space(4);

            if (!isSearching) {
                if (GUILayout.Button("Browse Sprite Assets", GUILayout.Height(22))) {
                    StartSpriteSearch();
                }

                // 검색창 + 버튼
                GUILayout.BeginHorizontal();
                searchQuery = GUILayout.TextField(searchQuery);
                if (GUILayout.Button("Search", GUILayout.Width(80))) {
                    Repaint(); // 검색 반영
                }
                GUILayout.EndHorizontal();
            }
            else {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Searching sprites... ({asyncIndex}/{(asyncGuids?.Length ?? 0)})", GUILayout.Height(22));
                if (GUILayout.Button("Cancel Search", GUILayout.Width(110), GUILayout.Height(22))) {
                    CancelSpriteSearch();
                }
                GUILayout.EndHorizontal();
            }

            // 2D Rigged Only 토글
            riggedOnly = EditorGUILayout.Toggle("2D Rigged Only", riggedOnly);

            GUILayout.Space(10);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            if (folderToSprites.Count == 0) {
                GUILayout.Label("No sprites found.");
            }
            else {
                foreach (var kvp in folderToSprites) {
                    string folder = kvp.Key;

                    // 이름 + Rigged 여부 필터
                    List<Texture2D> filtered = kvp.Value
                        .Where(tex =>
                        {
                            if (!string.IsNullOrEmpty(searchQuery) &&
                                !tex.name.ToLower().Contains(searchQuery.ToLower()))
                                return false;

                            if (riggedOnly) {
                                string path = AssetDatabase.GetAssetPath(tex);
                                var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>();
                                bool isRigged = sprites.Any(sprite =>
                                    sprite != null &&
                                    sprite.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.BlendWeight));

                                if (!isRigged)
                                    return false;
                            }

                            return true;
                        })
                        .ToList();

                    if (filtered.Count == 0)
                        continue;

                    if (!foldoutStates.ContainsKey(folder))
                        foldoutStates[folder] = false;

                    foldoutStates[folder] = EditorGUILayout.Foldout(foldoutStates[folder], $"{folder} ({filtered.Count})", true);

                    if (foldoutStates[folder]) {
                        EditorGUI.indentLevel++;
                        foreach (var tex in filtered) {
                            DrawSpriteInfo(tex);
                        }
                        EditorGUI.indentLevel--;
                        GUILayout.Space(5);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }


        private void StartSpriteSearch()
        {
            folderToSprites.Clear();
            foldoutStates.Clear();

            asyncGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
            asyncIndex = 0;
            isSearching = true;

            EditorApplication.update += AsyncSpriteSearchStep;
        }

        private void CancelSpriteSearch()
        {
            EditorApplication.update -= AsyncSpriteSearchStep;
            isSearching = false;
            asyncGuids = null;
            asyncIndex = 0;
            Repaint();
        }

        private void AsyncSpriteSearchStep()
        {
            int stepsPerFrame = 20;
            int processed = 0;

            while (asyncIndex < asyncGuids.Length && processed < stepsPerFrame) {
                string guid = asyncGuids[asyncIndex++];
                processed++;

                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.textureType != TextureImporterType.Sprite)
                    continue;

                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null)
                    continue;

                bool isRigged = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<Sprite>()
                    .Any(sprite => sprite != null && sprite.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.BlendWeight));

                if (riggedOnly && !isRigged)
                    continue;

                string folder = Path.GetDirectoryName(path).Replace("\\", "/");
                if (!folderToSprites.ContainsKey(folder))
                    folderToSprites[folder] = new List<Texture2D>();

                folderToSprites[folder].Add(tex);
            }

            if (asyncIndex >= asyncGuids.Length) {
                CancelSpriteSearch();
            }

            Repaint();
        }

        private void DrawSpriteInfo(Texture2D tex)
        {
            if (tex == null) return;

            EditorGUILayout.BeginVertical("box");

            // 텍스처 이름 출력 (가장 위)
            GUILayout.Label("Name: " + tex.name, EditorStyles.boldLabel);

            EditorGUILayout.ObjectField("Texture", tex, typeof(Texture2D), false);

            string path = AssetDatabase.GetAssetPath(tex);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null) {
                GUILayout.Label("Texture Type: " + importer.textureType);
                GUILayout.Label("Sprite Mode: " + importer.spriteImportMode);
                GUILayout.Label("Pixels Per Unit: " + importer.spritePixelsPerUnit.ToString("F2"));

                var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>();
                bool hasRigging = sprites.Any(sprite =>
                    sprite != null &&
                    sprite.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.BlendWeight));

                if (hasRigging)
                    DrawHighlightedLabel("2D Rigged: Yes");
                else
                    GUILayout.Label("2D Rigged: No");
            }

            float memorySizeKB = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex) / 1024f;
            GUILayout.Label("Memory Size: " + memorySizeKB.ToString("F1") + " KB");

            EditorGUILayout.EndVertical();
        }


        private void DrawHighlightedLabel(string text)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.DrawRect(rect, new Color(0.25f, 0.5f, 0.85f, 0.3f));
            EditorGUI.LabelField(rect, text);
        }
    }
}
#endif
