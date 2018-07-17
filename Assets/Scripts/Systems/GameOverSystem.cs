using Components;
using LeopotamGroup.Ecs;
using UnityEngine;

namespace Systems
{
    [EcsInject]
    public class GameOverSystem : IEcsRunSystem
    {
        EcsWorld _world = null;
        EcsFilter<GameOverEvent> _gameOverFilter = null;

        public void Run()
        {
            if (_gameOverFilter.EntitiesCount > 0)
            {
                _world.Dispose();
                
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
     #endif
            }
        }
    }
}