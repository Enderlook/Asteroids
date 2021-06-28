namespace Asteroids.PowerUps
{
    public readonly struct OnPowerUpPickedEvent
    {
        public readonly bool PickedByPlayer;

        public OnPowerUpPickedEvent(bool pickedByPlayer) => PickedByPlayer = pickedByPlayer;
    }
}
