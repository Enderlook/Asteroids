using System;

namespace Asteroids.AbilitySystem
{
    public partial class LaserTrigger
    {
        [Serializable]
        public new readonly struct State
        {
            // This class is in charge of storing and setting the laser trigger state to save the game.

            // Laser state is a superset of Laser memento, we can take advantage of that to keep DRY
            private readonly Memento memento;

            // Laser state requires also to save the base class Ability state
            // and since we are using structs for perfomance we don't have inheritance
            // so we rely on composition
            private readonly Ability.State parentState;

            public State(LaserTrigger laserTrigger)
            {
                memento = new Memento(laserTrigger);
                parentState = new Ability.State(laserTrigger);
            }

            public void Load(LaserTrigger laserTrigger)
            {
                parentState.Load(laserTrigger);
                memento.Load(laserTrigger);
            }
        }
    }
}
