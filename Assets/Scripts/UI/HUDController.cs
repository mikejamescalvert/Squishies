using System;
using System.Collections;
using UnityEngine;
using TMPro;

namespace Squishies
{
    /// <summary>
    /// Manages the in-game HUD display including score, best score, combo text, and timer.
    /// Finds child UI elements by name in the transform hierarchy.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        private TextMeshProUGUI scoreText;
        private TextMeshProUGUI bestScoreText;
        private TextMeshProUGUI comboText;
        private TextMeshProUGUI timerText;

        private Coroutine comboHideCoroutine;
        private Coroutine timerPulseCoroutine;

        private static readonly Color GoldColor = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TimerWarningColor = Color.red;

        private void Awake()
        {
            scoreText = FindChildTMP("ScoreText");
            bestScoreText = FindChildTMP("BestScoreText");
            comboText = FindChildTMP("ComboText");
            timerText = FindChildTMP("TimerText");

            if (comboText != null)
            {
                comboText.gameObject.SetActive(false);
            }

            if (timerText != null)
            {
                timerText.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += UpdateScore;
                ScoreManager.Instance.OnBestScoreBeaten += OnBestScoreBeaten;
            }

            if (ComboSystem.Instance != null)
            {
                ComboSystem.Instance.OnComboChanged += OnComboChanged;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRushTimeChanged += UpdateTimer;
            }

            UpdateScore(0);

            if (ScoreManager.Instance != null && GameManager.Instance != null)
            {
                UpdateBestScore(ScoreManager.Instance.GetBestScore(GameManager.Instance.CurrentMode));
            }
            else
            {
                UpdateBestScore(0);
            }
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= UpdateScore;
                ScoreManager.Instance.OnBestScoreBeaten -= OnBestScoreBeaten;
            }

            if (ComboSystem.Instance != null)
            {
                ComboSystem.Instance.OnComboChanged -= OnComboChanged;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRushTimeChanged -= UpdateTimer;
            }
        }

        /// <summary>
        /// Updates the score display and plays a scale punch animation.
        /// </summary>
        public void UpdateScore(int score)
        {
            if (scoreText == null) return;
            scoreText.text = $"Score: {score:N0}";
            UIAnimations.ScalePunch(this, scoreText.transform);
        }

        /// <summary>
        /// Updates the best score display text.
        /// </summary>
        public void UpdateBestScore(int best)
        {
            if (bestScoreText == null) return;
            bestScoreText.text = $"Best: {best:N0}";
        }

        /// <summary>
        /// Called when the player beats the best score. Updates display with emphasis.
        /// </summary>
        private void OnBestScoreBeaten(int score)
        {
            UpdateBestScore(score);

            if (bestScoreText != null)
            {
                UIAnimations.ScalePunch(this, bestScoreText.transform, 1.5f, 0.3f);
                StartCoroutine(FlashBestScoreGold());
            }
        }

        private IEnumerator FlashBestScoreGold()
        {
            if (bestScoreText == null) yield break;

            Color originalColor = bestScoreText.color;
            bestScoreText.color = GoldColor;

            yield return new WaitForSecondsRealtime(1f);

            // Lerp back to original color
            float elapsed = 0f;
            float fadeDuration = 0.5f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                bestScoreText.color = Color.Lerp(GoldColor, originalColor, t);
                yield return null;
            }
            bestScoreText.color = originalColor;
        }

        /// <summary>
        /// Handles combo changes. Shows combo text with appropriate message and hides it after a delay.
        /// </summary>
        private void OnComboChanged(int combo)
        {
            if (comboText == null) return;

            if (combo > 0)
            {
                comboText.gameObject.SetActive(true);

                switch (combo)
                {
                    case 1:
                        comboText.text = "Nice!";
                        break;
                    case 2:
                        comboText.text = "Great!";
                        break;
                    case 3:
                        comboText.text = "Amazing!";
                        break;
                    default:
                        comboText.text = "INCREDIBLE!";
                        break;
                }

                UIAnimations.ScalePunch(this, comboText.transform, 1.4f, 0.25f);

                // Cancel any existing hide coroutine and start a new one
                if (comboHideCoroutine != null)
                {
                    StopCoroutine(comboHideCoroutine);
                }
                comboHideCoroutine = StartCoroutine(HideComboAfterDelay(1.5f));
            }
            else
            {
                comboText.gameObject.SetActive(false);
                if (comboHideCoroutine != null)
                {
                    StopCoroutine(comboHideCoroutine);
                    comboHideCoroutine = null;
                }
            }
        }

        private IEnumerator HideComboAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (comboText != null)
            {
                comboText.gameObject.SetActive(false);
            }
            comboHideCoroutine = null;
        }

        /// <summary>
        /// Updates the timer display. Turns red and pulses when time is low.
        /// </summary>
        public void UpdateTimer(float time)
        {
            if (timerText == null) return;

            timerText.text = $"{Mathf.CeilToInt(time)}s";

            if (time < 10f)
            {
                timerText.color = TimerWarningColor;

                if (timerPulseCoroutine == null)
                {
                    timerPulseCoroutine = StartCoroutine(PulseTimer());
                }
            }
            else
            {
                timerText.color = Color.white;

                if (timerPulseCoroutine != null)
                {
                    StopCoroutine(timerPulseCoroutine);
                    timerPulseCoroutine = null;
                    timerText.transform.localScale = Vector3.one;
                }
            }
        }

        private IEnumerator PulseTimer()
        {
            while (true)
            {
                UIAnimations.ScalePunch(this, timerText.transform, 1.2f, 0.5f);
                yield return new WaitForSecondsRealtime(1f);
            }
        }

        /// <summary>
        /// Shows or hides the timer display.
        /// </summary>
        public void ShowTimer(bool show)
        {
            if (timerText != null)
            {
                timerText.gameObject.SetActive(show);
            }

            if (!show && timerPulseCoroutine != null)
            {
                StopCoroutine(timerPulseCoroutine);
                timerPulseCoroutine = null;
            }
        }

        /// <summary>
        /// Initializes HUD elements appropriate for the given game mode.
        /// Shows timer for Rush mode, hides it otherwise. Updates best score.
        /// </summary>
        public void InitializeForMode(GameMode mode)
        {
            ShowTimer(mode == GameMode.Rush);

            if (ScoreManager.Instance != null)
            {
                UpdateBestScore(ScoreManager.Instance.GetBestScore(mode));
            }
            else
            {
                UpdateBestScore(0);
            }

            UpdateScore(0);
        }

        /// <summary>
        /// Recursively searches the transform hierarchy for a child with the given name
        /// and returns its TextMeshProUGUI component.
        /// </summary>
        private TextMeshProUGUI FindChildTMP(string name)
        {
            Transform[] transforms = GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                if (t.name == name)
                {
                    TextMeshProUGUI tmp = t.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) return tmp;
                }
            }

            Debug.LogWarning($"[HUDController] Could not find child TextMeshProUGUI named '{name}'");
            return null;
        }
    }
}
