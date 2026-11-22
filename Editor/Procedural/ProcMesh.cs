using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.VR.StarterKit.Editor
{
    internal sealed class MeshData
    {
        public readonly List<Vector3> V = new List<Vector3>();
        public readonly List<Vector3> N = new List<Vector3>();
        public readonly List<Vector2> UV = new List<Vector2>();
        public readonly List<int> T = new List<int>();

        public int Add(Vector3 p, Vector3 n, Vector2 uv)
        {
            V.Add(p); N.Add(n); UV.Add(uv);
            return V.Count - 1;
        }

        /// <summary>Adds a triangle, flipping winding so it faces the average vertex normal.</summary>
        public void Tri(int a, int b, int c)
        {
            Vector3 g = Vector3.Cross(V[b] - V[a], V[c] - V[a]);
            if (g.sqrMagnitude < 1e-14f) return;
            if (Vector3.Dot(g, N[a] + N[b] + N[c]) < 0f) { int t = b; b = c; c = t; }
            T.Add(a); T.Add(b); T.Add(c);
        }

        public Mesh ToMesh(string name)
        {
            var m = new Mesh { name = name };
            if (V.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.SetVertices(V);
            m.SetNormals(N);
            m.SetUVs(0, UV);
            m.SetTriangles(T, 0);
            m.RecalculateBounds();
            m.RecalculateTangents();
            return m;
        }
    }

    /// <summary>Procedural mesh generators: rounded boxes, lathes, tubes and leaves.</summary>
    public static class ProcMesh
    {
        /// <summary>Appends an elliptical arc (angles in degrees; x = radius, y = height).</summary>
        public static void Arc(List<Vector2> pts, float cx, float cy, float rx, float ry, float a0, float a1, int steps)
        {
            for (int i = 0; i <= steps; i++)
            {
                float a = Mathf.Lerp(a0, a1, i / (float)steps) * Mathf.Deg2Rad;
                pts.Add(new Vector2(cx + Mathf.Cos(a) * rx, cy + Mathf.Sin(a) * ry));
            }
        }

        private static float[] AxisCoords(float half, float r, int seg)
        {
            var c = new float[2 * (seg + 1)];
            for (int k = 0; k <= seg; k++)
            {
                c[k] = -half + r * k / seg;
                c[seg + 1 + k] = (half - r) + r * k / seg;
            }
            return c;
        }

        /// <summary>Box with rounded edges. UVs are planar in metres divided by <paramref name="uvMeters"/>.</summary>
        public static Mesh RoundedBox(string name, Vector3 size, float radius, int seg, float uvMeters)
        {
            var md = new MeshData();
            Vector3 h = size * 0.5f;
            float r = Mathf.Max(0.0005f, Mathf.Min(radius, Mathf.Min(h.x, Mathf.Min(h.y, h.z)) * 0.98f));
            Vector3 ih = h - new Vector3(r, r, r);

            var coords = new float[3][];
            for (int a = 0; a < 3; a++) coords[a] = AxisCoords(h[a], r, seg);

            for (int a = 0; a < 3; a++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    int b = (a + 1) % 3, c = (a + 2) % 3;
                    float[] cb = coords[b], cc = coords[c];
                    int nb = cb.Length, nc = cc.Length;
                    int start = md.V.Count;

                    Vector3 faceN = Vector3.zero;
                    faceN[a] = s;

                    for (int j = 0; j < nc; j++)
                    {
                        for (int i = 0; i < nb; i++)
                        {
                            Vector3 p = Vector3.zero;
                            p[a] = s * h[a];
                            p[b] = cb[i];
                            p[c] = cc[j];

                            Vector3 inner = new Vector3(
                                Mathf.Clamp(p.x, -ih.x, ih.x),
                                Mathf.Clamp(p.y, -ih.y, ih.y),
                                Mathf.Clamp(p.z, -ih.z, ih.z));
                            Vector3 d = p - inner;
                            Vector3 n = d.sqrMagnitude > 1e-10f ? d.normalized : faceN;
                            Vector3 pos = inner + n * r;

                            Vector2 uv = new Vector2(pos[b], pos[c]) / uvMeters + new Vector2(a * 0.31f + (s > 0 ? 0.17f : 0f), a * 0.23f);
                            md.Add(pos, n, uv);
                        }
                    }

                    for (int j = 0; j < nc - 1; j++)
                    {
                        for (int i = 0; i < nb - 1; i++)
                        {
                            int i0 = start + j * nb + i;
                            md.Tri(i0, i0 + 1, i0 + nb);
                            md.Tri(i0 + 1, i0 + nb + 1, i0 + nb);
                        }
                    }
                }
            }
            return md.ToMesh(name);
        }

        /// <summary>
        /// Surface of revolution. Profile points are (radius, height) and must run from the bottom axis,
        /// out and up the outside, then back in over the top (so outward normals are correct).
        /// Duplicate consecutive points make a hard edge.
        /// </summary>
        public static Mesh Lathe(string name, IList<Vector2> prof, int segs, float uvMeters)
        {
            var md = new MeshData();
            int n = prof.Count;
            var segNormal = new Vector2[n - 1];
            var ok = new bool[n - 1];
            var arc = new float[n];
            float rMax = 0.001f;

            for (int i = 0; i < n; i++) rMax = Mathf.Max(rMax, prof[i].x);
            for (int i = 0; i < n - 1; i++)
            {
                Vector2 d = prof[i + 1] - prof[i];
                float len = d.magnitude;
                arc[i + 1] = arc[i] + len;
                if (len < 1e-5f) continue;
                d /= len;
                segNormal[i] = new Vector2(d.y, -d.x);
                ok[i] = true;
            }

            float circ = 2f * Mathf.PI * rMax;
            for (int i = 0; i < n; i++)
            {
                Vector2 nn = Vector2.zero;
                if (i > 0 && ok[i - 1]) nn += segNormal[i - 1];
                if (i < n - 1 && ok[i]) nn += segNormal[i];
                if (nn.sqrMagnitude < 1e-8f) nn = Vector2.up;
                nn.Normalize();

                for (int s = 0; s <= segs; s++)
                {
                    float a01 = s / (float)segs;
                    float ang = a01 * Mathf.PI * 2f;
                    float cos = Mathf.Cos(ang), sin = Mathf.Sin(ang);
                    md.Add(new Vector3(prof[i].x * cos, prof[i].y, prof[i].x * sin),
                           new Vector3(nn.x * cos, nn.y, nn.x * sin),
                           new Vector2(a01 * circ / uvMeters, arc[i] / uvMeters));
                }
            }

            for (int i = 0; i < n - 1; i++)
            {
                for (int s = 0; s < segs; s++)
                {
                    int a = i * (segs + 1) + s, b = a + 1, c = a + segs + 1, d = c + 1;
                    md.Tri(a, b, c);
                    md.Tri(b, d, c);
                }
            }
            return md.ToMesh(name);
        }

        /// <summary>Tube along a path. radius(t) and noise(t, angle01) shape it; noise must be periodic in angle01.</summary>
        public static Mesh Tube(string name, IList<Vector3> path, Func<float, float> radius, int sides,
                                Func<float, float, float> noise, float uvMeters, bool caps)
        {
            var md = new MeshData();
            int n = path.Count;
            Vector3 prevN = Vector3.zero;
            float arc = 0f;
            Vector3 firstN = Vector3.zero, firstB = Vector3.zero, firstT = Vector3.zero;
            Vector3 lastN = Vector3.zero, lastB = Vector3.zero, lastT = Vector3.zero;
            float firstR = 0f, lastR = 0f;

            for (int i = 0; i < n; i++)
            {
                Vector3 tan = (path[Mathf.Min(i + 1, n - 1)] - path[Mathf.Max(i - 1, 0)]).normalized;
                Vector3 nrm;
                if (i == 0) nrm = Vector3.Cross(tan, Mathf.Abs(tan.y) > 0.95f ? Vector3.right : Vector3.up).normalized;
                else nrm = (prevN - tan * Vector3.Dot(prevN, tan)).normalized;
                prevN = nrm;
                Vector3 bin = Vector3.Cross(tan, nrm);

                if (i > 0) arc += (path[i] - path[i - 1]).magnitude;
                float t = i / (float)(n - 1);
                float rad = radius(t);

                for (int k = 0; k <= sides; k++)
                {
                    float a01 = k / (float)sides;
                    float ang = a01 * Mathf.PI * 2f;
                    Vector3 dir = nrm * Mathf.Cos(ang) + bin * Mathf.Sin(ang);
                    float r = rad * (1f + (noise != null ? noise(t, a01) : 0f));
                    md.Add(path[i] + dir * r, dir, new Vector2(a01 * Mathf.PI * 2f * rad / uvMeters, arc / uvMeters));
                }

                if (i == 0) { firstN = nrm; firstB = bin; firstT = tan; firstR = rad; }
                if (i == n - 1) { lastN = nrm; lastB = bin; lastT = tan; lastR = rad; }
            }

            for (int i = 0; i < n - 1; i++)
            {
                for (int k = 0; k < sides; k++)
                {
                    int a = i * (sides + 1) + k, b = a + 1, c = a + sides + 1, d = c + 1;
                    md.Tri(a, b, c);
                    md.Tri(b, d, c);
                }
            }

            if (caps)
            {
                AddCap(md, path[0], firstN, firstB, -firstT, firstR, sides, uvMeters);
                AddCap(md, path[n - 1], lastN, lastB, lastT, lastR, sides, uvMeters);
            }
            return md.ToMesh(name);
        }

        private static void AddCap(MeshData md, Vector3 centre, Vector3 nrm, Vector3 bin, Vector3 normal, float rad, int sides, float uvMeters)
        {
            int c = md.Add(centre, normal, new Vector2(0.5f, 0.5f));
            int first = md.V.Count;
            for (int k = 0; k <= sides; k++)
            {
                float ang = k / (float)sides * Mathf.PI * 2f;
                Vector3 dir = nrm * Mathf.Cos(ang) + bin * Mathf.Sin(ang);
                md.Add(centre + dir * rad, normal, new Vector2(0.5f + Mathf.Cos(ang) * 0.5f, 0.5f + Mathf.Sin(ang) * 0.5f));
            }
            for (int k = 0; k < sides; k++) md.Tri(c, first + k, first + k + 1);
        }

        /// <summary>Double-sided leaf blade along +Z, width along X, with a V fold and a droop.</summary>
        public static Mesh Leaf(string name, float length, float width, int rows, float fold, float droop)
        {
            const int cols = 5;
            var P = new Vector3[rows + 1, cols];
            var Nrm = new Vector3[rows + 1, cols];

            for (int r = 0; r <= rows; r++)
            {
                float t = r / (float)rows;
                float outline = Mathf.Pow(Mathf.Sin(Mathf.PI * Mathf.Pow(t, 0.65f)), 0.9f);
                float w = width * 0.5f * outline;
                for (int c = 0; c < cols; c++)
                {
                    float u = -1f + 2f * c / (cols - 1);
                    P[r, c] = new Vector3(u * w, Mathf.Abs(u) * w * fold - droop * length * t * t, t * length);
                }
            }

            for (int r = 0; r <= rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Vector3 dx = P[r, Mathf.Min(c + 1, cols - 1)] - P[r, Mathf.Max(c - 1, 0)];
                    Vector3 dz = P[Mathf.Min(r + 1, rows), c] - P[Mathf.Max(r - 1, 0), c];
                    Vector3 nn = Vector3.Cross(dz, dx);
                    Nrm[r, c] = nn.sqrMagnitude < 1e-12f ? Vector3.up : nn.normalized;
                }
            }

            var md = new MeshData();
            var front = new int[rows + 1, cols];
            var back = new int[rows + 1, cols];
            for (int r = 0; r <= rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Vector2 uv = new Vector2(c / (float)(cols - 1), r / (float)rows);
                    front[r, c] = md.Add(P[r, c], Nrm[r, c], uv);
                    back[r, c] = md.Add(P[r, c], -Nrm[r, c], uv);
                }
            }

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols - 1; c++)
                {
                    md.Tri(front[r, c], front[r, c + 1], front[r + 1, c]);
                    md.Tri(front[r, c + 1], front[r + 1, c + 1], front[r + 1, c]);
                    md.Tri(back[r, c], back[r, c + 1], back[r + 1, c]);
                    md.Tri(back[r, c + 1], back[r + 1, c + 1], back[r + 1, c]);
                }
            }
            return md.ToMesh(name);
        }
    }
}
