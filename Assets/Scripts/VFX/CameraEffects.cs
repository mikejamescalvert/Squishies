using System.Collections;
using UnityEngine;

namespace Squishies
{
    public class CameraEffects : MonoBehaviour
    {
        public static CameraEffects Instance { get; private set; }

        private Vector3 originalPosition;
        private float originalSize;
        private Camera cam;

        private Coroutine shakeCoroutine;
        private Coroutine zoomCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = Camera.main;
            }

            if (cam != null)
            {
                originalPosition = cam.transform.position;
                originalSize = cam.orthographicSize;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Shakes the camera for the given duration and magnitude.
        /// Magnitude decays over time for a natural feel.
        /// </summary>
        public void Shake(float duration = 0.3f, float magnitude = 0.1f)
        {
            if (cam == null) return;

            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                cam.transform.position = originalPosition;
            }

            shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float currentMagnitude = Mathf.Lerp(magnitude, 0f, elapsed / duration);
                Vector2 offset = Random.insideUnitCircle * currentMagnitude;

                cam.transform.position = originalPosition + new Vector3(offset.x, offset.y, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            cam.transform.position = originalPosition;
            shakeCoroutine = null;
        }

        /// <summary>
        /// Performs a "zoom punch" effect: briefly zooms in then restores.
        /// Creates a satisfying impact feel.
        /// </summary>
        public void ZoomPunch(float amount = 0.1f, float duration = 0.2f)
        {
            if (cam == null || !cam.orthographic) return;

            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
                cam.orthographicSize = originalSize;
            }

            zoomCoroutine = StartCoroutine(ZoomPunchCoroutine(amount, duration));
        }

        private IEnumerator ZoomPunchCoroutine(float amount, float duration)
        {
            float halfDuration = duration / 2f;
            float elapsed = 0f;

            // Zoom in (decrease orthographic size)
            while (elapsed < halfDuration)
            {
                float t = elapsed / halfDuration;
                float smoothT = t * t * (3f - 2f * t); // Smoothstep
                cam.orthographicSize = Mathf.Lerp(originalSize, originalSize - amount, smoothT);
                elapsed += Time.deltaTime;
                yield return null;
            }

            cam.orthographicSize = originalSize - amount;

            // Zoom back out (restore orthographic size)
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                float t = elapsed / halfDuration;
                float smoothT = t * t * (3f - 2f * t); // Smoothstep
                cam.orthographicSize = Mathf.Lerp(originalSize - amount, originalSize, smoothT);
                elapsed += Time.deltaTime;
                yield return null;
            }

            cam.orthographicSize = originalSize;
            zoomCoroutine = null;
        }

        /// <summary>
        /// Updates the stored original position (call if camera moves intentionally).
        /// </summary>
        public void SetOriginalPosition(Vector3 position)
        {
            originalPosition = position;
        }

        /// <summary>
        /// Updates the stored original orthographic size (call if zoom changes intentionally).
        /// </summary>
        public void SetOriginalSize(float size)
        {
            originalSize = size;
        }
    }
}
