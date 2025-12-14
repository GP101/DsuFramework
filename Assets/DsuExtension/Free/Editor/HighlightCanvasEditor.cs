#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Dsu.Extension
{
    [CustomEditor(typeof(HighlightCanvas))]
    public class HighlightCanvasEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Begin position tracking
            Rect startRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true));
            float startY = startRect.y;

            // 기본 인스펙터 출력
            DrawDefaultInspector();

            GUILayout.Space(10);
            GUI.backgroundColor = Color.red;

            if (GUILayout.Button("Remove")) {
                HighlightCanvas targetScript = (HighlightCanvas)target;
                Undo.DestroyObjectImmediate(targetScript);
            }

            GUI.backgroundColor = Color.white;

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
    }
}
#endif
