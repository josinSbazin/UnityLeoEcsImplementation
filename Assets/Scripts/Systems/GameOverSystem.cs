using Components;
using LeopotamGroup.Ecs;
using UnityEngine;

namespace Systems
{
    [EcsInject]
    public class GameOverSystem : IEcsRunSystem
    {
        EcsFilter<GameOverEvent> _gameOverFilter = null;

        public void Run()
        {
            if (_gameOverFilter.EntitiesCount > 0)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
     #endif
            }
        }
    }
}