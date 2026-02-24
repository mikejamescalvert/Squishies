using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Squishies
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        public const int COLUMNS = 7;
        public const int ROWS = 9;

        [SerializeField] private float cellSize = 0.8f;
        [SerializeField] private float yOffset = 0.5f;

        private Squishy[,] grid;
        private int activeTypeCount = 4;
        private List<SquishyData> activeTypes;
        private ObjectPool<Squishy> squishyPool;
        private Transform gridParent;
        private bool isProcessing = false;

        public bool IsProcessing => isProcessing;
        public bool IsInitialized => grid != null;
        public float CellSize => cellSize;
        public float YOffset => yOffset;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void InitializeGrid(int numTypes)
        {
            Debug.Log($"[GridManager] InitializeGrid called with {numTypes} types");
            activeTypeCount = Mathf.Clamp(numTypes, 4, 7);
            activeTypes = SquishyDatabase.AllTypes.Take(activeTypeCount).ToList();

            // Clean up existing grid if reinitializing
            if (grid != null)
            {
                for (int x = 0; x < COLUMNS; x++)
                {
                    for (int y = 0; y < ROWS; y++)
                    {
                        if (grid[x, y] != null)
                        {
                            grid[x, y].ReturnToPool();
                            if (squishyPool != null)
                                squishyPool.Return(grid[x, y]);
                            grid[x, y] = null;
                        }
                    }
                }
            }

            // Create grid parent
            if (gridParent == null)
            {
                GameObject parentObj = new GameObject("GridParent");
                gridParent = parentObj.transform;
                gridParent.SetParent(transform);
                gridParent.localPosition = Vector3.zero;
            }

            // Only create pool if not already initialized
            if (squishyPool == null)
            {
                GameObject template = new GameObject("SquishyTemplate");
                template.transform.SetParent(gridParent);

                SpriteRenderer sr = template.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 1;

                Squishy squishyComponent = template.AddComponent<Squishy>();
                template.AddComponent<SquishyAnimator>();

                template.SetActive(false);

                squishyPool = new ObjectPool<Squishy>(squishyComponent, gridParent, 80);
            }

            // Initialize grid array
            grid = new Squishy[COLUMNS, ROWS];

            // Fill grid
            for (int x = 0; x < COLUMNS; x++)
            {
                for (int y = 0; y < ROWS; y++)
                {
                    SpawnSquishyAt(new Vector2Int(x, y), animate: false);
                }
            }

            // Start bounce-in animations with stagger
            StartCoroutine(BounceInStaggerCoroutine());
        }

        private IEnumerator BounceInStaggerCoroutine()
        {
            for (int y = 0; y < ROWS; y++)
            {
                for (int x = 0; x < COLUMNS; x++)
                {
                    Squishy squishy = grid[x, y];
                    if (squishy != null)
                    {
                        SquishyAnimator animator = squishy.GetComponent<SquishyAnimator>();
                        if (animator != null)
                        {
                            animator.PlayBounceIn();
                        }
                    }
                }
                yield return new WaitForSeconds(0.03f);
            }
        }

        public Squishy SpawnSquishyAt(Vector2Int pos, bool animate = true)
        {
            Squishy squishy = squishyPool.Get();

            // Pick random type from active types
            SquishyData data = activeTypes[UnityEngine.Random.Range(0, activeTypes.Count)];

            squishy.Initialize(data, pos, SquishySize.Normal);
            squishy.transform.position = GridToWorldPosition(pos);

            if (animate)
            {
                SquishyAnimator animator = squishy.GetComponent<SquishyAnimator>();
                if (animator != null)
                {
                    animator.PlayBounceIn();
                }
            }

            SetSquishyAt(pos, squishy);
            return squishy;
        }

        public Squishy GetSquishyAt(Vector2Int pos)
        {
            if (grid == null || !IsValidPosition(pos))
                return null;
            return grid[pos.x, pos.y];
        }

        public void SetSquishyAt(Vector2Int pos, Squishy s)
        {
            if (IsValidPosition(pos))
            {
                grid[pos.x, pos.y] = s;
            }
        }

        public void ClearCell(Vector2Int pos)
        {
            if (!IsValidPosition(pos))
                return;

            Squishy squishy = grid[pos.x, pos.y];
            if (squishy == null)
                return;

            // Check if this squishy has a ChonkyBehavior — if so, clear all occupied cells
            ChonkyBehavior chonky = squishy.GetComponent<ChonkyBehavior>();
            if (chonky != null)
            {
                List<Vector2Int> occupiedCells = chonky.OccupiedCells;
                foreach (Vector2Int cell in occupiedCells)
                {
                    if (IsValidPosition(cell))
                    {
                        grid[cell.x, cell.y] = null;
                    }
                }
            }
            else
            {
                grid[pos.x, pos.y] = null;
            }

            squishy.ReturnToPool();
            squishyPool.Return(squishy);
        }

        public bool IsValidPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < COLUMNS && pos.y >= 0 && pos.y < ROWS;
        }

        public bool IsCellEmpty(Vector2Int pos)
        {
            return grid != null && IsValidPosition(pos) && grid[pos.x, pos.y] == null;
        }

        public Vector3 GridToWorldPosition(Vector2Int pos)
        {
            return new Vector3((pos.x - 3f) * cellSize, (pos.y - 4f) * cellSize + yOffset, 0f);
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / cellSize + 3f);
            int y = Mathf.RoundToInt((worldPos.y - yOffset) / cellSize + 4f);
            return new Vector2Int(
                Mathf.Clamp(x, 0, COLUMNS - 1),
                Mathf.Clamp(y, 0, ROWS - 1)
            );
        }

        public IEnumerator ApplyGravityAndRefill()
        {
            isProcessing = true;

            // Gravity pass for each column
            for (int col = 0; col < COLUMNS; col++)
            {
                bool moved = true;
                while (moved)
                {
                    moved = false;
                    for (int row = 0; row < ROWS - 1; row++)
                    {
                        if (grid[col, row] == null && grid[col, row + 1] != null)
                        {
                            Squishy squishy = grid[col, row + 1];

                            // Skip Chonky squishies — handle them separately
                            ChonkyBehavior chonkyCheck = squishy.GetComponent<ChonkyBehavior>();
                            if (chonkyCheck != null)
                                continue;

                            grid[col, row] = squishy;
                            grid[col, row + 1] = null;
                            squishy.GridPosition = new Vector2Int(col, row);

                            Vector3 targetPos = GridToWorldPosition(new Vector2Int(col, row));
                            AnimateFall(squishy, targetPos);
                            moved = true;
                        }
                    }
                }
                yield return new WaitForSeconds(0.02f); // slight column stagger
            }

            yield return new WaitForSeconds(0.15f); // wait for falling animations

            // Refill pass
            for (int col = 0; col < COLUMNS; col++)
            {
                int emptyCount = 0;
                for (int row = ROWS - 1; row >= 0; row--)
                {
                    if (grid[col, row] == null)
                    {
                        SpawnSquishyAt(new Vector2Int(col, row), animate: false);
                        Squishy squishy = grid[col, row];
                        emptyCount++;
                        // Start position above grid
                        squishy.transform.position = GridToWorldPosition(new Vector2Int(col, ROWS + emptyCount));
                        AnimateFall(squishy, GridToWorldPosition(new Vector2Int(col, row)));
                    }
                }
            }

            yield return new WaitForSeconds(0.3f); // wait for refill animations
            isProcessing = false;

            // Check for possible moves
            if (!HasPossibleMoves())
            {
                ShuffleBoard();
            }
        }

        private void AnimateFall(Squishy squishy, Vector3 target)
        {
            StartCoroutine(AnimateFallCoroutine(squishy, target));
        }

        private IEnumerator AnimateFallCoroutine(Squishy squishy, Vector3 target)
        {
            if (squishy == null)
                yield break;

            Vector3 start = squishy.transform.position;
            float duration = 0.15f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (squishy == null || squishy.gameObject == null)
                    yield break;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Ease out with slight overshoot for bounce feel
                // Using a custom curve: overshoot then settle
                float easedT;
                if (t < 0.8f)
                {
                    // Move to slightly past target
                    easedT = Mathf.Lerp(0f, 1.05f, t / 0.8f);
                }
                else
                {
                    // Bounce back to target
                    float bounceT = (t - 0.8f) / 0.2f;
                    easedT = Mathf.Lerp(1.05f, 1f, bounceT);
                }

                squishy.transform.position = Vector3.LerpUnclamped(start, target, easedT);
                yield return null;
            }

            if (squishy != null && squishy.gameObject != null)
            {
                squishy.transform.position = target;
            }
        }

        public bool HasPossibleMoves()
        {
            bool[,] visited = new bool[COLUMNS, ROWS];

            for (int x = 0; x < COLUMNS; x++)
            {
                for (int y = 0; y < ROWS; y++)
                {
                    if (visited[x, y])
                        continue;

                    Squishy squishy = grid[x, y];
                    if (squishy == null || !squishy.IsActive)
                        continue;

                    // Check for Chonky combo: if this squishy is Chonky/MegaChonk, check adjacent for another
                    if (squishy.Size != SquishySize.Normal)
                    {
                        if (HasAdjacentChonky(new Vector2Int(x, y)))
                            return true;
                    }

                    // Flood fill to find connected component of same type
                    int componentSize = FloodFillCount(x, y, squishy.Data.squishyType, visited);
                    if (componentSize >= 3)
                        return true;
                }
            }

            return false;
        }

        private bool HasAdjacentChonky(Vector2Int pos)
        {
            Squishy current = grid[pos.x, pos.y];
            if (current == null || current.Size == SquishySize.Normal)
                return false;

            // Check all 8 directions
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    Vector2Int neighbor = new Vector2Int(pos.x + dx, pos.y + dy);
                    if (!IsValidPosition(neighbor))
                        continue;

                    Squishy neighborSquishy = grid[neighbor.x, neighbor.y];
                    if (neighborSquishy != null && neighborSquishy != current && neighborSquishy.Size != SquishySize.Normal)
                        return true;
                }
            }

            return false;
        }

        private int FloodFillCount(int startX, int startY, SquishyType type, bool[,] visited)
        {
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(startX, startY));
            visited[startX, startY] = true;
            int count = 0;

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                count++;

                // Check all 8 directions (Chebyshev adjacency)
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        int nx = current.x + dx;
                        int ny = current.y + dy;

                        if (nx < 0 || nx >= COLUMNS || ny < 0 || ny >= ROWS)
                            continue;

                        if (visited[nx, ny])
                            continue;

                        Squishy neighbor = grid[nx, ny];
                        if (neighbor == null || !neighbor.IsActive)
                            continue;

                        if (neighbor.Data.squishyType == type)
                        {
                            visited[nx, ny] = true;
                            queue.Enqueue(new Vector2Int(nx, ny));
                        }
                    }
                }
            }

            return count;
        }

        public void ShuffleBoard()
        {
            StartCoroutine(ShuffleBoardCoroutine());
        }

        private IEnumerator ShuffleBoardCoroutine()
        {
            isProcessing = true;

            // Collect all active squishies and their positions
            List<Squishy> squishies = new List<Squishy>();
            List<Vector2Int> positions = new List<Vector2Int>();

            for (int x = 0; x < COLUMNS; x++)
            {
                for (int y = 0; y < ROWS; y++)
                {
                    Squishy squishy = grid[x, y];
                    if (squishy != null)
                    {
                        squishies.Add(squishy);
                        positions.Add(new Vector2Int(x, y));
                    }
                }
            }

            // Fisher-Yates shuffle the squishies list
            for (int i = squishies.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                Squishy temp = squishies[i];
                squishies[i] = squishies[j];
                squishies[j] = temp;
            }

            // Clear the grid
            grid = new Squishy[COLUMNS, ROWS];

            // Reassign shuffled squishies to positions
            for (int i = 0; i < squishies.Count && i < positions.Count; i++)
            {
                Vector2Int pos = positions[i];
                Squishy squishy = squishies[i];
                grid[pos.x, pos.y] = squishy;
                squishy.GridPosition = pos;

                // Animate to new position
                Vector3 targetWorldPos = GridToWorldPosition(pos);
                AnimateFall(squishy, targetWorldPos);
            }

            yield return new WaitForSeconds(0.3f);
            isProcessing = false;

            // Re-check for possible moves; if still none, shuffle again
            if (!HasPossibleMoves())
            {
                ShuffleBoard();
            }
        }

        public void SetActiveTypeCount(int count)
        {
            activeTypeCount = Mathf.Clamp(count, 4, 7);
            activeTypes = SquishyDatabase.AllTypes.Take(activeTypeCount).ToList();
        }

        public List<SquishyData> GetActiveSquishyTypes()
        {
            return activeTypes;
        }

        public List<Vector2Int> GetOccupiedCellsByChonky(Squishy chonky)
        {
            if (chonky == null)
                return new List<Vector2Int>();

            ChonkyBehavior chonkyBehavior = chonky.GetComponent<ChonkyBehavior>();
            if (chonkyBehavior != null)
            {
                return chonkyBehavior.OccupiedCells;
            }

            return new List<Vector2Int> { chonky.GridPosition };
        }

        public Vector2Int? FindValidChonkyPosition(Vector2Int centroid, SquishySize size)
        {
            int dim;
            switch (size)
            {
                case SquishySize.Chonky:
                    dim = 2;
                    break;
                case SquishySize.MegaChonk:
                    dim = 3;
                    break;
                default:
                    return centroid;
            }

            // Search in expanding rings from the centroid
            int maxRadius = Mathf.Max(COLUMNS, ROWS);
            for (int radius = 0; radius < maxRadius; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius && radius > 0)
                            continue; // Only check the ring perimeter (except radius 0)

                        Vector2Int candidate = new Vector2Int(centroid.x + dx, centroid.y + dy);

                        // Check if the entire rect fits and all cells are empty
                        if (IsRectValid(candidate, dim))
                        {
                            return candidate;
                        }
                    }
                }
            }

            return null;
        }

        private bool IsRectValid(Vector2Int bottomLeft, int dim)
        {
            for (int x = bottomLeft.x; x < bottomLeft.x + dim; x++)
            {
                for (int y = bottomLeft.y; y < bottomLeft.y + dim; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    if (!IsValidPosition(cell) || !IsCellEmpty(cell))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void PlaceChonky(SquishyData data, Vector2Int position, SquishySize size)
        {
            Squishy squishy = squishyPool.Get();
            squishy.Initialize(data, position, size);

            // Add or get ChonkyBehavior
            ChonkyBehavior chonky = squishy.GetComponent<ChonkyBehavior>();
            if (chonky == null)
            {
                chonky = squishy.gameObject.AddComponent<ChonkyBehavior>();
            }
            chonky.Initialize(position, size);

            // Position at the center of the occupied area
            int dim = size == SquishySize.MegaChonk ? 3 : 2;
            float centerX = position.x + (dim - 1) / 2f;
            float centerY = position.y + (dim - 1) / 2f;
            Vector3 worldCenter = new Vector3(
                (centerX - 3f) * cellSize,
                (centerY - 4f) * cellSize + yOffset,
                0f
            );
            squishy.transform.position = worldCenter;

            // Set all occupied cells in grid to reference this squishy
            List<Vector2Int> occupiedCells = chonky.OccupiedCells;
            foreach (Vector2Int cell in occupiedCells)
            {
                if (IsValidPosition(cell))
                {
                    grid[cell.x, cell.y] = squishy;
                }
            }

            // Play bounce-in animation
            SquishyAnimator animator = squishy.GetComponent<SquishyAnimator>();
            if (animator != null)
            {
                animator.PlayBounceIn();
            }
        }
    }
}
