// ----------------------------------------------------------------------------
// The MIT License
// Unity integration https://github.com/Leopotam/ecs-unityintegration
// for ECS framework https://github.com/Leopotam/ecs
// Copyright (c) 2018 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LeopotamGroup.Ecs.UnityIntegration {
    public sealed class EcsEntityObserver : MonoBehaviour {
        public EcsWorld World;

        public int Id;
    }

    public sealed class EcsSystemsObserver : MonoBehaviour, IEcsSystemsDebugListener {
        EcsSystems _systems;

        public static GameObject Create (EcsSystems systems, string name = null) {
            if (systems == null) {
                throw new ArgumentNullException ("systems");
            }
            var go = new GameObject (name != null ? string.Format ("[ECS-SYSTEMS {0}]", name) : "[ECS-SYSTEMS]");
            DontDestroyOnLoad (go);
            go.hideFlags = HideFlags.NotEditable;
            var observer = go.AddComponent<EcsSystemsObserver> ();
            observer._systems = systems;
            systems.AddDebugListener (observer);
            return go;
        }

        public EcsSystems GetSystems () {
            return _systems;
        }

        void OnDestroy () {
            if (_systems != null) {
                _systems.RemoveDebugListener (this);
                _systems = null;
            }
        }

        void IEcsSystemsDebugListener.OnSystemsDestroyed () {
            // for immediate unregistering this MonoBehaviour from ECS.
            OnDestroy ();
            // for delayed destroying GameObject.
            Destroy (gameObject);
        }
    }

    public sealed class EcsWorldObserver : MonoBehaviour, IEcsWorldDebugListener {
        EcsWorld _world;

        readonly Dictionary<int, GameObject> _entities = new Dictionary<int, GameObject> (1024);

        static object[] _componentsCache = new object[32];

        public static GameObject Create (EcsWorld world, string name = null) {
            if (world == null) {
                throw new ArgumentNullException ("world");
            }
            var go = new GameObject (name != null ? string.Format ("[ECS-WORLD {0}]", name) : "[ECS-WORLD]");
            DontDestroyOnLoad (go);
            go.hideFlags = HideFlags.NotEditable;
            var observer = go.AddComponent<EcsWorldObserver> ();
            observer._world = world;
            world.AddDebugListener (observer);
            return go;
        }

        public EcsWorldStats GetStats () {
            return _world.GetStats ();
        }

        void IEcsWorldDebugListener.OnEntityCreated (int entity) {
            GameObject go;
            if (!_entities.TryGetValue (entity, out go)) {
                go = new GameObject ();
                go.transform.SetParent (transform, false);
                go.hideFlags = HideFlags.NotEditable;
                var unityEntity = go.AddComponent<EcsEntityObserver> ();
                unityEntity.World = _world;
                unityEntity.Id = entity;
                _entities[entity] = go;
                UpdateEntityName (entity);
            }
            go.SetActive (true);
        }

        void IEcsWorldDebugListener.OnEntityRemoved (int entity) {
            GameObject go;
            if (!_entities.TryGetValue (entity, out go)) {
                throw new Exception ("Unity visualization not exists, looks like a bug");
            }
            UpdateEntityName (entity);
            go.SetActive (false);
        }

        void IEcsWorldDebugListener.OnComponentAdded (int entity, object component) {
            UpdateEntityName (entity);
        }

        void IEcsWorldDebugListener.OnComponentRemoved (int entity, object component) {
            UpdateEntityName (entity);
        }

        void UpdateEntityName (int entity) {
            var entityName = entity.ToString ("D8");
            var count = _world.GetComponents (entity, ref _componentsCache);
            for (var i = 0; i < count; i++) {
                entityName = string.Format ("{0}:{1}", entityName, _componentsCache[i].GetType ().Name);
                _componentsCache[i] = null;
            }
            _entities[entity].name = entityName;
        }

        void OnDestroy () {
            if (_world != null) {
                _world.RemoveDebugListener (this);
                _world = null;
            }
        }
    }
}
#endif