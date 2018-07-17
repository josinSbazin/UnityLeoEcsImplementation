[![gitter](https://img.shields.io/gitter/room/leopotam/ecs.svg)](https://gitter.im/leopotam/ecs)
[![license](https://img.shields.io/github/license/Leopotam/ecs-ui.svg)](https://github.com/Leopotam/ecs-ui/blob/develop/LICENSE)
# Unity integration for Entity Component System framework
[Unity integration](https://github.com/Leopotam/ecs-unityintegration) for [ECS framework](https://github.com/Leopotam/ecs).

> Tested on unity 2018.1 (dependent on Unity engine) and contains assembly definition for compiling to separate assembly file for performance reason.

> Dependent on [ECS framework](https://github.com/Leopotam/ecs) - ECS framework should be imported to unity project first.

# Editor integration

## EcsWorld observer
Integration can be processed with one call of `LeopotamGroup.Ecs.UnityIntegration.EcsWorldObserver.Create()` metod - this call should be wrapped to `#if UNITY_EDITOR` preprocessor define:
```
public class Startup : MonoBehaviour {
    EcsSystems _systems;

    void Start () {
        var world = new EcsWorld ();
        
#if UNITY_EDITOR
        UnityIntegration.EcsWorldObserver.Create (world);
#endif  
        _systems = new EcsSystems(world)
            .Add (new RunSystem1());
            // Additional initialization here...
        _systems.Initialize ();
    }
}
```

Observer **must** be created before any entity will be created in ecs-world.

## EcsSystems observer
Integration can be processed with one call of `LeopotamGroup.Ecs.UnityIntegration.EcsSystemsObserver.Create()` metod - this call should be wrapped to `#if UNITY_EDITOR` preprocessor define:
```
public class Startup : MonoBehaviour {
    EcsSystems _systems;

    void Start () {
        var world = new EcsWorld ();
        
#if UNITY_EDITOR
        UnityIntegration.EcsWorldObserver.Create (world);
#endif        
        _systems = new EcsSystems(world)
            .Add (new RunSystem1());
            // Additional initialization here...
        _systems.Initialize ();
#if UNITY_EDITOR
        UnityIntegration.EcsSystemsObserver.Create (_systems);
#endif
    }
}
```

# Runtime integration
## UnityPrefabComponent
Supports spawning instances of prefab from `Resources` folder. Instead of removing instance will be placed in pool for reuse it in next time:
```
// Path to prefab inside Resources folder.
const string PrefabPath = "Test/Cube";
...
// Creating...
var prefab = _world.AddComponent<UnityPrefabComponent>(entity);
prefab.Attach(PrefabPath);
// prefab.Prefab field will be filled with instance of spawned unity prefab.
// This instance will be in deactivated state - should be activated after attaching!
prefab.Prefab.transform.Position = new Vector3(100f, 200f, 300f);
prefab.Prefab.SetActive(true);
...
// Destroying...
// No need to deactivate instance manually - component will do it automatically during pooling.
// Dont forget to call Detach method before removing component!
prefab.Detach();
_world.RemoveEntity<UnityPrefabComponent>(entity);
```

# FAQ

### I can't edit component fields at any ecs-entity observer.
By design, observer works as readonly copy of ecs world data - you can copy value, but not change it.

### I want to create custom inspector view for my component.
Custom component `MyComponent1`:
```
public enum MyEnum { True, False }

public class MyComponent1 {
    public MyEnum State;
    public string Name;
}
```
Inspector for `MyComponent1` (should be placed in `Editor` folder):
```
class MyComponent1Inspector : IEcsComponentInspector {
    Type IEcsComponentInspector.GetFieldType () {
        return typeof (MyComponent1);
    }

    void IEcsComponentInspector.OnGUI (string label, object value) {
        var component = value as MyComponent1;
        EditorGUILayout.LabelField (label, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.EnumPopup ("State", component.State);
        EditorGUILayout.TextField ("Name", component.Name);
        EditorGUI.indentLevel--;
    }
}
```

# License
The software released under the terms of the MIT license. Enjoy.