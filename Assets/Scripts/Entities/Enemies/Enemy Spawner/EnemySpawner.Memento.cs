using Asteroids.Scene;

using System;
using System.Collections;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public partial class EnemySpawner
    {
        private readonly struct Memento
        {
            /* This struct is in charge of storing and setting the enemy spawning state for rewinding
             * Technically, the create and set memento methods should be members of the Originator class
             * according to the pure Memento pattern.
             * However, that makes a bit convulted the EnemySpawner class and increase its responsabilities amount.
             * 
             * This is why me add that logic in the Memento type and rewind here. So for the EnemySpawner's point of view, it's only Memento.TrackForRewind(this).
             * Anyway, the implementation is not exposed because the Memento type is a nested type of the EnemySpawner class
             * which allow us to access the private state of the EnemySpawner without exposing it to other non-related classes.
             * 
             * This make easier to organice code.
             */

            // Cache objects to reduce allocations
            private static readonly Func<Memento, Memento, float, Memento> interpolateMementos = InterpolateMementos;
            private static readonly WaitForSeconds waitForSeconds = new WaitForSeconds(1);
            private static readonly WaitWhile waitWhile = new WaitWhile(() => GlobalMementoManager.IsRewinding);

            private readonly int remainingEnemies;

            private Memento(int remainingEnemies) => this.remainingEnemies = remainingEnemies;

            private Memento(EnemySpawner enemySpawner) : this(enemySpawner.remainingEnemies) { }

            public static void TrackForRewind(EnemySpawner enemySpawner)
            {
                // We handles anything related with EnemySpawner's rewind here.

                GlobalMementoManager.Subscribe(
                    () => new Memento(enemySpawner), // Memento of enemy spawner is only remaining enemies
                    (memento) => ConsumeMemento(memento, enemySpawner),
                    interpolateMementos
                );

                EventManager.Subscribe<StopRewindEvent>(() => enemySpawner.RecalculateEnemyCountManually());

                enemySpawner.StartCoroutine(CheckEnemyCount());

                IEnumerator CheckEnemyCount()
                {
                    while (true)
                    {
                        yield return waitForSeconds;
                        yield return waitWhile;

                        // Sometimes the rewind can bug enemy count
                        // So we check if every a while
                        enemySpawner.RecalculateEnemyCountManually();
                    }
                }
            }

            private static void ConsumeMemento(Memento? memento, EnemySpawner enemySpawner)
            {
                if (memento is Memento memento_)
                    enemySpawner.remainingEnemies = memento_.remainingEnemies;
            }

            private static Memento InterpolateMementos(Memento a, Memento b, float delta)
                => new Memento(Mathf.RoundToInt(Mathf.Lerp(a.remainingEnemies, b.remainingEnemies, delta)));
        }
    }
}