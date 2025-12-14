#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;

namespace Dsu.Extension
{
    public static class WhatUsesThis
    {
        const string CacheFilename = "Temp/WhatUsesThis.bin";
        static Dictionary<string, List<string>> _dict;
        public static Dictionary<string, List<string>> Dict => _dict ?? Load() ?? new Dictionary<string, List<string>>();

        public static List<string> results = new List<string>();
        public static List<string> results2 = new List<string>();

        public static void StartCleanBuildAsync()
        {
            _progressStep = 0;
            _progressState = BuildState.Initializing;
            _updateHandler = CleanBuildAsync;
            EditorApplication.update += _updateHandler;
        }

        private enum BuildState { Idle, Initializing, Collecting, Building, Saving, Done }
        private static BuildState _progressState = BuildState.Idle;
        private static int _progressStep = 0;
        private static Dictionary<string, string[]> _tempDependencies;
        private static IEnumerator<string> _assetEnumerator;
        private static EditorApplication.CallbackFunction _updateHandler;

        private static void CleanBuildAsync()
        {
            switch (_progressState) {
            case BuildState.Initializing:
                EditorUtility.DisplayProgressBar("Rebuilding...", "Finding all assets", 0.1f);
                var allAssets = AssetDatabase.FindAssets("").Select(AssetDatabase.GUIDToAssetPath).Distinct().ToArray();
                _tempDependencies = new Dictionary<string, string[]>();
                _assetEnumerator = allAssets.AsEnumerable().GetEnumerator();
                _progressStep = allAssets.Length;
                _progressState = BuildState.Collecting;
                break;

            case BuildState.Collecting:
                for (int i = 0; i < 20 && _assetEnumerator.MoveNext(); i++) {
                    string asset = _assetEnumerator.Current;
                    _tempDependencies[asset] = AssetDatabase.GetDependencies(asset, false);
                }
                float progress = 0.2f + (0.6f * (_tempDependencies.Count / (float)_progressStep));
                EditorUtility.DisplayProgressBar("Rebuilding...", $"Getting dependencies... [{_tempDependencies.Count}/{_progressStep}]", progress);

                if (_tempDependencies.Count >= _progressStep) {
                    _progressState = BuildState.Building;
                }
                break;

            case BuildState.Building:
                EditorUtility.DisplayProgressBar("Rebuilding...", "Building reverse map", 0.9f);
                _dict = new Dictionary<string, List<string>>();
                foreach (var d in _tempDependencies) {
                    foreach (var dep in d.Value) {
                        if (!_dict.TryGetValue(dep, out var list)) {
                            list = new List<string>();
                            _dict[dep] = list;
                        }
                        list.Add(d.Key);
                    }
                }
                _progressState = BuildState.Saving;
                break;

            case BuildState.Saving:
                Save();
                _progressState = BuildState.Done;
                break;

            case BuildState.Done:
                EditorUtility.ClearProgressBar();
                EditorApplication.update -= _updateHandler;
                _progressState = BuildState.Idle;
                break;
            }
        }

        static void Save()
        {
            if (_dict == null) return;
            using (var stream = new FileStream(CacheFilename, FileMode.Create)) {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, _dict);
            }
        }

        static Dictionary<string, List<string>> Load()
        {
            try {
                using (var stream = new FileStream(CacheFilename, FileMode.Open)) {
                    BinaryFormatter bin = new BinaryFormatter();
                    _dict = (Dictionary<string, List<string>>)bin.Deserialize(stream);
                }
            }
            catch {
                _dict = null;
            }
            return _dict;
        }

        [MenuItem("Assets/Dsu/This is used by")]
        private static void FindParentAssets()
        {
            var selectedObj = Selection.activeObject;
            if (selectedObj != null) {
                ThisIsUsedByWindow.OpenWithAsset(selectedObj);
            }
        }

