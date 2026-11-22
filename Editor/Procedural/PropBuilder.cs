using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;

namespace Fusion.VR.StarterKit.Editor
{
    /// <summary>Shared jungle building blocks: parts, text, keys, buttons, trunks, bamboo, vines and leaves.</summary>
    public sealed class PropBuilder
    {
        public static readonly Color Cream = new Color(0.96f, 0.9f, 0.7f, 1f);

        private readonly MeshBank _bank;

        public PropBuilder(MeshBank bank) { _bank = bank; }

        public static string K(string prefix, params float[] v)
        {
            var sb = new StringBuilder(prefix);
            foreach (float f in v) sb.Append('_').Append(f.ToString("F3", CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        public Mesh M(string key, Func<Mesh> make) => _bank.Get(key, make);

        // ------------------------------------------------------------------ Basics

        public Transform Empty(Transform parent, string name, Vector3 pos, Vector3 euler)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(euler);
            return go.transform;
        }

        public GameObject Part(Transform parent, string name, Mesh mesh, Material material, Vector3 pos, Vector3 euler, Vector3? scale = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale ?? Vector3.one;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        public GameObject Part(Transform parent, string name, Mesh mesh, JungleMat mat, Vector3 pos, Vector3 euler, Vector3? scale = null)
        {
            return Part(parent, name, mesh, JungleMaterials.Get(mat), pos, euler, scale);
        }

        public GameObject Box(Transform parent, string name, Vector3 size, float radius, Vector3 pos, Vector3 euler, JungleMat mat)
        {
            Mesh mesh = M(K("box", size.x, size.y, size.z, radius), () => ProcMesh.RoundedBox("", size, radius, 3, 0.6f));
            return Part(parent, name, mesh, mat, pos, euler);
        }

        public Material Glow(string name, Color color) => JungleMaterials.Solid("Glow_" + name, color, 0.6f, color * 1.4f);
        public Material Paint(string name, Color color) => JungleMaterials.Solid("Paint_" + name, color, 0.7f, Color.black);

        /// <summary>World text. <paramref name="em"/> is the approximate letter height in metres.</summary>
        public TextMeshPro Text(Transform parent, string name, string text, float em, Color color, Vector3 pos, Vector2 box,
                                TextAlignmentOptions align = TextAlignmentOptions.Center, Vector3? euler = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            TextMeshPro tmp = go.AddComponent<TextMeshPro>(); // replaces Transform with RectTransform
            RectTransform tr = tmp.rectTransform;
            tr.localPosition = pos;
            tr.localRotation = Quaternion.Euler(euler ?? new Vector3(0f, 180f, 0f));
            tr.localScale = Vector3.one;
            tr.sizeDelta = box;
            tmp.text = text;
            tmp.fontSize = em * 10f;
            tmp.color = color;
            tmp.alignment = align;
            tmp.overflowMode = TextOverflowModes.Overflow;
            return tmp;
        }

        // ------------------------------------------------------------------ Natural shapes

        public static List<Vector2> DomeProfile(float r, float h, int steps = 8)
        {
            var p = new List<Vector2> { new Vector2(0f, 0f), new Vector2(r, 0f) };
            ProcMesh.Arc(p, 0f, 0f, r, h, 0f, 90f, steps);
            return p;
        }

        public GameObject Trunk(Transform parent, string name, Vector3 pos, float height, float radius, float lean, int seed)
        {
            Mesh mesh = M(K("trunk", height, radius, lean, seed), () =>
            {
                var path = new List<Vector3>();
                const int n = 14;
                for (int i = 0; i < n; i++)
                {
                    float t = i / (n - 1f);
                    path.Add(new Vector3(Mathf.Sin(t * 2.2f + seed) * lean * t, t * height, Mathf.Cos(t * 1.7f + seed) * lean * t));
                }
                return ProcMesh.Tube("", path,
                    t => radius * (1f + 1.6f * Mathf.Pow(1f - t, 6f)) * (1f - 0.25f * t), 14,
                    (t, a) => 0.2f * (JungleNoise.Value(a * 7f, t * 9f, 7, 0, seed) - 0.5f), 0.9f, true);
            });
            return Part(parent, name, mesh, JungleMat.Bark, pos, new Vector3(0f, seed * 37f % 360f, 0f));
        }

        public GameObject Bamboo(Transform parent, string name, Vector3 pos, float height, float radius)
        {
            Mesh mesh = M(K("bamboo", height, radius), () =>
            {
                var prof = new List<Vector2> { new Vector2(0f, 0f), new Vector2(radius, 0f) };
                const float step = 0.25f;
                int nodes = Mathf.FloorToInt((height - 0.02f) / step);
                for (int k = 1; k <= nodes; k++)
                {
                    float y = k * step;
                    prof.Add(new Vector2(radius, y - 0.012f));
                    prof.Add(new Vector2(radius * 1.09f, y - 0.006f));
                    prof.Add(new Vector2(radius * 1.09f, y + 0.006f));
                    prof.Add(new Vector2(radius, y + 0.012f));
                }
                prof.Add(new Vector2(radius, height));
                prof.Add(new Vector2(0f, height));
                return ProcMesh.Lathe("", prof, 12, 1f);
            });
            return Part(parent, name, mesh, JungleMat.Bamboo, pos, Vector3.zero);
        }

        public Mesh LeafMesh(int variant)
        {
            switch (variant % 3)
            {
                case 0: return M("leaf_0", () => ProcMesh.Leaf("", 0.32f, 0.14f, 8, 0.35f, 0.25f));
                case 1: return M("leaf_1", () => ProcMesh.Leaf("", 0.26f, 0.12f, 8, 0.45f, 0.35f));
                default: return M("leaf_2", () => ProcMesh.Leaf("", 0.4f, 0.16f, 9, 0.3f, 0.2f));
            }
        }

        /// <summary>Radial cluster of leaves (each variant is about 0.3 m long).</summary>
        public void Leaves(Transform parent, string name, Vector3 pos, int count, float length, int seed)
        {
            Transform g = Empty(parent, name, pos, Vector3.zero);
            var rng = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                float yaw = i * 360f / count + (float)rng.NextDouble() * 20f;
                float pitch = 15f + (float)rng.NextDouble() * 50f;
                float roll = ((float)rng.NextDouble() - 0.5f) * 40f;
                float s = length / 0.3f * (0.8f + (float)rng.NextDouble() * 0.4f);
                Part(g, "Leaf", LeafMesh(i), JungleMat.Leaf, Vector3.zero, new Vector3(-pitch, yaw, roll), Vector3.one * s);
            }
        }

        private static List<Vector3> VinePath(Vector3 a, Vector3 b, float amp, int seed, int n)
        {
            Vector3 dir = b - a;
            Vector3 p1 = Vector3.Cross(dir, Vector3.up);
            if (p1.sqrMagnitude < 1e-4f) p1 = Vector3.Cross(dir, Vector3.right);
            p1.Normalize();
            Vector3 p2 = Vector3.Cross(dir.normalized, p1);

            var path = new List<Vector3>();
            for (int i = 0; i <= n; i++)
            {
                float t = i / (float)n;
                float w = Mathf.Sin(Mathf.PI * t);
                path.Add(Vector3.Lerp(a, b, t) + (p1 * Mathf.Sin(t * 11f + seed) + p2 * Mathf.Cos(t * 9f + seed * 0.7f)) * amp * w);
            }
            return path;
        }

        /// <summary>Twisting vine with small leaves growing from it.</summary>
        public void Vine(Transform parent, Vector3 a, Vector3 b, float radius, float amp, int seed)
        {
            Mesh mesh = M(K("vine", a.x, a.y, a.z, b.x, b.y, b.z, radius, amp, seed), () =>
                ProcMesh.Tube("", VinePath(a, b, amp, seed, 16), t => radius * (1f - 0.5f * t), 6, null, 0.4f, true));
            Part(parent, "Vine", mesh, JungleMat.Moss, Vector3.zero, Vector3.zero);

            List<Vector3> path = VinePath(a, b, amp, seed, 16);
            var rng = new System.Random(seed + 3);
            int[] spots = { 3, 6, 9, 12, 14 };
            for (int i = 0; i < spots.Length; i++)
            {
                Part(parent, "VineLeaf", LeafMesh(i + seed), JungleMat.Leaf, path[spots[i]],
                     new Vector3(-30f + (float)rng.NextDouble() * 60f, (float)rng.NextDouble() * 360f, 0f), Vector3.one * 0.55f);
            }
        }

        /// <summary>Squashed moss dome for resting on surfaces.</summary>
        public void Moss(Transform parent, Vector3 pos, float radius, int seed)
        {
            Mesh mesh = M(K("mossdome", radius), () => ProcMesh.Lathe("", DomeProfile(radius, radius * 0.5f, 6), 14, 0.5f));
            Part(parent, "Moss", mesh, JungleMat.Moss, pos, new Vector3(0f, seed * 53f % 360f, 0f), new Vector3(1f, 0.45f, 0.8f));
        }

        // ------------------------------------------------------------------ Buttons

        private static FusionVRInteractableButton SetupButton(GameObject root, Transform visual, Vector3 colliderCenter, Vector3 colliderSize, Vector3 pressOffset)
        {
            BoxCollider col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = colliderCenter;
            col.size = colliderSize;

            FusionVRInteractableButton btn = root.AddComponent<FusionVRInteractableButton>();
            btn.HandTag = string.Empty;
            btn.VisualTarget = visual;
            btn.PressOffset = pressOffset;
            return btn;
        }

        /// <summary>Rounded wooden key with a flat label on top. The object is named "Key_{keyName}".</summary>
        public FusionVRInteractableButton Key(Transform parent, string keyName, string label, Vector3 pos, Vector2 size, JungleMat mat = JungleMat.Wood, float em = 0.03f)
        {
            Transform root = Empty(parent, "Key_" + keyName, pos, Vector3.zero);
            Mesh mesh = M(K("key", size.x, size.y), () => ProcMesh.RoundedBox("", new Vector3(size.x, 0.022f, size.y), 0.008f, 3, 0.5f));
            GameObject vis = Part(root, "Visual", mesh, mat, new Vector3(0f, 0.011f, 0f), Vector3.zero);
            Text(vis.transform, "Label", label, em, Cream, new Vector3(0f, 0.0125f, 0f), new Vector2(size.x, size.y), TextAlignmentOptions.Center, new Vector3(90f, 180f, 0f));
            return SetupButton(root.gameObject, vis.transform, new Vector3(0f, 0.02f, 0f), new Vector3(size.x, 0.05f, size.y), new Vector3(0f, -0.012f, 0f));
        }

        /// <summary>Stone-ringed dome button. A flat label is placed at <paramref name="labelOffset"/>.</summary>
        public FusionVRInteractableButton DomeButton(Transform parent, string name, string label, Vector3 pos, float radius,
                                                     Material domeMaterial, Vector3 labelOffset, Vector3 labelEuler, float em = 0.028f)
        {
            Transform root = Empty(parent, name, pos, Vector3.zero);

            Mesh ring = M(K("domering", radius), () =>
            {
                float R = radius * 1.35f;
                var prof = new List<Vector2> { new Vector2(0f, 0f), new Vector2(R, 0f), new Vector2(R, 0.015f), new Vector2(R * 0.9f, 0.025f), new Vector2(0f, 0.025f) };
                return ProcMesh.Lathe("", prof, 20, 0.5f);
            });
            Mesh dome = M(K("dome", radius), () => ProcMesh.Lathe("", DomeProfile(radius, radius * 0.7f), 20, 0.5f));

            Part(root, "Base", ring, JungleMat.Stone, Vector3.zero, Vector3.zero);
            GameObject vis = Part(root, "Visual", dome, domeMaterial, new Vector3(0f, 0.022f, 0f), Vector3.zero);

            FusionVRInteractableButton btn = SetupButton(root.gameObject, vis.transform,
                new Vector3(0f, 0.04f, 0f), new Vector3(radius * 2.4f, 0.08f, radius * 2.4f), new Vector3(0f, -0.012f, 0f));

            if (!string.IsNullOrEmpty(label))
            {
                Text(root, "Label", label, em, Cream, labelOffset, new Vector2(0.3f, 0.06f), TextAlignmentOptions.Center, labelEuler);
            }
            return btn;
        }
    }
}
