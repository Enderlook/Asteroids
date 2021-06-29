using Asteroids.PowerUps;
using Asteroids.Scene;

using Enderlook.GOAP;
using Enderlook.StateMachine;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        private StateBuilder<object, object, object> CreateAndAddWaitForPowerUpSpawnAbility(StateMachineBuilder<object, object, object> builder, int index)
        {
            Node<float> node = new Node<float>(
                (BossState _, ref BossState now) =>
                {
                    if (now.PowerUps == 0)
                        return SatisfactionResult.Satisfied;
                    return SatisfactionResult.NotProgressed;
                },
                (ref BossState worldState) => worldState.PowerUps == 0,
                worldState => Mathf.Max(PowerUpManager.SpawnTime - worldState.TimeSinceLastPowerUpWasSpawned, 1),
                (float _, ref BossState worldState) => {
                    worldState.PowerUps++;
                    worldState.TimeSinceLastPowerUpWasSpawned = 0;
                },
                null,
                cost => cost
            );
            actions[index] = node;

            return builder.In(node)
                .OnEntry(() => EventManager.Subscribe<OnPowerUpSpawnEvent>(Next))
                .OnExit(() => EventManager.Unsubscribe<OnPowerUpSpawnEvent>(Next));
        }
    }
}