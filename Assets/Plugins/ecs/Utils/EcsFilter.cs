// ----------------------------------------------------------------------------
// The MIT License
// Simple Entity Component System framework https://github.com/Leopotam/ecs
// Copyright (c) 2017-2018 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System;
using LeopotamGroup.Ecs.Internals;

namespace LeopotamGroup.Ecs {
    /// <summary>
    /// Marks component class to be not autofilled as ComponentX in filter.
    /// </summary>
    [AttributeUsage (AttributeTargets.Class)]
    public sealed class EcsIgnoreInFilterAttribute : Attribute { }

    /// <summary>
    /// Container for single component for sharing between systems.
    /// </summary>
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
    public class EcsFilterSingle<Inc1> : EcsFilter where Inc1 : class, new () {
        public Inc1 Data;

        protected EcsFilterSingle () {
            IncludeMask.SetBit (EcsComponentPool<Inc1>.Instance.GetComponentTypeIndex (), true);
        }

        /// <summary>
        /// Creates entity with single component at specified EcsWorld.
        /// </summary>
        /// <param name="world">World instance.</param>
        public static Inc1 Create (EcsWorld world) {
            world.GetFilter<EcsFilterSingle<Inc1>> ();
            var data = world.CreateEntityWith<Inc1> ();
            world.ProcessDelayedUpdates ();
            return data;
        }

        public override void RaiseOnAddEvent (int entity) {
#if DEBUG
            if (EntitiesCount > 0) {
                throw new Exception (string.Format ("Cant add entity \"{1}\" to single filter \"{0}\": another one already added.", GetType (), entity));
            }
#endif
            Data = World.GetComponent<Inc1> (entity);
            Entities[EntitiesCount++] = entity;
        }

        public override void RaiseOnRemoveEvent (int entity) {
#if DEBUG
            if (EntitiesCount != 1 || Entities[0] != entity) {
                throw new Exception (string.Format ("Cant remove entity \"{1}\" from single filter \"{0}\".", GetType (), entity));
            }
#endif
            EntitiesCount--;
            Data = null;
        }
    }

    /// <summary>
    /// Container for filtered entities based on specified constraints.
    /// </summary>
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
    public class EcsFilter<Inc1> : EcsFilter where Inc1 : class, new () {
        public Inc1[] Components1;
        bool _allow1;

