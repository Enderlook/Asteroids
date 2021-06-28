using Asteroids.PowerUps;

using Enderlook.GOAP;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        private sealed class PickPowerUpAction : IFSMState, IAction<BossState, IGoal<BossState>>, IGoal<BossState>
        {
            private readonly Boss boss;

            private Transform powerUp;

            public PickPowerUpAction(Boss boss) => this.boss = boss;

            private readonly struct Handle : IActionHandle<BossState, IGoal<BossState>>
            {
                private readonly Boss boss;
                private readonly float cost;
                private readonly PickPowerUpAction parent;

                public Handle(Boss boss, float cost, PickPowerUpAction parent)
                {
                    this.boss = boss;
                    this.cost = cost;
                    this.parent = parent;
                }

                void IActionHandle<BossState, IGoal<BossState>>.ApplyEffect(ref BossState worldState)
                    => worldState.BossHealth = Mathf.Max(worldState.BossHealth + boss.healthRestoredPerPowerUp, parent.boss.lifes);

                bool IActionHandle<BossState, IGoal<BossState>>.CheckProceduralPreconditions() => true;

                bool IActionHandle<BossState, IGoal<BossState>>.GetCostAndRequiredGoal(out float cost, out IGoal<BossState> goal)
                {
                    cost = this.cost;
                    goal = parent;
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
                => acceptor.Accept(new Handle(boss, boss.lifes - worldState.BossHealth, this));

            void IFSMState.OnEntry()
            {
                FindPowerUp();

                if (powerUp == null)
                    boss.WorldIsNotAsExpected();
            }

            private void FindPowerUp()
            {
                float distance = float.PositiveInfinity;

                foreach (PowerUpTemplate.PickupBehaviour pickup in FindObjectsOfType<PowerUpTemplate.PickupBehaviour>())
                {
                    float newDistance = (pickup.transform.position - boss.transform.position).sqrMagnitude;
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        powerUp = pickup.transform;
                    }
                }
            }

            void IFSMState.OnExit() => powerUp = null;

            void IFSMState.OnUpdate()
            {
                if (powerUp == null)
                    // Player picked the power up, look for a new one.
                    FindPowerUp();

                if (powerUp == null)
                    boss.WorldIsNotAsExpected();
                else
                    boss.MoveAndRotateTowards(powerUp.position);
            }
        }
    }
}