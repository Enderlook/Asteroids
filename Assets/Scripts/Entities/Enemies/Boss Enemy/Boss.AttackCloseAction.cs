using Enderlook.GOAP;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        private sealed class AttackCloseAction : IFSMState, IAction<BossState, IGoal<BossState>>, IActionHandle<BossState, IGoal<BossState>>, IGoal<BossState>
        {
            private readonly Boss boss;

            private float initialTime;
            private float initialRotation;

            public AttackCloseAction(Boss boss) => this.boss = boss;

            void IActionHandle<BossState, IGoal<BossState>>.ApplyEffect(ref BossState worldState)
            {
                worldState.PlayerHealth--;
                worldState.AdvanceTime(CloseAttackDuration);
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
                if (distance <= ClosestDistanceToPlayer && !boss.IsTooHurt(now))
                    return SatisfactionResult.Satisfied;
                if (distance < Vector3.Distance(before.PlayerPosition, before.BossPosition) || now.BossHealth > before.BossHealth)
                    return SatisfactionResult.Progressed;
                return SatisfactionResult.NotProgressed;
            }

            bool IGoal<BossState>.CheckAndTrySatisfy(ref BossState worldState)
                => Vector3.Distance(worldState.PlayerPosition, worldState.BossPosition) <= ClosestDistanceToPlayer && !boss.IsTooHurt(worldState);

            bool IActionHandle<BossState, IGoal<BossState>>.CheckProceduralPreconditions() => true;

            void IAction<BossState, IGoal<BossState>>.Visit<TActionHandleAcceptor>(ref TActionHandleAcceptor acceptor, BossState worldState)
                => acceptor.Accept(this);

            void IFSMState.OnEntry()
            {
                boss.closeRange.gameObject.SetActive(true);
                initialRotation = boss.rigidbody.rotation;
                initialTime = Time.time;
            }

            void IFSMState.OnExit() => boss.closeRange.gameObject.SetActive(false);

            void IFSMState.OnUpdate()
            {
                float percent = (Time.time - initialTime) / CloseAttackDuration;
                if (percent >= 1)
                {
                    boss.rigidbody.rotation = initialRotation;
                    boss.Next();
                }
                else
                    boss.rigidbody.rotation = initialRotation + (360 * percent);
            }
        }
    }
}