using System;

namespace Asteroids.WeaponSystem
{
    public sealed partial class BombWeapon
    {
        public sealed partial class Bomb
        {
            [Serializable]
            public readonly struct State
            {
                // This class is in charge of storing and setting a bomb state to save the game.

                // Bomb state is actually a memento, we can take advantage of that to keep DRY
                private readonly Memento memento;

                public State(Bomb bomb) => memento = new Memento(bomb);

                public void Load(BombWeapon bombWeapon, Bomb bomb) => memento.Load(bombWeapon, bomb);
            }
        }
    }
}