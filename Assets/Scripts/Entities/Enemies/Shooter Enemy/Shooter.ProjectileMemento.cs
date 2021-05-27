//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using Asteroids.Scene;

using System;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public partial class Shooter
    {
        [Serializable]
        private readonly struct ProjectileMemento
        {
            /* This struct is in charge of storing and setting the bullets state for rewinding
             * Technically, the create and set memento methods should be members of the Originator class
             * according to the pure Memento pattern.
             * However, that makes a bit convulted the Shooter class and increase its responsabilities amount.
             * 
             * This is why me add that logic in the Memento type and rewind here. So for the Shooter's point of view, it's only Memento.TrackForRewind.
             * Anyway, the implementation is not exposed because the Memento type is a nested type of the Shooter class
             * which allow us to access the private state of the Shooter without exposing it to other non-related classes.
             * 
             * This make easier to organice code.
             */

            // Cache delegate to reduce allocations
            private static readonly Func<ProjectileMemento, ProjectileMemento, float, ProjectileMemento> interpolateMementos = InterpolateMementos;

            private readonly bool enabled;
            private readonly SerializableVector2 position;
            private readonly float rotation;
            private readonly SerializableVector2 velocity;
            private readonly float angularVelocity;

            public ProjectileMemento(Rigidbody2D rigidbody) : this(
                rigidbody.gameObject.activeSelf,
                rigidbody.position,
                rigidbody.rotation,
                rigidbody.velocity,
                rigidbody.angularVelocity
            ) { }

            public ProjectileMemento(bool enabled, Vector2 position, float rotation, Vector2 velocity, float angularVelocity)
            {
                this.enabled = enabled;
                this.position = position;
                this.rotation = rotation;
                this.velocity = velocity;
                this.angularVelocity = angularVelocity;
            }

            public static void TrackForRewind(Shooter shooter, Rigidbody2D rigidbody) => GlobalMementoManager.Subscribe(
                    () => new ProjectileMemento(rigidbody),
                    (memento) => ConsumeMemento(memento, shooter, rigidbody),
                    interpolateMementos
                );

            private static void ConsumeMemento(ProjectileMemento? memento, Shooter shooter, Rigidbody2D rigidbody)
            {
                if (memento is ProjectileMemento memento_)
                    memento_.Load(shooter, rigidbody);
                else if (rigidbody.gameObject.activeSelf) // Don't pool something already pooled
                    shooter.builder.Store(rigidbody);
            }

            public void Load(Shooter shooter, Rigidbody2D rigidbody)
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

            private static ProjectileMemento InterpolateMementos(
                ProjectileMemento a,
                ProjectileMemento b,
                float delta
                ) => new ProjectileMemento(
                    delta > .5f ? b.enabled : a.enabled,
                    Vector2.Lerp(a.position, b.position, delta),
                    Mathf.LerpAngle(a.rotation, b.rotation, delta),
                    Vector2.Lerp(a.velocity, b.velocity, delta),
                    Mathf.Lerp(a.angularVelocity, b.angularVelocity, delta)
                );
        }
    }
}
