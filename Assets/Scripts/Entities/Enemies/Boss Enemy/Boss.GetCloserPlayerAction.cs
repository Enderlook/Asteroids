using Asteroids.Entities.Player;

using Enderlook.GOAP;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        private sealed class GetCloserPlayerAction : IFSMState, IAction<BossState, IGoal<BossState>>
        {
            private readonly Boss boss;

            public GetCloserPlayerAction(Boss boss) => this.boss = boss;

            private readonly struct Handle : IActionHandle<BossState, IGoal<BossState>>
            {
                private readonly float distance;
                private readonly Boss boss;

                public Handle(Boss boss, float distance)
                {
                    this.boss = boss;
                    this.distance = distance;
                }

                void IActionHandle<BossState, IGoal<BossState>>.ApplyEffect(ref BossState worldState)
                {
                    Vector3 difference = worldState.PlayerPosition - worldState.BossPosition;
                    worldState.BossPosition -= Vector3.Normalize(difference) * distance;
                    worldState.AdvanceTime(distance / boss.movementSpeed);
                }

                bool IActionHandle<BossState, IGoal<BossState>>.CheckProceduralPreconditions() => true;

                bool IActionHandle<BossState, IGoal<BossState>>.GetCostAndRequiredGoal(out float cost, out IGoal<BossState> goal)
                {
                    cost = distance;
                    goal = default;
                    return false;
                }
            }

            void IAction<BossState, IGoal<BossState>>.Visit<TActionHandleAcceptor>(ref TActionHandleAcceptor acceptor, BossState worldState)
                => acceptor.Accept(new Handle(boss, Mathf.Max(Vector3.Distance(worldState.PlayerPosition, worldState.BossPosition) - boss.requiredDistanceToPlayerForCloseAttack, 0)));

            public void OnEntry() { }

            public void OnExit() { }

            public void OnUpdate()
            {
                boss.MoveAndRotateTowards(PlayerController.Position);
                if (Vector3.Distance(PlayerController.Position, boss.transform.position) < boss.requiredDistanceToPlayerForCloseAttack)
                    boss.Next();
            }
        }
    }
}