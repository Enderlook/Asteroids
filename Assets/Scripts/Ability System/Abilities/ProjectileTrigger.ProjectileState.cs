using System;

using UnityEngine;

namespace Asteroids.AbilitySystem
{
    public partial class ProjectileTrigger
    {
        [Serializable]
        public readonly struct ProjectileState
        {
            // This class is in charge of storing and setting a projectile state to save the game.

            // Projectiles state is actually a memento, we can take advantage of that to keep DRY
            private readonly Memento memento;

            public ProjectileState(Rigidbody2D rigidbody) => memento = new Memento(rigidbody);

            public void Load(ProjectileTrigger projectileTrigger, Rigidbody2D rigidbody) => memento.Load(projectileTrigger, rigidbody);
        }
    }
}
