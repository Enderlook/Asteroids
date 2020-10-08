using Asteroids.Entities;
using Asteroids.Utils;

using Enderlook.Unity.Attributes;
using Enderlook.Unity.Components.ScriptableSound;

using UnityEngine;

using Resources = Asteroids.Utils.Resources;

namespace Asteroids.AbilitySystem
{
    [CreateAssetMenu(menuName = "Asteroids/Ability System/Abilities/Projectile", fileName = "Projectile Ability")]
    public class ProjectileTrigger : Ability
    {
#pragma warning disable CS0649
        [SerializeField, DrawTexture, Tooltip("Sprite of the projectile to fire")]
        private string sprite;

        [SerializeField, Min(0), Tooltip("Force at which the projectile is fired.")]
        private float force;

        [SerializeField, Layer, Tooltip("Layer of the projectile.")]
        private int projectileLayer;
#pragma warning restore CS0649

        private AbilitiesManager abilitiesManager;

        private Pool<Rigidbody2D> pool;

        private SimpleSoundPlayer soundPlayer;

        public override void Initialize(AbilitiesManager abilitiesManager)
        {
            this.abilitiesManager = abilitiesManager;
            base.Initialize(abilitiesManager);
            pool = new Pool<Rigidbody2D>(ProjectileConstructor, ProjectileInitializer, ProjectileDeinitializer);

            soundPlayer = SimpleSoundPlayer.CreateOneTimePlayer(abilitySound, false, false);
        }

        private Rigidbody2D ProjectileConstructor()
        {
            GameObject projectile = new GameObject("Projectile")
            {
                layer = projectileLayer
            };

            Rigidbody2D rigidbody2D = projectile.AddComponent<Rigidbody2D>();
            rigidbody2D.gravityScale = 0;

            SpriteRenderer spriteRenderer = projectile.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load<Sprite>(sprite);

            projectile.AddComponent<PolygonCollider2D>();

            ExecuteOnCollision executeOnCollision = projectile.AddComponent<ExecuteOnCollision>();
            executeOnCollision.Subscribe(() => pool.Store(rigidbody2D));

            return rigidbody2D;
        }

        private void ProjectileInitializer(Rigidbody2D rigidbody2D) => rigidbody2D.gameObject.SetActive(true);

        private void ProjectileDeinitializer(Rigidbody2D rigidbody2D)
        {
            rigidbody2D.velocity = default;
            rigidbody2D.gameObject.SetActive(false);
        }

        public override void Execute()
        {
            Transform castPoint = abilitiesManager.CastPoint;
            Rigidbody2D playerRigidbody = abilitiesManager.Rigidbody2D;

            Rigidbody2D rigidbody2D = pool.Get();

            // Don't use Rigidbody2D because it takes a frame to update and produces a visual bug
            Transform transform = rigidbody2D.transform;
            transform.position = castPoint.position;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, playerRigidbody.rotation));

            rigidbody2D.velocity = (Vector2)(abilitiesManager.CastPoint.up * force) + playerRigidbody.velocity;

            soundPlayer.Play();
        }
    }
}
