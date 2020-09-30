using Asteroids.Events;

using Enderlook.Enumerables;
using Enderlook.Unity.Attributes;

using System.Collections.Generic;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    [CreateAssetMenu(menuName = "Asteroids/Enemy Builder")]
    public class EnemyBuilderData : ScriptableObject
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Points got when destroyed.")]
        private int scoreWhenDestroyed;

        [SerializeField, Tooltip("Mass of the enemy")]
        private float mass;

        [SerializeField, Tooltip("A random sprite will be picked by the enemy.")]
        private Sprite[] sprites;

        [SerializeField, Layer, Tooltip("Layer of the enemy.")]
        private int layer;
#pragma warning restore CS0649

        public EnemyHandler Create() => new EnemyHandler(this);

        public void Initialize(EnemyHandler handler) => handler.Initialize(this);

        public void Deinitialize(EnemyHandler handler) => handler.Deinitialize();

        public class EnemyHandler
        {
            // Cache components for fast lookup.

            private readonly GameObject gameObject;
            private readonly SpriteRenderer spriteRenderer;
            private readonly Rigidbody2D rigidbody;
            private readonly BreakOnCollision breakOnCollision;
            private readonly PolygonCollider2D collider;
            private readonly List<Vector2> physicsShape;

            public void Initialize(Vector2 position, Vector2 point, float speed)
            {
                rigidbody.position = position;
                rigidbody.AddForce((rigidbody.position - point).normalized * speed, ForceMode2D.Impulse);
            }

            public EnemyHandler(EnemyBuilderData data)
            {
                gameObject = new GameObject(data.name);

                rigidbody = gameObject.AddComponent<Rigidbody2D>();
                rigidbody.gravityScale = 0;
                breakOnCollision = gameObject.AddComponent<BreakOnCollision>();
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                gameObject.AddComponent<ScreenWrapper>();
                collider = gameObject.AddComponent<PolygonCollider2D>();

                physicsShape = new List<Vector2>();

                Initialize(data);
            }

            public void Initialize(EnemyBuilderData data)
            {
                Sprite sprite = data.sprites.RandomPick();

                spriteRenderer.sprite = sprite;
                gameObject.SetActive(true);
                rigidbody.velocity = Vector2.zero;
                rigidbody.mass = data.mass;
                breakOnCollision.SetDestroyedEvent(new EnemyDestroyedEvent(this, data.scoreWhenDestroyed));

                int count = sprite.GetPhysicsShapeCount();
                for (int i = 0; i < count; i++)
                {
                    sprite.GetPhysicsShape(i, physicsShape);
                    collider.SetPath(i, physicsShape);
                }
            }

            public void Deinitialize() => gameObject.SetActive(false);
        }
    }
}