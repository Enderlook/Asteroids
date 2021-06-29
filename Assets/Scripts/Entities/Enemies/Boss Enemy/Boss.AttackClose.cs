using Enderlook.GOAP;
using Enderlook.StateMachine;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        private StateBuilder<object, object, object> CreateAndAddAttackCloseAbility(StateMachineBuilder<object, object, object> builder, int index)
        {
            Node node = new Node(
                (BossState before, ref BossState now) =>
                {
                    float distance = Vector3.Distance(now.PlayerPosition, now.BossPosition);
                    if (distance <= requiredDistanceToPlayerForCloseAttack && !IsTooHurt(now))
                        return SatisfactionResult.Satisfied;
                    if (distance < Vector3.Distance(before.PlayerPosition, before.BossPosition) || now.BossHealth > before.BossHealth)
                        return SatisfactionResult.Progressed;
                    return SatisfactionResult.NotProgressed;
                },
                (ref BossState worldState)
                    => Vector3.Distance(worldState.PlayerPosition, worldState.BossPosition) <= requiredDistanceToPlayerForCloseAttack && !IsTooHurt(worldState),
                (ref BossState worldState) =>
                {
                    worldState.PlayerHealth--;
                    worldState.AdvanceTime(CloseAttackDuration);
                },
                null,
                () => 2
            );
            actions[index] = node;

            float initialTime = 0;
            float initialRotation = 0;
            return builder.In(node)
                .OnEntry(() =>
                {
                    closeRange.gameObject.SetActive(true);
                    initialRotation = rigidbody.rotation;
                    initialTime = Time.time;
                })
                .OnExit(() => closeRange.gameObject.SetActive(false))
                .OnUpdate(() =>
                {
                    float percent = (Time.time - initialTime) / CloseAttackDuration;
                    if (percent >= 1)
                    {
                        rigidbody.rotation = initialRotation;
                        Next();
                    }
                    else
                        rigidbody.rotation = initialRotation + (360 * percent);
                });
        }
    }
}