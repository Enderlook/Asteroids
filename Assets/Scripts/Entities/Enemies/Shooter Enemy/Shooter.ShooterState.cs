//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using System;

namespace Asteroids.WeaponSystem
{
    public partial class Shooter
    {
        [Serializable]
        public readonly struct ShooterState
        {
            // This class is in charge of storing and setting a shooter state to save the game.

            // Projectiles state iss actually a memento, we can take advantage of that to keep DRY
            private readonly ShooterMemento memento;

            public ShooterState(Shooter shooter) => memento = new ShooterMemento(shooter);

            public void Load(Shooter shooter) => memento.Load(shooter);
        }
    }
}
