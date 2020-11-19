using System;

namespace Asteroids.WeaponSystem
{
    public partial class Weapon
    {
        [Serializable]
        public readonly struct State
        {
            // This class is in charge of storing and setting the ability state to save the game.

            // Ability state the same as Ability memento, we can take advantage of that to keep DRY
            private readonly Memento memento;

            public State(Weapon weapon) => memento = new Memento(weapon);

            public void Load(Weapon weapon) => memento.Load(weapon);
        }
    }
}
