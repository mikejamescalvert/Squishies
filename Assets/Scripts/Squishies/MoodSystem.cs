using System.Collections.Generic;
using UnityEngine;

namespace Squishies
{
    public class MoodSystem : MonoBehaviour
    {
        public static MoodSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SpreadHappiness(Vector2Int center, int radius = 2)
        {
            for (int x = center.x - radius; x <= center.x + radius; x++)
            {
                for (int y = center.y - radius; y <= center.y + radius; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (!GridManager.Instance.IsValidPosition(pos))
                        continue;

                    Squishy squishy = GridManager.Instance.GetSquishyAt(pos);
                    if (squishy != null && squishy.IsActive)
                    {
                        squishy.SetMood(Mood.Happy);
                        squishy.HappyTurnsRemaining = 5;
                    }
                }
            }
        }

        public void ProcessTurnAging()
        {
            for (int x = 0; x < GridManager.COLUMNS; x++)
            {
                for (int y = 0; y < GridManager.ROWS; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Squishy squishy = GridManager.Instance.GetSquishyAt(pos);
                    if (squishy == null || !squishy.IsActive)
                        continue;

                    if (squishy.CurrentMood == Mood.Happy)
                    {
                        squishy.HappyTurnsRemaining--;
                        if (squishy.HappyTurnsRemaining <= 0)
                        {
                            squishy.SetMood(Mood.Neutral);
                        }
                    }
                    else
                    {
                        squishy.TurnsSinceMatched++;
                        if (squishy.TurnsSinceMatched >= 8)
                        {
                            squishy.SetMood(Mood.Sad);
                        }
                    }
                }
            }
        }

        public void MakeAllHappy(int duration = 5)
        {
            for (int x = 0; x < GridManager.COLUMNS; x++)
            {
                for (int y = 0; y < GridManager.ROWS; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Squishy squishy = GridManager.Instance.GetSquishyAt(pos);
                    if (squishy != null && squishy.IsActive)
                    {
                        squishy.SetMood(Mood.Happy);
                        squishy.HappyTurnsRemaining = duration;
                    }
                }
            }
        }

        public float GetAverageMoodMultiplier(List<Vector2Int> positions)
        {
            if (positions == null || positions.Count == 0)
                return 1.0f;

            float total = 0f;
            int count = 0;

            foreach (Vector2Int pos in positions)
            {
                Squishy squishy = GridManager.Instance.GetSquishyAt(pos);
                if (squishy != null && squishy.IsActive)
                {
                    total += squishy.GetMoodMultiplier();
                    count++;
                }
            }

            if (count == 0)
                return 1.0f;

            return total / count;
        }
    }
}
