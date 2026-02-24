using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Squishies
{
    /// <summary>
    /// Controls the main menu panel overlay. Allows the player to select a game mode
    /// (Zen or Rush) and displays best scores for each mode. The menu is a full-screen
    /// panel within the Game scene rather than a separate scene.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private Button zenButton;
        private Button rushButton;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI zenBestText;
        private TextMeshProUGUI rushBestText;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            zenButton = FindChild<Button>("ZenButton");
            rushButton = FindChild<Button>("RushButton");
            titleText = FindChild<TextMeshProUGUI>("TitleText");
            zenBestText = FindChild<TextMeshProUGUI>("ZenBestText");
            rushBestText = FindChild<TextMeshProUGUI>("RushBestText");

            // Wire up button listeners
            if (zenButton != null)
            {
                zenButton.onClick.AddListener(OnZenModeSelected);
            }

            if (rushButton != null)
            {
                rushButton.onClick.AddListener(OnRushModeSelected);
            }
        }

        private void Start()
        {
            // Animate the title with a scale bounce
            if (titleText != null)
            {
                titleText.transform.localScale = Vector3.zero;
                StartCoroutine(AnimateTitleEntrance());
            }

            // Update best scores display
            UpdateBestScores();
        }

        /// <summary>
        /// Animates the title text scaling in with a bounce effect.
        /// </summary>
        private IEnumerator AnimateTitleEntrance()
        {
            float duration = 0.5f;
            float overshoot = 1.15f;
            Vector3 targetScale = Vector3.one;
            Vector3 overshootScale = Vector3.one * overshoot;

            // Scale from zero to overshoot
            float elapsed = 0f;
            float halfDuration = duration * 0.6f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                // Ease out curve for a snappy feel
                float easedT = 1f - (1f - t) * (1f - t);
                titleText.transform.localScale = Vector3.Lerp(Vector3.zero, overshootScale, easedT);
                yield return null;
            }
            titleText.transform.localScale = overshootScale;

            // Settle back to normal scale
            elapsed = 0f;
            float settleDuration = duration * 0.4f;
            while (elapsed < settleDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / settleDuration);
                titleText.transform.localScale = Vector3.Lerp(overshootScale, targetScale, t);
                yield return null;
            }
            titleText.transform.localScale = targetScale;
        }

        /// <summary>
        /// Updates the best score labels for each game mode.
        /// Falls back to PlayerPrefs if ScoreManager is not yet available.
        /// </summary>
        private void UpdateBestScores()
        {
            if (ScoreManager.Instance != null)
            {
                int zenBest = ScoreManager.Instance.GetBestScore(GameMode.Zen);
                int rushBest = ScoreManager.Instance.GetBestScore(GameMode.Rush);

                if (zenBestText != null)
                {
                    zenBestText.text = $"Best: {zenBest:N0}";
                }

                if (rushBestText != null)
                {
                    rushBestText.text = $"Best: {rushBest:N0}";
                }
            }
            else
            {
                // ScoreManager not available yet (first load), read from PlayerPrefs directly
                int zenBest = PlayerPrefs.GetInt("BestScore_Zen", 0);
                int rushBest = PlayerPrefs.GetInt("BestScore_Rush", 0);

                if (zenBestText != null)
                {
                    zenBestText.text = $"Best: {zenBest:N0}";
                }

                if (rushBestText != null)
                {
                    rushBestText.text = $"Best: {rushBest:N0}";
                }
            }
        }

        /// <summary>
        /// Called when the Zen mode button is pressed. Hides the menu and starts Zen mode.
        /// </summary>
        public void OnZenModeSelected()
        {
            AudioManager audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.PlayUIClick();
            }

            HideMenu();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame(GameMode.Zen);
            }
        }

        /// <summary>
        /// Called when the Rush mode button is pressed. Hides the menu and starts Rush mode.
        /// </summary>
        public void OnRushModeSelected()
        {
            AudioManager audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.PlayUIClick();
            }

            HideMenu();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame(GameMode.Rush);
            }
        }

        /// <summary>
        /// Shows the main menu panel with a fade-in animation and refreshes best scores.
        /// </summary>
        public void ShowMenu()
        {
            gameObject.SetActive(true);
            UpdateBestScores();

            if (canvasGroup != null)
            {
                UIAnimations.FadeIn(this, canvasGroup, 0.3f);
            }
        }

        /// <summary>
        /// Hides the main menu panel with a fade-out animation.
        /// Falls back to disabling the gameObject if no CanvasGroup is present.
        /// </summary>
        public void HideMenu()
        {
            if (canvasGroup != null)
            {
                UIAnimations.FadeOut(this, canvasGroup, 0.3f);
            }
            else
            {
                gameObject.SetActive(false);
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

            Debug.LogWarning($"[MainMenuController] Could not find child '{name}' with component {typeof(T).Name}");
            return null;
        }
    }
}
