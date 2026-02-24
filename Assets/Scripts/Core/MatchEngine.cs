using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Squishies
{
    public class MatchEngine : MonoBehaviour
    {
        public static MatchEngine Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Returns true if two grid positions are adjacent (Chebyshev distance <= 1 and not the same cell).
        /// </summary>
        public bool IsAdjacent(Vector2Int a, Vector2Int b)
        {
            if (a == b)
                return false;

            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return dx <= 1 && dy <= 1;
        }

        /// <summary>
        /// Determines if a new cell can be added to the current path.
        /// Handles backtracking, type matching, wildcard, and Chonky combos.
        /// Returns true if valid (and modifies path for backtrack case).
        /// </summary>
        public bool CanExtendPath(List<Vector2Int> currentPath, Vector2Int newCell)
        {
            // 1. Must be a valid grid position
            if (!GridManager.Instance.IsValidPosition(newCell))
                return false;

            // 3. Must be adjacent to last cell in path
            if (currentPath.Count > 0 && !IsAdjacent(currentPath[currentPath.Count - 1], newCell))
                return false;

            // 5. Backtrack check: if this is the second-to-last cell, undo the last step
            if (currentPath.Count >= 2 && newCell == currentPath[currentPath.Count - 2])
            {
                currentPath.RemoveAt(currentPath.Count - 1);
                return true;
            }

            // 2. Must not already be in path
            if (currentPath.Contains(newCell))
                return false;

            // 4. Squishy at newCell must exist and be active
            Squishy squishyAtNew = GridManager.Instance.GetSquishyAt(newCell);
            if (squishyAtNew == null || !squishyAtNew.IsActive)
                return false;

            // If path is empty, this is the starting cell — always valid
            if (currentPath.Count == 0)
                return true;

            Squishy firstSquishy = GridManager.Instance.GetSquishyAt(currentPath[0]);
            if (firstSquishy == null)
                return false;

            // 8. Special: Chonky combo — two Chonky/MegaChonk squishies adjacent, path length 1 going to 2
            if (currentPath.Count == 1)
            {
                if (firstSquishy.Size != SquishySize.Normal && squishyAtNew.Size != SquishySize.Normal)
                {
                    // Allow Chonky combo regardless of type
                    return true;
                }
            }

            // 6. Wildcard check
            if (AbilitySystem.WildcardActive)
                return true;

            // 7. Type must match the first squishy in path
            if (squishyAtNew.Data.squishyType != firstSquishy.Data.squishyType)
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if both squishies are Chonky or MegaChonk.
        /// </summary>
        public bool IsChonkyCombo(Squishy a, Squishy b)
        {
            if (a == null || b == null)
                return false;

            return a.Size != SquishySize.Normal && b.Size != SquishySize.Normal;
        }

        /// <summary>
        /// Executes a match from the given path. Starts a coroutine internally.
        /// </summary>
        public void ExecuteMatch(List<Vector2Int> path)
        {
            StartCoroutine(ExecuteMatchCoroutine(path));
        }

        private IEnumerator ExecuteMatchCoroutine(List<Vector2Int> path)
        {
            // 1. Guard checks
            if (path == null || path.Count < 2)
                yield break;

            // 2. Check for Chonky combo
            Squishy firstSquishy = GridManager.Instance.GetSquishyAt(path[0]);
            Squishy secondSquishy = path.Count >= 2 ? GridManager.Instance.GetSquishyAt(path[1]) : null;
            bool isChonkCombo = path.Count == 2 && firstSquishy != null && secondSquishy != null &&
                                IsChonkyCombo(firstSquishy, secondSquishy);

            // If path has less than 3 cells and it's not a chonky combo, abort
            if (path.Count < 3 && !isChonkCombo)
                yield break;

            // 3. Collect all squishies, their world positions, and unique cells
            List<Squishy> pathSquishies = new List<Squishy>();
            HashSet<Vector2Int> uniqueCells = new HashSet<Vector2Int>();
            List<Vector3> worldPositions = new List<Vector3>();

            foreach (Vector2Int pos in path)
            {
                Squishy squishy = GridManager.Instance.GetSquishyAt(pos);
                if (squishy == null)
                    continue;

                pathSquishies.Add(squishy);

                // 4. For Chonky squishies, collect all their occupied cells
                List<Vector2Int> occupiedCells = GridManager.Instance.GetOccupiedCellsByChonky(squishy);
                foreach (Vector2Int cell in occupiedCells)
                {
                    uniqueCells.Add(cell);
                }

                worldPositions.Add(squishy.transform.position);
            }

            if (pathSquishies.Count == 0)
                yield break;

            // 5. Calculate average mood multiplier
            float avgMoodMult = 1f;
            if (MoodSystem.Instance != null)
            {
                float totalMult = 0f;
                int moodCount = 0;
                foreach (Squishy s in pathSquishies)
                {
                    totalMult += s.GetMoodMultiplier();
                    moodCount++;
                }
                if (moodCount > 0)
                    avgMoodMult = totalMult / moodCount;
            }

            // 6. Calculate centroid of all cleared cells
            Vector3 centroidWorld = Vector3.zero;
            Vector2 centroidGrid = Vector2.zero;
            foreach (Vector2Int cell in uniqueCells)
            {
                centroidWorld += GridManager.Instance.GridToWorldPosition(cell);
                centroidGrid += new Vector2(cell.x, cell.y);
            }
            if (uniqueCells.Count > 0)
            {
                centroidWorld /= uniqueCells.Count;
                centroidGrid /= uniqueCells.Count;
            }
            Vector2Int centroidGridInt = new Vector2Int(
                Mathf.RoundToInt(centroidGrid.x),
                Mathf.RoundToInt(centroidGrid.y)
            );

            // 7. Match count
            int matchCount = uniqueCells.Count;

            // 8. Determine merge result
            bool shouldCreateMegaChonk = ComboSystem.Instance != null && ComboSystem.Instance.ShouldCreateMegaChonk(path.Count);
            bool shouldCreateChonky = ComboSystem.Instance != null && ComboSystem.Instance.ShouldCreateChonky(path.Count);
            bool merging = shouldCreateMegaChonk || shouldCreateChonky;

            // 9. Animate pops/merges
            foreach (Squishy squishy in pathSquishies)
            {
                SquishyAnimator animator = squishy.GetComponent<SquishyAnimator>();
                if (animator != null)
                {
                    if (merging)
                    {
                        animator.PlayMerge(centroidWorld, null);
                    }
                    else
                    {
                        animator.PlayPop(null);
                    }
                }
            }

            // 10. Wait for pop animations
            yield return new WaitForSeconds(0.2f);

            // 11. Clear cells and play VFX
            SquishyData matchData = firstSquishy != null ? firstSquishy.Data : null;
            foreach (Vector2Int cell in uniqueCells)
            {
                Vector3 cellWorldPos = GridManager.Instance.GridToWorldPosition(cell);

                // Play pop VFX
                if (matchData != null)
                {
                    // Use JuiceManager via null-conditional
                    JuiceManager juiceManager = JuiceManager.Instance;
                    if (juiceManager != null)
                    {
                        juiceManager.PlayPopEffect(cellWorldPos, matchData.color);
                    }
                }

                GridManager.Instance.ClearCell(cell);
            }

            // 12. Score
            if (isChonkCombo)
            {
                ScoreManager.Instance?.AddChonkyComboBonus();

                // Massive screen shake and confetti
                JuiceManager juiceInstance = JuiceManager.Instance;
                if (juiceInstance != null)
                {
                    juiceInstance.PlayScreenShake(1.0f);
                    juiceInstance.PlayConfetti(centroidWorld);
                }
            }
            else
            {
                ScoreManager.Instance?.AddMatchScore(matchCount, avgMoodMult);
            }

            // 13. Create Chonky/MegaChonk if applicable
            if (shouldCreateMegaChonk && matchData != null)
            {
                Vector2Int? validPos = GridManager.Instance.FindValidChonkyPosition(centroidGridInt, SquishySize.MegaChonk);
                if (validPos.HasValue)
                {
                    GridManager.Instance.PlaceChonky(matchData, validPos.Value, SquishySize.MegaChonk);
                    ScoreManager.Instance?.AddMegaChonkBonus();
                }

                AbilitySystem abilitySystem = AbilitySystem.Instance;
                if (abilitySystem != null)
                {
                    abilitySystem.ExecuteAbility(matchData.megaAbility, centroidGridInt);
                }

                GameManager.Instance?.AddRushTime(5f);
            }
            else if (shouldCreateChonky && matchData != null)
            {
                Vector2Int? validPos = GridManager.Instance.FindValidChonkyPosition(centroidGridInt, SquishySize.Chonky);
                if (validPos.HasValue)
                {
                    GridManager.Instance.PlaceChonky(matchData, validPos.Value, SquishySize.Chonky);
                    ScoreManager.Instance?.AddChonkyBonus();
                }

                GameManager.Instance?.AddRushTime(2f);
            }

            if (isChonkCombo)
            {
                GameManager.Instance?.AddRushTime(10f);
            }

            // 14. Register match with combo system
            if (ComboSystem.Instance != null)
            {
                ComboSystem.Instance.RegisterMatch(path.Count);
            }

            // 15. Audio
            AudioManager audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.PlayPop(path.Count);
            }

            // 16. Combo level feedback
            int comboLevel = ComboSystem.Instance != null ? ComboSystem.Instance.GetComboLevel() : 0;
            if (comboLevel > 0)
            {
                JuiceManager juiceInst = JuiceManager.Instance;
                if (juiceInst != null)
                {
                    juiceInst.PlayComboText(comboLevel, centroidWorld);
                }

                AudioManager audioInst = AudioManager.Instance;
                if (audioInst != null)
                {
                    audioInst.PlayCombo(comboLevel);
                }
            }

            // 17. Reset wildcard
            if (AbilitySystem.WildcardActive)
            {
                AbilitySystem.WildcardActive = false;
            }

            // 18. Mood system updates
            MoodSystem moodSystem = MoodSystem.Instance;
            if (moodSystem != null)
            {
                moodSystem.SpreadHappiness(centroidGridInt, 2);
                moodSystem.ProcessTurnAging();
            }

            // 20. Gravity and refill
            yield return GridManager.Instance.StartCoroutine(GridManager.Instance.ApplyGravityAndRefill());

            // 21. Notify game manager
            GameManager.Instance?.OnTurnComplete();
        }
    }
}
