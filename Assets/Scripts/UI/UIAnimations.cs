using System.Collections;
using UnityEngine;
using TMPro;

namespace Squishies
{
    /// <summary>
    /// Static utility class providing common UI animations via coroutines.
    /// Each public method starts a coroutine on the provided MonoBehaviour owner.
    /// </summary>
    public static class UIAnimations
    {
        /// <summary>
        /// Scales the target up to punchScale over duration/2, then back to 1.0 over duration/2.
        /// </summary>
        public static Coroutine ScalePunch(MonoBehaviour owner, Transform target, float punchScale = 1.3f, float duration = 0.2f)
        {
            return owner.StartCoroutine(ScalePunchCoroutine(target, punchScale, duration));
        }

        /// <summary>
        /// Fades a CanvasGroup alpha from 0 to 1 over the given duration.
        /// Sets interactable and blocksRaycasts to true when finished.
        /// </summary>
        public static Coroutine FadeIn(MonoBehaviour owner, CanvasGroup group, float duration = 0.3f)
        {
            return owner.StartCoroutine(FadeInCoroutine(group, duration));
        }

        /// <summary>
        /// Fades a CanvasGroup alpha from 1 to 0 over the given duration.
        /// Sets interactable and blocksRaycasts to false when finished.
        /// </summary>
        public static Coroutine FadeOut(MonoBehaviour owner, CanvasGroup group, float duration = 0.3f)
        {
            return owner.StartCoroutine(FadeOutCoroutine(group, duration));
        }

        /// <summary>
        /// Moves the target's localPosition.y upward by the given distance over duration.
        /// If the target has a CanvasGroup, it also fades out simultaneously.
        /// </summary>
        public static Coroutine FloatUp(MonoBehaviour owner, Transform target, float distance = 50f, float duration = 0.5f)
        {
            return owner.StartCoroutine(FloatUpCoroutine(target, distance, duration));
        }

        /// <summary>
        /// Animates a number counting up from 'from' to 'to', updating the TextMeshProUGUI each frame.
        /// </summary>
        public static Coroutine CountUp(MonoBehaviour owner, TextMeshProUGUI text, int from, int to, float duration = 0.5f)
        {
            return owner.StartCoroutine(CountUpCoroutine(text, from, to, duration));
        }

        private static IEnumerator ScalePunchCoroutine(Transform target, float punchScale, float duration)
        {
            if (target == null) yield break;

            float halfDuration = duration * 0.5f;
            Vector3 originalScale = Vector3.one;
            Vector3 punchedScale = Vector3.one * punchScale;

            // Scale up phase
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                target.localScale = Vector3.Lerp(originalScale, punchedScale, t);
                yield return null;
            }
            target.localScale = punchedScale;

            // Scale back down phase
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                target.localScale = Vector3.Lerp(punchedScale, originalScale, t);
                yield return null;
            }
            target.localScale = originalScale;
        }

        private static IEnumerator FadeInCoroutine(CanvasGroup group, float duration)
        {
            if (group == null) yield break;

            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                group.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }

            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }

        private static IEnumerator FadeOutCoroutine(CanvasGroup group, float duration)
        {
            if (group == null) yield break;

            group.alpha = 1f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                group.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }

            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        private static IEnumerator FloatUpCoroutine(Transform target, float distance, float duration)
        {
            if (target == null) yield break;

            Vector3 startPos = target.localPosition;
            Vector3 endPos = startPos + new Vector3(0f, distance, 0f);
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.localPosition = Vector3.Lerp(startPos, endPos, t);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                }

                yield return null;
            }

            target.localPosition = endPos;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        private static IEnumerator CountUpCoroutine(TextMeshProUGUI text, int from, int to, float duration)
        {
            if (text == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                int current = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
                text.text = $"Score: {current:N0}";
                yield return null;
            }

            text.text = $"Score: {to:N0}";
        }
    }
}
