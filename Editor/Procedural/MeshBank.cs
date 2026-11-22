using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Fusion.VR.StarterKit.Editor
{
    /// <summary>
    /// Stores a station's generated meshes as sub-assets of one .asset file. Meshes are matched by name so
    /// regenerating updates them in place and existing prefab / scene references stay valid.
    /// </summary>
    public sealed class MeshBank
    {
        private readonly string _path;
        private readonly Dictionary<string, Mesh> _cache = new Dictionary<string, Mesh>();
        private readonly Dictionary<string, Mesh> _existing = new Dictionary<string, Mesh>();
        private bool _fileExists;

        public MeshBank(string assetPath)
        {
            _path = assetPath;
            JungleMaterials.EnsureDir(Path.GetDirectoryName(assetPath).Replace('\\', '/'));

            if (File.Exists(assetPath))
            {
                _fileExists = true;
                foreach (UnityEngine.Object o in AssetDatabase.LoadAllAssetsAtPath(assetPath))
                {
                    if (o is Mesh m) _existing[m.name] = m;
                }
            }
        }

        /// <summary>Returns the mesh for <paramref name="key"/>, generating it with <paramref name="make"/> once per run.</summary>
        public Mesh Get(string key, Func<Mesh> make)
        {
            if (_cache.TryGetValue(key, out Mesh cached)) return cached;

            Mesh fresh = make();
            fresh.name = key;
            Mesh result;

            if (_existing.TryGetValue(key, out Mesh existing) && existing != null)
            {
                existing.Clear();
                existing.indexFormat = fresh.indexFormat;
                existing.vertices = fresh.vertices;
                existing.normals = fresh.normals;
                existing.tangents = fresh.tangents;
                existing.uv = fresh.uv;
                existing.triangles = fresh.triangles;
                existing.RecalculateBounds();
                EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(fresh);
                result = existing;
            }
            else if (!_fileExists)
            {
                AssetDatabase.CreateAsset(fresh, _path);
                _fileExists = true;
                result = fresh;
            }
            else
            {
                AssetDatabase.AddObjectToAsset(fresh, _path);
                result = fresh;
            }

            _cache[key] = result;
            return result;
        }

        public void Save() => AssetDatabase.SaveAssets();
    }
}
