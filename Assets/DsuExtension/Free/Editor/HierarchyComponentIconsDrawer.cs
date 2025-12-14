#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

namespace Dsu.Extension
{
    [InitializeOnLoad]
    public class HierarchyComponentIconsDrawer
    {
        static float iconSize = 18f;
        static float spacing = -2f;

        static HierarchyComponentIconsDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            if (!DsuEditorSettings.HierarchyIconsEnabled) return;

            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null) return;

            Component[] components = go.GetComponents<Component>();

            bool shouldRedHighlight = false;
            if (DsuEditorSettings.HighlightSpecial) {
                foreach (var comp in components) {
                    if (comp == null) continue;
                    Type type = comp.GetType();
                    if ((type.Name.StartsWith("Highlight") || type.Name.StartsWith("Show")) && type.Namespace.StartsWith("Dsu")) {
                        shouldRedHighlight = true;
                        break;
                    }
                }
            }

            bool shouldGreenHighlight = false;
            if (DsuEditorSettings.HighlightDisabled) {
                foreach (var comp in components) {
                    if (comp is Behaviour behaviour && !behaviour.enabled) {
                        shouldGreenHighlight = true;
                        break;
                    }
                }
            }

            if (shouldRedHighlight || shouldGreenHighlight) {
                Handles.BeginGUI();
                Color prevColor = Handles.color;
                Handles.color = shouldRedHighlight ? DsuEditorSettings.SpecialHighlightColor : DsuEditorSettings.DisabledHighlightColor;
                Rect outlineRect = new Rect(selectionRect.x, selectionRect.y + 1, selectionRect.width - 18, selectionRect.height - 2);
                Handles.DrawSolidRectangleWithOutline(outlineRect, Color.clear, Handles.color);
                Handles.color = prevColor;
                Handles.EndGUI();
            }

            string layerName = DsuEditorSettings.ShowLayer ? LayerMask.LayerToName(go.layer) : "";
            string tagName = DsuEditorSettings.ShowTag ? go.tag : "";

            string displayText = "";
            if (DsuEditorSettings.ShowLayer && DsuEditorSettings.ShowTag)
                displayText = "[" + tagName + " | " + layerName + "]";
            else if (DsuEditorSettings.ShowLayer)
                displayText = "[" + layerName + "]";
            else if (DsuEditorSettings.ShowTag)
                displayText = "[" + tagName + "]";

