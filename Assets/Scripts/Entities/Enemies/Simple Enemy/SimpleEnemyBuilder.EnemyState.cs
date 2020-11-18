using Asteroids.Utils;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public partial class SimpleEnemyBuilder
    {
        public readonly struct EnemyState
        {
            // This class is in charge of storing and setting the ability state to save the game.

            // Ability state the same as enemy's memento, we can take advantage of that to keep DRY
            private readonly Memento memento;

            public EnemyState(Rigidbody2D rigidbody, SpriteRenderer spriteRenderer)
                => memento = new Memento(rigidbody, spriteRenderer);

            public void Load(IPool<GameObject, (Vector3 position, Vector3 speed)> pool, GameObject enemy)
                => memento.Load(pool, enemy.GetComponent<Rigidbody2D>(), enemy.GetComponent<SpriteRenderer>(), enemy.GetComponent<PolygonCollider2D>());
        }
    }
}