// ----------------------------------------------------------------------------
// The MIT License
// Simple Entity Component System framework https://github.com/Leopotam/ecs
// Copyright (c) 2017-2018 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System;
#if !UNITY_WEBGL
using System.Threading;
#endif

namespace LeopotamGroup.Ecs {
    /// <summary>
    /// Base system for multithreading processing. In WebGL - will work like IEcsRunSystem system.
    /// </summary>
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
    public abstract class EcsMultiThreadSystem : IEcsInitSystem, IEcsRunSystem {
#if !UNITY_WEBGL
        WorkerDesc[] _descs;

        ManualResetEvent[] _syncs;
#endif
        EcsMultiThreadJob _localJob;

        EcsWorld _world;

        EcsFilter _filter;

        Action<EcsMultiThreadJob> _worker;

        int _minJobSize;

        int _threadsCount;

        bool _forceSyncState;

        /// <summary>
        /// Force synchronized threads to main thread (lock main thread and await results from threads).
        /// </summary>
        public void ForceSync () {
#if !UNITY_WEBGL
            WaitHandle.WaitAll (_syncs);
#endif
        }

        public virtual void Destroy () {
#if !UNITY_WEBGL
            for (var i = 0; i < _descs.Length; i++) {
                var desc = _descs[i];
                _descs[i] = null;
                desc.Thread.Interrupt ();
                desc.Thread.Join (10);
                _syncs[i].Close ();
                _syncs[i] = null;
            }
#endif
            _world = null;
            _filter = null;
            _worker = null;
        }

        public virtual void Initialize () {
            _world = GetWorld ();
            _filter = GetFilter ();
            _worker = GetWorker ();
            _minJobSize = GetMinJobSize ();
            _threadsCount = GetThreadsCount ();
            _forceSyncState = GetForceSyncState ();
#if DEBUG
            if (_world == null) {
                throw new Exception ("Invalid EcsWorld");
            }
            if (_filter == null) {
                throw new Exception ("Invalid EcsFilter");
            }
            if (_minJobSize < 1) {
                throw new Exception ("Invalid JobSize");
            }
            if (_threadsCount < 1) {
                throw new Exception ("Invalid ThreadsCount");
            }
#endif
#if !UNITY_WEBGL
            _descs = new WorkerDesc[_threadsCount];
            _syncs = new ManualResetEvent[_threadsCount];
            EcsMultiThreadJob job;
            for (var i = 0; i < _descs.Length; i++) {
                job = new EcsMultiThreadJob ();
                job.World = _world;
                var desc = new WorkerDesc ();
                desc.Job = job;
                desc.Thread = new Thread (ThreadProc);
                desc.Thread.IsBackground = true;
#if DEBUG
                desc.Thread.Name = string.Format ("ECS-{0:X}-{1}", this.GetHashCode (), i);
#endif
                desc.HasWork = new ManualResetEvent (false);
                desc.WorkDone = new ManualResetEvent (true);
                desc.Worker = _worker;
                _descs[i] = desc;
                _syncs[i] = desc.WorkDone;
                desc.Thread.Start (desc);
            }
#endif
            _localJob = new EcsMultiThreadJob ();
            _localJob.World = _world;
        }

        void IEcsRunSystem.Run () {
            var count = _filter.EntitiesCount;
            var processed = 0;
#if !UNITY_WEBGL
            var jobSize = count / (_threadsCount + 1);
            int workersCount;
            if (jobSize > _minJobSize) {
                workersCount = _threadsCount + 1;
            } else {
                workersCount = count / _minJobSize;
                jobSize = _minJobSize;
            }
            for (var i = 0; i < workersCount - 1; i++) {
                var desc = _descs[i];
                desc.Job.Entities = _filter.Entities;
                desc.Job.From = processed;
                processed += jobSize;
                desc.Job.To = processed;
                desc.WorkDone.Reset ();
                desc.HasWork.Set ();
            }
#endif
            _localJob.Entities = _filter.Entities;
            _localJob.From = processed;
            _localJob.To = count;
            _worker (_localJob);
            if (_forceSyncState) {
                ForceSync ();
            }
        }
#if !UNITY_WEBGL
        void ThreadProc (object rawDesc) {
            var desc = (WorkerDesc) rawDesc;
            try {
                while (Thread.CurrentThread.IsAlive) {
                    desc.HasWork.WaitOne ();
                    desc.HasWork.Reset ();
                    desc.Worker (desc.Job);
                    desc.WorkDone.Set ();
                }
            } catch { }
        }

        sealed class WorkerDesc {
            public Thread Thread;
            public ManualResetEvent HasWork;
            public ManualResetEvent WorkDone;
            public Action<EcsMultiThreadJob> Worker;
            public EcsMultiThreadJob Job;
        }
#endif
        /// <summary>
        /// EcsWorld instance to use in custom worker.
        /// </summary>
        protected abstract EcsWorld GetWorld ();

        /// <summary>
        /// Source filter for processing entities from it.
        /// </summary>
        protected abstract EcsFilter GetFilter ();

        /// <summary>
        /// Custom processor of received entities.
        /// </summary>
        protected abstract Action<EcsMultiThreadJob> GetWorker ();

        /// <summary>
        /// Minimal amount of entities to process by one worker.
        /// </summary>
        protected abstract int GetMinJobSize ();

        /// <summary>
        /// How many threads should be used by this system.
        /// </summary>
        protected abstract int GetThreadsCount ();

        /// <summary>
        /// Should threads be force synchronized to main thread (lock main thread and await results from threads).
        /// Use with care - ForceSync() method should be called in current update frame!
        /// </summary>
        protected virtual bool GetForceSyncState () {
            return true;
        }
    }

    /// <summary>
    /// Job info for multithreading processing.
    /// </summary>
    public struct EcsMultiThreadJob {
        /// <summary>
        /// EcsWorld instance.
        /// </summary>
        public IEcsReadOnlyWorld World;

        /// <summary>
        /// Entities list to processing.
        /// </summary>
        public int[] Entities;

        /// <summary>
        /// Index of first entity in list to processing.
        /// </summary>
        public int From;

        /// <summary>
        /// Index of entity after last item to processing (should be excluded from processing).
        /// </summary>
        public int To;
    }
}