// ----------------------------------------------------------------------------
// The MIT License
// Simple Entity Component System framework https://github.com/Leopotam/ecs
// Copyright (c) 2017-2018 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------
#if !LEOECS_DISABLE_INJECT
using System;
using System.Reflection;

namespace LeopotamGroup.Ecs {
    [AttributeUsage (AttributeTargets.Class)]
    public sealed class EcsInjectAttribute : Attribute { }
}

namespace LeopotamGroup.Ecs.Internals {
    /// <summary>
    /// Processes dependency injection to ecs systems. For internal use only.
    /// </summary>
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
#endif
    static class EcsInjections {
        public static void Inject (EcsWorld world, IEcsSystem system) {
            var systemType = system.GetType ();
            if (!Attribute.IsDefined (systemType, typeof (EcsInjectAttribute))) {
                return;
            }
            var worldType = world.GetType ();
            var filterType = typeof (EcsFilter);

            foreach (var f in systemType.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                // EcsWorld
                if (f.FieldType.IsAssignableFrom (worldType) && !f.IsStatic) {
                    f.SetValue (system, world);
                }
                // EcsFilter
#if DEBUG
                if (f.FieldType == filterType) {
                    throw new Exception (
                        string.Format ("Cant use EcsFilter type at \"{0}\" system for dependency injection, use generic version instead", system));
                }
#endif
                if (f.FieldType.IsSubclassOf (filterType) && !f.IsStatic) {
                    f.SetValue (system, world.GetFilter (f.FieldType));
                }
            }
        }
    }
}
#endif