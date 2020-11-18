namespace Asteroids.Scene
{
    public partial class GameManager
    {
        public readonly struct LevelTerminationEvent
        {
            public readonly bool HasWon;

            public bool HasLost => !HasWon;

            public LevelTerminationEvent(bool hasWon) => HasWon = hasWon;

            public static LevelTerminationEvent Win => new LevelTerminationEvent(true);

            public static LevelTerminationEvent Lose => new LevelTerminationEvent(false);
        }
    }
}