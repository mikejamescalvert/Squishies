using UnityEngine;
using System.Collections.Generic;

namespace Squishies
{
    public static class SpriteGenerator
    {
        private static Dictionary<Color, Sprite> _circleCache = new Dictionary<Color, Sprite>();
        private static Dictionary<Mood, Sprite> _faceCache = new Dictionary<Mood, Sprite>();

        public static Sprite GetCircleSprite(Color color, int size = 64)
        {
            if (_circleCache.TryGetValue(color, out Sprite cached))
                return cached;

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            float radius = size / 2f;
            float centerX = size / 2f;
            float centerY = size / 2f;

            Color transparent = new Color(0f, 0f, 0f, 0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - centerX + 0.5f;
                    float dy = y - centerY + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist < radius - 1f)
                    {
                        texture.SetPixel(x, y, color);
                    }
                    else if (dist < radius)
                    {
                        // Anti-alias the edge
                        float alpha = Mathf.Clamp01(radius - dist) * color.a;
                        texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                    }
                    else
                    {
                        texture.SetPixel(x, y, transparent);
                    }
                }
            }

            texture.Apply();

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );

            _circleCache[color] = sprite;
            return sprite;
        }

        public static Sprite GetFaceSprite(Mood mood, int size = 64)
        {
            if (_faceCache.TryGetValue(mood, out Sprite cached))
                return cached;

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            Color transparent = new Color(0f, 0f, 0f, 0f);
            Color black = Color.black;

            // Clear to transparent
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, transparent);
                }
            }

            // Draw eyes - two small filled circles
            // Eyes at ~30% from top = ~70% from bottom in texture coords
            int eyeY = Mathf.RoundToInt(size * 0.70f);
            int leftEyeX = Mathf.RoundToInt(size * 0.35f);
            int rightEyeX = Mathf.RoundToInt(size * 0.65f);
            int eyeRadius = Mathf.Max(2, size / 16);

            DrawFilledCircle(texture, leftEyeX, eyeY, eyeRadius, black);
            DrawFilledCircle(texture, rightEyeX, eyeY, eyeRadius, black);

            // Draw mouth at ~65% from top = ~35% from bottom in texture coords
            int mouthY = Mathf.RoundToInt(size * 0.35f);
            int mouthLeftX = Mathf.RoundToInt(size * 0.35f);
            int mouthRightX = Mathf.RoundToInt(size * 0.65f);
            int mouthWidth = mouthRightX - mouthLeftX;

            switch (mood)
            {
                case Mood.Happy:
                    // Smile: curved up arc
                    DrawArc(texture, mouthLeftX, mouthRightX, mouthY, size, black, arcUp: true);
                    break;

                case Mood.Neutral:
                    // Straight horizontal line
                    int lineThickness = Mathf.Max(1, size / 32);
                    for (int x = mouthLeftX; x <= mouthRightX; x++)
                    {
                        for (int t = 0; t < lineThickness; t++)
                        {
                            int py = mouthY + t;
                            if (py >= 0 && py < size)
                                texture.SetPixel(x, py, black);
                        }
                    }
                    break;

                case Mood.Sad:
                    // Frown: curved down arc
                    DrawArc(texture, mouthLeftX, mouthRightX, mouthY, size, black, arcUp: false);
                    break;
            }

            texture.Apply();

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );

            _faceCache[mood] = sprite;
            return sprite;
        }

        private static void DrawFilledCircle(Texture2D texture, int cx, int cy, int radius, Color color)
        {
            int size = texture.width;
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                for (int x = cx - radius; x <= cx + radius; x++)
                {
                    if (x < 0 || x >= size || y < 0 || y >= size)
                        continue;

                    float dx = x - cx;
                    float dy = y - cy;
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static void DrawArc(Texture2D texture, int leftX, int rightX, int centerY, int size, Color color, bool arcUp)
        {
            int mouthWidth = rightX - leftX;
            int arcHeight = Mathf.Max(3, mouthWidth / 4);
            int lineThickness = Mathf.Max(1, size / 32);

            for (int x = leftX; x <= rightX; x++)
            {
                // Normalize x to 0..1 across mouth width
                float t = (float)(x - leftX) / mouthWidth;
                // Parabolic arc: 4*t*(1-t) gives 0 at edges, 1 at center
                float arcOffset = 4f * t * (1f - t) * arcHeight;

                int py;
                if (arcUp)
                {
                    // Smile: arc goes down from center (in texture coords, down = lower y for frown visual)
                    // Actually for a smile (curved up visually), the center dips down in texture coords
                    py = centerY - Mathf.RoundToInt(arcOffset);
                }
                else
                {
                    // Frown: arc goes up in texture coords (visually curves down)
                    py = centerY + Mathf.RoundToInt(arcOffset);
                }

                for (int t2 = 0; t2 < lineThickness; t2++)
                {
                    int drawY = py + t2;
                    if (x >= 0 && x < size && drawY >= 0 && drawY < size)
                    {
                        texture.SetPixel(x, drawY, color);
                    }
                }
            }
        }
    }
}
