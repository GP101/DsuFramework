#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dsu.Framework
{
    public class UIReferenceFinder : UnityEditor.EditorWindow
    {
        private Vector2 scroll;
        private List<ReferenceInfo> results = new List<ReferenceInfo>();
        private bool showBadOnly = false;
        private bool showHelpMessage = false;
        private int maxHierarchyDepth = 2;
        private string depthInput = "2";

        private string searchQuery = ""; // 검색 문자열

        private enum LastAction
        {
            FindAllUIReference,
            FindBadUIReference,
            FindAllUIObject
        }

        private LastAction lastAction = LastAction.FindAllUIReference;

        private static readonly Type[] uiTypes = new Type[]
        {
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Graphic),
            typeof(Image),
            typeof(Text),
            typeof(Button),
            typeof(Toggle),
            typeof(Slider),
            typeof(Scrollbar),
            typeof(Dropdown),
            typeof(InputField),
            typeof(Selectable),
            typeof(CanvasGroup),
        };

        [MenuItem("Tools/UI Reference Finder")]
        public static void ShowWindow()
        {
            GetWindow<UIReferenceFinder>("UI Reference Finder");
        }

        private void OnEnable()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnDisable()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            results.Clear();
            Repaint();
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.EnteredPlayMode) {
                results.Clear();
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Find All UI Reference")) {
                showBadOnly = false;
                showHelpMessage = false;
                lastAction = LastAction.FindAllUIReference;
                FindUIReferences();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Find Bad UI Reference")) {
                showBadOnly = true;
                showHelpMessage = false;
                lastAction = LastAction.FindBadUIReference;
                FindUIReferences();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Find All UI Object")) {
                showBadOnly = false;
                showHelpMessage = false;
                lastAction = LastAction.FindAllUIObject;
                FindAllUIObjects();
            }
            GUILayout.EndHorizontal();

            // 검색 UI 추가
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search", GUILayout.Width(70));
            searchQuery = GUILayout.TextField(searchQuery);
            if (GUILayout.Button("Search", GUILayout.Width(80))) {
                Repaint(); // 검색 후 다시 그림
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Depth", GUILayout.Width(70));
            GUI.SetNextControlName("DepthField");
            depthInput = GUILayout.TextField(depthInput, GUILayout.Width(50));

            bool doAction = false;
            if (int.TryParse(depthInput, out int newDepth)) {
                newDepth = Mathf.Max(0, newDepth);
                if (newDepth != maxHierarchyDepth) {
                    maxHierarchyDepth = newDepth;
                    doAction = true;
                }
            }

            if (doAction) {
                switch (lastAction) {
                case LastAction.FindAllUIObject:
                    FindAllUIObjects();
                    break;
                case LastAction.FindBadUIReference:
                    FindUIReferences();
                    break;
                default:
                    FindUIReferences();
                    break;
                }
                Event.current.Use();
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear", GUILayout.Width(80))) {
                results.Clear();
                showHelpMessage = false;
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            scroll = GUILayout.BeginScrollView(scroll);

            GUIStyle leftAlignedButton = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip
            };

            var sortedResults = new List<ReferenceInfo>(results);
            sortedResults.Sort((a, b) => {
                int depthA = GetHierarchyDepth(a.gameObject.transform);
                int depthB = GetHierarchyDepth(b.gameObject.transform);
                if (depthA != depthB) return depthA.CompareTo(depthB);
                return string.Compare(a.gameObject.name, b.gameObject.name, StringComparison.OrdinalIgnoreCase);
            });

            foreach (var info in sortedResults) {
                if (info.gameObject == null) continue;

                // 검색 조건 추가
                if (!string.IsNullOrEmpty(searchQuery)) {
                    bool matchFound = info.gameObject.name.ToLower().Contains(searchQuery.ToLower());

                    foreach (var comp in info.componentInfos) {
                        if (comp.component != null &&
                            comp.component.GetType().Name.ToLower().Contains(searchQuery.ToLower())) {
                            matchFound = true;
                            break;
                        }
                    }

                    if (!matchFound) continue;
                }

                int indentLevel = GetHierarchyDepth(info.gameObject.transform);
                float indentWidth = indentLevel * 20f;

                // 각 게임 객체를 회색 블록으로 감싸기
                EditorGUILayout.BeginVertical("box");
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(indentWidth);
                GUILayout.Label(info.gameObject.name, EditorStyles.boldLabel);
                
                // Select 버튼 추가
                if (GUILayout.Button("Select", GUILayout.Width(50))) {
                    Selection.activeObject = info.gameObject;
                }
                
                // Focus 버튼 추가
                if (GUILayout.Button("Focus", GUILayout.Width(50))) {
                    Selection.activeObject = info.gameObject;
                    EditorGUIUtility.PingObject(info.gameObject);
                    SceneView.FrameLastActiveSceneView();
                }
                GUILayout.EndHorizontal();

                foreach (var comp in info.componentInfos) {
                    if (comp.component == null) continue;

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(indentWidth + 20);

                    GUIContent content = EditorGUIUtility.ObjectContent(comp.component, comp.component.GetType());
                    Texture icon = content.image;

                    if (icon != null)
                        GUILayout.Box(icon, GUIStyle.none, GUILayout.Width(20), GUILayout.Height(20));
                    else
                        GUILayout.Space(20);

                    GUILayout.Label(content.text, EditorStyles.boldLabel);
                    GUILayout.EndHorizontal();

                    foreach (var fieldInfo in comp.referencedFields) {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(indentWidth + 40);
                        GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
                        {
                            normal = {
                                textColor = showBadOnly ? Color.red : Color.white
                            }
                        };

                        GUILayout.Label($"{fieldInfo.fieldName} → {fieldInfo.value?.name ?? "None"}", labelStyle);
                        GUILayout.EndHorizontal();
                    }
                }
                
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
            }

            GUILayout.EndScrollView();

            if (showHelpMessage) {
                EditorGUILayout.HelpBox("Use UIController.cs to apply MVC Pattern", MessageType.Warning);
            }
        }

        private void FindUIReferences()
        {
            results.Clear();
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            bool anyBadReference = false;

            foreach (var obj in allObjects) {
                if (GetHierarchyDepth(obj.transform) > maxHierarchyDepth) continue;

                Component[] comps = obj.GetComponents<Component>();
                List<ComponentInfo> matchedComponents = new List<ComponentInfo>();

                foreach (var comp in comps) {
                    if (comp == null) continue;

                    Type type = comp.GetType();
                    FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    List<ReferenceField> matchedFields = new List<ReferenceField>();

                    foreach (var field in fields) {
                        if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null) continue;
                        if (!typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType)) continue;

                        UnityEngine.Object value = field.GetValue(comp) as UnityEngine.Object;
                        if (value == null) continue;

                        bool isUIRef = false;
                        foreach (var uiType in uiTypes) {
                            if (uiType.IsInstanceOfType(value)) {
                                isUIRef = true;
                                break;
                            }
                        }

                        if (!isUIRef) continue;

                        bool isInCanvas = obj.GetComponentInParent<Canvas>() != null;
                        if (showBadOnly && isInCanvas) continue;

                        matchedFields.Add(new ReferenceField
                        {
                            fieldName = field.Name,
                            value = value
                        });

                        if (showBadOnly && !isInCanvas) {
                            anyBadReference = true;
                        }
                    }

                    if (matchedFields.Count > 0) {
                        matchedComponents.Add(new ComponentInfo
                        {
                            component = comp,
                            referencedFields = matchedFields
                        });
                    }
                }

                if (matchedComponents.Count > 0) {
                    results.Add(new ReferenceInfo
                    {
                        gameObject = obj,
                        componentInfos = matchedComponents
                    });
                }
            }

            showHelpMessage = showBadOnly && anyBadReference;
        }

        private void FindAllUIObjects()
        {
            results.Clear();
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

            foreach (var obj in allObjects) {
                if (GetHierarchyDepth(obj.transform) > maxHierarchyDepth) continue;

                Component[] comps = obj.GetComponents<Component>();
                List<ComponentInfo> matchedComponents = new List<ComponentInfo>();

                foreach (var comp in comps) {
                    if (comp == null) continue;

                    Type type = comp.GetType();
                    foreach (var uiType in uiTypes) {
                        if (uiType.IsAssignableFrom(type)) {
                            matchedComponents.Add(new ComponentInfo
                            {
                                component = comp,
                                referencedFields = new List<ReferenceField>()
                            });
                            break;
                        }
                    }
                }

                if (matchedComponents.Count > 0) {
                    results.Add(new ReferenceInfo
                    {
                        gameObject = obj,
                        componentInfos = matchedComponents
                    });
                }
            }
        }

        private int GetHierarchyDepth(Transform transform)
        {
            int depth = 0;
            while (transform.parent != null) {
                depth++;
                transform = transform.parent;
            }
            return depth;
        }

        private class ReferenceInfo
        {
            public GameObject gameObject;
            public List<ComponentInfo> componentInfos;
        }

        private class ComponentInfo
        {
            public Component component;
            public List<ReferenceField> referencedFields;
        }

        private class ReferenceField
        {
            public string fieldName;
            public UnityEngine.Object value;
        }
    }
}
#endif
