namespace Asteroids.Scene
{
    public sealed partial class GameManager
    {
        public readonly struct ScoreHasChangedEvent
        {
            public readonly int NewScore;

            public ScoreHasChangedEvent(int newScore) => NewScore = newScore;
        }
    }
}