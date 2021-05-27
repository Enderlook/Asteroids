//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using Asteroids.Scene;
using Asteroids.Utils;

using Enderlook.Unity.Components.ScriptableSound;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Resources = Asteroids.Utils.Resources;

namespace Asteroids.Entities.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed partial class Shooter : MonoBehaviour
    {
        private static readonly BuilderFactoryPool<Rigidbody2D, Shooter, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Constructor construct = ProjectileConstructor;
        private static readonly BuilderFactoryPool<Rigidbody2D, Shooter, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Initializer initialize = ProjectileInitializer;
        private static readonly BuilderFactoryPool<Rigidbody2D, Shooter, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Initializer commonInitialize = ProjectileCommonInitializer;
        private static readonly BuilderFactoryPool<Rigidbody2D, Shooter, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Deinitializer deinitialize = ProjectileDeinitializer;

        private ShooterEnemyFlyweight flyweight;
        private Transform shootPoint;

        private BuilderFactoryPool<Rigidbody2D, Shooter, (Vector3 position, Quaternion rotation, Vector3 velocity)> builder;

        private SimpleSoundPlayer soundPlayer;
        private float nextCast;
        private new Rigidbody2D rigidbody2D;
        private new SpriteRenderer renderer;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            ShooterMemento.TrackForRewind(this);

            builder = new BuilderFactoryPool<Rigidbody2D, Shooter, (Vector3 position, Quaternion rotation, Vector3 velocity)>
            {
                flyweight = this,
                constructor = construct,
                initializer = initialize,
                commonInitializer = commonInitialize,
                deinitializer = deinitialize
            };

            rigidbody2D = GetComponent<Rigidbody2D>();
            renderer = GetComponent<SpriteRenderer>();

            GameSaver.SubscribeShooterEnemy(this, () => new ShooterState(this));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]    
        private void Update()
        {
            if (GlobalMementoManager.IsRewinding)
                return;

            if (Time.time >= nextCast)
                Fire();
        }

        public void Construct(ShooterEnemyFlyweight flyweight, Transform shootPoint)
        {
            this.flyweight = flyweight;
            this.shootPoint = shootPoint;
            soundPlayer = SimpleSoundPlayer.CreateOneTimePlayer(flyweight.ShootSound, false, false);
        }

        public void Load(ShooterState state, List<ProjectileState> projectileStates) => StartCoroutine(OnLoadGame(state, projectileStates));

        private IEnumerator OnLoadGame(ShooterState state, List<ProjectileState> projectileStates)
        {
            yield return null;
            state.Load(this);
            foreach (ProjectileState state_ in projectileStates)
                state_.Load(this, CreateBullet());
        }

        private static Rigidbody2D ProjectileConstructor(in Shooter flyweight, in (Vector3 position, Quaternion rotation, Vector3 velocity) parameters)
        {
            GameObject projectile = new GameObject("Projectile")
            {
                layer = flyweight.flyweight.ProjectileLayer
            };

            Rigidbody2D rigidbody = projectile.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0;

            SpriteRenderer spriteRenderer = projectile.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load<Sprite>(flyweight.flyweight.Sprite);

            projectile.AddComponent<PolygonCollider2D>();

            ReturnToPoolOnCollision returnToPoolOnCollision = projectile.AddComponent<ReturnToPoolOnCollision>();
            returnToPoolOnCollision.pool = flyweight.builder;

            ProjectileMemento.TrackForRewind(flyweight, rigidbody);

            GameSaver.SubscribeShooterBullet(flyweight, () => new ProjectileState(rigidbody));

            return rigidbody;
        }

        private static void ProjectileInitializer(in Shooter flyweight, Rigidbody2D rigidbody2D, in (Vector3 position, Quaternion rotation, Vector3 velocity) parameters) {}

        private static void ProjectileDeinitializer(Rigidbody2D rigidbody2D)
        {
            rigidbody2D.velocity = default;
            rigidbody2D.gameObject.SetActive(false);
        }

        private static void ProjectileCommonInitializer(in Shooter flyweight, Rigidbody2D rigidbody2D, in (Vector3 position, Quaternion rotation, Vector3 velocity) parameters)
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

        private void Fire() 
        {
            if (!renderer.isVisible)
                return;

            nextCast = Time.time + flyweight.Cooldown;
            CreateBullet(); 
        }

        private Rigidbody2D CreateBullet()
        {
            Rigidbody2D bullet = builder.Create((
                shootPoint.position,
                Quaternion.Euler(new Vector3(0, 0, rigidbody2D.rotation)),
                (Vector2)(-shootPoint.up * flyweight.Force) + rigidbody2D.velocity
            ));

            soundPlayer.Play();

            return bullet;
        }

        private sealed class ReturnToPoolOnCollision : MonoBehaviour
        {
            public IPool<Rigidbody2D, (Vector3 position, Quaternion rotation, Vector3 velocity)> pool;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
            private void OnCollisionEnter2D(Collision2D collision) => pool.Store(GetComponent<Rigidbody2D>());
        }
    }
}
