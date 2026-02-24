using System.Collections.Generic;
using UnityEngine;

namespace Squishies
{
    public class InputHandler : MonoBehaviour
    {
        public bool InputEnabled { get; set; } = true;

        public List<Vector2Int> CurrentPath { get; private set; } = new List<Vector2Int>();

        public event System.Action<List<Vector2Int>> OnPathCompleted;
        public event System.Action<List<Vector2Int>> OnPathUpdated;

        private bool isDragging = false;
        private GridInputMapper gridMapper;
        private PathDrawer pathDrawer;

        private void Awake()
        {
            gridMapper = GetComponent<GridInputMapper>();
            if (gridMapper == null)
            {
                gridMapper = gameObject.AddComponent<GridInputMapper>();
            }
        }

        private void Start()
        {
            if (pathDrawer == null)
            {
                pathDrawer = FindObjectOfType<PathDrawer>();
            }
        }

        private void Update()
        {
            if (!InputEnabled || GridManager.Instance == null || GridManager.Instance.IsProcessing)
                return;

            // Get input position (works for both mouse and touch)
            bool inputDown, inputHeld, inputUp;
            Vector3 inputPosition;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                inputPosition = touch.position;
                inputDown = touch.phase == TouchPhase.Began;
                inputHeld = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
                inputUp = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
            }
            else
            {
                inputPosition = Input.mousePosition;
                inputDown = Input.GetMouseButtonDown(0);
                inputHeld = Input.GetMouseButton(0);
                inputUp = Input.GetMouseButtonUp(0);
            }

            // --- Input Down: Start a new path ---
            if (inputDown)
            {
                Vector2Int? gridPos = gridMapper.ScreenToGrid(inputPosition);
                if (gridPos.HasValue && GridManager.Instance.GetSquishyAt(gridPos.Value) != null)
                {
                    isDragging = true;
                    CurrentPath.Clear();
                    CurrentPath.Add(gridPos.Value);
                    UpdatePathVisual();
                }
            }

            // --- Input Held: Extend or backtrack the path ---
            if (inputHeld && isDragging)
            {
                Vector2Int? gridPos = gridMapper.ScreenToGrid(inputPosition);
                if (gridPos.HasValue && !CurrentPath.Contains(gridPos.Value))
                {
                    // Try to extend path
                    if (MatchEngine.Instance != null && MatchEngine.Instance.CanExtendPath(CurrentPath, gridPos.Value))
                    {
                        CurrentPath.Add(gridPos.Value);
                        UpdatePathVisual();
                        OnPathUpdated?.Invoke(CurrentPath);

                        // Play subtle audio feedback with escalating pitch
                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlayPop(CurrentPath.Count - 1);
                        }
                    }
                    else
                    {
                        if (pathDrawer != null)
                        {
                            pathDrawer.ShowInvalidFeedback();
                        }
                    }
                }
                else if (gridPos.HasValue && CurrentPath.Count >= 2 && gridPos.Value == CurrentPath[CurrentPath.Count - 2])
                {
                    // Backtrack: remove last element when hovering over second-to-last
                    CurrentPath.RemoveAt(CurrentPath.Count - 1);
                    UpdatePathVisual();
                }
            }

            // --- Input Up: Complete or cancel the path ---
            if (inputUp && isDragging)
            {
                isDragging = false;

                if (CurrentPath.Count >= 3)
                {
                    OnPathCompleted?.Invoke(new List<Vector2Int>(CurrentPath));
                    if (MatchEngine.Instance != null)
                    {
                        MatchEngine.Instance.ExecuteMatch(new List<Vector2Int>(CurrentPath));
                    }
                }
                else if (CurrentPath.Count == 2)
                {
                    // Check chonky combo (two large squishies adjacent)
                    var a = GridManager.Instance.GetSquishyAt(CurrentPath[0]);
                    var b = GridManager.Instance.GetSquishyAt(CurrentPath[1]);
                    if (a != null && b != null && MatchEngine.Instance != null && MatchEngine.Instance.IsChonkyCombo(a, b))
                    {
                        OnPathCompleted?.Invoke(new List<Vector2Int>(CurrentPath));
                        MatchEngine.Instance.ExecuteMatch(new List<Vector2Int>(CurrentPath));
                    }
                }

                CurrentPath.Clear();
                if (pathDrawer != null)
                {
                    pathDrawer.ClearPath();
                }
            }
        }

        /// <summary>
        /// Updates the visual path line based on the current path and the color of the first squishy.
        /// </summary>
        private void UpdatePathVisual()
        {
            if (pathDrawer == null) return;
            if (CurrentPath.Count == 0) return;

            Color pathColor = Color.white;

            Squishy firstSquishy = GridManager.Instance.GetSquishyAt(CurrentPath[0]);
            if (firstSquishy != null && firstSquishy.Data != null)
            {
                pathColor = firstSquishy.Data.color;
            }

            pathDrawer.UpdatePath(CurrentPath, pathColor);
        }
    }
}
