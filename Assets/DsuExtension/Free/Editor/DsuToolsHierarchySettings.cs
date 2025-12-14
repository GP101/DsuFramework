#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Dsu.Extension
{
    public class DsuToolsHierarchySettings : EditorWindow
    {
        private static bool hierarchyFoldout = true;
        private Vector2 scrollPos;

        [MenuItem("Tools/Dsu Tools/Hierarchy Settings", false, 100)]
        public static void ShowWindow()
        {
            GetWindow<DsuToolsHierarchySettings>("Dsu Hierarchy Settings");
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            float prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 250f;

            EditorGUI.BeginChangeCheck();

            DsuEditorSettings.EnableWorldTransform = EditorGUILayout.Toggle("Enable World Transform", DsuEditorSettings.EnableWorldTransform);

            GUILayout.Space(10);
            hierarchyFoldout = EditorGUILayout.Foldout(hierarchyFoldout, "Enable Hierarchy Icons", true);
            if (hierarchyFoldout) {
                EditorGUI.indentLevel++;

                DsuEditorSettings.HierarchyIconsEnabled = EditorGUILayout.Toggle("Enable Hierarchy Icons", DsuEditorSettings.HierarchyIconsEnabled);

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                if (DsuEditorSettings.HierarchyIconsEnabled) {
                    EditorGUI.indentLevel++;
                    DsuEditorSettings.ShowScriptIcon = EditorGUILayout.Toggle("Show Script Icon", DsuEditorSettings.ShowScriptIcon);
                    DsuEditorSettings.ShowScriptIconWithCount = EditorGUILayout.Toggle("Script Icon with Count", DsuEditorSettings.ShowScriptIconWithCount);
                    DsuEditorSettings.ShowUIIcon = EditorGUILayout.Toggle("Show UI Icon", DsuEditorSettings.ShowUIIcon);
                    DsuEditorSettings.Show2DIcon = EditorGUILayout.Toggle("Show 2D Icon", DsuEditorSettings.Show2DIcon);
                    DsuEditorSettings.ShowOtherIcons = EditorGUILayout.Toggle("Show Other Icons", DsuEditorSettings.ShowOtherIcons);

                    GUILayout.Space(5);
                    DsuEditorSettings.ShowLayer = EditorGUILayout.Toggle("Show Layer Name", DsuEditorSettings.ShowLayer);
                    DsuEditorSettings.ShowTag = EditorGUILayout.Toggle("Show Tag Name", DsuEditorSettings.ShowTag);
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);
            DsuEditorSettings.HighlightSpecial = EditorGUILayout.Toggle("Highlight Component(Show...;Highlight...)", DsuEditorSettings.HighlightSpecial);
            DsuEditorSettings.HighlightDisabled = EditorGUILayout.Toggle("Highlight Disabled Components", DsuEditorSettings.HighlightDisabled);
            DsuEditorSettings.MaxIconCount = EditorGUILayout.IntSlider("Max Icon Count", DsuEditorSettings.MaxIconCount, 0, 20);
            DsuEditorSettings.SpecialHighlightColor = EditorGUILayout.ColorField("Special Highlight Color", DsuEditorSettings.SpecialHighlightColor);
            DsuEditorSettings.DisabledHighlightColor = EditorGUILayout.ColorField("Disabled Highlight Color", DsuEditorSettings.DisabledHighlightColor);

            if (EditorGUI.EndChangeCheck()) {
                Repaint();
            }

            EditorGUIUtility.labelWidth = prevLabelWidth;

            GUILayout.Space(10);
            EditorGUILayout.HelpBox("These settings apply to hierarchy visuals and transform inspector in Dsu Tools.", MessageType.Info);

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif
