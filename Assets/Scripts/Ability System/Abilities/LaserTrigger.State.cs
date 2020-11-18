namespace Asteroids.AbilitySystem
{
    public partial class LaserTrigger
    {
        public new readonly struct State
        {
            // This class is in charge of storing and setting the laser trigger state to save the game.

            private readonly float currentDuration;

            // Laser state requires also to save the base class Ability state
            // and since we are using structs for perfomance we don't have inheritance
            // so we rely on composition
            private readonly Ability.State parentState;

            public State(LaserTrigger laserTrigger)
            {
                currentDuration = laserTrigger.currentDuration;
                parentState = new Ability.State(laserTrigger);
            }

            public void Load(LaserTrigger laserTrigger)
            {
                parentState.Load(laserTrigger);
                laserTrigger.currentDuration = currentDuration;
            }
        }
    }
}
