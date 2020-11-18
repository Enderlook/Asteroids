namespace Asteroids.Scene
{
    public partial class GameManager
    {
        public readonly struct ScoreHasChangedEvent
        {
            public readonly int NewScore;

            public ScoreHasChangedEvent(int newScore) => NewScore = newScore;
        }
    }
}