using Asteroids.Scene;

using System;

using UnityEngine;

namespace Asteroids.AbilitySystem
{
    public partial class ProjectileTrigger
    {
        [Serializable]
        private readonly struct Memento
        {
            /* This struct is in charge of storing and setting the bullets state for rewinding
             * Technically, the create and set memento methods should be members of the Originator class
             * according to the pure Memento pattern.
             * However, that makes a bit convulted the ProjectileTrigger class and increase its responsabilities amount.
             * 
             * This is why me add that logic in the Memento type and rewind here. So for the ProjectileTrigger's point of view, it's only Memento.TrackForRewind.
             * Anyway, the implementation is not exposed because the Memento type is a nested type of the ProjectileTrigger class
             * which allow us to access the private state of the ProjectileTrigger without exposing it to other non-related classes.
             * 
             * This make easier to organice code.
             */

            // Cache delegate to reduce allocations
            private static readonly Func<Memento, Memento, float, Memento> interpolateMementos = InterpolateMementos;

            public readonly bool enabled;
            public readonly SerializableVector2 position;
            public readonly float rotation;
            public readonly SerializableVector2 velocity;
            public readonly float angularVelocity;

            public Memento(Rigidbody2D rigidbody) : this(
                rigidbody.gameObject.activeSelf,
                rigidbody.position,
                rigidbody.rotation,
                rigidbody.velocity,
                rigidbody.angularVelocity
            ) { }

            public Memento(bool enabled, Vector2 position, float rotation, Vector2 velocity, float angularVelocity)
            {
                this.enabled = enabled;
                this.position = position;
                this.rotation = rotation;
                this.velocity = velocity;
                this.angularVelocity = angularVelocity;
            }

            public static void TrackForRewind(ProjectileTrigger projectileTrigger, Rigidbody2D rigidbody) => GlobalMementoManager.Subscribe(
                    () => new Memento(rigidbody),
                    (memento) => ConsumeMemento(memento, projectileTrigger, rigidbody),
                    interpolateMementos
                );

            private static void ConsumeMemento(Memento? memento, ProjectileTrigger projectileTrigger, Rigidbody2D rigidbody)
            {
                if (memento is Memento memento_)
                    memento_.Load(projectileTrigger, rigidbody);
                else
                    projectileTrigger.builder.Store(rigidbody);
            }

            public void Load(ProjectileTrigger projectileTrigger, Rigidbody2D rigidbody)
            {
                if (enabled)
                {
                    // Since bullets are pooled, we must force the pool to give us control of this instance in case it was in his control.
                    projectileTrigger.builder.ExtractIfHas(rigidbody);

                    rigidbody.position = position;
                    rigidbody.rotation = rotation;
                    rigidbody.velocity = velocity;
                    rigidbody.angularVelocity = angularVelocity;
                }
                else
                    projectileTrigger.builder.Store(rigidbody);
            }

            private static Memento InterpolateMementos(
                Memento a,
                Memento b,
                float delta
                ) => new Memento(
                    delta > .5f ? b.enabled : a.enabled,
                    Vector2.Lerp(a.position, b.position, delta),
                    Mathf.LerpAngle(a.rotation, b.rotation, delta),
                    Vector2.Lerp(a.velocity, b.velocity, delta),
                    Mathf.Lerp(a.angularVelocity, b.angularVelocity, delta)
                );
        }
    }
}
