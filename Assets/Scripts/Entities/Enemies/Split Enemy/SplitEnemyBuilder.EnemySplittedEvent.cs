namespace Asteroids.Entities.Enemies
{
    public sealed partial class SplitEnemyBuilder
    {
        public readonly struct EnemySplittedEvent
        {
            public readonly int Amount;

            public EnemySplittedEvent(int amount) => Amount = amount;
        }
    }
}