//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using Asteroids.Scene;

using System;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public partial class Bomber
    {
        [Serializable]
        private readonly struct BomberMemento
        {
            /* This struct is in charge of storing and setting the abilities state for rewinding
             * Technically, the create and set memento methods should be members of the Originator class
             * according to the pure Memento pattern.
             * However, that makes a bit convulted the Ability class and increase its responsabilities amount.
             * 
             * This is why me add that logic in the Memento type and rewind here. So for the Bomber's point of view, it's only Memento.TrackForRewind.
             * Anyway, the implementation is not exposed because the Memento type is a nested type of the Ability class
             * which allow us to access the private state of the Bomber without exposing it to other non-related classes.
             * 
             * This make easier to organice code.
             */

            // Cache delegate to reduce allocations
            private static readonly Func<BomberMemento, BomberMemento, float, BomberMemento> interpolateMementos = InterpolateMementos;

            private readonly float cooldown;

            private BomberMemento(float cooldown) => this.cooldown = cooldown;

            public BomberMemento(Bomber shooter) : this(shooter.nextCast - Time.fixedDeltaTime) { }

            public static void TrackForRewind(Bomber shooter)
                => GlobalMementoManager.Subscribe(
                    () => new BomberMemento(shooter), // Memento of ability is only its cooldown
                    (memento) => ConsumeMemento(memento, shooter),
                    interpolateMementos
                );

            private static void ConsumeMemento(BomberMemento? memento, Bomber shooter)
            {
                if (memento is BomberMemento memento_)
                    memento_.Load(shooter);
            }

            public void Load(Bomber shooter) => shooter.nextCast = cooldown + Time.fixedDeltaTime;

            private static BomberMemento InterpolateMementos(BomberMemento a, BomberMemento b, float delta)
                => new BomberMemento(Mathf.Lerp(a.cooldown, b.cooldown, delta));
        }
    }
}
