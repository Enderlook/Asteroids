using Asteroids.Events;

using Enderlook.Enumerables;
using Enderlook.Unity.Attributes;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public abstract class EnemyGeneratorSimple<TData, THandler> : EnemyGenerator<TData, THandler>
        where TData : EnemyGeneratorSimple<TData, THandler>
        where THandler : EnemyGenerator<TData, THandler>.Handler, new()
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

        public new abstract class Handler : EnemyGenerator<TData, THandler>.Handler
        {
            // Cache components for fast lookup.

            private SpriteRenderer spriteRenderer;
            private ExecuteOnCollision onCollision;
            private PolygonCollider2D collider;
            private List<Vector2> physicsShape;

            public override void Create(TData data)
            {
                base.Create(data);
                onCollision = GameObject.AddComponent<ExecuteOnCollision>();
                spriteRenderer = GameObject.AddComponent<SpriteRenderer>();
                collider = GameObject.AddComponent<PolygonCollider2D>();
                physicsShape = new List<Vector2>();
                GameObject.layer = Data.layer;
                GameObject.AddComponent<ScreenWrapper>();

                SubInitialize();
            }

            public override void Initialize()
            {
                base.Initialize();
                SubInitialize();
            }

            private void SubInitialize()
            {
                Sprite sprite = Data.sprites.RandomPick();

                spriteRenderer.sprite = sprite;
                Rigidbody.mass = Data.mass;

                onCollision.RemoveAllListeners();
                onCollision.Subscribe(OnCollision);

                int count = sprite.GetPhysicsShapeCount();
                for (int i = 0; i < count; i++)
                {
                    sprite.GetPhysicsShape(i, physicsShape);
                    collider.SetPath(i, physicsShape);
                }
            }

            private void OnCollision()
            {
                EventManager.Raise(CreateEnemyDestroyedEvent());
                ExecuteOnCollision();
                ReturnToPool();
            }

            protected virtual EnemyDestroyedEvent CreateEnemyDestroyedEvent() => new EnemyDestroyedEvent(Data.scoreWhenDestroyed);

            protected virtual void ExecuteOnCollision() { }
        }
    }
}