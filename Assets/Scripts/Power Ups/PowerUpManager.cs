using Asteroids.Entities;
using Asteroids.Entities.Player;
using Asteroids.Events;

using Enderlook.Enumerables;
using Enderlook.Unity.Attributes;
using Enderlook.Unity.Serializables.Ranges;

using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

namespace Asteroids.PowerUps
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class PowerUpManager : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Determines each how many seconds a new power up is spawned.")]
        private float spawnTime = 20;

        [SerializeField, Tooltip("Types of power ups.")]
        private PowerUpTemplate[] templates;

        [SerializeField, Layer, Tooltip("Determines the layer of the power ups.")]
        private int layer;

        [SerializeField, Tooltip("Speed of the power up.")]
        private RangeFloat speed;
#pragma warning restore CS0649

        private new Camera camera;
        private float cooldown;
        private bool canSpawn;
        private AudioSource audioSource;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            camera = Camera.main;
            cooldown = spawnTime;
            canSpawn = true;
            EventManager.Subscribe<OnPowerUpPickedEvent>(OnPowerUpPicked);
            audioSource = GetComponent<AudioSource>();

            // For gameplay reasons power ups are not tracked by the rewind feature
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

            GameObject powerUp = templates.RandomPick().CreatePickup(audioSource, layer);
            powerUp.layer = layer;
            powerUp.transform.position = position; // Don't udpate frmo rigidbody because that has one frame delay
            powerUp.GetComponent<Rigidbody2D>().velocity = speed;
        }
    }
}