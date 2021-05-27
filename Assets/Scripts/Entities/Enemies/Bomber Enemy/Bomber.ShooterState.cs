//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using System;

namespace Asteroids.Entities.Enemies
{
    public partial class Bomber
    {
        [Serializable]
        public readonly struct BomberState
        {
            // This class is in charge of storing and setting a shooter state to save the game.

            // Projectiles state iss actually a memento, we can take advantage of that to keep DRY
            private readonly BomberMemento memento;

            public BomberState(Bomber shooter) => memento = new BomberMemento(shooter);

            public void Load(Bomber shooter) => memento.Load(shooter);
        }
    }
}
