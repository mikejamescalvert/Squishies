using System;
using System.Collections;
using UnityEngine;

namespace Squishies
{
    public class SquishyAnimator : MonoBehaviour
    {
        private Coroutine currentAnimation;
        private Coroutine idleCoroutine;

        public void PlayIdle()
        {
            if (idleCoroutine != null)
            {
                StopCoroutine(idleCoroutine);
            }
            idleCoroutine = StartCoroutine(IdleRoutine());
        }

        private IEnumerator IdleRoutine()
        {
            float seed = Mathf.Abs(GetInstanceID()) * 0.01f;
            float speed = 2.0f + (seed % 1.0f) * 0.5f;

            while (true)
            {
                float t = Mathf.Sin(Time.time * speed + seed);
                float scale = Mathf.Lerp(0.95f, 1.05f, (t + 1f) * 0.5f);
                transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
        }

        public void PlayPop(Action onComplete = null)
        {
            StopIdleAndCurrent();
            currentAnimation = StartCoroutine(PopRoutine(onComplete));
        }

        private IEnumerator PopRoutine(Action onComplete)
        {
            // Squash
            yield return LerpScale(transform.localScale, new Vector3(0.8f, 1.2f, 1f), 0.05f);
            // Scale to zero
            yield return LerpScale(transform.localScale, new Vector3(0f, 0f, 1f), 0.15f);

            currentAnimation = null;
            onComplete?.Invoke();
        }

        public void PlayMerge(Vector3 target, Action onComplete = null)
        {
            StopIdleAndCurrent();
            currentAnimation = StartCoroutine(MergeRoutine(target, onComplete));
        }

        private IEnumerator MergeRoutine(Vector3 target, Action onComplete)
        {
            Vector3 startPos = transform.position;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = startScale * 0.5f;
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.position = Vector3.Lerp(startPos, target, t);
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            transform.position = target;
            transform.localScale = endScale;

            currentAnimation = null;
            onComplete?.Invoke();
        }

        public void PlayMoodChange(Mood mood)
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            currentAnimation = StartCoroutine(MoodChangeRoutine());
        }

        private IEnumerator MoodChangeRoutine()
        {
            Vector3 baseScale = transform.localScale;
            Vector3 punchedScale = baseScale * 1.15f;

            yield return LerpScale(baseScale, punchedScale, 0.1f);
            yield return LerpScale(punchedScale, baseScale, 0.1f);

            currentAnimation = null;
        }

        public void PlayBounceIn()
        {
            StopIdleAndCurrent();
            currentAnimation = StartCoroutine(BounceInRoutine());
        }

        private IEnumerator BounceInRoutine()
        {
            transform.localScale = Vector3.zero;

            // Scale up to 1.15
            yield return LerpScale(Vector3.zero, new Vector3(1.15f, 1.15f, 1f), 0.15f);
            // Down to 0.95
            yield return LerpScale(new Vector3(1.15f, 1.15f, 1f), new Vector3(0.95f, 0.95f, 1f), 0.1f);
            // Settle to 1.0
            yield return LerpScale(new Vector3(0.95f, 0.95f, 1f), Vector3.one, 0.08f);

            currentAnimation = null;
            PlayIdle();
        }

        public void PlayWobble()
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            currentAnimation = StartCoroutine(WobbleRoutine());
        }

        private IEnumerator WobbleRoutine()
        {
            yield return LerpRotation(0f, 5f, 0.05f);
            yield return LerpRotation(5f, -5f, 0.05f);
            yield return LerpRotation(-5f, 0f, 0.05f);

            currentAnimation = null;
        }

        public void PlaySquashStretch()
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            currentAnimation = StartCoroutine(SquashStretchRoutine());
        }

        private IEnumerator SquashStretchRoutine()
        {
            yield return LerpScale(transform.localScale, new Vector3(1.2f, 0.8f, 1f), 0.08f);
            yield return LerpScale(new Vector3(1.2f, 0.8f, 1f), new Vector3(0.85f, 1.15f, 1f), 0.08f);
            yield return LerpScale(new Vector3(0.85f, 1.15f, 1f), Vector3.one, 0.06f);

            currentAnimation = null;
        }

        public void StopAllAnimations()
        {
            StopAllCoroutines();
            idleCoroutine = null;
            currentAnimation = null;
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
        }

        private void StopIdleAndCurrent()
        {
            if (idleCoroutine != null)
            {
                StopCoroutine(idleCoroutine);
                idleCoroutine = null;
            }
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }
        }

        private IEnumerator LerpScale(Vector3 from, Vector3 to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(from, to, t);
                yield return null;
            }
            transform.localScale = to;
        }

        private IEnumerator LerpRotation(float fromZ, float toZ, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float angle = Mathf.Lerp(fromZ, toZ, t);
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
                yield return null;
            }
            transform.rotation = Quaternion.Euler(0f, 0f, toZ);
        }
    }
}
