using UnityEngine;

namespace Fusion.VR.StarterKit.Editor
{
    /// <summary>Colour + height data for a generated texture.</summary>
    public sealed class TextureData
    {
        public readonly int Size;
        public readonly Color[] Albedo;
        public readonly float[] Height;

        public TextureData(int size)
        {
            Size = size;
            Albedo = new Color[size * size];
            Height = new float[size * size];
        }

        public void Set(int x, int y, Color c, float h)
        {
            c.a = 1f;
            Albedo[y * Size + x] = c;
            Height[y * Size + x] = h;
        }
    }

    /// <summary>Procedural jungle textures: wood planks, bark, mossy stone, moss, leaf and bamboo.</summary>
    public static class JungleTextures
    {
        private static Color Scale(Color c, float k) => new Color(c.r * k, c.g * k, c.b * k, 1f);

        public static TextureData Wood(int size, int planks, int seed, Color dark, Color light)
        {
            var t = new TextureData(size);
            int grainPeriodY = planks * 24;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size, v = (float)y / size;
                    float pf = v * planks;
                    int plank = Mathf.Min(planks - 1, (int)pf);
                    float pv = pf - plank;
                    float off = JungleNoise.Hash01(plank, 0, seed) * 8f;

                    float g = JungleNoise.Fbm(u * 4f + off, v * grainPeriodY, 4, grainPeriodY, 3, seed + plank);
                    float warp = JungleNoise.Fbm(u * 2f, v * planks * 6f, 2, planks * 6, 2, seed + 99);
                    float ring = Mathf.Sin((pv * 14f + warp * 5f + g * 1.5f) * Mathf.PI * 2f) * 0.5f + 0.5f;
                    float tone = Mathf.Clamp01(g * 0.65f + ring * 0.35f);
                    float tint = 0.9f + 0.2f * JungleNoise.Hash01(plank, 1, seed + 5);

                    Color c = Scale(Color.Lerp(dark, light, tone), tint);
                    float h = tone * 0.5f;

                    float seam = Mathf.Min(pv, 1f - pv) * size / planks;
                    if (seam < 2f)
                    {
                        float k = Mathf.SmoothStep(0.25f, 1f, seam / 2f);
                        c = Scale(c, k);
                        h *= k * 0.5f;
                    }
                    t.Set(x, y, c, h);
                }
            }
            return t;
        }

        public static TextureData Bark(int size, int seed, Color dark, Color light)
        {
            var t = new TextureData(size);
            Color moss = new Color(0.24f, 0.38f, 0.14f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size, v = (float)y / size;
                    float n = JungleNoise.Fbm(u * 10f, v * 3f, 10, 3, 4, seed);
                    float ridged = 1f - Mathf.Abs(n * 2f - 1f);
                    float crack = Mathf.Pow(ridged, 2.5f);
                    Color c = Color.Lerp(dark, light, crack);

                    float m = JungleNoise.Fbm(u * 5f + 3f, v * 5f, 5, 5, 3, seed + 7);
                    float mossMask = Mathf.Clamp01((m - 0.58f) * 5f);
                    c = Color.Lerp(c, moss, mossMask * 0.8f);
                    t.Set(x, y, c, Mathf.Lerp(crack, 0.5f, mossMask));
                }
            }
            return t;
        }

        public static TextureData Stone(int size, int seed, Color dark, Color light, Color moss)
        {
            var t = new TextureData(size);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size, v = (float)y / size;
                    JungleNoise.Voronoi(u * 5f, v * 5f, 5, seed, out float edge);
                    float baseN = JungleNoise.Fbm(u * 8f, v * 8f, 8, 8, 4, seed + 3);
                    float crack = Mathf.Clamp01(edge * 6f);

                    Color c = Color.Lerp(dark, light, baseN * 0.8f + 0.2f * crack);
                    c = Scale(c, Mathf.Lerp(0.55f, 1f, crack));

                    float m = JungleNoise.Fbm(u * 6f + 1f, v * 6f, 6, 6, 4, seed + 11);
                    float mask = Mathf.Clamp01((m - 0.5f) * 3f + (1f - crack) * 0.5f - 0.2f);
                    c = Color.Lerp(c, moss, mask);
                    t.Set(x, y, c, crack * 0.6f + baseN * 0.4f);
                }
            }
            return t;
        }

        public static TextureData Moss(int size, int seed)
        {
            var t = new TextureData(size);
            Color deep = new Color(0.1f, 0.26f, 0.08f);
            Color bright = new Color(0.42f, 0.62f, 0.2f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size, v = (float)y / size;
                    float n = JungleNoise.Fbm(u * 12f, v * 12f, 12, 12, 4, seed);
                    float speck = JungleNoise.Value(u * 64f, v * 64f, 64, 64, seed + 5);
                    float k = Mathf.Clamp01(n + (speck > 0.82f ? 0.25f : 0f));
                    t.Set(x, y, Color.Lerp(deep, bright, k), k);
                }
            }
            return t;
        }

        /// <summary>Leaf texture for non-tiling leaf UVs: u across the blade, v from base to tip.</summary>
        public static TextureData Leaf(int size, int seed)
        {
            var t = new TextureData(size);
            Color centre = new Color(0.36f, 0.64f, 0.18f);
            Color edgeCol = new Color(0.08f, 0.3f, 0.09f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size, v = (float)y / size;
                    float cu = Mathf.Abs(u - 0.5f) * 2f;

                    Color c = Color.Lerp(centre, edgeCol, Mathf.Pow(cu, 1.5f));
                    float rib = Mathf.Clamp01(1f - cu / (0.04f + 0.05f * (1f - v)));
                    float phase = Mathf.Repeat((v - cu * 0.55f) * 10f, 1f);
                    float vein = Mathf.Pow(1f - Mathf.Abs(phase - 0.5f) * 2f, 8f) * (1f - 0.6f * cu);
                    float mott = JungleNoise.Fbm(u * 6f, v * 10f, 0, 0, 3, seed);

                    c = Scale(c, 0.85f + 0.3f * mott);
                    c = new Color(c.r + vein * 0.1f + rib * 0.2f, c.g + vein * 0.14f + rib * 0.18f, c.b + vein * 0.04f + rib * 0.05f, 1f);
                    t.Set(x, y, c, rib * 0.6f + vein * 0.3f);
                }
            }
            return t;
        }

        public static TextureData Bamboo(int size, int seed)
        {
            var t = new TextureData(size);
            Color a = new Color(0.4f, 0.54f, 0.2f);
            Color b = new Color(0.72f, 0.8f, 0.42f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size, v = (float)y / size;
                    float streak = JungleNoise.Fbm(u * 24f, v * 3f, 24, 3, 3, seed);
                    float nv = Mathf.Repeat(v * 4f, 1f);
                    float ring = 1f - Mathf.Clamp01(Mathf.Min(nv, 1f - nv) / 0.04f);

                    Color c = Color.Lerp(a, b, streak);
                    c = Color.Lerp(c, new Color(0.62f, 0.62f, 0.3f), ring * 0.5f);
                    c = Scale(c, 1f - ring * 0.5f);
                    t.Set(x, y, c, 0.5f - ring * 0.4f);
                }
            }
            return t;
        }
    }
}
