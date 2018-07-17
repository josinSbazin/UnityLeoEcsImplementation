using Systems;
using LeopotamGroup.Ecs;
using UnityEngine;

public class GameStartup : MonoBehaviour
{
    public float PlayerSpeed;
    
    EcsWorld _world;
    EcsSystems _systems;

    private void OnEnable()
    {
        _world = new EcsWorld();
        
#if UNITY_EDITOR
        LeopotamGroup.Ecs.UnityIntegration.EcsWorldObserver.Create (_world);
#endif     
        _systems = new EcsSystems(_world)
            .Add(new UserInputSystem())
            .Add(new MovePlayerSystem())
            .Add(new CheckPointSystem())
            .Add(new GameOverSystem());
        _systems.Initialize();
#if UNITY_EDITOR
        LeopotamGroup.Ecs.UnityIntegration.EcsSystemsObserver.Create (_systems);
#endif
    }

    private void Update()
    {
        _systems.Run();
    }

    private void OnDisable()
    {
        _systems.Destroy();
        _world.Dispose();
    }
}