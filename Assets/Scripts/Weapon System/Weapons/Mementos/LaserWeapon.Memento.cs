using Asteroids.Scene;

using System;

using UnityEngine;

namespace Asteroids.WeaponSystem
{
    public partial class LaserWeapon
    {
        private readonly struct Memento
        {
            /* This struct is in charge of storing and setting the laser state for rewinding
             * Technically, the create and set memento methods should be members of the Originator class
             * according to the pure Memento pattern.
             * However, that makes a bit convulted the LaserTrigger class and increase its responsabilities amount.
             * 
             * This is why me add that logic in the Memento type and rewind here. So for the LaserTrigger's point of view, it's only Memento.TrackForRewind.
             * Anyway, the implementation is not exposed because the Memento type is a nested type of the LaserTrigger class
             * which allow us to access the private state of the LaserTrigger without exposing it to other non-related classes.
             * 
             * This make easier to organice code.
             */

            // Cache delegate to reduce allocations
            private static readonly Func<Memento, Memento, float, Memento> interpolateMementos = InterpolateMementos;

            private readonly float duration;

            private Memento(float duration) => this.duration = duration;

            private Memento(LaserWeapon laserWeapon) : this(laserWeapon.duration) { }

            public static void TrackForRewind(LaserWeapon laserWeapon)
                => GlobalMementoManager.Subscribe(
                    () => new Memento(laserWeapon), // Memento of laser is only its duration, cooldown is already tracked in parent
                    (memento) => ConsumeMemento(memento, laserWeapon),
                    interpolateMementos
                );

            private static void ConsumeMemento(Memento? memento, LaserWeapon laserWeapon)
            {
                if (memento is Memento memento_)
                {
                    laserWeapon.currentDuration = memento_.duration;
                    if (laserWeapon.currentDuration > 0)
                        laserWeapon.lineRenderer.enabled = true;
                    else
                        laserWeapon.lineRenderer.enabled = false;
                }
                else
                {
                    laserWeapon.currentDuration = 0;
                    laserWeapon.lineRenderer.enabled = false;
                }
            }

            private static Memento InterpolateMementos(Memento a, Memento b, float delta)
                => new Memento(Mathf.Lerp(a.duration, b.duration, delta));
        }
    }
}
