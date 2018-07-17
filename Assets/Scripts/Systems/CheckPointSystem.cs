using Components;
using LeopotamGroup.Ecs;
using UnityEngine;

namespace Systems
{
    [EcsInject]
    public class CheckPointSystem : IEcsInitSystem, IEcsRunSystem
    {
        EcsWorld _world = null;
        EcsFilter<Player> _playerFilter = null;
        EcsFilter<CheckPoint> _checkPointFilter = null;
        
        public void Initialize()
        {
            const string CheckPointTag = "CheckPoint";
            
            foreach (var unityObject in GameObject.FindGameObjectsWithTag(CheckPointTag))
            {
                var tr = unityObject.transform;
                var checkPoint = _world.CreateEntityWith<CheckPoint>();
                checkPoint.Coords = tr.localPosition;
            }
        }

        public void Destroy()
        {
        }

        public void Run()
        {
            for (int i = 0; i < _playerFilter.EntitiesCount; i++)
            {
                var playerPosition = _playerFilter.Components1[i].Transform.localPosition;
                for (int j = 0; j < _checkPointFilter.EntitiesCount; j++)
                {
                    var checkPointCoords = _checkPointFilter.Components1[j].Coords;
                    var magnitude = (checkPointCoords - playerPosition).magnitude;
                    if (magnitude <= 1.0f)
                    {
                        _world.CreateEntityWith<GameOverEvent>();
                        return;
                    }
                }
            }
        }
    }
}