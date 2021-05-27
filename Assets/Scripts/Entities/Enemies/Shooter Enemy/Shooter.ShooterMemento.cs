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
        private readonly struct ShooterMemento
        {
            /* This struct is in charge of storing and setting the abilities state for rewinding
             * Technically, the create and set memento methods should be members of the Originator class
             * according to the pure Memento pattern.
             * However, that makes a bit convulted the Ability class and increase its responsabilities amount.
             * 
             * This is why me add that logic in the Memento type and rewind here. So for the Shooter's point of view, it's only Memento.TrackForRewind.
             * Anyway, the implementation is not exposed because the Memento type is a nested type of the Ability class
             * which allow us to access the private state of the Shooter without exposing it to other non-related classes.
             * 
             * This make easier to organice code.
             */

            // Cache delegate to reduce allocations
            private static readonly Func<ShooterMemento, ShooterMemento, float, ShooterMemento> interpolateMementos = InterpolateMementos;

            private readonly float cooldown;

            private ShooterMemento(float cooldown) => this.cooldown = cooldown;

            public ShooterMemento(Shooter shooter) : this(shooter.nextCast - Time.fixedDeltaTime) { }

            public static void TrackForRewind(Shooter shooter)
                => GlobalMementoManager.Subscribe(
                    () => new ShooterMemento(shooter), // Memento of ability is only its cooldown
                    (memento) => ConsumeMemento(memento, shooter),
                    interpolateMementos
                );

            private static void ConsumeMemento(ShooterMemento? memento, Shooter shooter)
            {
                if (memento is ShooterMemento memento_)
                    memento_.Load(shooter);
            }

            public void Load(Shooter shooter) => shooter.nextCast = cooldown + Time.fixedDeltaTime;

            private static ShooterMemento InterpolateMementos(ShooterMemento a, ShooterMemento b, float delta)
                => new ShooterMemento(Mathf.Lerp(a.cooldown, b.cooldown, delta));
        }
    }
}
