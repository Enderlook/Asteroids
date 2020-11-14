using Asteroids.Events;

using Enderlook.Enumerables;
using Enderlook.Unity.Attributes;
using Enderlook.Unity.Components.ScriptableSound;

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Audio;

using Resources = Asteroids.Utils.Resources;

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

        [SerializeField, Tooltip("A random sprite will be picked by the enemy."), DrawTexture(false)]
        private string[] sprites;

        [SerializeField, Layer, Tooltip("Layer of the enemy.")]
        private int layer;

        [SerializeField, Tooltip("Sound played on death.")]
        private Sound deathSound;

        [SerializeField, Tooltip("Audio mixer group of death sound.")]
        private AudioMixerGroup audioMixerGroup;
#pragma warning restore CS0649

        public new abstract class Handler : EnemyGenerator<TData, THandler>.Handler
        {
            // Cache components for fast lookup.

            private SpriteRenderer spriteRenderer;
            private ExecuteOnCollision onCollision;
            private PolygonCollider2D collider;
            private static List<Vector2> physicsShape = new List<Vector2>();

            private SimpleSoundPlayer player;

            public override void Create(TData data)
            {
                base.Create(data);
                onCollision = GameObject.AddComponent<ExecuteOnCollision>();
                spriteRenderer = GameObject.AddComponent<SpriteRenderer>();
                collider = GameObject.AddComponent<PolygonCollider2D>();
                GameObject.layer = Data.layer;
                GameObject.AddComponent<ScreenWrapper>();

                player = SimpleSoundPlayer.CreateOneTimePlayer(data.deathSound, false, false);
                player.GetComponent<AudioSource>().outputAudioMixerGroup = data.audioMixerGroup;

                SubInitialize();
            }

            public override void Initialize()
            {
                base.Initialize();
                SubInitialize();
            }

            private void SubInitialize()
            {
                Sprite sprite = Resources.Load<Sprite>(Data.sprites.RandomPick());

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
                player.Play();
                EventManager.Raise(CreateEnemyDestroyedEvent());
                ExecuteOnCollision();
                ReturnToPool();
            }

            protected virtual EnemyDestroyedEvent CreateEnemyDestroyedEvent() => new EnemyDestroyedEvent(Data.scoreWhenDestroyed);

            protected virtual void ExecuteOnCollision() { }
        }
    }
}