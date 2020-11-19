using Asteroids.Scene;
using Asteroids.Utils;

using System;

using UnityEngine;

using Resources = Asteroids.Utils.Resources;

namespace Asteroids.Entities.Enemies
{
    public partial class SimpleEnemyBuilder
    {
        [Serializable]
        public readonly struct EnemyState
        {
            // This class is in charge of storing and setting the ability state to save the game.

            public readonly bool enabled;
            public readonly SerializableVector2 position;
            public readonly float rotation;
            public readonly SerializableVector2 velocity;
            public readonly float angularVelocity;
            public readonly string sprite;

            public EnemyState(Rigidbody2D rigidbody, string sprite)
            {
                enabled = rigidbody.gameObject.activeSelf;
                position = rigidbody.position;
                rotation = rigidbody.rotation;
                velocity = rigidbody.velocity;
                angularVelocity = rigidbody.angularVelocity;
                this.sprite = sprite;
            }

            public void Load(IPool<GameObject, (Vector3 position, Vector3 speed)> pool, GameObject enemy)
            {

                Rigidbody2D rigidbody = enemy.GetComponent<Rigidbody2D>();
                SpriteRenderer spriteRenderer = enemy.GetComponent<SpriteRenderer>();
                PolygonCollider2D collider = enemy.GetComponent<PolygonCollider2D>();

                if (enabled)
                {
                    // Since enemies are pooled, we must force the pool to give us control of this instance in case it was in his control.
                    pool.ExtractIfHas(rigidbody.gameObject); // Read from rigidbody to reduce closure size

                    rigidbody.position = position;
                    rigidbody.rotation = rotation;
                    rigidbody.velocity = velocity;
                    rigidbody.angularVelocity = angularVelocity;
                    spriteRenderer.sprite = Resources.Load<Sprite>(sprite);

                    int count = spriteRenderer.sprite.GetPhysicsShapeCount();
                    for (int i = 0; i < count; i++)
                    {
                        spriteRenderer.sprite.GetPhysicsShape(i, physicsShape);
                        collider.SetPath(i, physicsShape);
                    }
                }
                else
                    pool.Store(rigidbody.gameObject); // Read from rigidbody to reduce closure size
            }
        }
    }
}