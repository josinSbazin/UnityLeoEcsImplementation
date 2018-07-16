using Systems;
using LeopotamGroup.Ecs;
using UnityEngine;

public class GameStartup : MonoBehaviour
{
    EcsWorld _world;
    EcsSystems _systems;

    private void OnEnable()
    {
        _world = new EcsWorld();
        _systems = new EcsSystems(_world)
            .Add(new UserInputSystem())
            .Add(new MovePlayerSystem())
            .Add(new CheckPointSystem())
            .Add(new GameOverSystem());
        _systems.Initialize();
    }

    private void Update()
    {
        _systems.Run();
    }

    private void OnDisable()
    {
        _systems.Destroy();
    }
}