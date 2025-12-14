#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Dsu.Extension
{
    [CustomEditor(typeof(HighlightThis))]
    public class HighlightThisEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Begin position tracking
            Rect startRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true));

            Rect rect = EditorGUILayout.BeginVertical();

            DrawContent();

            EditorGUILayout.EndVertical();
            // End position tracking
            Rect endRect = GUILayoutUtility.GetLastRect();

            float padding = 6f;
            float top = startRect.yMin - padding;
            float bottom = endRect.yMax + padding;
            float left = 5f;
            float right = EditorGUIUtility.currentViewWidth - 15f;

            Vector3[] linePoints = new Vector3[]
            {
                new Vector3(left, top),
                new Vector3(right, top),
                new Vector3(right, bottom),
                new Vector3(left, bottom),
                new Vector3(left, top)
            };

            Handles.color = Color.red;
            Handles.DrawAAPolyLine(2f, linePoints);
        }

        private void DrawContent()
        {
            HighlightThis highlight = (HighlightThis)target;

            // 기본 필드
            DrawDefaultInspector();

            GUILayout.Space(10);

            // Remove 버튼 스타일
            GUIStyle removeButtonStyle = new GUIStyle(GUI.skin.button);
            removeButtonStyle.normal.textColor = Color.white;
            removeButtonStyle.fontStyle = FontStyle.Bold;
            removeButtonStyle.alignment = TextAnchor.MiddleCenter;
            removeButtonStyle.padding = new RectOffset(10, 10, 6, 6);

            // 버튼 배경색 빨간색
            Color originalBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;

            if (GUILayout.Button("Remove", removeButtonStyle)) {
                DestroyImmediate(highlight);
                return;
            }

            GUI.backgroundColor = originalBackgroundColor;
        }
    }
}
#endif
