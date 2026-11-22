using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Fusion.VR.StarterKit.Editor
{
    /// <summary>Material kinds that use generated textures.</summary>
    public enum JungleMat { Wood, DarkWood, Bark, Stone, Moss, Leaf, Bamboo, Screen }

    /// <summary>Creates and caches the jungle materials and their generated textures as project assets.</summary>
    public static class JungleMaterials
    {
        public const string Root = "Assets/FusionVR/StarterKit/Generated";
        public const string TexDir = Root + "/Textures";
        public const string MatDir = Root + "/Materials";
        public const string MeshDir = Root + "/Meshes";
        public const string PrefabDir = "Assets/FusionVR/StarterKit/Prefabs";

        private static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();

        public static void EnsureDir(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        public static Material Get(JungleMat kind)
        {
            string path = MatDir + "/Jungle_" + kind + ".mat";
            if (Cache.TryGetValue(path, out Material cached) && cached != null) return cached;

            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) m = Create(kind, path);
            Cache[path] = m;
            return m;
        }

        /// <summary>Re-generates every texture and material in place.</summary>
        public static void RegenerateAll()
        {
            Cache.Clear();
            foreach (JungleMat kind in System.Enum.GetValues(typeof(JungleMat)))
            {
                Create(kind, MatDir + "/Jungle_" + kind + ".mat");
            }
            Cache.Clear();
        }

        /// <summary>Untextured material with a fixed colour and optional emission.</summary>
        public static Material Solid(string name, Color color, float smoothness, Color emission)
        {
            EnsureDir(MatDir);
            string path = MatDir + "/Solid_" + name + ".mat";
            if (Cache.TryGetValue(path, out Material cached) && cached != null) return cached;

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool isNew = mat == null;
            if (isNew) mat = new Material(LitShader());

            Apply(mat, null, null, color, smoothness, emission);
            if (isNew) AssetDatabase.CreateAsset(mat, path); else EditorUtility.SetDirty(mat);
            Cache[path] = mat;
            return mat;
        }

        private static Material Create(JungleMat kind, string path)
        {
            EnsureDir(TexDir);
            EnsureDir(MatDir);

            TextureData td = null;
            float smooth = 0.25f;
            float normalStrength = 3f;
            Color solid = Color.white;

            switch (kind)
            {
                case JungleMat.Wood:
                    td = JungleTextures.Wood(512, 5, 11, new Color(0.22f, 0.12f, 0.06f), new Color(0.58f, 0.37f, 0.2f));
                    break;
                case JungleMat.DarkWood:
                    td = JungleTextures.Wood(512, 4, 23, new Color(0.1f, 0.06f, 0.035f), new Color(0.34f, 0.21f, 0.11f));
                    break;
                case JungleMat.Bark:
                    td = JungleTextures.Bark(512, 31, new Color(0.14f, 0.1f, 0.07f), new Color(0.42f, 0.31f, 0.21f));
                    normalStrength = 5f;
                    smooth = 0.1f;
                    break;
                case JungleMat.Stone:
                    td = JungleTextures.Stone(512, 41, new Color(0.24f, 0.26f, 0.24f), new Color(0.55f, 0.57f, 0.52f), new Color(0.2f, 0.4f, 0.12f));
                    normalStrength = 4f;
                    smooth = 0.15f;
                    break;
                case JungleMat.Moss:
                    td = JungleTextures.Moss(256, 51);
                    smooth = 0.05f;
                    break;
                case JungleMat.Leaf:
                    td = JungleTextures.Leaf(256, 61);
                    smooth = 0.4f;
                    normalStrength = 2f;
                    break;
                case JungleMat.Bamboo:
                    td = JungleTextures.Bamboo(512, 71);
                    smooth = 0.45f;
                    break;
                case JungleMat.Screen:
                    solid = new Color(0.03f, 0.07f, 0.05f);
                    smooth = 0.85f;
                    break;
            }

            Texture2D albedo = null, normal = null;
            if (td != null)
            {
                albedo = SaveTexture(td.Albedo, td.Size, TexDir + "/" + kind + "_Albedo.png", false);
                normal = SaveTexture(HeightToNormal(td.Height, td.Size, normalStrength), td.Size, TexDir + "/" + kind + "_Normal.png", true);
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool isNew = mat == null;
            if (isNew) mat = new Material(LitShader());

            Apply(mat, albedo, normal, solid, smooth, Color.black);
            if (isNew) AssetDatabase.CreateAsset(mat, path); else EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return mat;
        }

        private static void Apply(Material mat, Texture2D albedo, Texture2D normal, Color tint, float smooth, Color emission)
        {
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", tint);
            if (albedo != null) mat.mainTexture = albedo;

            if (normal != null && mat.HasProperty("_BumpMap"))
            {
                mat.SetTexture("_BumpMap", normal);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smooth);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smooth);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);

            if (emission.maxColorComponent > 0.001f)
            {
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission);
            }
        }

        private static Shader LitShader()
        {
            string[] names = { "Universal Render Pipeline/Lit", "Standard", "Universal Render Pipeline/Simple Lit" };
            foreach (string n in names)
            {
                Shader s = Shader.Find(n);
                if (s != null) return s;
            }
            return Shader.Find("Sprites/Default");
        }

        private static Color[] HeightToNormal(float[] h, int size, float strength)
        {
            var result = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float l = h[y * size + (x + size - 1) % size];
                    float r = h[y * size + (x + 1) % size];
                    float d = h[((y + size - 1) % size) * size + x];
                    float u = h[((y + 1) % size) * size + x];
                    Vector3 n = new Vector3((l - r) * strength, (d - u) * strength, 1f).normalized;
                    result[y * size + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                }
            }
            return result;
        }

        private static Texture2D SaveTexture(Color[] pixels, int size, string path, bool isNormal)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, isNormal);
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = !isNormal;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Trilinear;
                importer.anisoLevel = 4;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = 512;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
