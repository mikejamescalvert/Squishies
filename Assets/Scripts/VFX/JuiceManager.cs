using System.Collections;
using UnityEngine;
using TMPro;

namespace Squishies
{
    public class JuiceManager : MonoBehaviour
    {
        public static JuiceManager Instance { get; private set; }

        private static readonly string[] ComboTexts = { "Nice!", "Great!", "Amazing!", "INCREDIBLE!" };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Plays a small particle pop effect at the given position.
        /// </summary>
        public void PlayPopEffect(Vector3 worldPos, Color color)
        {
            if (ParticleManager.Instance != null)
            {
                ParticleManager.Instance.PlayBurst(worldPos, color, 8);
            }
        }

        /// <summary>
        /// Plays a larger merge effect with particles and a zoom punch.
        /// </summary>
        public void PlayMergeEffect(Vector3 worldPos, Color color)
        {
            if (ParticleManager.Instance != null)
            {
                ParticleManager.Instance.PlayBurst(worldPos, color, 15);
            }

            CameraEffects.Instance?.ZoomPunch();
        }

        /// <summary>
        /// Displays animated combo text that scales up, floats upward, and fades out.
        /// </summary>
        public void PlayComboText(int comboLevel, Vector3 worldPos)
        {
            StartCoroutine(ComboTextCoroutine(comboLevel, worldPos));
        }

        private IEnumerator ComboTextCoroutine(int comboLevel, Vector3 worldPos)
        {
            // Clamp combo level to valid range
            int index = Mathf.Clamp(comboLevel - 1, 0, ComboTexts.Length - 1);
            string text = ComboTexts[index];

            // Create world-space TextMeshPro object
            GameObject textObj = new GameObject("ComboText");
            textObj.transform.position = worldPos;

            TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = 4f + comboLevel * 1.5f; // Larger for higher combos
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = 20;

            // Configure for world space visibility
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;

            // Outline for readability
            tmp.fontMaterial.EnableKeyword("OUTLINE_ON");
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f);
            tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);

            // Start at scale 0
            textObj.transform.localScale = Vector3.zero;

            // Phase 1: Scale up 0 -> 1.5 -> 1.0 over 0.2s
            float scaleUpDuration = 0.2f;
            float elapsed = 0f;

            // Scale from 0 to 1.5 (first half)
            while (elapsed < scaleUpDuration * 0.5f)
            {
                float t = elapsed / (scaleUpDuration * 0.5f);
                float scale = Mathf.Lerp(0f, 1.5f, t);
                textObj.transform.localScale = Vector3.one * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Scale from 1.5 to 1.0 (second half)
            elapsed = 0f;
            while (elapsed < scaleUpDuration * 0.5f)
            {
                float t = elapsed / (scaleUpDuration * 0.5f);
                float scale = Mathf.Lerp(1.5f, 1.0f, t);
                textObj.transform.localScale = Vector3.one * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }

            textObj.transform.localScale = Vector3.one;

            // Phase 2: Float up by 1.5 units over 0.8s while fading alpha to 0
            float floatDuration = 0.8f;
            elapsed = 0f;
            Vector3 startPos = textObj.transform.position;
            Color startColor = tmp.color;

            while (elapsed < floatDuration)
            {
                float t = elapsed / floatDuration;

                // Float upward
                textObj.transform.position = startPos + new Vector3(0f, 1.5f * t, 0f);

                // Fade out alpha
                tmp.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(textObj);
        }

        /// <summary>
        /// Triggers a camera screen shake with the given intensity.
        /// </summary>
        public void PlayScreenShake(float intensity = 0.3f)
        {
            CameraEffects.Instance?.Shake(0.3f, intensity);
        }

        /// <summary>
        /// Plays a confetti burst with multiple random colors.
        /// </summary>
        public void PlayConfetti(Vector3 worldPos)
        {
            if (ParticleManager.Instance == null) return;

            // Emit several bursts with different bright colors for a confetti effect
            Color[] confettiColors = new Color[]
            {
                Color.red,
                Color.yellow,
                Color.green,
                Color.cyan,
                Color.magenta,
                new Color(1f, 0.5f, 0f), // Orange
                new Color(0.5f, 0f, 1f)  // Purple
            };

            // Pick a random color for the main burst
            Color mainColor = confettiColors[Random.Range(0, confettiColors.Length)];
            ParticleManager.Instance.PlayBurst(worldPos, mainColor, 25);
        }

        /// <summary>
        /// Displays a floating score popup that drifts upward and fades.
        /// </summary>
        public void PlayScorePopup(int score, Vector3 worldPos)
        {
            StartCoroutine(ScorePopupCoroutine(score, worldPos));
        }

        private IEnumerator ScorePopupCoroutine(int score, Vector3 worldPos)
        {
            // Create world-space TextMeshPro object
            GameObject textObj = new GameObject("ScorePopup");
            textObj.transform.position = worldPos;

            TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
            tmp.text = $"+{score}";
            tmp.fontSize = 3.5f;
            tmp.color = new Color(1f, 1f, 0.3f, 1f); // Yellow-ish
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = 20;

            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;

            // Outline for readability
            tmp.fontMaterial.EnableKeyword("OUTLINE_ON");
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.15f);
            tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.2f, 0.1f, 0f));

            // Start at full scale
            textObj.transform.localScale = Vector3.one;

            // Float up by 1 unit over 0.6s while fading alpha to 0
            float duration = 0.6f;
            float elapsed = 0f;
            Vector3 startPos = textObj.transform.position;
            Color startColor = tmp.color;

            while (elapsed < duration)
            {
                float t = elapsed / duration;

                // Float upward
                textObj.transform.position = startPos + new Vector3(0f, 1f * t, 0f);

                // Fade out alpha (ease out)
                float alpha = 1f - (t * t);
                tmp.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

                // Slight scale up for pop feel
                float scale = 1f + 0.2f * (1f - t);
                textObj.transform.localScale = Vector3.one * scale;

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(textObj);
        }
    }
}
