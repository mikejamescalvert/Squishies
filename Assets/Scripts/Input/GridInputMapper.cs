using UnityEngine;

namespace Squishies
{
    public class GridInputMapper : MonoBehaviour
    {
        private float cellSize = 0.8f;
        private float yOffset = 0.5f;

        /// <summary>
        /// Converts a screen position (mouse or touch) to grid coordinates.
        /// Returns null if the position is outside the valid grid bounds.
        /// </summary>
        public Vector2Int? ScreenToGrid(Vector3 screenPos)
        {
            if (GridManager.Instance != null)
            {
                cellSize = GridManager.Instance.CellSize;
                yOffset = GridManager.Instance.YOffset;
            }

            Camera cam = Camera.main;
            if (cam == null) return null;

            Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);

            int x = Mathf.RoundToInt(worldPos.x / cellSize + 3f);
            int y = Mathf.RoundToInt((worldPos.y - yOffset) / cellSize + 4f);

            if (x >= 0 && x < 7 && y >= 0 && y < 9)
            {
                return new Vector2Int(x, y);
            }

            return null;
        }
    }
}
