using System;

namespace Asteroids.WeaponSystem
{
    public partial class BombWeapon
    {
        [Serializable]
        public new readonly struct State
        {
            // This class is in charge of storing and setting the bomb weapon state to save the game.

            // Bomb weapon state is a superset of bomb weapon memento, we can take advantage of that to keep DRY
            private readonly Memento memento;

            // Bomb weapon state requires also to save the base class Ability state
            // and since we are using structs for perfomance we don't have inheritance
            // so we rely on composition
            private readonly Weapon.State parentState;

            public State(BombWeapon bompWeapon)
            {
                memento = new Memento(bompWeapon);
                parentState = new Weapon.State(bompWeapon);
            }

            public void Load(BombWeapon bompWeapon)
            {
                parentState.Load(bompWeapon);
                memento.Load(bompWeapon);
            }
        }
    }
}