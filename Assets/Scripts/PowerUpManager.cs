using Asteroids.Entities;
using Asteroids.Entities.Player;
using Asteroids.Events;

using Enderlook.Unity.Serializables.Ranges;

using System;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

namespace Asteroids
{
    public class PowerUpManager : MonoBehaviour
    {
        private static List<Vector2> physicsShape = new List<Vector2>();

#pragma warning disable CS0649
        [SerializeField, Tooltip("Determines each how many seconds a new power up is spawned.")]
        private float spawnTime = 20;

        [SerializeField, Tooltip("Health pack sprite.")]
        private Sprite healthPackSprite;

        [SerializeField, Tooltip("Rewind pack sprite.")]
        private Sprite rewindPackSprite;

        [SerializeField, Tooltip("Speed of the power up.")]
        private RangeFloat speed;
#pragma warning restore CS0649

        private new Camera camera;
        private float cooldown;
        private bool canSpawn;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            camera = Camera.main;
            cooldown = spawnTime;
            canSpawn = true;
            EventManager.Subscribe<OnPowerUpPickedEvent>(OnPowerUpPicked);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            if (canSpawn)
            {
                cooldown -= Time.deltaTime;
                if (cooldown < 0)
                {
                    canSpawn = false;
                    cooldown = spawnTime;
                    SpawnPowerUp();
                }
            }
        }

        private void OnPowerUpPicked() => canSpawn = true;

        private void SpawnPowerUp()
        {
            Vector3 positionViewport;

            switch (Random.Range(0, 4))
            {
                case 0:
                    positionViewport = new Vector3(1.05f, Random.value, 0);
                    break;
                case 1:
                    positionViewport = new Vector3(-0.05f, Random.value, 0);
                    break;
                case 2:
                    positionViewport = new Vector3(Random.value, 1.05f, 0);
                    break;
                case 3:
                    positionViewport = new Vector3(Random.value, -0.05f, 0);
                    break;
                default:
                    Debug.LogError("Impossible state.");
                    goto case 0;
            }

            Vector2 position = camera.ViewportToWorldPoint(positionViewport);

            Vector2 speed = (position - new Vector2(Random.value, Random.value)).normalized * this.speed.Value;

            // We don't pool power ups because only a single one can be active at the same time
            // Pooling a single power up would be an overkill

            GameObject powerUp = new GameObject("Power up");
            Rigidbody2D rigidbody = powerUp.AddComponent<Rigidbody2D>();
            powerUp.transform.position = position; // Use transform because rigidbody.position has a one frame delay
            rigidbody.velocity = speed;
            SpriteRenderer renderer = powerUp.AddComponent<SpriteRenderer>();
            PolygonCollider2D collider = powerUp.AddComponent<PolygonCollider2D>();
            powerUp.AddComponent<ScreenWrapper>();

            IPickup pickup = new Pickup(powerUp);
            Sprite sprite;
            if (Random.value > .5)
            {
                pickup = new HealthPickupDecorator(pickup);
                sprite = healthPackSprite;
            }
            else
            {
                pickup = new RewindPickupDecorator(pickup);
                sprite = rewindPackSprite;
            }

            renderer.sprite = sprite;
            PickupBehaviour pickupBehaviour = powerUp.AddComponent<PickupBehaviour>();
            pickupBehaviour.pickup = pickup;

            int count = sprite.GetPhysicsShapeCount();
            for (int i = 0; i < count; i++)
            {
                sprite.GetPhysicsShape(i, physicsShape);
                collider.SetPath(i, physicsShape);
            }
        }

        private readonly struct OnPowerUpPickedEvent { }

        public sealed class PickupBehaviour : MonoBehaviour
        {
            public IPickup pickup;

            private void OnCollisionEnter2D(Collision2D collision)
            {
                if (collision.gameObject.GetComponent<Player>() != null)
                    pickup.PickUp();
            }
        }

        private sealed class Pickup : IPickup
        {
            // This class could be prefectly inlined in the PickupBehaviour,
            // but we need it to make the decorator pattern.
            // Alternatively, PickupBehaviour could implement IPickup, and make player responsability to pickup powerups,
            // thought that defeats SOLID.

            private GameObject gameObject;

            public Pickup(GameObject gameObject) => this.gameObject = gameObject;

            public void PickUp()
            {
                Destroy(gameObject);
                EventManager.Raise(new OnPowerUpPickedEvent());
            }
        }

        private sealed class HealthPickupDecorator : IPickup
        {
            private IPickup decorable;

            public HealthPickupDecorator(IPickup pickup) => decorable = pickup;

            public void PickUp()
            {
                decorable.PickUp();
                FindObjectOfType<Player>().AddNewLife();
            }
        }

        private sealed class RewindPickupDecorator : IPickup
        {
            private IPickup decorable;

            public RewindPickupDecorator(IPickup pickup) => decorable = pickup;

            public void PickUp()
            {
                decorable.PickUp();
                throw new NotImplementedException();
            }
        }

        public interface IPickup
        {
            void PickUp();
        }
    }
}
