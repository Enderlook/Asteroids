using Asteroids.Scene;

using System;

namespace Asteroids.WeaponSystem
{
    public sealed partial class BombWeapon
    {
        [Serializable]
        private readonly struct Memento
        {
            /* This struct is in charge of storing and setting the bomb state for rewinding
             * Technically, the create and set memento methods should be members of the Originator class
             * according to the pure Memento pattern.
             * However, that makes a bit convulted the BombWeapon class and increase its responsabilities amount.
             * 
             * This is why me add that logic in the Memento type and rewind here. So for the BombWeapon's point of view, it's only Memento.TrackForRewind.
             * Anyway, the implementation is not exposed because the Memento type is a nested type of the BombWeapon class
             * which allow us to access the private state of the BombWeapon without exposing it to other non-related classes.
             * 
             * This make easier to organice code.
             */

            // Cache delegate to reduce allocations
            private static readonly Func<Memento, Memento, float, Memento> interpolateMementos = InterpolateMementos;

            private readonly int last;

            public Memento(BombWeapon bombWeapon)
            {
                last = bombWeapon.last?.id ?? 0;
            }

            public static void TrackForRewind(BombWeapon bombWeapon)
                => GlobalMementoManager.Subscribe(
                    () => new Memento(bombWeapon), // Memento of bomb weapon is only last bomb and if it's exploding, cooldown is already tracked in parent
                    (memento) => ConsumeMemento(memento, bombWeapon),
                    interpolateMementos
                );

            private static void ConsumeMemento(Memento? memento, BombWeapon bombWeapon)
            {
                if (memento is Memento memento_)
                    memento_.Load(bombWeapon);
                else
                {
                    bombWeapon.lastId = 0;
                    bombWeapon.last = null;
                }
            }

            public void Load(BombWeapon bombWeapon)
            {
                bombWeapon.lastId = last;
                bombWeapon.last = null;
            }

            private static Memento InterpolateMementos(Memento a, Memento b, float delta)
                => delta < .5f ? a : b;
        }
    }
}