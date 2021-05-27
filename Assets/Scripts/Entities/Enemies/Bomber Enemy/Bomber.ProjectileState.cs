//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using System;

namespace Asteroids.Entities.Enemies
{
    public partial class Bomber
    {
        [Serializable]
        public readonly struct ProjectileState
        {
            // This class is in charge of storing and setting a projectile state to save the game.

            // Projectiles state is actually a memento, we can take advantage of that to keep DRY
            private readonly Bomb.ProjectileMemento memento;

            public ProjectileState(Bomb bomb) => memento = new Bomb.ProjectileMemento(bomb);

            public void Load(Bomber shooter, Bomb bomb) => memento.Load(shooter, bomb);
        }
    }
}
