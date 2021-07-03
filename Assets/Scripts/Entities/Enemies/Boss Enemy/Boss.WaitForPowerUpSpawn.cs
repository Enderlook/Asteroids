using Asteroids.PowerUps;
using Asteroids.Scene;

using Enderlook.GOAP;
using Enderlook.StateMachine;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        private void CreateAndAddWaitForPowerUpSpawnAbility(StateMachineBuilder<object, object, object> builder, StateBuilder<object, object, object>[] builders, int index)
        {
            Node<float> node = new Node<float>(
                (BossState _, ref BossState now) =>
                {
                    if (!now.HasPowerUpInScene)
                        return SatisfactionResult.Satisfied;
                    return SatisfactionResult.NotProgressed;
                },
                (ref BossState worldState) => !worldState.HasPowerUpInScene,
                worldState => Mathf.Max(PowerUpManager.SpawnTime - worldState.TimeSinceLastPowerUpWasSpawned, 1),
                (float _, ref BossState worldState) => {
                    worldState.HasPowerUpInScene = true;
                    worldState.TimeSinceLastPowerUpWasSpawned = 0;
                },
                null,
                cost => cost
            );
            actions[index] = node;

            builders[index] = builder.In(node)
                .OnEntry(() =>
                {
                    shield.SetActive(true);
                    EventManager.Subscribe<OnPowerUpSpawnEvent>(Next);
                })
                .OnExit(() =>
                {
                    shield.SetActive(false);
                    EventManager.Unsubscribe<OnPowerUpSpawnEvent>(Next);
                });
        }
    }
}