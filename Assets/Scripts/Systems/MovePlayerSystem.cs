using Components;
using LeopotamGroup.Ecs;
using UnityEngine;

namespace Systems
{
    public enum Direction
    {
        Left,
        Right
    }

    [EcsInject]
    sealed class MovePlayerSystem : IEcsInitSystem, IEcsRunSystem
    {
        private const float PlayerSpeed = 10f;
        
        const string PlayerTag = "Player";

        EcsWorld _world = null;
        EcsFilter<Player> _playerFilter = null;
        EcsFilter<UserInputEvent> _userInputFilter = null;

        public void Initialize()
        {
            foreach (var unityObject in GameObject.FindGameObjectsWithTag(PlayerTag))
            {
                var tr = unityObject.transform;
                var player = _world.CreateEntityWith<Player>();
                player.Speed = PlayerSpeed;
                player.Transform = tr;
            }
        }

        public void Destroy()
        {
        }

        public void Run()
        {
            for (int i = 0; i < _userInputFilter.EntitiesCount; i++)
            {
                var direction = _userInputFilter.Components1[i].MoveDirection;
                for (int j = 0; j < _playerFilter.EntitiesCount; j++)
                {
                    var player = _playerFilter.Components1[j];

                    var delta = Time.deltaTime * player.Speed;

                    if (direction == Direction.Left)
                    {
                        delta *= -1f;
                    }

                    player.Transform.localPosition += new Vector3(delta, 0f, 0f);
                }
                _world.RemoveEntity(_userInputFilter.Entities[i]);
            }
        }
    }
}