        protected EcsFilter () {
            _allow1 = !EcsComponentPool<Inc1>.Instance.IsIgnoreInFilter;
            Components1 = _allow1 ? new Inc1[MinSize] : null;
            IncludeMask.SetBit (EcsComponentPool<Inc1>.Instance.GetComponentTypeIndex (), true);
        }
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public override void RaiseOnAddEvent (int entity) {
            if (Entities.Length == EntitiesCount) {
                Array.Resize (ref Entities, EntitiesCount << 1);
                if (_allow1) {
                    Array.Resize (ref Components1, EntitiesCount << 1);
                }
            }
            if (_allow1) {
                Components1[EntitiesCount] = World.GetComponent<Inc1> (entity);
            }
            Entities[EntitiesCount++] = entity;
        }
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public override void RaiseOnRemoveEvent (int entity) {
            for (var i = 0; i < EntitiesCount; i++) {
                if (Entities[i] == entity) {
                    EntitiesCount--;
                    Array.Copy (Entities, i + 1, Entities, i, EntitiesCount - i);
                    if (_allow1) {
                        Array.Copy (Components1, i + 1, Components1, i, EntitiesCount - i);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Container for filtered entities based on specified constraints.
        /// </summary>
        public class Exclude<Exc1> : EcsFilter<Inc1> where Exc1 : class, new () {
            protected Exclude () {
                ExcludeMask.SetBit (EcsComponentPool<Exc1>.Instance.GetComponentTypeIndex (), true);
                ValidateMasks (1, 1);
            }
        }

        /// <summary>
        /// Container for filtered entities based on specified constraints.
        /// </summary>
        public class Exclude<Exc1, Exc2> : EcsFilter<Inc1> where Exc1 : class, new () where Exc2 : class, new () {
            protected Exclude () {
                ExcludeMask.SetBit (EcsComponentPool<Exc1>.Instance.GetComponentTypeIndex (), true);
                ExcludeMask.SetBit (EcsComponentPool<Exc2>.Instance.GetComponentTypeIndex (), true);
                ValidateMasks (1, 2);
            }
        }
    }

    /// <summary>
    /// Container for filtered entities based on specified constraints.
    /// </summary>
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
    public class EcsFilter<Inc1, Inc2> : EcsFilter where Inc1 : class, new () where Inc2 : class, new () {
        public Inc1[] Components1;
        public Inc2[] Components2;
        bool _allow1;
        bool _allow2;

        internal EcsFilter () {
            _allow1 = !EcsComponentPool<Inc1>.Instance.IsIgnoreInFilter;
            _allow2 = !EcsComponentPool<Inc2>.Instance.IsIgnoreInFilter;
            Components1 = _allow1 ? new Inc1[MinSize] : null;
            Components2 = _allow2 ? new Inc2[MinSize] : null;
            IncludeMask.SetBit (EcsComponentPool<Inc1>.Instance.GetComponentTypeIndex (), true);
            IncludeMask.SetBit (EcsComponentPool<Inc2>.Instance.GetComponentTypeIndex (), true);
            ValidateMasks (2, 0);
        }
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public override void RaiseOnAddEvent (int entity) {
            if (Entities.Length == EntitiesCount) {
                Array.Resize (ref Entities, EntitiesCount << 1);
                if (_allow1) {
                    Array.Resize (ref Components1, EntitiesCount << 1);
                }
                if (_allow2) {
                    Array.Resize (ref Components2, EntitiesCount << 1);
                }
            }
            if (_allow1) {
                Components1[EntitiesCount] = World.GetComponent<Inc1> (entity);
            }
            if (_allow2) {
                Components2[EntitiesCount] = World.GetComponent<Inc2> (entity);
            }
            Entities[EntitiesCount++] = entity;
        }
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public override void RaiseOnRemoveEvent (int entity) {
            for (var i = 0; i < EntitiesCount; i++) {
                if (Entities[i] == entity) {
                    EntitiesCount--;
                    Array.Copy (Entities, i + 1, Entities, i, EntitiesCount - i);
                    if (_allow1) {
                        Array.Copy (Components1, i + 1, Components1, i, EntitiesCount - i);
                    }
                    if (_allow2) {
                        Array.Copy (Components2, i + 1, Components2, i, EntitiesCount - i);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Container for filtered entities based on specified constraints.
        /// </summary>
        public class Exclude<Exc1> : EcsFilter<Inc1, Inc2> where Exc1 : class, new () {
            protected Exclude () {
                ExcludeMask.SetBit (EcsComponentPool<Exc1>.Instance.GetComponentTypeIndex (), true);
                ValidateMasks (2, 1);
            }
        }

        /// <summary>
        /// Container for filtered entities based on specified constraints.
        /// </summary>
        public class Exclude<Exc1, Exc2> : EcsFilter<Inc1, Inc2> where Exc1 : class, new () where Exc2 : class, new () {
            protected Exclude () {
                ExcludeMask.SetBit (EcsComponentPool<Exc1>.Instance.GetComponentTypeIndex (), true);
                ExcludeMask.SetBit (EcsComponentPool<Exc2>.Instance.GetComponentTypeIndex (), true);
                ValidateMasks (2, 2);
            }
        }
    }

    /// <summary>
    /// Container for filtered entities based on specified constraints.
    /// </summary>
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
    public class EcsFilter<Inc1, Inc2, Inc3> : EcsFilter where Inc1 : class, new () where Inc2 : class, new () where Inc3 : class, new () {
        public Inc1[] Components1;
        public Inc2[] Components2;
        public Inc3[] Components3;
        bool _allow1;
        bool _allow2;
        bool _allow3;

        protected EcsFilter () {
            _allow1 = !EcsComponentPool<Inc1>.Instance.IsIgnoreInFilter;
            _allow2 = !EcsComponentPool<Inc2>.Instance.IsIgnoreInFilter;
            _allow3 = !EcsComponentPool<Inc3>.Instance.IsIgnoreInFilter;
            Components1 = _allow1 ? new Inc1[MinSize] : null;
            Components2 = _allow2 ? new Inc2[MinSize] : null;
            Components3 = _allow3 ? new Inc3[MinSize] : null;
            IncludeMask.SetBit (EcsComponentPool<Inc1>.Instance.GetComponentTypeIndex (), true);
            IncludeMask.SetBit (EcsComponentPool<Inc2>.Instance.GetComponentTypeIndex (), true);
            IncludeMask.SetBit (EcsComponentPool<Inc3>.Instance.GetComponentTypeIndex (), true);
            ValidateMasks (3, 0);
        }
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public override void RaiseOnAddEvent (int entity) {
            if (Entities.Length == EntitiesCount) {
                Array.Resize (ref Entities, EntitiesCount << 1);
                if (_allow1) {
                    Array.Resize (ref Components1, EntitiesCount << 1);
                }
                if (_allow2) {
                    Array.Resize (ref Components2, EntitiesCount << 1);
                }
                if (_allow3) {
                    Array.Resize (ref Components3, EntitiesCount << 1);
                }
            }
            if (_allow1) {
                Components1[EntitiesCount] = World.GetComponent<Inc1> (entity);
            }
            if (_allow2) {
                Components2[EntitiesCount] = World.GetComponent<Inc2> (entity);
            }
            if (_allow3) {
                Components3[EntitiesCount] = World.GetComponent<Inc3> (entity);
            }
            Entities[EntitiesCount++] = entity;
        }
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public override void RaiseOnRemoveEvent (int entity) {
            for (var i = 0; i < EntitiesCount; i++) {
                if (Entities[i] == entity) {
                    EntitiesCount--;
                    Array.Copy (Entities, i + 1, Entities, i, EntitiesCount - i);
                    if (_allow1) {
                        Array.Copy (Components1, i + 1, Components1, i, EntitiesCount - i);
                    }
                    if (_allow2) {
                        Array.Copy (Components2, i + 1, Components2, i, EntitiesCount - i);
                    }
                    if (_allow3) {
                        Array.Copy (Components3, i + 1, Components3, i, EntitiesCount - i);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Container for filtered entities based on specified constraints.
        /// </summary>
        public class Exclude<Exc1> : EcsFilter<Inc1, Inc2, Inc3> where Exc1 : class, new () {
            protected Exclude () {
                ExcludeMask.SetBit (EcsComponentPool<Exc1>.Instance.GetComponentTypeIndex (), true);
                ValidateMasks (3, 1);
            }
        }

        /// <summary>
        /// Container for filtered entities based on specified constraints.
        /// </summary>
        public class Exclude<Exc1, Exc2> : EcsFilter<Inc1, Inc2, Inc3> where Exc1 : class, new () where Exc2 : class, new () {
            protected Exclude () {
                ExcludeMask.SetBit (EcsComponentPool<Exc1>.Instance.GetComponentTypeIndex (), true);
                ExcludeMask.SetBit (EcsComponentPool<Exc2>.Instance.GetComponentTypeIndex (), true);
                ValidateMasks (3, 2);
            }
        }
    }

    /// <summary>
    /// Container for filtered entities based on specified constraints.
    /// </summary>
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
    public class EcsFilter<Inc1, Inc2, Inc3, Inc4> : EcsFilter where Inc1 : class, new () where Inc2 : class, new () where Inc3 : class, new () where Inc4 : class, new () {
        public Inc1[] Components1;
        public Inc2[] Components2;
        public Inc3[] Components3;
        public Inc4[] Components4;
        bool _allow1;
        bool _allow2;
        bool _allow3;
        bool _allow4;

        protected EcsFilter () {
            _allow1 = !EcsComponentPool<Inc1>.Instance.IsIgnoreInFilter;
            _allow2 = !EcsComponentPool<Inc2>.Instance.IsIgnoreInFilter;
            _allow3 = !EcsComponentPool<Inc3>.Instance.IsIgnoreInFilter;
            _allow4 = !EcsComponentPool<Inc4>.Instance.IsIgnoreInFilter;
            Components1 = _allow1 ? new Inc1[MinSize] : null;
            Components2 = _allow2 ? new Inc2[MinSize] : null;
            Components3 = _allow3 ? new Inc3[MinSize] : null;
            Components4 = _allow4 ? new Inc4[MinSize] : null;
            IncludeMask.SetBit (EcsComponentPool<Inc1>.Instance.GetComponentTypeIndex (), true);
            IncludeMask.SetBit (EcsComponentPool<Inc2>.Instance.GetComponentTypeIndex (), true);
            IncludeMask.SetBit (EcsComponentPool<Inc3>.Instance.GetComponentTypeIndex (), true);
            IncludeMask.SetBit (EcsComponentPool<Inc4>.Instance.GetComponentTypeIndex (), true);
            ValidateMasks (4, 0);
        }
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public override void RaiseOnAddEvent (int entity) {
            if (Entities.Length == EntitiesCount) {
                Array.Resize (ref Entities, EntitiesCount << 1);
                if (_allow1) {
                    Array.Resize (ref Components1, EntitiesCount << 1);
                }
                if (_allow2) {
                    Array.Resize (ref Components2, EntitiesCount << 1);
                }
                if (_allow3) {
                    Array.Resize (ref Components3, EntitiesCount << 1);
                }
                if (_allow4) {
                    Array.Resize (ref Components4, EntitiesCount << 1);
                }
            }
            if (_allow1) {
                Components1[EntitiesCount] = World.GetComponent<Inc1> (entity);
            }
            if (_allow2) {
                Components2[EntitiesCount] = World.GetComponent<Inc2> (entity);
            }
            if (_allow3) {
                Components3[EntitiesCount] = World.GetComponent<Inc3> (entity);
            }
            if (_allow4) {
                Components4[EntitiesCount] = World.GetComponent<Inc4> (entity);
            }
            Entities[EntitiesCount++] = entity;
        }
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public override void RaiseOnRemoveEvent (int entity) {
            for (var i = 0; i < EntitiesCount; i++) {
                if (Entities[i] == entity) {
                    EntitiesCount--;
                    Array.Copy (Entities, i + 1, Entities, i, EntitiesCount - i);
                    if (_allow1) {
                        Array.Copy (Components1, i + 1, Components1, i, EntitiesCount - i);
                    }
                    if (_allow2) {
                        Array.Copy (Components2, i + 1, Components2, i, EntitiesCount - i);
                    }
                    if (_allow3) {
                        Array.Copy (Components3, i + 1, Components3, i, EntitiesCount - i);
                    }
                    if (_allow4) {
                        Array.Copy (Components4, i + 1, Components4, i, EntitiesCount - i);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Container for filtered entities based on specified constraints.
        /// </summary>
        public class Exclude<Exc1> : EcsFilter<Inc1, Inc2, Inc3, Inc4> where Exc1 : class, new () {
            protected Exclude () {
                ExcludeMask.SetBit (EcsComponentPool<Exc1>.Instance.GetComponentTypeIndex (), true);
                ValidateMasks (4, 1);
            }
        }

        /// <summary>
        /// Container for filtered entities based on specified constraints.
        /// </summary>
        public class Exclude<Exc1, Exc2> : EcsFilter<Inc1, Inc2, Inc3, Inc4> where Exc1 : class, new () where Exc2 : class, new () {
            protected Exclude () {
                ExcludeMask.SetBit (EcsComponentPool<Exc1>.Instance.GetComponentTypeIndex (), true);
                ExcludeMask.SetBit (EcsComponentPool<Exc2>.Instance.GetComponentTypeIndex (), true);
                ValidateMasks (4, 2);
            }
        }
    }

    /// <summary>
    /// Container for filtered entities based on specified constraints.
    /// </summary>
    public abstract class EcsFilter {
        /// <summary>
        /// Default minimal size for components / entities buffers.
        /// </summary>
        protected const int MinSize = 32;

        /// <summary>
        /// Mask of included (required) components with this filter.
        /// Do not change it manually!
        /// </summary>
        public readonly EcsComponentMask IncludeMask = new EcsComponentMask ();

        /// <summary>
        /// Mask of excluded (denied) components with this filter.
        /// Do not change it manually!
        /// </summary>
        public readonly EcsComponentMask ExcludeMask = new EcsComponentMask ();

        /// <summary>
        /// Instance of connected EcsWorld.
        /// Do not change it manually!
        /// </summary>
        protected EcsWorld World;

        internal void SetWorld (EcsWorld world) {
            World = world;
        }

        /// <summary>
        /// Will be raised by EcsWorld for new compatible with this filter entity.
        /// Do not call it manually!
        /// </summary>
        /// <param name="entity"></param>
        public abstract void RaiseOnAddEvent (int entity);

        /// <summary>
        /// Will be raised by EcsWorld for old already non-compatible with this filter entity.
        /// Do not call it manually!
        /// </summary>
        /// <param name="entity"></param>
        public abstract void RaiseOnRemoveEvent (int entity);

        /// <summary>
        /// Storage of filtered entities.
        /// Important: Length of this storage can be larger than real amount of items,
        /// use EntitiesCount instead of Entities.Length!
        /// Do not change it manually!
        /// </summary>
        public int[] Entities = new int[MinSize];

        /// <summary>
        /// Amount of filtered entities.
        /// </summary>
        public int EntitiesCount;

        /// <summary>
        /// Vaidates amount of constraint components.
        /// </summary>
        /// <param name="inc">Valid amount for included components.</param>
        /// <param name="exc">Valid amount for excluded components.</param>
        [System.Diagnostics.Conditional ("DEBUG")]
        protected void ValidateMasks (int inc, int exc) {
            if (IncludeMask.BitsCount != inc || ExcludeMask.BitsCount != exc) {
                throw new Exception (string.Format ("Invalid filter type \"{0}\": duplicated component types.", GetType ()));
            }
            if (IncludeMask.IsIntersects (ExcludeMask)) {
                throw new Exception (string.Format ("Invalid filter type \"{0}\": Include types intersects with exclude types.", GetType ()));
            }
        }
#if DEBUG
        public override string ToString () {
            return string.Format ("Filter(+{0} -{1})", IncludeMask, ExcludeMask);
        }
#endif
    }
}