        [MenuItem("Assets/Dsu/This uses")]
        private static void FindChildAssets()
        {
            results2.Clear();
            var selectedObj = Selection.activeObject;
            if (selectedObj != null) {
                string selected = AssetDatabase.GetAssetPath(selectedObj);
                results2.Add($"[{selected}] uses");

                foreach (var d in AssetDatabase.GetDependencies(selected, false)) {
                    results2.Add($"  {d}");
                }

                ThisUsesWindow.OpenWithAsset(selectedObj);
            }
        }
    }

    public class ThisIsUsedByWindow : UnityEditor.EditorWindow
    {
        private Vector2 scrollPosition;
        private int selectedIndex = -1;
        private Object selectedAsset;

        [MenuItem("Tools/This is used by")]
        public static void OpenWindow()
        {
            var window = GetWindow<ThisIsUsedByWindow>("This is used by");
            window.Show();
        }

        public static void OpenWithAsset(Object asset)
        {
            var window = GetWindow<ThisIsUsedByWindow>("This is used by");
            window.selectedAsset = asset;
            window.RefreshUsedByList(asset);
            window.Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            // Rebuild 버튼 추가
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Rebuild Info", GUILayout.Width(100))) {
                WhatUsesThis.StartCleanBuildAsync();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            selectedAsset = EditorGUILayout.ObjectField("Drop asset here", selectedAsset, typeof(Object), false);
            if (EditorGUI.EndChangeCheck()) {
                RefreshUsedByList(selectedAsset);
            }

            if (GUILayout.Button("Clear", GUILayout.Width(60))) {
                WhatUsesThis.results.Clear();
                selectedAsset = null;
                selectedIndex = -1;
            }
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < WhatUsesThis.results.Count; i++) {
                string result = WhatUsesThis.results[i];
                string path = result.Trim().Replace("  ", "");
                GUIStyle style = new GUIStyle(EditorStyles.objectField);
                if (i == 0) style = new GUIStyle(EditorStyles.largeLabel);

                Object asset = null;
                Texture icon = null;
                if (i > 0) {
                    asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                    if (asset != null)
                        icon = AssetPreview.GetMiniThumbnail(asset);
                }

                GUILayout.BeginHorizontal();
                if (icon != null)
                    GUILayout.Label(icon, GUILayout.Width(18), GUILayout.Height(18));
                else if (i > 0)
                    GUILayout.Space(18);

                if (GUILayout.Button(result, style)) {
                    selectedIndex = i;
                    if (asset != null) {
                        EditorGUIUtility.PingObject(asset);
                        Selection.activeObject = asset;
                    }
                }

                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void RefreshUsedByList(Object assetObj)
        {
            if (assetObj == null) return;

            string selected = AssetDatabase.GetAssetPath(assetObj);
            WhatUsesThis.results.Clear();
            WhatUsesThis.results.Add($"[{selected}] is used by");

            if (WhatUsesThis.Dict.TryGetValue(selected, out var dependants)) {
                foreach (var d in dependants) {
                    WhatUsesThis.results.Add($"  {d}");
                }
            }

            selectedIndex = -1;
        }
    }

    public class ThisUsesWindow : UnityEditor.EditorWindow
    {
        private Vector2 scrollPosition;
        private int selectedIndex = -1;
        private Object selectedAsset;

        [MenuItem("Tools/This uses")]
        public static void OpenWindow()
        {
            var window = GetWindow<ThisUsesWindow>("This uses");
            window.Show();
        }

        public static void OpenWithAsset(Object asset)
        {
            var window = GetWindow<ThisUsesWindow>("This uses");
            window.selectedAsset = asset;
            window.RefreshUsesList(asset);
            window.Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            selectedAsset = EditorGUILayout.ObjectField("Drop asset here", selectedAsset, typeof(Object), false);
            if (EditorGUI.EndChangeCheck()) {
                RefreshUsesList(selectedAsset);
            }

            if (GUILayout.Button("Clear", GUILayout.Width(60))) {
                WhatUsesThis.results2.Clear();
                selectedAsset = null;
                selectedIndex = -1;
            }
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < WhatUsesThis.results2.Count; i++) {
                string result = WhatUsesThis.results2[i];
                string path = result.Trim().Replace("  ", "");
                GUIStyle style = new GUIStyle(EditorStyles.objectField);
                if (i == 0) style = new GUIStyle(EditorStyles.largeLabel);

                Object asset = null;
                Texture icon = null;
                if (i > 0) {
                    asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                    if (asset != null)
                        icon = AssetPreview.GetMiniThumbnail(asset);
                }

                GUILayout.BeginHorizontal();
                if (icon != null)
                    GUILayout.Label(icon, GUILayout.Width(16), GUILayout.Height(16));
                else if (i > 0)
                    GUILayout.Space(18);

                if (GUILayout.Button(result, style)) {
                    selectedIndex = i;
                    if (asset != null) {
                        EditorGUIUtility.PingObject(asset);
                        Selection.activeObject = asset;
                    }
                }

                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void RefreshUsesList(Object assetObj)
        {
            if (assetObj == null) return;

            string selected = AssetDatabase.GetAssetPath(assetObj);
            WhatUsesThis.results2.Clear();
            WhatUsesThis.results2.Add($"[{selected}] uses");

            foreach (var d in AssetDatabase.GetDependencies(selected, false)) {
                WhatUsesThis.results2.Add($"  {d}");
            }

            selectedIndex = -1;
        }
    }
}
#endif