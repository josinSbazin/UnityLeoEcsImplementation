// ----------------------------------------------------------------------------
// The MIT License
// Unity integration https://github.com/Leopotam/ecs-unityintegration
// for ECS framework https://github.com/Leopotam/ecs
// Copyright (c) 2018 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace LeopotamGroup.Ecs.UnityIntegration {
    /// <summary>
    /// Spawns prefab from Resources folder with pooling support.
    /// </summary>
    public sealed class UnityPrefabComponent {
        public GameObject Prefab;

        static Dictionary<string, PrefabList> _prefabPools = new Dictionary<string, PrefabList> (16);

        string _path;

        /// <summary>
        /// Spawns prefab from specified path and assign to Prefab field.
        /// </summary>
        /// <param name="prefabPath">Prefab path in Resources folder.</param>
        public void Attach (string prefabPath) {
#if DEBUG
            if (string.IsNullOrEmpty (prefabPath)) {
                throw new Exception ("prefabPath");
            }
            if (_path != null) {
                throw new Exception ("Already attached");
            }
#endif
            PrefabList list;
            if (!_prefabPools.TryGetValue (prefabPath, out list)) {
                list = new PrefabList ();
                list.Prefab = Resources.Load<GameObject> (prefabPath);
#if DEBUG
                if (list.Prefab == null) {
                    throw new Exception (string.Format ("Cant load prefab from \"{0}\"", prefabPath));
                }
#endif
                _prefabPools[prefabPath] = list;
            }
            _path = prefabPath;
            if (list.Count > 0) {
                list.Count--;
                Prefab = list.Instances[list.Count];
                list.Instances[list.Count] = null;
            } else {
                Prefab = UnityEngine.Object.Instantiate (list.Prefab);
                Prefab.SetActive (false);
            }
        }

        /// <summary>
        /// Detaches and recycles prefab instance to pool.
        /// Instance will be automatically deactivated.
        /// </summary>
        public void Detach () {
            if ((object) Prefab != null && _path != null) {
                Prefab.SetActive (false);
                PrefabList list;
                if (!_prefabPools.TryGetValue (_path, out list)) {
                    // Detaching after DestroyPool call.
                    list = new PrefabList ();
                    list.Prefab = Prefab;
                    _prefabPools[_path] = list;
                } else {
                    if (list.Count == list.Instances.Length) {
                        Array.Resize (ref list.Instances, list.Count << 1);
                    }
                    list.Instances[list.Count++] = Prefab;
                }
                Prefab = null;
            }
        }

        /// <summary>
        /// Destroys all pooled instances of prefabs loaded from specified path.
        /// </summary>
        /// <param name="prefabPath">Prefab path in Resources folder.</param>
        public static void DestroyPool (string prefabPath) {
            PrefabList list;
            if (_prefabPools.TryGetValue (prefabPath, out list)) {
                for (var i = 0; i < list.Count; i++) {
                    UnityEngine.Object.Destroy (list.Instances[i]);
                    list.Instances[i] = null;
                }
                list.Count = 0;
                UnityEngine.Object.Destroy (list.Prefab);
                list.Prefab = null;
                _prefabPools.Remove (prefabPath);
            }
        }

        sealed class PrefabList {
            public GameObject Prefab;
            public int Count;
            public GameObject[] Instances = new GameObject[4];
        }
    }
}