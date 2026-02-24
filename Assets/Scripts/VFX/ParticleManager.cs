using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Squishies
{
    public class ParticleManager : MonoBehaviour
    {
        public static ParticleManager Instance { get; private set; }

        private const int PoolSize = 12;
        private List<ParticleSystem> particlePool = new List<ParticleSystem>();
        private int nextPoolIndex = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CreateParticlePool();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Creates a pool of particle systems configured for burst effects.
        /// </summary>
        private void CreateParticlePool()
        {
            for (int i = 0; i < PoolSize; i++)
            {
                GameObject psObj = new GameObject($"PooledParticle_{i}");
                psObj.transform.SetParent(transform);

                ParticleSystem ps = psObj.AddComponent<ParticleSystem>();

                // Stop the default playing behavior
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                // Main module configuration
                var main = ps.main;
                main.startLifetime = 0.5f;
                main.startSpeed = 3f;
                main.startSize = 0.1f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 50;
                main.playOnAwake = false;
                main.loop = false;

                // Emission module: burst only, no rate over time
                var emission = ps.emission;
                emission.rateOverTime = 0f;
                emission.enabled = true;

                // Shape module: sphere with small radius
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.1f;

                // Color over lifetime: fade out alpha
                var colorOverLifetime = ps.colorOverLifetime;
                colorOverLifetime.enabled = true;
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(Color.white, 0f),
                        new GradientColorKey(Color.white, 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(0f, 1f)
                    }
                );
                colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

                // Size over lifetime: shrink
                var sizeOverLifetime = ps.sizeOverLifetime;
                sizeOverLifetime.enabled = true;
                sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

                // Renderer setup
                var renderer = psObj.GetComponent<ParticleSystemRenderer>();
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.sortingOrder = 15;

                psObj.SetActive(false);
                particlePool.Add(ps);
            }
        }

        /// <summary>
        /// Plays a burst of particles at the given world position with the specified color.
        /// </summary>
        public void PlayBurst(Vector3 position, Color color, int count = 10)
        {
            ParticleSystem ps = GetFromPool();
            if (ps == null) return;

            ps.gameObject.SetActive(true);
            ps.transform.position = position;

            // Set start color
            var main = ps.main;
            main.startColor = color;

            // Clear any leftover particles and emit burst
            ps.Clear();
            ps.Emit(count);

            StartCoroutine(ReturnToPoolAfterLifetime(ps, main.startLifetime.constant + 0.1f));
        }

        /// <summary>
        /// Gets the next available particle system from the pool (round-robin).
        /// </summary>
        private ParticleSystem GetFromPool()
        {
            if (particlePool.Count == 0) return null;

            ParticleSystem ps = particlePool[nextPoolIndex];
            nextPoolIndex = (nextPoolIndex + 1) % particlePool.Count;
            return ps;
        }

        /// <summary>
        /// Returns a particle system to the pool after its particles have expired.
        /// </summary>
        private IEnumerator ReturnToPoolAfterLifetime(ParticleSystem ps, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.gameObject.SetActive(false);
            }
        }
    }
}
