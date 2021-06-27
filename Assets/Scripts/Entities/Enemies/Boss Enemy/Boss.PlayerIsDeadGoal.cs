using Enderlook.GOAP;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        public sealed class PlayerIsDeadGoal : IGoal<BossState>
        {
            SatisfactionResult IGoal<BossState>.CheckAndTrySatisfy(BossState before, ref BossState now)
            {
                if (now.PlayerHealth == 0)
                    return SatisfactionResult.Satisfied;
                if (now.PlayerHealth < before.PlayerHealth)
                    return SatisfactionResult.Progressed;
                return SatisfactionResult.NotProgressed;
            }

            bool IGoal<BossState>.CheckAndTrySatisfy(ref BossState worldState) => worldState.PlayerHealth == 0;
        }
    }
}