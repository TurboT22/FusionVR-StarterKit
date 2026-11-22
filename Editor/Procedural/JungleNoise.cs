using UnityEngine;

namespace Fusion.VR.StarterKit.Editor
{
    /// <summary>Tileable value noise, fBm and Voronoi used by the texture generators.</summary>
    public static class JungleNoise
    {
        public static float Hash01(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / 16777216f;
            }
        }

        private static int Wrap(int i, int period)
        {
            if (period <= 0) return i;
            i %= period;
            return i < 0 ? i + period : i;
        }

        /// <summary>Smooth value noise, periodic in x (px) and y (py) when those are &gt; 0.</summary>
        public static float Value(float x, float y, int px, int py, int seed)
        {
            int xi = Mathf.FloorToInt(x);
            int yi = Mathf.FloorToInt(y);
            float fx = x - xi;
            float fy = y - yi;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);

            float a = Hash01(Wrap(xi, px), Wrap(yi, py), seed);
            float b = Hash01(Wrap(xi + 1, px), Wrap(yi, py), seed);
            float c = Hash01(Wrap(xi, px), Wrap(yi + 1, py), seed);
            float d = Hash01(Wrap(xi + 1, px), Wrap(yi + 1, py), seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        public static float Fbm(float x, float y, int px, int py, int octaves, int seed)
        {
            float sum = 0f, amp = 0.5f, norm = 0f;
            for (int o = 0; o < octaves; o++)
            {
                sum += amp * Value(x, y, px, py, seed + o * 17);
                norm += amp;
                x *= 2f; y *= 2f;
                px *= 2; py *= 2;
                amp *= 0.5f;
            }
            return sum / norm;
        }

        /// <summary>Tileable Voronoi. Returns F1; <paramref name="edge"/> is F2 - F1 (0 on cell borders).</summary>
        public static float Voronoi(float x, float y, int period, int seed, out float edge)
        {
            int xi = Mathf.FloorToInt(x);
            int yi = Mathf.FloorToInt(y);
            float f1 = 9f, f2 = 9f;

            for (int j = -1; j <= 1; j++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    int cx = xi + i, cy = yi + j;
                    int wx = Wrap(cx, period), wy = Wrap(cy, period);
                    float px = cx + Hash01(wx, wy, seed);
                    float py = cy + Hash01(wx, wy, seed + 31);
                    float dx = px - x, dy = py - y;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d < f1) { f2 = f1; f1 = d; }
                    else if (d < f2) f2 = d;
                }
            }
            edge = f2 - f1;
            return f1;
        }
    }
}
