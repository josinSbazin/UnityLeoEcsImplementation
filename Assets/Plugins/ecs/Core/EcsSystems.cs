// ----------------------------------------------------------------------------
// The MIT License
// Simple Entity Component System framework https://github.com/Leopotam/ecs
// Copyright (c) 2017-2018 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System;

namespace LeopotamGroup.Ecs {
    /// <summary>
    /// Base interface for all ecs systems.
    /// </summary>
    public interface IEcsSystem { }

    /// <summary>
    /// Allows custom initialization / deinitialization for ecs system.
    /// </summary>
    public interface IEcsInitSystem : IEcsSystem {
        /// <summary>
        /// Initializes system inside EcsWorld instance.
        /// </summary>
        void Initialize ();

        /// <summary>
        /// Destroys all internal allocated data.
        /// </summary>
        void Destroy ();
    }

    /// <summary>
    /// Allows custom logic processing.
    /// </summary>
    public interface IEcsRunSystem : IEcsSystem {
        /// <summary>
        /// Custom logic.
        /// </summary>
        void Run ();
    }

#if DEBUG
    /// <summary>
    /// Debug interface for systems events processing.
    /// </summary>
    public interface IEcsSystemsDebugListener {
        void OnSystemsDestroyed ();
    }
#endif

    /// <summary>
    /// Logical group of systems.
    /// </summary>
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
    public sealed class EcsSystems {
#if DEBUG
        /// <summary>
        /// List of all debug listeners.
        /// </summary>
        readonly System.Collections.Generic.List<IEcsSystemsDebugListener> _debugListeners = new System.Collections.Generic.List<IEcsSystemsDebugListener> (4);

        readonly public System.Collections.Generic.List<bool> DisabledInDebugSystems = new System.Collections.Generic.List<bool> (4);
#endif

        /// <summary>
        /// Ecs world instance.
        /// </summary>
        readonly EcsWorld _world;

        /// <summary>
        /// Registered IEcsInitSystem systems.
        /// </summary>
        IEcsInitSystem[] _initSystems = new IEcsInitSystem[16];

        /// <summary>
        /// Count of registered IEcsInitSystem systems.
        /// </summary>
        int _initSystemsCount;

        /// <summary>
        /// Registered IEcsRunSystem systems.
        /// </summary>
        IEcsRunSystem[] _runSystems = new IEcsRunSystem[16];

        /// <summary>
        /// Count of registered IEcsRunSystem systems.
        /// </summary>
        int _runSystemsCount;

#if DEBUG
        /// <summary>
        /// Is Initialize method was called?
        /// </summary>
        bool _inited;
#endif

        public EcsSystems (EcsWorld world) {
#if DEBUG
            if (world == null) {
                throw new ArgumentNullException ();
            }
#endif
            _world = world;
        }

#if DEBUG
        /// <summary>
        /// Adds external event listener.
        /// </summary>
        /// <param name="observer">Event listener.</param>
        public void AddDebugListener (IEcsSystemsDebugListener observer) {
            if (_debugListeners.Contains (observer)) {
                throw new Exception ("Listener already exists");
            }
            _debugListeners.Add (observer);
        }

        /// <summary>
        /// Removes external event listener.
        /// </summary>
        /// <param name="observer">Event listener.</param>
        public void RemoveDebugListener (IEcsSystemsDebugListener observer) {
            _debugListeners.Remove (observer);
        }
#endif

        /// <summary>
        /// Gets all init systems.
        /// </summary>
        /// <param name="list">List to put results in it. If null - will be created.</param>
        /// <returns>Amount of systems in list.</returns>
        public int GetInitSystems (ref IEcsInitSystem[] list) {
            if (list == null || list.Length < _initSystemsCount) {
                list = new IEcsInitSystem[_initSystemsCount];
            }
            Array.Copy (_initSystems, 0, list, 0, _initSystemsCount);
            return _initSystemsCount;
        }

        /// <summary>
        /// Gets all run systems.
        /// </summary>
        /// <param name="list">List to put results in it. If null - will be created.</param>
        /// <returns>Amount of systems in list.</returns>
        public int GetRunSystems (ref IEcsRunSystem[] list) {
            if (list == null || list.Length < _runSystemsCount) {
                list = new IEcsRunSystem[_runSystemsCount];
            }
            Array.Copy (_runSystems, 0, list, 0, _runSystemsCount);
            return _runSystemsCount;
        }

        /// <summary>
        /// Adds new system to processing.
        /// </summary>
        /// <param name="system">System instance.</param>
        public EcsSystems Add (IEcsSystem system) {
#if DEBUG
            if (system == null) {
                throw new ArgumentNullException ();
            }
#endif
#if !LEOECS_DISABLE_INJECT
            Internals.EcsInjections.Inject (_world, system);
#endif

            var initSystem = system as IEcsInitSystem;
            if (initSystem != null) {
                if (_initSystemsCount == _initSystems.Length) {
                    Array.Resize (ref _initSystems, _initSystemsCount << 1);
                }
                _initSystems[_initSystemsCount++] = initSystem;
            }

            var runSystem = system as IEcsRunSystem;
            if (runSystem != null) {
                if (_runSystemsCount == _runSystems.Length) {
                    Array.Resize (ref _runSystems, _runSystemsCount << 1);
                }
                _runSystems[_runSystemsCount++] = runSystem;
            }
            return this;
        }

        /// <summary>
        /// Closes registration for new systems, initialize all registered.
        /// </summary>
        public void Initialize () {
#if DEBUG
            if (_inited) {
                throw new Exception ("Group already initialized.");
            }
            for (var i = 0; i < _runSystemsCount; i++) {
                DisabledInDebugSystems.Add (false);
            }
            _inited = true;
#endif
            for (var i = 0; i < _initSystemsCount; i++) {
                _initSystems[i].Initialize ();
                _world.ProcessDelayedUpdates ();
            }
        }

        /// <summary>
        /// Destroys all registered external data, full cleanup for internal data.
        /// </summary>
        public void Destroy () {
#if DEBUG
            if (!_inited) {
                throw new Exception ("Group not initialized.");
            }
            for (var i = _debugListeners.Count - 1; i >= 0; i--) {
                _debugListeners[i].OnSystemsDestroyed ();
            }
            _debugListeners.Clear ();
            DisabledInDebugSystems.Clear ();
            _inited = false;
#endif
            for (var i = _initSystemsCount - 1; i >= 0; i--) {
                _initSystems[i].Destroy ();
                _initSystems[i] = null;
            }
            _initSystemsCount = 0;

            for (var i = _runSystemsCount - 1; i >= 0; i--) {
                _runSystems[i] = null;
            }
            _runSystemsCount = 0;
        }

        /// <summary>
        /// Processes all IEcsRunSystem systems.
        /// </summary>
        public void Run () {
#if DEBUG
            if (!_inited) {
                throw new Exception ("Group not initialized.");
            }
#endif
            for (var i = 0; i < _runSystemsCount; i++) {
#if DEBUG
                if (DisabledInDebugSystems[i]) {
                    continue;
                }
#endif
                _runSystems[i].Run ();
                _world.ProcessDelayedUpdates ();
            }
        }
    }
}