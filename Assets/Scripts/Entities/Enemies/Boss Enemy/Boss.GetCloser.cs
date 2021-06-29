using Asteroids.Entities.Player;

using Enderlook.StateMachine;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        private StateBuilder<object, object, object> CreateAndAddGetCloserAbility(StateMachineBuilder<object, object, object> builder, int index)
        {
            Node<float> node = new Node<float>(
                null,
                null,
                worldState => Mathf.Max(Vector3.Distance(worldState.PlayerPosition, worldState.BossPosition) - requiredDistanceToPlayerForCloseAttack, 0),
                (float distance, ref BossState worldState) =>
                {
                    Vector3 difference = worldState.PlayerPosition - worldState.BossPosition;
                    worldState.BossPosition -= Vector3.Normalize(difference) * distance;
                    worldState.AdvanceTime(distance / movementSpeed);
                },
                null,
                distance => distance
            );
            actions[index] = node;

            return builder.In(node)
                .OnUpdate(() =>
                {
                    MoveAndRotateTowards(PlayerController.Position);
                    if (Vector3.Distance(PlayerController.Position, transform.position) < requiredDistanceToPlayerForCloseAttack)
                        Next();
                });
        }
    }
}