﻿using Enderlook.GOAP;
using Enderlook.StateMachine;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        private void CreateAndAddAttackFarAbility(StateMachineBuilder<object, object, object> builder, StateBuilder<object, object, object>[] builders, int index)
        {
            Node node = new Node(
                (BossState before, ref BossState now) =>
                {
                    float distance = Vector3.Distance(now.PlayerPosition, now.BossPosition);
                    if (distance >= FurtherDistanceToPlayer && !IsTooHurt(now))
                        return SatisfactionResult.Satisfied;
                    if (distance > Vector3.Distance(before.PlayerPosition, before.BossPosition) || now.BossHealth > before.BossHealth)
                        return SatisfactionResult.Progressed;
                    return SatisfactionResult.NotProgressed;
                },
                (ref BossState worldState)
                    => Vector3.Distance(worldState.PlayerPosition, worldState.BossPosition) >= FurtherDistanceToPlayer && !IsTooHurt(worldState),
                (ref BossState worldState) =>
                {
                    worldState.PlayerHealth--;
                    worldState.AdvanceTime(AverageTimeRequiredByFarAttack);
                },
                null,
                () => 4
            );
            actions[index] = node;

            BossShooter bossShooter = GetComponent<BossShooter>();
            builders[index] = builder.In(node)
                .OnEntry(() => bossShooter.enabled = true)
                .OnExit(() => bossShooter.enabled = false);
        }
    }
}