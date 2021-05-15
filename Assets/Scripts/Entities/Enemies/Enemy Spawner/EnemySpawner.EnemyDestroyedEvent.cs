namespace Asteroids.Entities.Enemies
{
    public sealed partial class EnemySpawner
    {
        public readonly struct EnemyDestroyedEvent
        {
            public readonly int Score;

            public EnemyDestroyedEvent(int score) => Score = score;
        }
    }
}