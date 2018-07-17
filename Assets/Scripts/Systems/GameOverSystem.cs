using Components;
using LeopotamGroup.Ecs;
using UnityEngine;

namespace Systems
{
    [EcsInject]
    public class GameOverSystem : IEcsRunSystem
    {
        EcsWorld _world = null;

        EcsFilter<Player> _playerFilter = null;
        EcsFilter<CheckPoint> _checkPointFilter = null;
        EcsFilter<GameOverEvent> _gameOverFilter = null;
        EcsFilter<UserInputEvent> _userInputFilter = null;

        public void Run()
        {
            if (_gameOverFilter.EntitiesCount > 0)
            {
                ClearWorld();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
     #endif
            }
        }

        private void ClearWorld()
        {
            for (var i = 0; i < _playerFilter.EntitiesCount; i++)
            {
                var component = _playerFilter.Components1[i];
                component.Transform = null;
                component.Speed = 0;
                _world.RemoveEntity(_playerFilter.Entities[i]);
            }

            for (var i = 0; i < _checkPointFilter.EntitiesCount; i++)
            {
                _checkPointFilter.Components1[i].Coords = Vector3.zero;
                _world.RemoveEntity(_checkPointFilter.Entities[i]);
            }

            for (var i = 0; i < _userInputFilter.EntitiesCount; i++)
            {
                _userInputFilter.Components1[i].MoveDirection = Direction.Left;
                _world.RemoveEntity(_userInputFilter.Entities[i]);
            }

            for (var i = 0; i < _gameOverFilter.EntitiesCount; i++)
            {
                _world.RemoveEntity(_gameOverFilter.Entities[i]);
            }
        }
    }
}