using System;

using UnityEngine;

namespace Asteroids.AbilitySystem
{
    public abstract partial class Ability
    {
        private readonly struct Memento
        {
            /* This struct is in charge of storing and setting the abilities state for rewinding
             * Technically, the create and set memento methods should be members of the Originator class
             * according to the pure Memento pattern.
             * However, that makes a bit convulted the Ability class and increase its responsabilities amount.
             * 
             * This is why me add that logic in the Memento type and rewind here. So for the LaserTrigger's point of view, it's only Memento.TrackForRewind.
             * Anyway, the implementation is not exposed because the Memento type is a nested type of the Ability class
             * which allow us to access the private state of the Ability without exposing it to other non-related classes.
             * 
             * This make easier to organice code.
             */

            // Cache delegate to reduce allocations
            private static readonly Func<Memento, Memento, float, Memento> interpolateMementos = InterpolateMementos;

            private readonly float cooldown;

            private Memento(float cooldown) => this.cooldown = cooldown;

            private Memento(Ability ability) : this(ability.nextCast - Time.fixedDeltaTime) { }

            public static void TrackForRewind(Ability ability)
                => GlobalMementoManager.Subscribe(
                    () => new Memento(ability), // Memento of ability is only its cooldown
                    (memento) => ConsumeMemento(memento, ability),
                    interpolateMementos
                );

            private static void ConsumeMemento(Memento? memento, Ability ability)
            {
                if (memento is Memento memento_)
                    ability.nextCast = memento_.cooldown + Time.fixedDeltaTime;
            }

            private static Memento InterpolateMementos(Memento a, Memento b, float delta)
                => new Memento(Mathf.Lerp(a.cooldown, b.cooldown, delta));
        }
    }
}
