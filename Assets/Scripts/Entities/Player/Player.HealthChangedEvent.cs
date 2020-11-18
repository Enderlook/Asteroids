namespace Asteroids.Entities.Player
{
    public partial class Player
    {
        public readonly struct HealthChangedEvent
        {
            public readonly bool HasIncreased;

            public HealthChangedEvent(bool hasIncreased) => HasIncreased = hasIncreased;

            public static HealthChangedEvent Increase => new HealthChangedEvent(true);

            public static HealthChangedEvent Decrease => new HealthChangedEvent(false);
        }
    }
}
