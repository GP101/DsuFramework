#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dsu.Extension
{
    // Based on bjorn.slettemark@outlook.com
    public class AssetBookmarksEditor : UnityEditor.EditorWindow
    {
        private static List<UnityEngine.Object> bookmarkedObjects = new List<UnityEngine.Object>();
        private UnityEngine.Object selectedObject;
        private enum SortType { Type, Name, TimeAdded, Asset }
        private SortType currentSortType = SortType.Name;
        private bool sortAscending = true;
        private GUIStyle headerStyle;
        private Vector2 scrollPosition;

        private const string SaveFileName = "AssetBookmarks.json";
        private const string FolderName = "DsuExtension";

        private float mouseDownTime;
        private Vector2 mouseDownPosition;
        private const float dragThreshold = 0.5f;
        private const float dragDistance = 5f;

        [System.Serializable]
        private class BookmarkData
        {
            public List<string> assetPaths = new List<string>();
        }

        [MenuItem("Assets/Dsu/Add to Bookmarks", false, 50)]
        public static void AddToBookmarks()
        {
            UnityEngine.Object[] newObjects = Selection.objects;
            foreach (var newObject in newObjects) {
                if (newObject != null && !bookmarkedObjects.Contains(newObject)) {
                    bookmarkedObjects.Add(newObject);
                }
            }
            SaveBookmarks();
        }

        [MenuItem("Tools/Dsu Tools/Asset Bookmarks")]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetBookmarksEditor>("Asset Bookmarks");
            window.Show();
            LoadBookmarks();
        }

        private void OnEnable()
        {
            LoadBookmarks();
        }

        private void OnDisable()
        {
            SaveBookmarks();
        }

        private void OnGUI()
        {
            if (headerStyle == null)
                headerStyle = new GUIStyle(EditorStyles.label) { fontSize = 12 };

            Event evt = Event.current;
            HandleExternalDragAndDrop(evt);

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            DrawSortableHeader("Name", SortType.Name);
            DrawSortableHeader("Asset", SortType.Asset);
            DrawSortableHeader("Type", SortType.Type);
            DrawSortableHeader("Date", SortType.TimeAdded);

            if (GUILayout.Button("Clear All Bookmarks", EditorStyles.toolbarButton, GUILayout.Width(150))) {
                bookmarkedObjects.Clear();
                SaveBookmarks();
            }
            GUILayout.EndHorizontal();

            RenderBookmarksList(evt);
        }

        private void HandleExternalDragAndDrop(Event evt)
        {
            Rect dropArea = new Rect(0, 0, position.width, position.height);
            if (dropArea.Contains(evt.mousePosition)) {
                if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform) {
                        DragAndDrop.AcceptDrag();
                        foreach (var draggedObject in DragAndDrop.objectReferences) {
                            if (!bookmarkedObjects.Contains(draggedObject)) {
                                bookmarkedObjects.Add(draggedObject);
                            }
                        }
                        SaveBookmarks();
                        evt.Use();
                    }
                }
            }
        }

        private void RenderBookmarksList(Event evt)
        {
            bookmarkedObjects.RemoveAll(item => item == null);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
            UnityEngine.Object objectToRemove = null;

            for (int i = 0; i < bookmarkedObjects.Count; i++) {
                var obj = bookmarkedObjects[i];
                if (obj == null) continue;

                GUILayout.BeginHorizontal();
                Rect entireRowRect = GUILayoutUtility.GetRect(0, 18, GUILayout.ExpandWidth(true));

                if (obj == selectedObject) {
                    EditorGUI.DrawRect(entireRowRect, new Color(0.25f, 0.5f, 0.85f, 0.3f));
                }

                Rect iconRect = new Rect(entireRowRect.x, entireRowRect.y, 16, 16);
                Rect labelRect = new Rect(iconRect.xMax, entireRowRect.y, entireRowRect.width - 40, 16);
                Rect removeButtonRect = new Rect(labelRect.xMax, entireRowRect.y, 18, 18);

                Texture icon = AssetPreview.GetMiniThumbnail(obj);
                GUI.DrawTexture(iconRect, icon);

                string assetPath = AssetDatabase.GetAssetPath(obj);
                string objectName = GetObjectNameWithParentFolder(obj);
                GUI.Label(labelRect, new GUIContent(objectName, assetPath));

                if (GUI.Button(removeButtonRect, "x")) {
                    objectToRemove = obj;
                }

                HandleAssetInteractions(evt, obj, objectName, entireRowRect, i);

                GUILayout.EndHorizontal();
            }

            if (objectToRemove != null) {
                bookmarkedObjects.Remove(objectToRemove);
                SaveBookmarks();
            }

            GUILayout.EndScrollView();

            if (evt.type == EventType.MouseUp || evt.type == EventType.MouseLeaveWindow) {
                mouseDownTime = 0;
                mouseDownPosition = Vector2.zero;
            }
        }

        private void HandleAssetInteractions(Event evt, UnityEngine.Object obj, string objectName, Rect itemRect, int index)
        {
            switch (evt.type) {
            case EventType.MouseDown:
                if (itemRect.Contains(evt.mousePosition) && evt.button == 0) {
                    mouseDownTime = Time.realtimeSinceStartup;
                    mouseDownPosition = evt.mousePosition;
                    evt.Use();
                }
                break;

            case EventType.MouseUp:
                if (itemRect.Contains(evt.mousePosition) && evt.button == 0) {
                    float mouseUpTime = Time.realtimeSinceStartup;
                    if (mouseUpTime - mouseDownTime < dragThreshold) {
                        selectedObject = obj;
                        EditorGUIUtility.PingObject(obj);
                        Selection.activeObject = obj;
                    }
                    evt.Use();
                }
                break;

            case EventType.ContextClick:
                if (itemRect.Contains(evt.mousePosition)) {
                    selectedObject = obj; // highlight also on right-click
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                    EditorUtility.DisplayPopupMenu(new Rect(evt.mousePosition, Vector2.zero), "Assets/", null);
                    evt.Use();
                }
                break;

            case EventType.MouseDrag:
                if (evt.button == 0 && itemRect.Contains(mouseDownPosition)) {
                    float currentTime = Time.realtimeSinceStartup;
                    float mouseHoldTime = currentTime - mouseDownTime;
                    float dragDistanceMoved = Vector2.Distance(mouseDownPosition, evt.mousePosition);

                    if (mouseHoldTime > dragThreshold || dragDistanceMoved > dragDistance) {
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.objectReferences = new UnityEngine.Object[] { obj };
                        DragAndDrop.SetGenericData("BookmarkIndex", index);
                        DragAndDrop.StartDrag(objectName);
                        evt.Use();
                    }
                }
                break;

            case EventType.MouseMove:
                Repaint();
                break;
            }
        }

        private string GetObjectNameWithParentFolder(UnityEngine.Object obj)
        {
            string objectName = obj.name;
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(assetPath) && System.IO.Directory.Exists(assetPath)) {
                string parentFolder = System.IO.Path.GetDirectoryName(assetPath);
                if (!string.IsNullOrEmpty(parentFolder)) {
                    string parentFolderName = System.IO.Path.GetFileName(parentFolder);
                    if (!string.IsNullOrEmpty(parentFolderName)) {
                        objectName = parentFolderName + " / " + objectName;
                    }
                }
            }
            return objectName;
        }

        private void DrawSortableHeader(string headerName, SortType sortType)
        {
            GUILayout.BeginHorizontal();
            bool isCurrentSortType = currentSortType == sortType;
            GUILayout.Label(isCurrentSortType ? (sortAscending ? "ASC" : "DESC") : " ", GUILayout.Width(40));
            if (GUILayout.Button(headerName, headerStyle)) {
                if (isCurrentSortType)
                    sortAscending = !sortAscending;
                else {
                    currentSortType = sortType;
                    sortAscending = true;
                }
                SortBookmarkedObjects();
            }
            GUILayout.EndHorizontal();
        }

        private void SortBookmarkedObjects()
        {
            if (currentSortType == SortType.TimeAdded && !sortAscending) {
                bookmarkedObjects.Reverse();
            }
            else {
                switch (currentSortType) {
                case SortType.Type:
                    bookmarkedObjects.Sort((a, b) => string.Compare(a.GetType().Name, b.GetType().Name, StringComparison.Ordinal) * (sortAscending ? 1 : -1));
                    break;
                case SortType.Name:
                    bookmarkedObjects.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal) * (sortAscending ? 1 : -1));
                    break;
                case SortType.Asset:
                    bookmarkedObjects.Sort((a, b) =>
                    {
                        string extA = System.IO.Path.GetExtension(AssetDatabase.GetAssetPath(a)).ToLower();
                        string extB = System.IO.Path.GetExtension(AssetDatabase.GetAssetPath(b)).ToLower();
                        return string.Compare(extA, extB, StringComparison.Ordinal) * (sortAscending ? 1 : -1);
                    });
                    break;
                }
            }
        }

        private static void LoadBookmarks()
        {
            bookmarkedObjects.Clear();
            string path = Path.Combine(Application.dataPath, FolderName, SaveFileName);
            if (File.Exists(path)) {
                string json = File.ReadAllText(path);
                BookmarkData data = JsonUtility.FromJson<BookmarkData>(json);
                if (data != null && data.assetPaths != null) {
                    foreach (string assetPath in data.assetPaths) {
                        if (!string.IsNullOrEmpty(assetPath)) {
                            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                            if (obj != null) {
                                bookmarkedObjects.Add(obj);
                            }
                        }
                    }
                }
            }
        }

        private static void SaveBookmarks()
        {
            BookmarkData data = new BookmarkData();
            foreach (var obj in bookmarkedObjects) {
                string path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path)) {
                    data.assetPaths.Add(path);
                }
            }

            string folderPath = Path.Combine(Application.dataPath, FolderName);
            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(Path.Combine(folderPath, SaveFileName), json);
        }
    }
}
#endif
