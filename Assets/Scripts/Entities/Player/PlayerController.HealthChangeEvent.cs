namespace Asteroids.Entities.Player
{
    public partial class PlayerController
    {
        public readonly struct HealthChangedEvent
        {
            public enum ReasonType
            {
                LoseByHit,
                EarnByScore,
                EarnByPowerUp,
            }

            public readonly ReasonType Reason;

            public bool IsIncrease => !IsDecrease;

            public bool IsDecrease => Reason == ReasonType.LoseByHit;

            private HealthChangedEvent(ReasonType reason) => Reason = reason;

            public static HealthChangedEvent IncreaseByScore => new HealthChangedEvent(ReasonType.EarnByScore);

            public static HealthChangedEvent IncreaseByPowerUp => new HealthChangedEvent(ReasonType.EarnByPowerUp);

            public static HealthChangedEvent Decrease => new HealthChangedEvent(ReasonType.LoseByHit);
        }
    }
}