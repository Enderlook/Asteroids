//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using System;

using UnityEngine;

namespace Asteroids.WeaponSystem
{
    public partial class Shooter
    {
        [Serializable]
        public readonly struct ProjectileState
        {
            // This class is in charge of storing and setting a projectile state to save the game.

            // Projectiles state is actually a memento, we can take advantage of that to keep DRY
            private readonly ProjectileMemento memento;

            public ProjectileState(Rigidbody2D rigidbody) => memento = new ProjectileMemento(rigidbody);

            public void Load(Shooter shooter, Rigidbody2D rigidbody) => memento.Load(shooter, rigidbody);
        }
    }
}