            GUIStyle textStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 10,
                clipping = TextClipping.Clip
            };

            Vector2 textSize = string.IsNullOrEmpty(displayText) ? Vector2.zero : textStyle.CalcSize(new GUIContent(displayText));

            Rect toggleRect = new Rect(selectionRect.xMax - 16f, selectionRect.y, 16f, selectionRect.height);
            bool newActive = GUI.Toggle(toggleRect, go.activeSelf, GUIContent.none);
            if (newActive != go.activeSelf) {
                Undo.RecordObject(go, "Toggle Active State");
                go.SetActive(newActive);
                EditorApplication.RepaintHierarchyWindow();
            }

            Rect tagLayerRect = new Rect(toggleRect.x - textSize.x - 4f, selectionRect.y, textSize.x, selectionRect.height);
            if (!string.IsNullOrEmpty(displayText))
                GUI.Label(tagLayerRect, displayText, textStyle);

            float iconX = tagLayerRect.x - iconSize;

            int scriptComponentCount = 0;
            Component scriptComponentToShow = null;

            for (int i = 0; i < components.Length; i++) {
                var comp = components[i];
                if (comp == null || comp is Transform) continue;

                Type type = comp.GetType();
                if (!ShouldShowComponentIcon(comp, type)) continue;

                if (typeof(MonoBehaviour).IsAssignableFrom(type)) {
                    if (DsuEditorSettings.ShowScriptIcon) {
                        bool isIncreaseCounter = true;
                        if (DsuEditorSettings.ShowScriptIconWithCount) {
                            string asmName = type.Assembly.GetName().Name;
                            if (asmName.StartsWith("UnityEngine") || asmName.StartsWith("UnityEditor")) {
                                isIncreaseCounter = false;
                            }
                        }
                        if (isIncreaseCounter) {
                            scriptComponentCount++;
                            if (scriptComponentToShow == null)
                                scriptComponentToShow = comp;
                        }
                    }
                }
            }

            int shownCount = 0;
            float currentX = iconX;

            if (DsuEditorSettings.ShowScriptIconWithCount) {
                if (scriptComponentCount > 0 && scriptComponentToShow != null && shownCount < DsuEditorSettings.MaxIconCount - 1) {
                    Texture icon = GetIconForComponent(scriptComponentToShow, scriptComponentToShow.GetType());
                    if (icon != null) {
                        if (scriptComponentCount > 1) {
                            Rect countBoxRect = new Rect(currentX, selectionRect.y + 1f, iconSize - 4, iconSize - 4);

                            Handles.BeginGUI();
                            Handles.DrawSolidRectangleWithOutline(countBoxRect, new Color(0f, 0f, 0f, 0.75f), Color.white);
                            Handles.EndGUI();

                            GUIStyle countStyle = new GUIStyle(EditorStyles.boldLabel)
                            {
                                alignment = TextAnchor.MiddleRight,
                                fontSize = 10,
                                normal = { textColor = Color.white }
                            };
                            GUI.Label(countBoxRect, "x" + scriptComponentCount.ToString(), countStyle);

                            currentX -= iconSize + spacing;
                            shownCount++;
                        }

                        Rect iconRect = new Rect(currentX, selectionRect.y - 1f, iconSize, iconSize);
                        GUI.Label(iconRect, new GUIContent(icon, scriptComponentToShow.GetType().Name));
                        currentX -= iconSize + spacing;
                        shownCount++;
                    }
                }
            }

            for (int i = components.Length - 1; i >= 0 && shownCount < DsuEditorSettings.MaxIconCount; i--) {
                var comp = components[i];
                if (comp == null || comp is Transform) continue;

                Type type = comp.GetType();
                if (!ShouldShowComponentIcon(comp, type)) continue;

                bool isMonoBehaviour = typeof(MonoBehaviour).IsAssignableFrom(type);
                if (isMonoBehaviour && DsuEditorSettings.ShowScriptIconWithCount) {
                    string asmName = type.Assembly.GetName().Name;
                    if (!asmName.StartsWith("UnityEngine") && !asmName.StartsWith("UnityEditor")) {
                        continue;
                    }
                }

                Texture icon = GetIconForComponent(comp, type);
                if (icon == null) continue;

                Rect iconRect = new Rect(currentX, selectionRect.y - 1f, iconSize, iconSize);

                if (iconRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown) {
                    if (comp is MonoBehaviour mb && !mb.enabled) {
                        Undo.RecordObject(mb, "Enable Component");
                        mb.enabled = true;
                    }
                    Selection.activeObject = comp;
                    EditorGUIUtility.PingObject(comp);
                    Event.current.Use();
                }

                GUI.Label(iconRect, new GUIContent(icon, type.Name));
                currentX -= iconSize + spacing;
                shownCount++;
            }
        }

        private static Texture GetIconForComponent(Component comp, Type type)
        {
            GUIContent iconContent = EditorGUIUtility.ObjectContent(comp, type);
            if (iconContent != null && iconContent.image != null)
                return iconContent.image;

            if (comp is MonoBehaviour mb) {
                MonoScript script = MonoScript.FromMonoBehaviour(mb);
                if (script != null)
                    return AssetPreview.GetMiniThumbnail(script);
            }

            return null;
        }

        private static bool ShouldShowComponentIcon(Component comp, Type type)
        {
            if (type == null) return false;

            bool isMonoBehaviour = typeof(MonoBehaviour).IsAssignableFrom(type);
            if (isMonoBehaviour) {
                //string asmName = type.Assembly.GetName().Name;
                //if (!asmName.StartsWith("UnityEngine") && !asmName.StartsWith("UnityEditor")) {
                //    return DsuEditorSettings.ShowScriptIcon;
                //}
                //return false;
                return DsuEditorSettings.ShowScriptIcon;
            }

            if (comp is RectTransform ||
                comp is CanvasRenderer ||
                comp is Canvas ||
                comp is CanvasGroup ||
                comp.GetType().Namespace == "UnityEngine.UI" ||
                comp.GetType().Namespace == "TMPro") {
                return DsuEditorSettings.ShowUIIcon;
            }

            if (comp is SpriteRenderer ||
                comp is Collider2D ||
                comp is Rigidbody2D ||
                type.Name.Contains("Tilemap") ||
                type.Namespace == "UnityEngine.Tilemaps") {
                return DsuEditorSettings.Show2DIcon;
            }

            return DsuEditorSettings.ShowOtherIcons;
        }
    }
}
#endif