using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dsu.Extension
{
    [AddComponentMenu("Dsu/HighlightCollider2D")]
    public class HighlightCollider2D : MonoBehaviour
    {
        public bool enableHighlight = true;
        public Color highlightColor = Color.yellow;
        public bool drawWireframe = true;
        public bool showColliderName = true;

        [Tooltip("If enabled, CompositeCollider2D path will be rendered instead of individual colliders.")]
        public bool useCompositeCollider = false;

        private Renderer[] childRenderers;
        private Color[] originalColors;

        void Start()
        {
            childRenderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[childRenderers.Length];

            for (int i = 0; i < childRenderers.Length; i++) {
                originalColors[i] = childRenderers[i].material.color;
            }

            HighlightColliders();
        }

        void HighlightColliders()
        {
            Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();

            foreach (Collider2D collider in childColliders) {
                Renderer renderer = collider.GetComponent<Renderer>();

                if (renderer != null) {
                    renderer.material.color = highlightColor;
                }
            }
        }

        void ResetColors()
        {
            for (int i = 0; i < childRenderers.Length; i++) {
                childRenderers[i].material.color = originalColors[i];
            }
        }

        void OnDisable()
        {
            ResetColors();
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!enableHighlight)
                return;

            Gizmos.color = highlightColor;

            if (useCompositeCollider) {
                CompositeCollider2D composite = GetComponent<CompositeCollider2D>();
                if (composite != null) {
                    DrawCompositeCollider(composite);
                    return;
                }
            }

            Collider2D parentCollider = GetComponent<Collider2D>();
            if (parentCollider != null) {
                Vector3 sizeOffset = GetEdgeOffset(parentCollider);
                if (drawWireframe) {
                    Gizmos.DrawWireCube(parentCollider.bounds.center, parentCollider.bounds.size + sizeOffset);
                }
                else {
                    Gizmos.DrawCube(parentCollider.bounds.center, parentCollider.bounds.size + sizeOffset);
                }

                if( showColliderName )
                    DrawLabelAboveCollider(parentCollider, gameObject.name);
            }

            Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D collider in childColliders) {
                if (collider == parentCollider) continue;

                Vector3 sizeOffset = GetEdgeOffset(collider);
                if (drawWireframe) {
                    Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size + sizeOffset);
                }
                else {
                    Gizmos.DrawCube(collider.bounds.center, collider.bounds.size + sizeOffset);
                }

                if( showColliderName )
                    DrawLabelAboveCollider(collider, collider.gameObject.name);
            }
        }

#if UNITY_EDITOR
        void DrawLabelAboveCollider(Collider2D collider, string label)
        {
            Vector3 topLeft = new Vector3(
                collider.bounds.min.x,
                collider.bounds.max.y,
                collider.transform.position.z
            );

            // GUIStyle 정의
            GUIStyle backgroundStyle = new GUIStyle(EditorStyles.helpBox);
            backgroundStyle.normal.background = Texture2D.grayTexture;
            backgroundStyle.normal.textColor = Color.white;
            backgroundStyle.alignment = TextAnchor.MiddleCenter;
            backgroundStyle.fontSize = 10;
            backgroundStyle.padding = new RectOffset(4, 4, 2, 2);

            Handles.BeginGUI();

            Vector3 screenPos = HandleUtility.WorldToGUIPoint(topLeft);
            Vector2 size = backgroundStyle.CalcSize(new GUIContent(label));
            Rect rect = new Rect(screenPos.x - size.x / 2, screenPos.y - size.y, size.x, size.y);

            GUI.Label(rect, label, backgroundStyle);

            Handles.EndGUI();
        }
#endif

        Vector3 GetEdgeOffset(Collider2D collider)
        {
            float edgeRadius = 0f;
            if (collider is BoxCollider2D boxCollider) {
                edgeRadius = boxCollider.edgeRadius;
            }
            return new Vector3(edgeRadius, edgeRadius, 0f);
        }

        void DrawCompositeCollider(CompositeCollider2D composite)
        {
            int pathCount = composite.pathCount;
            Vector2[] points = new Vector2[composite.pointCount];

            for (int i = 0; i < pathCount; i++) {
                int pointCount = composite.GetPath(i, points);

                for (int j = 0; j < pointCount; j++) {
                    Vector3 start = composite.transform.TransformPoint(points[j]);
                    Vector3 end = composite.transform.TransformPoint(points[(j + 1) % pointCount]);

                    Gizmos.DrawLine(start, end);
                }
            }
        }
#endif
    }
}
