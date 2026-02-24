using UnityEngine;

namespace Squishies
{
    public enum Mood { Happy, Neutral, Sad }
    public enum SquishySize { Normal, Chonky, MegaChonk }

    public class Squishy : MonoBehaviour
    {
        public SquishyData Data { get; private set; }
        public Mood CurrentMood { get; private set; }
        public Vector2Int GridPosition { get; set; }
        public SquishySize Size { get; set; }
        public int TurnsSinceMatched { get; set; }
        public bool IsActive { get; set; }
        public int HappyTurnsRemaining { get; set; }

        private SpriteRenderer spriteRenderer;
        private SpriteRenderer faceRenderer;
        private Transform faceTransform;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            Transform existingFace = transform.Find("Face");
            if (existingFace != null)
            {
                faceTransform = existingFace;
                faceRenderer = faceTransform.GetComponent<SpriteRenderer>();
            }
            else
            {
                GameObject faceObj = new GameObject("Face");
                faceObj.transform.SetParent(transform, false);
                faceObj.transform.localPosition = Vector3.zero;
                faceTransform = faceObj.transform;
                faceRenderer = faceObj.AddComponent<SpriteRenderer>();
            }

            faceRenderer.sortingOrder = 2;
        }

        public void Initialize(SquishyData data, Vector2Int gridPos, SquishySize size = SquishySize.Normal)
        {
            Data = data;
            GridPosition = gridPos;
            Size = size;
            CurrentMood = Mood.Neutral;
            TurnsSinceMatched = 0;
            HappyTurnsRemaining = 0;
            IsActive = true;
            gameObject.SetActive(true);

            spriteRenderer.sprite = SpriteGenerator.GetCircleSprite(data.color);
            faceRenderer.sprite = SpriteGenerator.GetFaceSprite(Mood.Neutral);

            if (size == SquishySize.Normal)
            {
                transform.localScale = Vector3.one;
            }
        }

        public void SetMood(Mood mood)
        {
            CurrentMood = mood;
            faceRenderer.sprite = SpriteGenerator.GetFaceSprite(mood);

            float moodScale;
            switch (mood)
            {
                case Mood.Happy:
                    moodScale = 1.05f;
                    break;
                case Mood.Sad:
                    moodScale = 0.9f;
                    break;
                default:
                    moodScale = 1.0f;
                    break;
            }

            if (faceTransform != null)
            {
                faceTransform.localScale = new Vector3(moodScale, moodScale, 1f);
            }
        }

        public float GetMoodMultiplier()
        {
            switch (CurrentMood)
            {
                case Mood.Happy:
                    return 1.5f;
                case Mood.Sad:
                    return 0.75f;
                default:
                    return 1.0f;
            }
        }

        public void ReturnToPool()
        {
            IsActive = false;
            gameObject.SetActive(false);
            TurnsSinceMatched = 0;
            HappyTurnsRemaining = 0;
        }
    }
}
