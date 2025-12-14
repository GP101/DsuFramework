using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Tilemaps;

namespace Dsu.Extension
{
    [AddComponentMenu("Dsu/HighlightTilemap2D")]
    public class HighlightTilemap2D : MonoBehaviour
    {
        public Color highlightColor = Color.cyan;
        public bool enableHighlight = true;

        [Tooltip("Use CompositeCollider2D shape if available.")]
        public bool useCompositeCollider = false;

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

            Tilemap tilemap = GetComponent<Tilemap>();
            if (tilemap == null)
                return;

            BoundsInt bounds = tilemap.cellBounds;

            for (int x = bounds.xMin; x < bounds.xMax; x++) {
                for (int y = bounds.yMin; y < bounds.yMax; y++) {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);
                    TileBase tile = tilemap.GetTile(cellPos);

                    if (tile != null) {
                        Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);
                        Vector3 tileSize = tilemap.cellSize;

                        Gizmos.DrawWireCube(worldPos, tileSize);
                    }
                }
            }
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
