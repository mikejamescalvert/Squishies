using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Squishies
{
    /// <summary>
    /// Manages the game over overlay panel. Displays final score, best score,
    /// and provides play again / menu buttons. Uses CanvasGroup for fade animations.
    /// </summary>
    public class GameOverPanel : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private TextMeshProUGUI finalScoreText;
        private TextMeshProUGUI bestScoreLabel;
        private TextMeshProUGUI newBestLabel;
        private Button playAgainButton;
        private Button menuButton;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            finalScoreText = FindChild<TextMeshProUGUI>("FinalScoreText");
            bestScoreLabel = FindChild<TextMeshProUGUI>("BestScoreLabel");
            newBestLabel = FindChild<TextMeshProUGUI>("NewBestLabel");
            playAgainButton = FindChild<Button>("PlayAgainButton");
            menuButton = FindChild<Button>("MenuButton");

            // Start hidden
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (newBestLabel != null)
            {
                newBestLabel.gameObject.SetActive(false);
            }

            // Wire up button listeners
            if (playAgainButton != null)
            {
                playAgainButton.onClick.AddListener(OnPlayAgain);
            }

            if (menuButton != null)
            {
                menuButton.onClick.AddListener(OnMenu);
            }
        }

        /// <summary>
        /// Shows the game over panel with the final results. Plays fade-in, score count-up,
        /// and scale punch animations.
        /// </summary>
        /// <param name="finalScore">The player's final score.</param>
        /// <param name="bestScore">The best score for the current mode.</param>
        /// <param name="isNewBest">Whether the player achieved a new best score.</param>
        public void Show(int finalScore, int bestScore, bool isNewBest)
        {
            gameObject.SetActive(true);

            if (bestScoreLabel != null)
            {
                bestScoreLabel.text = $"Best: {bestScore:N0}";
            }

            if (newBestLabel != null)
            {
                newBestLabel.gameObject.SetActive(isNewBest);
                if (isNewBest)
                {
                    newBestLabel.text = "New Best!";
                }
            }

            // Start with score at zero for the count-up animation
            if (finalScoreText != null)
            {
                finalScoreText.text = "Score: 0";
            }

            // Fade in the panel
            UIAnimations.FadeIn(this, canvasGroup, 0.3f);

            // After a short delay, animate the score and punch it
            StartCoroutine(AnimateScoreDisplay(finalScore));
        }

        private IEnumerator AnimateScoreDisplay(int finalScore)
        {
            // Wait for fade-in to partially complete
            yield return new WaitForSecondsRealtime(0.2f);

            if (finalScoreText != null)
            {
                // Count up the score from 0 to final
                UIAnimations.CountUp(this, finalScoreText, 0, finalScore, 0.8f);
            }

            // Wait for count-up to finish, then punch
            yield return new WaitForSecondsRealtime(0.85f);

            if (finalScoreText != null)
            {
                UIAnimations.ScalePunch(this, finalScoreText.transform, 1.3f, 0.25f);
            }
        }

        /// <summary>
        /// Hides the game over panel with a fade-out animation.
        /// </summary>
        public void Hide()
        {
            UIAnimations.FadeOut(this, canvasGroup, 0.3f);
        }

        /// <summary>
        /// Called when the Play Again button is pressed. Hides the panel and restarts the game
        /// in the current mode.
        /// </summary>
        private void OnPlayAgain()
        {
            Hide();

            AudioManager audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.PlayUIClick();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame(GameManager.Instance.CurrentMode);
            }
        }

        /// <summary>
        /// Called when the Menu button is pressed. Plays a click sound and returns to the main menu.
        /// </summary>
        private void OnMenu()
        {
            AudioManager audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.PlayUIClick();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMainMenu();
            }
        }

        /// <summary>
        /// Recursively searches the transform hierarchy for a child with the given name
        /// and returns the specified component type.
        /// </summary>
        private T FindChild<T>(string name) where T : Component
        {
            Transform[] transforms = GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                if (t.name == name)
                {
                    T comp = t.GetComponent<T>();
                    if (comp != null) return comp;
                }
            }

            Debug.LogWarning($"[GameOverPanel] Could not find child '{name}' with component {typeof(T).Name}");
            return null;
        }
    }
}
