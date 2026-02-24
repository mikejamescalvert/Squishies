using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Squishies
{
    public class AbilitySystem : MonoBehaviour
    {
        public static AbilitySystem Instance { get; private set; }
        public static bool WildcardActive = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ExecuteAbility(MegaChonkAbility ability, Vector2Int position)
        {
            StartCoroutine(ExecuteAbilityRoutine(ability, position));
        }

        private IEnumerator ExecuteAbilityRoutine(MegaChonkAbility ability, Vector2Int position)
        {
            switch (ability)
            {
                case MegaChonkAbility.RadialBurst:
                    yield return RadialBurstRoutine(position);
                    break;
                case MegaChonkAbility.RowClear:
                    yield return RowClearRoutine(position);
                    break;
                case MegaChonkAbility.ColumnClear:
                    yield return ColumnClearRoutine(position);
                    break;
                case MegaChonkAbility.ColorDrain:
                    yield return ColorDrainRoutine(position);
                    break;
                case MegaChonkAbility.Shuffle:
                    yield return ShuffleRoutine();
                    break;
                case MegaChonkAbility.HappinessBurst:
                    yield return HappinessBurstRoutine();
                    break;
                case MegaChonkAbility.Wildcard:
                    WildcardActive = true;
                    break;
            }
        }

        private IEnumerator RadialBurstRoutine(Vector2Int position)
        {
            List<Vector2Int> toClear = new List<Vector2Int>();
            int radius = 2;

            for (int x = position.x - radius; x <= position.x + radius; x++)
            {
                for (int y = position.y - radius; y <= position.y + radius; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (!GridManager.Instance.IsValidPosition(pos))
                        continue;

                    Squishy squishy = GridManager.Instance.GetSquishyAt(pos);
                    if (squishy != null && squishy.IsActive)
                    {
                        toClear.Add(pos);
                    }
                }
            }

            yield return ClearPositionsAndRefill(toClear);
        }

        private IEnumerator RowClearRoutine(Vector2Int position)
        {
            List<Vector2Int> toClear = new List<Vector2Int>();

            for (int x = 0; x < GridManager.COLUMNS; x++)
            {
                Vector2Int pos = new Vector2Int(x, position.y);
                Squishy squishy = GridManager.Instance.GetSquishyAt(pos);
                if (squishy != null && squishy.IsActive)
                {
                    toClear.Add(pos);
                }
            }

            yield return ClearPositionsAndRefill(toClear);
        }

        private IEnumerator ColumnClearRoutine(Vector2Int position)
        {
            List<Vector2Int> toClear = new List<Vector2Int>();

            for (int y = 0; y < GridManager.ROWS; y++)
            {
                Vector2Int pos = new Vector2Int(position.x, y);
                Squishy squishy = GridManager.Instance.GetSquishyAt(pos);
                if (squishy != null && squishy.IsActive)
                {
                    toClear.Add(pos);
                }
            }

            yield return ClearPositionsAndRefill(toClear);
        }

        private IEnumerator ColorDrainRoutine(Vector2Int position)
        {
            Squishy megaChonk = GridManager.Instance.GetSquishyAt(position);
            SquishyType ownType = megaChonk != null ? megaChonk.Data.squishyType : SquishyType.Bloop;

            // Collect all types currently on the grid
            List<SquishyType> typesOnGrid = new List<SquishyType>();
            for (int x = 0; x < GridManager.COLUMNS; x++)
            {
                for (int y = 0; y < GridManager.ROWS; y++)
                {
                    Squishy s = GridManager.Instance.GetSquishyAt(new Vector2Int(x, y));
                    if (s != null && s.IsActive && !typesOnGrid.Contains(s.Data.squishyType))
                    {
                        typesOnGrid.Add(s.Data.squishyType);
                    }
                }
            }

            // Remove own type from candidates
            typesOnGrid.Remove(ownType);

            if (typesOnGrid.Count == 0)
                yield break;

            // Pick a random type to drain
            SquishyType targetType = typesOnGrid[Random.Range(0, typesOnGrid.Count)];

            // Collect all squishies of that type
            List<Vector2Int> toClear = new List<Vector2Int>();
            for (int x = 0; x < GridManager.COLUMNS; x++)
            {
                for (int y = 0; y < GridManager.ROWS; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Squishy s = GridManager.Instance.GetSquishyAt(pos);
                    if (s != null && s.IsActive && s.Data.squishyType == targetType)
                    {
                        toClear.Add(pos);
                    }
                }
            }

            yield return ClearPositionsAndRefill(toClear);
        }

        private IEnumerator ShuffleRoutine()
        {
            // Delegate to GridManager's shuffle which properly updates the grid array
            GridManager.Instance.ShuffleBoard();
            JuiceManager.Instance?.PlayScreenShake(0.5f);
            yield return new WaitForSeconds(0.3f);
        }

        private IEnumerator HappinessBurstRoutine()
        {
            if (MoodSystem.Instance != null)
            {
                MoodSystem.Instance.MakeAllHappy(5);
            }

            JuiceManager.Instance?.PlayConfetti(Vector3.zero);
            JuiceManager.Instance?.PlayScreenShake(0.5f);

            yield return null;
        }

        private IEnumerator ClearPositionsAndRefill(List<Vector2Int> toClear)
        {
            int clearedCount = 0;

            foreach (Vector2Int pos in toClear)
            {
                Squishy squishy = GridManager.Instance.GetSquishyAt(pos);
                if (squishy != null && squishy.IsActive)
                {
                    Vector3 worldPos = GridManager.Instance.GridToWorldPosition(pos);
                    Color color = squishy.Data.color;

                    GridManager.Instance.ClearCell(pos);
                    JuiceManager.Instance?.PlayPopEffect(worldPos, color);
                    clearedCount++;
                }
            }

            if (clearedCount > 0)
            {
                ScoreManager.Instance?.AddMatchScore(clearedCount, 1f);
                JuiceManager.Instance?.PlayScreenShake(0.5f);

                yield return new WaitForSeconds(0.1f);

                yield return GridManager.Instance.ApplyGravityAndRefill();
            }
        }
    }
}
