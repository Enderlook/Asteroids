using Enderlook.GOAP;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        public sealed class PickHealthPackAction : IAction<BossState, IGoal<BossState>>, IGoal<BossState>
        {
            private readonly struct Handle : IActionHandle<BossState, IGoal<BossState>>
            {
                private readonly float cost;
                private readonly PickHealthPackAction goal;

                public Handle(float cost, PickHealthPackAction goal)
                {
                    this.cost = cost;
                    this.goal = goal;
                }

                void IActionHandle<BossState, IGoal<BossState>>.ApplyEffect(ref BossState worldState)
                    => worldState.BossHealth = Mathf.Max(worldState.BossHealth + HealthRestoredPerPack, worldState.BossMaxHealth);

                bool IActionHandle<BossState, IGoal<BossState>>.CheckProceduralPreconditions() => true;

                bool IActionHandle<BossState, IGoal<BossState>>.GetCostAndRequiredGoal(out float cost, out IGoal<BossState> goal)
                {
                    cost = this.cost;
                    goal = this.goal;
                    return true;
                }
            }

            SatisfactionResult IGoal<BossState>.CheckAndTrySatisfy(BossState before, ref BossState now)
            {
                if (now.PowerUps > 0)
                {
                    now.PowerUps--;
                    return SatisfactionResult.Satisfied;
                }
                return SatisfactionResult.NotProgressed;
            }

            bool IGoal<BossState>.CheckAndTrySatisfy(ref BossState worldState)
            {
                if (worldState.PowerUps > 0)
                {
                    worldState.PowerUps--;
                    return true;
                }
                return false;
            }

            void IAction<BossState, IGoal<BossState>>.Visit<TActionHandleAcceptor>(ref TActionHandleAcceptor acceptor, BossState worldState)
                => acceptor.Accept(new Handle(worldState.BossMaxHealth - worldState.BossHealth, this));
        }
    }
}