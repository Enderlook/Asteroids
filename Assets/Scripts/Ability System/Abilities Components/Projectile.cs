using AvalonStudios.Additions.Extensions;

using UnityEngine;

namespace Asteroids.AbilitySystem
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField, Tooltip("Damage")]
        private int damage = 1;

        [SerializeField, Tooltip("Speed at which the projectile is fired")]
        private float speed = 0;

        [SerializeField, Tooltip("Layers to hit")]
        private LayerMask hitLayers = default;

        private new Rigidbody2D rigidbody;

        private void Awake() => rigidbody = GetComponent<Rigidbody2D>();

        private void FixedUpdate() => rigidbody.MovePosition(rigidbody.position + ((Vector2)transform.up * speed * Time.fixedDeltaTime));

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GameObject obj = collision.gameObject;
            if (obj.MatchLayer(hitLayers))
            {
                Debug.Log($"Damage: {damage}");
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            GameObject obj = collision.gameObject;
            if (obj.MatchLayer(hitLayers))
            {
                Debug.Log($"Damage: {damage}");
                Destroy(gameObject);
            }
        }

        public static void AddComponentTo(GameObject obj, int damage, float speed, LayerMask hitLayers = default)
        {
            Projectile projectile = obj.AddComponent<Projectile>();
            projectile.damage = damage;
            projectile.speed = speed;
            projectile.hitLayers = hitLayers;
        }
    }
}
