using Enderlook.GOAP;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        public sealed class AttackCloseAction : IAction<BossState, IGoal<BossState>>, IActionHandle<BossState, IGoal<BossState>>, IGoal<BossState>
        {
            void IActionHandle<BossState, IGoal<BossState>>.ApplyEffect(ref BossState worldState)
            {
                worldState.PlayerHealth--;
                worldState.AdvanceTime(AverageTimeRequiredByCloseAttack);
            }

            bool IActionHandle<BossState, IGoal<BossState>>.GetCostAndRequiredGoal(out float cost, out IGoal<BossState> goal)
            {
                cost = 2;
                goal = this;
                return true;
            }

            SatisfactionResult IGoal<BossState>.CheckAndTrySatisfy(BossState before, ref BossState now)
            {
                float distance = Vector3.Distance(now.PlayerPosition, now.BossPosition);
                if (distance <= ClosestDistanceToPlayer && !now.IsBossTooHurt)
                    return SatisfactionResult.Satisfied;
                if (distance < Vector3.Distance(before.PlayerPosition, before.BossPosition) || now.BossHealth > before.BossHealth)
                    return SatisfactionResult.Progressed;
                return SatisfactionResult.NotProgressed;
            }

            bool IGoal<BossState>.CheckAndTrySatisfy(ref BossState worldState)
                => Vector3.Distance(worldState.PlayerPosition, worldState.BossPosition) <= ClosestDistanceToPlayer && !worldState.IsBossTooHurt;

            bool IActionHandle<BossState, IGoal<BossState>>.CheckProceduralPreconditions() => true;

            void IAction<BossState, IGoal<BossState>>.Visit<TActionHandleAcceptor>(ref TActionHandleAcceptor acceptor, BossState worldState)
                => acceptor.Accept(this);
        }
    }
}