using Enderlook.GOAP;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        public sealed class GetCloserPlayerAction : IAction<BossState, IGoal<BossState>>
        {
            private readonly struct Handle : IActionHandle<BossState, IGoal<BossState>>
            {
                private readonly float distance;

                public Handle(float distance) => this.distance = distance;

                void IActionHandle<BossState, IGoal<BossState>>.ApplyEffect(ref BossState worldState)
                {
                    Vector3 difference = worldState.PlayerPosition - worldState.BossPosition;
                    worldState.BossPosition -= Vector3.Normalize(difference) * distance;
                    worldState.AdvanceTime(distance / worldState.BossMovementSpeed);
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
                => acceptor.Accept(new Handle(Vector3.Distance(worldState.PlayerPosition, worldState.BossPosition) - ClosestDistanceToPlayer));
        }
    }
}