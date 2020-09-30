using Asteroids.Entities.Enemies;

namespace Asteroids.Events
{
    public readonly struct LevelTerminationEvent
    {
        public readonly bool HasWon;

        public LevelTerminationEvent(bool hasWon) => HasWon = hasWon;

        public static LevelTerminationEvent Win => new LevelTerminationEvent(true);

        public static LevelTerminationEvent Lose => new LevelTerminationEvent(false);
    }

    public readonly struct PlayerHealthChangedEvent
    {
        public readonly bool HasIncreased;

        public PlayerHealthChangedEvent(bool hasIncreased) => HasIncreased = hasIncreased;

        public static PlayerHealthChangedEvent Increase => new PlayerHealthChangedEvent(true);

        public static PlayerHealthChangedEvent Decrease => new PlayerHealthChangedEvent(false);
    }

    public readonly struct EnemyDestroyedEvent
    {
        public readonly EnemyBuilderData.EnemyHandler Enemy;
        public readonly int Score;

        public EnemyDestroyedEvent(EnemyBuilderData.EnemyHandler enemy, int score)
        {
            Enemy = enemy;
            Score = score;
        }
    }

    public readonly struct ScoreHasChangedEvent
    {
        public readonly int NewScore;

        public ScoreHasChangedEvent(int newScore) => NewScore = newScore;
    }
}
