using UnityEngine;

namespace Squishies
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        public int CurrentScore { get; private set; }

        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnBestScoreBeaten;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AddMatchScore(int squishyCount, float avgMoodMultiplier)
        {
            int points = Mathf.RoundToInt(squishyCount * 10 * avgMoodMultiplier);
            CurrentScore += points;
            OnScoreChanged?.Invoke(CurrentScore);
            CheckBestScore();
        }

        public void AddChonkyBonus()
        {
            CurrentScore += 100;
            OnScoreChanged?.Invoke(CurrentScore);
            CheckBestScore();
        }

        public void AddMegaChonkBonus()
        {
            CurrentScore += 500;
            OnScoreChanged?.Invoke(CurrentScore);
            CheckBestScore();
        }

        public void AddChonkyComboBonus()
        {
            CurrentScore += 1000;
            OnScoreChanged?.Invoke(CurrentScore);
            CheckBestScore();
        }

        public int GetBestScore(GameMode mode)
        {
            return PlayerPrefs.GetInt("BestScore_" + mode.ToString(), 0);
        }

        public void SaveBestScore(GameMode mode)
        {
            if (CurrentScore > GetBestScore(mode))
            {
                PlayerPrefs.SetInt("BestScore_" + mode.ToString(), CurrentScore);
                PlayerPrefs.Save();
            }
        }

        public void ResetScore()
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        private void CheckBestScore()
        {
            // Check against all modes — the caller context determines which mode is active,
            // but we check against the current game mode from GameManager
            if (GameManager.Instance != null)
            {
                GameMode currentMode = GameManager.Instance.CurrentMode;
                int best = GetBestScore(currentMode);
                if (CurrentScore > best)
                {
                    OnBestScoreBeaten?.Invoke(CurrentScore);
                }
            }
        }
    }
}
