using Asteroids.Scene;

using System;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public partial class BossShooter
    {
        [Serializable]
        public readonly struct ProjectileState
        {
            // This class is in charge of storing and setting a projectile state to save the game.

            private readonly bool enabled;
            private readonly SerializableVector2 position;
            private readonly float rotation;
            private readonly SerializableVector2 velocity;
            private readonly float angularVelocity;

            public ProjectileState(Rigidbody2D rigidbody)
            {
                enabled = rigidbody.gameObject.activeSelf;
                position = rigidbody.position;
                rotation = rigidbody.rotation;
                velocity = rigidbody.velocity;
                angularVelocity = rigidbody.angularVelocity;
            }

            public void Load(BossShooter shooter, Rigidbody2D rigidbody)
            {
                if (enabled)
                {
                    // Since bullets are pooled, we must force the pool to give us control of this instance in case it was in his control.
                    shooter.builder.ExtractIfHas(rigidbody);

                    rigidbody.position = position;
                    rigidbody.rotation = rotation;
                    rigidbody.velocity = velocity;
                    rigidbody.angularVelocity = angularVelocity;
                }
                else if (rigidbody.gameObject.activeSelf) // Don't pool something already pooled
                    shooter.builder.Store(rigidbody);
            }
        }
    }
}
