using Components;
using LeopotamGroup.Ecs;
using UnityEngine;

namespace Systems
{
	[EcsInject]
	sealed class UserInputSystem : IEcsRunSystem
	{
		EcsWorld _world = null;

		public void Run()
		{
			var x = Input.GetAxis("Horizontal");
			Direction direction;
			if (x > 0f)
			{
				direction = Direction.Right;
			} else if (x < 0f)
			{
				direction = Direction.Left;
			}
			else
			{
				return;
			}

			var inputEvent = _world.CreateEntityWith<UserInputEvent>();
			inputEvent.MoveDirection = direction;
		} 
	}
}
