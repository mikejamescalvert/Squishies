using UnityEngine;
using UnityEngine.SceneManagement;

namespace Squishies
{
    public enum GameMode { Zen, Rush, Puzzle }
    public enum GameState { Menu, Playing, Paused, GameOver }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameMode CurrentMode { get; private set; }
        public GameState CurrentState { get; private set; }
        public float RushTimeRemaining { get; private set; }

        private int turnCount;
        private int[] zenTypeThresholds = { 2000, 5000, 10000, 20000, 35000 };

        public event System.Action<GameState> OnGameStateChanged;
        public event System.Action<float> OnRushTimeChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing && CurrentMode == GameMode.Rush)
            {
                RushTimeRemaining -= Time.deltaTime;
                if (RushTimeRemaining <= 0f)
                {
                    RushTimeRemaining = 0f;
                    EndGame();
                }
                OnRushTimeChanged?.Invoke(RushTimeRemaining);
            }
        }

        public void StartGame(GameMode mode)
        {
            CurrentMode = mode;
            CurrentState = GameState.Playing;
            turnCount = 0;

            if (ComboSystem.Instance != null)
            {
                ComboSystem.Instance.ResetCombo();
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetScore();
            }

            if (GridManager.Instance != null)
            {
                GridManager.Instance.InitializeGrid(4);
            }

            if (CurrentMode == GameMode.Rush)
            {
                RushTimeRemaining = 90f;
            }

            OnGameStateChanged?.Invoke(CurrentState);

            // Enable input
            InputHandler inputHandler = Object.FindObjectOfType<InputHandler>();
            if (inputHandler != null)
            {
                inputHandler.InputEnabled = true;
            }

            // Initialize HUD for this mode
            HUDController hud = Object.FindObjectOfType<HUDController>(true);
            if (hud != null)
            {
                hud.InitializeForMode(CurrentMode);
            }

            // Hide game over panel if visible
            GameOverPanel gameOverPanel = Object.FindObjectOfType<GameOverPanel>(true);
            if (gameOverPanel != null)
            {
                gameOverPanel.Hide();
            }
        }

        public void EndGame()
        {
            CurrentState = GameState.GameOver;

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.SaveBestScore(CurrentMode);
            }

            OnGameStateChanged?.Invoke(CurrentState);

            // Disable input
            InputHandler inputHandler = Object.FindObjectOfType<InputHandler>();
            if (inputHandler != null)
            {
                inputHandler.InputEnabled = false;
            }

            // Show game over panel
            GameOverPanel gameOverPanel = Object.FindObjectOfType<GameOverPanel>(true);
            if (gameOverPanel != null)
            {
                int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
                int bestScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetBestScore(CurrentMode) : 0;
                bool isNewBest = finalScore >= bestScore && finalScore > 0;
                gameOverPanel.Show(finalScore, bestScore, isNewBest);
            }
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                CurrentState = GameState.Paused;
                OnGameStateChanged?.Invoke(CurrentState);
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Playing;
                OnGameStateChanged?.Invoke(CurrentState);
            }
        }

        public void OnTurnComplete()
        {
            turnCount++;

            if (CurrentMode == GameMode.Zen)
            {
                int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;

                // Determine how many types should be active: 4 + number of thresholds passed
                int thresholdsPassed = 0;
                for (int i = 0; i < zenTypeThresholds.Length; i++)
                {
                    if (currentScore >= zenTypeThresholds[i])
                    {
                        thresholdsPassed++;
                    }
                    else
                    {
                        break;
                    }
                }

                int targetTypeCount = Mathf.Min(4 + thresholdsPassed, 7);

                if (GridManager.Instance != null)
                {
                    GridManager.Instance.SetActiveTypeCount(targetTypeCount);
                }
            }
            // Rush mode time check is handled in Update()
        }

        public void AddRushTime(float seconds)
        {
            if (CurrentMode == GameMode.Rush)
            {
                RushTimeRemaining += seconds;
                OnRushTimeChanged?.Invoke(RushTimeRemaining);
            }
        }

        public void LoadMainMenu()
        {
            CurrentState = GameState.Menu;
            OnGameStateChanged?.Invoke(CurrentState);

            // Show the main menu panel overlay
            MainMenuController menu = Object.FindObjectOfType<MainMenuController>(true);
            if (menu != null)
            {
                menu.ShowMenu();
            }

            // Hide game over panel if showing
            GameOverPanel gameOverPanel = Object.FindObjectOfType<GameOverPanel>(true);
            if (gameOverPanel != null)
            {
                gameOverPanel.Hide();
            }
        }
    }
}
