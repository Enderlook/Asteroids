//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using Asteroids.Entities.Player;
using Asteroids.Scene;
using Asteroids.Utils;

using Enderlook.Unity.Attributes;
using Enderlook.Unity.Components.ScriptableSound;

using System.Collections.Generic;

using UnityEngine;

using Resources = Asteroids.Utils.Resources;

namespace Asteroids.Entities.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed partial class BossShooter : MonoBehaviour
    {
        private static readonly BuilderFactoryPool<Rigidbody2D, BossShooter, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Constructor construct = ProjectileConstructor;
        private static readonly BuilderFactoryPool<Rigidbody2D, BossShooter, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Initializer initialize = ProjectileInitializer;
        private static readonly BuilderFactoryPool<Rigidbody2D, BossShooter, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Initializer commonInitialize = ProjectileCommonInitializer;
        private static readonly BuilderFactoryPool<Rigidbody2D, BossShooter, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Deinitializer deinitialize = ProjectileDeinitializer;

#pragma warning disable CS0649
        [SerializeField, Layer, Tooltip("Layer of proyectile to spawn.")]
        private int projectileLayer;

        [SerializeField, Tooltip("Scale of the projectile sprite.")]
        private float spriteScale;

        [SerializeField, DrawTexture, Tooltip("Sprite of the projectile.")]
        private string sprite;

        [SerializeField, Tooltip("Shooting sound.")]
        private Sound shootSound;

        [SerializeField, Tooltip("Force of shooter projectiles.")]
        private float force;

        [SerializeField, Min(0), Tooltip("Cooldown between shoots.")]
        private float cooldown;

        [SerializeField, Min(1), Tooltip("Amount of shoots.")]
        private int shootsCount;

        [SerializeField, Tooltip("Shooting point where projectiles are spawned.")]
        private Transform shootPoint;
#pragma warning restore CS0649

        private BuilderFactoryPool<Rigidbody2D, BossShooter, (Vector3 position, Quaternion rotation, Vector3 velocity)> builder;

        private SimpleSoundPlayer soundPlayer;
        private float nextCast;
        private int remainingShoots;
        private new Rigidbody2D rigidbody;
        private Boss boss;

        public float Duration => shootsCount * cooldown;

        // Since boss doesn't move by rigidbody velocity, we use this to calculate its velocity.
        private Vector2 lastPosition;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            boss = GetComponent<Boss>();

            builder = new BuilderFactoryPool<Rigidbody2D, BossShooter, (Vector3 position, Quaternion rotation, Vector3 velocity)>
            {
                flyweight = this,
                constructor = construct,
                initializer = initialize,
                commonInitializer = commonInitialize,
                deinitializer = deinitialize
            };

            rigidbody = GetComponent<Rigidbody2D>();
            lastPosition = rigidbody.position;

            Debug.Assert(shootSound != null);
            soundPlayer = SimpleSoundPlayer.CreateOneTimePlayer(shootSound, false, false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]    
        private void FixedUpdate()
        {
            boss.MoveAndRotateTowards(PlayerController.Position, Boss.RequiredDistanceToPlayerForFarAttack);

            if (Time.fixedTime >= nextCast)
            {
                nextCast = Time.fixedTime + cooldown;
                CreateBullet();

                if (remainingShoots-- == 0)
                    boss.Next();
            }

            lastPosition = rigidbody.position;
        }

        private void OnEnable()
        {
            nextCast = 0;
            remainingShoots = shootsCount;
        }

        public void Load(List<ProjectileState> projectileStates)
        {
            foreach (ProjectileState state_ in projectileStates)
                state_.Load(this, CreateBullet());
        }

        private static Rigidbody2D ProjectileConstructor(in BossShooter flyweight, in (Vector3 position, Quaternion rotation, Vector3 velocity) parameters)
        {
            GameObject projectile = new GameObject("Projectile")
            {
                layer = flyweight.projectileLayer
            };

            projectile.transform.localScale *= flyweight.spriteScale;

            Rigidbody2D rigidbody = projectile.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0;

            SpriteRenderer spriteRenderer = projectile.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load<Sprite>(flyweight.sprite);

            projectile.AddComponent<PolygonCollider2D>();

            ReturnToPoolOnCollision returnToPoolOnCollision = projectile.AddComponent<ReturnToPoolOnCollision>();
            returnToPoolOnCollision.pool = flyweight.builder;

            GameSaver.SubscribeBossBullet(() => new ProjectileState(rigidbody));

            return rigidbody;
        }

        private static void ProjectileInitializer(in BossShooter flyweight, Rigidbody2D rigidbody2D, in (Vector3 position, Quaternion rotation, Vector3 velocity) parameters) {}

        private static void ProjectileDeinitializer(Rigidbody2D rigidbody2D)
        {
            rigidbody2D.velocity = default;
            rigidbody2D.gameObject.SetActive(false);
        }

        private static void ProjectileCommonInitializer(in BossShooter flyweight, Rigidbody2D rigidbody2D, in (Vector3 position, Quaternion rotation, Vector3 velocity) parameters)
        {
            // We enable the gameObject here instead in ProjectileInitializer, because that method is executed later
            // and so it produces a bug because rigibodies doesn't work when their gameObjects are disabled
            rigidbody2D.gameObject.SetActive(true);

            // Don't use Rigidbody2D because it takes a frame to update and produces a visual bug
            Transform transform = rigidbody2D.transform;
            transform.position = parameters.position;
            transform.rotation = parameters.rotation;
            rigidbody2D.velocity = parameters.velocity;
        }

        private Rigidbody2D CreateBullet()
        {
            Rigidbody2D bullet = builder.Create((
                shootPoint.position,
                Quaternion.Euler(new Vector3(0, 0, rigidbody.rotation)),
                (Vector2)(-shootPoint.up * force) + (rigidbody.position - lastPosition)
            ));

            soundPlayer.Play();

            return bullet;
        }

        private sealed class ReturnToPoolOnCollision : MonoBehaviour
        {
            public IPool<Rigidbody2D, (Vector3 position, Quaternion rotation, Vector3 velocity)> pool;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
            private void OnCollisionEnter2D(Collision2D collision)
            {
                Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
                pool.ExtractIfHas(rigidbody); // Silent odd bug of double store. TODO: Is this a problem?
                pool.Store(rigidbody);
            }
        }
    }
}
