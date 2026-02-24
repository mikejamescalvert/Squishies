using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Squishies
{
    public class PathDrawer : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private Coroutine invalidFeedbackCoroutine;
        private List<Vector3> originalPositions = new List<Vector3>();

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            ConfigureLineRenderer();
        }

        private void ConfigureLineRenderer()
        {
            lineRenderer.startWidth = 0.15f;
            lineRenderer.endWidth = 0.15f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            Color white70 = new Color(1f, 1f, 1f, 0.7f);
            lineRenderer.startColor = white70;
            lineRenderer.endColor = white70;
            lineRenderer.sortingOrder = 10;
            lineRenderer.useWorldSpace = true;
            lineRenderer.numCornerVertices = 5;
            lineRenderer.numCapVertices = 5;
            lineRenderer.positionCount = 0;
        }

        /// <summary>
        /// Updates the drawn path line based on grid positions.
        /// Converts each grid position to world position via GridManager.
        /// </summary>
        public void UpdatePath(List<Vector2Int> path, Color color)
        {
            if (path == null || path.Count == 0)
            {
                ClearPath();
                return;
            }

            lineRenderer.positionCount = path.Count;

            originalPositions.Clear();

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 worldPos = GridManager.Instance != null
                    ? GridManager.Instance.GridToWorldPosition(path[i])
                    : Vector3.zero;
                lineRenderer.SetPosition(i, worldPos);
                originalPositions.Add(worldPos);
            }

            Color lineColor = new Color(color.r, color.g, color.b, 0.7f);
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
        }

        /// <summary>
        /// Clears the drawn path line.
        /// </summary>
        public void ClearPath()
        {
            lineRenderer.positionCount = 0;
            originalPositions.Clear();
        }

        /// <summary>
        /// Briefly flashes the line red and shakes it to indicate an invalid move.
        /// </summary>
        public void ShowInvalidFeedback()
        {
            if (invalidFeedbackCoroutine != null)
            {
                StopCoroutine(invalidFeedbackCoroutine);
            }
            invalidFeedbackCoroutine = StartCoroutine(InvalidFeedbackCoroutine());
        }

        private IEnumerator InvalidFeedbackCoroutine()
        {
            if (lineRenderer.positionCount == 0)
            {
                yield break;
            }

            // Store current colors
            Color previousStartColor = lineRenderer.startColor;
            Color previousEndColor = lineRenderer.endColor;

            // Flash red
            Color redFlash = new Color(1f, 0.2f, 0.2f, 0.9f);
            lineRenderer.startColor = redFlash;
            lineRenderer.endColor = redFlash;

            // Shake the line positions with small random offsets
            float elapsed = 0f;
            float duration = 0.15f;

            while (elapsed < duration)
            {
                for (int i = 0; i < lineRenderer.positionCount && i < originalPositions.Count; i++)
                {
                    Vector2 randomOffset = Random.insideUnitCircle * 0.05f;
                    Vector3 shakenPos = originalPositions[i] + new Vector3(randomOffset.x, randomOffset.y, 0f);
                    lineRenderer.SetPosition(i, shakenPos);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Restore original positions
            for (int i = 0; i < lineRenderer.positionCount && i < originalPositions.Count; i++)
            {
                lineRenderer.SetPosition(i, originalPositions[i]);
            }

            // Restore original colors
            lineRenderer.startColor = previousStartColor;
            lineRenderer.endColor = previousEndColor;

            invalidFeedbackCoroutine = null;
        }
    }
}
