using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dsu.Extension
{
    [AddComponentMenu("Dsu/ShowLocalAxis")]
    public class ShowLocalAxis : MonoBehaviour
    {
        [Range(1f, 100f)]
        public float handleLength = 10f;
        public bool showHandles = true;
        public bool xAxisHandleCap = false;
        public bool yAxisHandleCap = false;
        public bool zAxisHandleCap = false;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showHandles)
                return;

            Matrix4x4 prevMatrix = Handles.matrix;
            Handles.matrix = transform.localToWorldMatrix;

            float arrowSize = handleLength * 0.8f;

            // Z axis (blue)
            Handles.color = Color.blue;
            if (!zAxisHandleCap) {
                Handles.DrawLine(Vector3.zero, Vector3.forward * arrowSize);
                Handles.ArrowHandleCap(0, Vector3.forward * arrowSize, Quaternion.LookRotation(Vector3.forward), handleLength * 0.1f, EventType.Repaint);
            }
            else {
                Handles.ArrowHandleCap(0, Vector3.zero, Quaternion.LookRotation(Vector3.forward), handleLength, EventType.Repaint);
            }

            // X axis (red)
            Handles.color = Color.red;
            if (!xAxisHandleCap) {
                Handles.DrawLine(Vector3.zero, Vector3.right * arrowSize);
                Handles.ArrowHandleCap(0, Vector3.right * arrowSize, Quaternion.LookRotation(Vector3.right), handleLength * 0.1f, EventType.Repaint);
            }
            else {
                Handles.ArrowHandleCap(0, Vector3.zero, Quaternion.LookRotation(Vector3.right), handleLength, EventType.Repaint);
            }

            // Y axis (green)
            Handles.color = Color.green;
            if (!yAxisHandleCap) {
                Handles.DrawLine(Vector3.zero, Vector3.up * arrowSize);
                Handles.ArrowHandleCap(0, Vector3.up * arrowSize, Quaternion.LookRotation(Vector3.up), handleLength * 0.1f, EventType.Repaint);
            }
            else {
                Handles.ArrowHandleCap(0, Vector3.zero, Quaternion.LookRotation(Vector3.up), handleLength, EventType.Repaint);
            }

            Handles.matrix = prevMatrix;
        }
#endif
    }
}
