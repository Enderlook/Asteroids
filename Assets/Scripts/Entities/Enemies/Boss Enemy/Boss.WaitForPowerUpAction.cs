using Asteroids.PowerUps;
using Asteroids.Scene;

using Enderlook.GOAP;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        private sealed class WaitForPowerUpAction : IFSMState, IAction<BossState, IGoal<BossState>>, IGoal<BossState>
        {
            private readonly Boss boss;

            public WaitForPowerUpAction(Boss boss) => this.boss = boss;

            private readonly struct Handle : IActionHandle<BossState, IGoal<BossState>>
            {
                private readonly float cost;
                private readonly WaitForPowerUpAction goal;

                public Handle(float cost, WaitForPowerUpAction goal)
                {
                    this.cost = cost;
                    this.goal = goal;
                }

                void IActionHandle<BossState, IGoal<BossState>>.ApplyEffect(ref BossState worldState)
                {
                    worldState.PowerUps++;
                    worldState.TimeSinceLastPowerUpWasSpawned = 0;
                }

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
                if (now.PowerUps == 0)
                    return SatisfactionResult.Satisfied;
                return SatisfactionResult.NotProgressed;
            }

            bool IGoal<BossState>.CheckAndTrySatisfy(ref BossState worldState) => worldState.PowerUps == 0;

            void IAction<BossState, IGoal<BossState>>.Visit<TActionHandleAcceptor>(ref TActionHandleAcceptor acceptor, BossState worldState)
                => acceptor.Accept(new Handle(Mathf.Max(PowerUpManager.SpawnTime - worldState.TimeSinceLastPowerUpWasSpawned, 1), this));

            void IFSMState.OnEntry() => EventManager.Subscribe<OnPowerUpSpawnEvent>(boss.Next);

            void IFSMState.OnExit() => EventManager.Unsubscribe<OnPowerUpSpawnEvent>(boss.Next);

            void IFSMState.OnUpdate() { }
        }
    }
}