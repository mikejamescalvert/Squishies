using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Squishies
{
    public class ChonkyBehavior : MonoBehaviour
    {
        public List<Vector2Int> OccupiedCells { get; private set; }

        private Coroutine wobbleCoroutine;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Vector2Int origin, SquishySize size)
        {
            OccupiedCells = new List<Vector2Int>();

            if (size == SquishySize.Chonky)
            {
                OccupiedCells.Add(origin);
                OccupiedCells.Add(origin + new Vector2Int(1, 0));
                OccupiedCells.Add(origin + new Vector2Int(0, 1));
                OccupiedCells.Add(origin + new Vector2Int(1, 1));

                transform.localScale = new Vector3(1.8f, 1.8f, 1f);
            }
            else if (size == SquishySize.MegaChonk)
            {
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        OccupiedCells.Add(origin + new Vector2Int(x, y));
                    }
                }

                transform.localScale = new Vector3(2.6f, 2.6f, 1f);
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 5;
            }
        }

        public void PlayIdleWobble()
        {
            if (wobbleCoroutine != null)
            {
                StopCoroutine(wobbleCoroutine);
            }
            wobbleCoroutine = StartCoroutine(IdleWobbleRoutine());
        }

        private IEnumerator IdleWobbleRoutine()
        {
            Vector3 baseScale = transform.localScale;
            float seed = Mathf.Abs(GetInstanceID()) * 0.01f;
            float speed = 1.0f + (seed % 1.0f) * 0.25f;

            while (true)
            {
                float t = Mathf.Sin(Time.time * speed + seed);
                float offset = t * 0.08f;
                transform.localScale = new Vector3(
                    baseScale.x + offset,
                    baseScale.y + offset,
                    1f
                );
                yield return null;
            }
        }
    }
}
