using Asteroids.Entities.Player;

using Enderlook.StateMachine;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        private void CreateAndAddGetFurtherAbility(StateMachineBuilder<object, object, object> builder, StateBuilder<object, object, object>[] builders, int index)
        {
            Node<float> node = new Node<float>(
                null,
                null,
                worldState => Mathf.Max(RequiredDistanceToPlayerForFarAttack - Vector3.Distance(worldState.PlayerPosition, worldState.BossPosition), 0),
                (float distance, ref BossState worldState) =>
                {
                    Vector3 difference = worldState.BossPosition + worldState.PlayerPosition;
                    worldState.BossPosition += Vector3.Normalize(difference) * distance;
                    worldState.AdvanceTime(distance / movementSpeed);
                },
                null,
                distance => distance
            );
            actions[index] = node;

            builders[index] = builder.In(node)
                .OnUpdate(() =>
                {
                    MoveAndRotateAway(PlayerController.Position);
                    if (Vector3.Distance(PlayerController.Position, transform.position) >= RequiredDistanceToPlayerForFarAttack)
                        Next();
                });
        }
    }
}