namespace Asteroids.UI
{
    public sealed partial class PauseManager
    {
        public readonly struct PauseEvent
        {
            public readonly bool IsPaused;

            public bool IsPlaying => !IsPaused;

            public PauseEvent(bool isPaused) => IsPaused = isPaused;

            public static PauseEvent Pause => new PauseEvent(true);

            public static PauseEvent Play => new PauseEvent(false);
        }
    }
}
