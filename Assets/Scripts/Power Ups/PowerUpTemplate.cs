using Asteroids.Entities;
using Asteroids.Entities.Player;
using Asteroids.Events;

using Enderlook.Unity.Attributes;

using System.Collections.Generic;

using UnityEngine;

namespace Asteroids.PowerUps
{
    public abstract class PowerUpTemplate : ScriptableObject
    {
        private static List<Vector2> physicsShape = new List<Vector2>();

        [field: SerializeField, IsProperty, Tooltip("Power up sprite.")]
        protected Sprite sprite { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Power up pickup sound.")]
        protected AudioClip sound { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Scale of power up.")]
        protected float scale { get; private set; }

        public GameObject CreatePickup(AudioSource audioSource, int layer)
        {
            // We don't pool power ups because only a single one can be active at the same time
            // Pooling a single power up would be an overkill
            // Also, the exercise didn't request for pooling, factory nor builder patterns in power ups
            // Only decorator

            GameObject powerUp = new GameObject("Power up");
            powerUp.transform.localScale *= scale;

            powerUp.AddComponent<Rigidbody2D>();

            SpriteRenderer renderer = powerUp.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;

            audioSource.clip = sound;
            audioSource.pitch = 1;

            PolygonCollider2D collider = powerUp.AddComponent<PolygonCollider2D>();
            int count = sprite.GetPhysicsShapeCount();
            for (int i = 0; i < count; i++)
            {
                sprite.GetPhysicsShape(i, physicsShape);
                collider.SetPath(i, physicsShape);
            }

            powerUp.AddComponent<ScreenWrapper>();

            PickupBehaviour pickupBehaviour = powerUp.AddComponent<PickupBehaviour>();
            pickupBehaviour.pickup = GetPickup(audioSource);

            return powerUp;
        }

        protected virtual IPickup GetPickup(AudioSource audioSource) => new PickupDecorable(audioSource);

        public sealed class PickupBehaviour : MonoBehaviour
        {
            public IPickup pickup;

            private void OnCollisionEnter2D(Collision2D collision)
            {
                if (collision.gameObject.GetComponent<Player>() != null)
                {
                    pickup.PickUp();
                    Destroy(gameObject);
                }
            }
        }

        private sealed class PickupDecorable : IPickup
        {
            // This class could be prefectly inlined in the PickupBehaviour,
            // but we need it to make the decorator pattern.
            // Alternatively, PickupBehaviour could implement IPickup, and make player responsability to pickup powerups,
            // thought that defeats SOLID.

            private AudioSource audioSource;

            public PickupDecorable(AudioSource audioSource) => this.audioSource = audioSource;

            public void PickUp()
            {
                EventManager.Raise(new OnPowerUpPickedEvent());
                audioSource.Play();
            }
        }
    }
}
