[![gitter](https://img.shields.io/gitter/room/leopotam/ecs.svg)](https://gitter.im/leopotam/ecs)
[![license](https://img.shields.io/github/license/Leopotam/ecs.svg)](https://github.com/Leopotam/ecs/blob/develop/LICENSE)
# LeoECS - Another one Entity Component System framework
Performance and zero memory allocation / small size, no dependencies on any game engine - main goals of this project.

> Tested on unity 2018.1 (not dependent on it) and contains assembly definition for compiling to separate assembly file for performance reason.

> **Important!** Dont forget to use `DEBUG` builds for development and `RELEASE` builds in production: all internal error checks / exception throwing works only in `DEBUG` builds and eleminated for performance reasons in `RELEASE`.

# Main parts of ecs

## Component
Container for user data without / with small logic inside. Can be used any user class without any additional inheritance:
```
class WeaponComponent {
    public int Ammo;
    public string GunName;
}
```

> **Important!** Dont forget to manually init all fields of new added component. Default value initializers will not work due all components can be reused automatically multiple times through builtin pooling mechanism (no destroying / creating new instance for each request for performance reason).

> **Important!** Dont forget to cleanup reference links to instances of another components / engine classes before removing components from entity, otherwise it can lead to memory leaks.

## Entity
Сontainer for components. Implemented with int id-s for more simplified api:
```
int entity = _world.CreateEntity ();
_world.RemoveEntity (entity);
```

> **Important!** Entities without components on them will be automatically removed from `EcsWorld` right after finish execution of current system.

## System
Сontainer for logic for processing filtered entities. User class should implements `IEcsInitSystem` or / and `IEcsRunSystem` interfaces:
```
class WeaponSystem : IEcsInitSystem {
    void IEcsInitSystem.Initialize () {
        // Will be called once during world initialization.
    }

    void IEcsInitSystem.Destroy () {
        // Will be called once during world destruction.
    }
}
```

```
class HealthSystem : IEcsRunSystem {
    void IEcsRunSystem.Run () {
        // Will be called on each EcsSystems.Run() call.
    }
}
```

# Data injection
> **Important!** Will not work when LEOECS_DISABLE_INJECT preprocessor constant defined.

With `[EcsInject]` attribute over `IEcsSystem` class all compatible `EcsWorld` and `EcsFilter<>` fields of instance of this class will be auto-initialized (auto-injected):
```
[EcsInject]
class HealthSystem : IEcsSystem {
    EcsWorld _world = null;

    EcsFilter<WeaponComponent> _weaponFilter = null;
}
```

# Special classes

## EcsFilter<>
Container for keep filtered entities with specified component list:
```
[EcsInject]
class WeaponSystem : IEcsInitSystem, IEcsRunSystem {
    EcsWorld _world = null;

    // We wants to get entities with "WeaponComponent" and without "HealthComponent".
    EcsFilter<WeaponComponent>.Exclude<HealthComponent> _filter = null;

    void IEcsInitSystem.Initialize () {
        _world.CreateEntityWith<WeaponComponent> ();
    }

    void IEcsInitSystem.Destroy () { }

    void IEcsRunSystem.Run () {
        // Important: foreach-loop cant be used for filtered entities!
        for (var i = 0; i < _filter.EntitiesCount; i++) {
            // Components1 array fill be automatically filled with instances of type "WeaponComponent".
            var weapon = _filter.Components1[i];
            weapon.Ammo = System.Math.Max (0, weapon.Ammo - 1);
        }
    }
}
```

All compatible entities will be stored at `filter.Entities` array, amount of them - at `filter.EntitiesCount`.

> Important: `filter.Entities` cant be iterated with foreach-loop, for-loop should be used instead with filter.EntitiesCount value as upper-bound.

All components from filter `Include` constraint will be stored at `filter.Components1`, `filter.Components2`, etc - in same order as they were used in filter type declaration.

If autofilling not required (for example, for flag-based components without data), `EcsIgnoreInFilter` attribute can be used for decrease memory usage and increase performance:
```
class Component1 { }

[EcsIgnoreInFilter]
class Component2 { }

[EcsInject]
class TestSystem : IEcsSystem {
    EcsFilter<Component1, Component2> _filter;

    public Test() {
        for (var i = 0; i < _filter.EntitiesCount; i++) {
            // its valid code.
            var component1 = _filter.Components1[i];

            // its invalid code due to _filter.Components2 is null for memory / performance reasons.
            var component2 = _filter.Components2[i];
        }
    }
}
```

> Important: Any filter supports up to 4 component types as "include" constraints and up to 2 component types as "exclude" constraints. Shorter constraints - better performance.

> Important: If you will try to use 2 filters with same components but in different order - you will get exception with detailed info about conflicted types, but only in `DEBUG` mode. In `RELEASE` mode all checks will be skipped.

## EcsWorld
Root level container for all entities / components, works like isolated environment.

## EcsSystems
Group of systems to process `EcsWorld` instance:
```
class Startup : MonoBehaviour {
    EcsSystems _systems;

    void OnEnable() {
        // create ecs environment.
        var world = new EcsWorld ();
        _systems = new EcsSystems(world)
            .Add (new WeaponSystem ());
        _systems.Initialize ();
    }
    
    void Update() {
        // process all dependent systems.
        _systems.Run ();
    }

    void OnDisable() {
        // destroy systems logical group.
        _systems.Destroy ();
    }
}
```

# Sharing data between systems
If some component should be shared between systems `EcsFilterSingle<>` filter class can be used in this case:
```
class MySharedData {
    public string PlayerName;
    public int AchivementsCount;
}

[EcsInject]
class ChangePlayerName : IEcsInitSystem {
    EcsFilterSingle<MySharedData> _shared = null;

    void IEcsInitSystem.Initialize () {
        _shared.Data.PlayerName = "Jack";
    }

    void IEcsInitSystem.Destroy () { }
}

[EcsInject]
class SpawnPlayerModel : IEcsInitSystem {
    EcsFilterSingle<MySharedData> _shared = null;

    void IEcsInitSystem.Initialize () {
        Debug.LogFormat("Player with name {0} should be spawn here", _shared.Data.PlayerName);
    }

    void IEcsInitSystem.Destroy () { }
}

class Startup : Monobehaviour {
    EcsWorld _world;

    EcsSystems _systems;

    void OnEnable() {
        _world = new MyWorld (_sharedData);
        
        // This method should be called before any system will be added to EcsSystems group.
        var data = EcsFilterSingle<MySharedData>.Create(_world);
        data.PlayerName = "Unknown";
        data.AchivementsCount = 123;

        _systems = new EcsSystems(_world)
            .Add (ChangePlayerName())
            .Add (SpawnPlayerModel());
        // All EcsFilterSingle<MySharedData> fields already injected here and systems can be initialized correctly.
        _systems.Initialize();
    }

    void OnDisable() {
        // var data = _world.GetFilter<EcsFilterSingle<MySharedData>>().Data;
        // Do not forget to cleanup all reference links inside shared components to another data here.
        // ...

        _world.Dispose();
        _systems = null;
        _world = null;
    }
}
```

> Important: `EcsFilterSingle<>.Create(EcsWorld)` method should be called before any system will be added to EcsSystems group connected to same `EcsWorld` instance.

Another way - creating custom world class with inheritance from `EcsWorld` and filling shared fields manually.

# Examples
[Snake game](https://github.com/Leopotam/ecs-snake)

[Pacman game](https://github.com/SH42913/pacmanecs)

# Extensions
[Engine independent types](https://github.com/Leopotam/ecs-types)

[Unity integration](https://github.com/Leopotam/ecs-unityintegration)

[Unity uGui event bindings](https://github.com/Leopotam/ecs-ui)

# License
The software released under the terms of the MIT license. Enjoy.

# FAQ

### My project complex enough, I need more than 256 components. How I can do it?

There are no components limit, but for performance / memory usage reason better to keep amount of components on each entity less or equals 8.

### I want to create alot of new entities with new components on start, how to speed up this process?

In this case custom component creator can be used (for speed up 2x or more):

```
class MyComponent { }

class Startup : Monobehaviour {
    EcsSystems _systems;

    void OnEnable() {
        var world = new MyWorld (_sharedData);
        
        EcsWorld.RegisterComponentCreator<MyComponent> (() => new MyComponent());
        
        _systems = new EcsSystems(world)
            .Add (MySystem());
        _systems.Initialize();
    }
}
```

### I want to process one system at MonoBehaviour.Update() and another - at MonoBehaviour.FixedUpdate(). How I can do it?

For splitting systems by `MonoBehaviour`-method multiple `EcsSystems` logical groups should be used:
```
EcsSystems _update;
EcsSystems _fixedUpdate;

void OnEnable() {
    var world = new EcsWorld();
    _update = new EcsSystems(world).Add(new UpdateSystem());
    _fixedUpdate = new EcsSystems(world).Add(new FixedUpdateSystem());
}

void Update() {
    _update.Run();
}

void FixedUpdate() {
    _fixedUpdate.Run();
}
```

### I do not need dependency injection through `Reflection` (I heard, it's very slooooow! / I want to use my own way to inject). How I can do it?

Builtin Reflection-based DI can be removed with **LEOECS_DISABLE_INJECT** preprocessor define:
* No `EcsInject` attribute.
* No automatic injection for `EcsWorld` and `EcsFilter<>` fields.
* Less code size.`

`EcsWorld` should be injected somehow (for example, through constructor of system), `EcsFilter<>` data can be requested through `EcsWorld.GetFilter<>` method.

### I used reactive systems and filter events before, but now I can't find them. How I can get it back?

Reactive events support was removed for performance reason and for more clear execution flow of components processing by systems:
* Less internal magic.
* Less code size.
* Small performance gain.
* Less memory usage.

If you really need them - better to stay on ["v20180422 release"](https://github.com/Leopotam/ecs/releases/tag/v20180422).

### I need more than 4 components in filter, how i can do it?

First of all - looks like there are problems in architecture and better to rethink it. Anyway, custom filter can be implemented it this way:

```
// Custom class should be inherited from EcsFilter.
public class CustomEcsFilter<Inc1> : EcsFilter where Inc1 : class, new () {
    public Inc1[] Components1;
    bool _allow1;

    // Access can be any, even non-public.
    protected CustomEcsFilter () {
        // We should check - is requested type should be not auto-filled in Components1 array.
        _allow1 = !EcsComponentPool<Inc1>.Instance.IsIgnoreInFilter;
        Components1 = _allow1 ? new Inc1[MinSize] : null;

        // And set valid bit of required component at IncludeMask.
        IncludeMask.SetBit (EcsComponentPool<Inc1>.Instance.GetComponentTypeIndex (), true);

        // Its recommended method for masks validation (will be auto-removed in RELEASE-mode).
        ValidateMasks (1, 0);
    }

    // This method will be called for all new compatible entities.
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

    // This method will be removed for added before, but already non-compatible entities.
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

    // Even exclude filters can be declared in this way.
    public class Exclude<Exc1, Exc2> : CustomEcsFilter<Inc1> where Exc1 : class, new () {
        internal Exclude () {
            // Update ExcludeMask for 2 denied types.
            ExcludeMask.SetBit (EcsComponentPool<Exc1>.Instance.GetComponentTypeIndex (), true);
            ExcludeMask.SetBit (EcsComponentPool<Exc2>.Instance.GetComponentTypeIndex (), true);
            // And validate all masks (1 included type, 2 excluded type).
            ValidateMasks (1, 2);
        }
    }
}
```

> You can even add your own events inside `RaiseOnAddEvent` / `RaiseOnRemoveEvent` calls, but i do not recommend it and you will do it at your own peril.

### How it fast relative to Entitas?

[Previous version](https://github.com/Leopotam/ecs/releases/tag/v20180422) was benchmarked at [this repo](https://github.com/echeg/unityecs_speedtest). Current version works in slightly different manner, better to grab last versions of ECS frameworks and check boths locally on your code.