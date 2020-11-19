using Asteroids.Scene;
using Asteroids.Utils;

using Enderlook.Unity.Attributes;
using Enderlook.Unity.Components.ScriptableSound;

using System.Collections;

using UnityEngine;

using Resources = Asteroids.Utils.Resources;

namespace Asteroids.AbilitySystem
{
    [CreateAssetMenu(menuName = "Asteroids/Ability System/Abilities/Projectile", fileName = "Projectile Ability")]
    public partial class ProjectileTrigger : Ability
    {
        private static readonly BuilderFactoryPool<Rigidbody2D, ProjectileTrigger, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Constructor construct = ProjectileConstructor;
        private static readonly BuilderFactoryPool<Rigidbody2D, ProjectileTrigger, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Initializer initialize = ProjectileInitializer;
        private static readonly BuilderFactoryPool<Rigidbody2D, ProjectileTrigger, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Initializer commonInitialize = ProjectileCommonInitializer;
        private static readonly BuilderFactoryPool<Rigidbody2D, ProjectileTrigger, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Deinitializer deinitialize = ProjectileDeinitializer;

#pragma warning disable CS0649
        [SerializeField, DrawTexture, Tooltip("Sprite of the projectile to fire")]
        private string sprite;

        [SerializeField, Min(0), Tooltip("Force at which the projectile is fired.")]
        private float force;

        [SerializeField, Layer, Tooltip("Layer of the projectile.")]
        private int projectileLayer;
#pragma warning restore CS0649

        private AbilitiesManager abilitiesManager;

        private BuilderFactoryPool<Rigidbody2D, ProjectileTrigger, (Vector3 position, Quaternion rotation, Vector3 velocity)> builder;

        private SimpleSoundPlayer soundPlayer;

        public override void Initialize(AbilitiesManager abilitiesManager)
        {
            this.abilitiesManager = abilitiesManager;
            base.Initialize(abilitiesManager);
            builder = new BuilderFactoryPool<Rigidbody2D, ProjectileTrigger, (Vector3 position, Quaternion rotation, Vector3 velocity)>
            {
                flyweight = this,
                constructor = construct,
                initializer = initialize,
                commonInitializer = commonInitialize,
                deinitializer = deinitialize
            };

            soundPlayer = SimpleSoundPlayer.CreateOneTimePlayer(abilitySound, false, false);
            GameSaver.SubscribeProjectileTrigger(
                () => new State(this),
                (parameter) => {
                    // Fixes null reference exception bug with SimpleSoundPlayer
                    abilitiesManager.StartCoroutine(Work());
                    IEnumerator Work()
                    {
                        yield return null;
                        parameter.Item1.Load(this);
                        foreach (ProjectileState state in parameter.Item2)
                            state.Load(this, CreateBullet());
                    }
                }
            );
        }

        private static Rigidbody2D ProjectileConstructor(in ProjectileTrigger flyweight, in (Vector3 position, Quaternion rotation, Vector3 velocity) parameters)
        {
            GameObject projectile = new GameObject("Projectile")
            {
                layer = flyweight.projectileLayer
            };

            Rigidbody2D rigidbody = projectile.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0;

            SpriteRenderer spriteRenderer = projectile.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load<Sprite>(flyweight.sprite);

            projectile.AddComponent<PolygonCollider2D>();

            ReturnToPoolOnCollision returnToPoolOnCollision = projectile.AddComponent<ReturnToPoolOnCollision>();
            returnToPoolOnCollision.pool = flyweight.builder;

            BuilderFactoryPool<Rigidbody2D, ProjectileTrigger, (Vector3 position, Quaternion rotation, Vector3 velocity)> builder = flyweight.builder;

            Memento.TrackForRewind(flyweight, rigidbody);

            GameSaver.SubscribeProjectileTriggerBullet(() => new ProjectileState(rigidbody));

            return rigidbody;
        }

        private static void ProjectileInitializer(in ProjectileTrigger flyweight, Rigidbody2D rigidbody2D, in (Vector3 position, Quaternion rotation, Vector3 velocity) parameters)
        {
        }

        private static void ProjectileDeinitializer(Rigidbody2D rigidbody2D)
        {
            rigidbody2D.velocity = default;
            rigidbody2D.gameObject.SetActive(false);
        }

        private static void ProjectileCommonInitializer(in ProjectileTrigger flyweight, Rigidbody2D rigidbody2D, in (Vector3 position, Quaternion rotation, Vector3 velocity) parameters)
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

        public override void Execute() => CreateBullet();

        private Rigidbody2D CreateBullet()
        {
            Transform castPoint = abilitiesManager.CastPoint;
            Rigidbody2D playerRigidbody = abilitiesManager.Rigidbody2D;

            Rigidbody2D bullet = builder.Create((
                castPoint.position,
                Quaternion.Euler(new Vector3(0, 0, playerRigidbody.rotation)),
                (Vector2)(abilitiesManager.CastPoint.up * force) + playerRigidbody.velocity
            ));

            soundPlayer.Play();

            return bullet;
        }

        private class ReturnToPoolOnCollision : MonoBehaviour
        {
            public IPool<Rigidbody2D, (Vector3 position, Quaternion rotation, Vector3 velocity)> pool;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
            private void OnCollisionEnter2D(Collision2D collision) => pool.Store(GetComponent<Rigidbody2D>());
        }
    }
}
