using Asteroids.Scene;

using Enderlook.Unity.Attributes;
using Enderlook.Unity.Serializables.Ranges;

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
        private AudioSource audioSource;
        private GameObject powerUp;
        private int lastTemplate = -1;

        private static PowerUpManager instance;

        public static float SpawnTime => instance.spawnTime;

        public static int PowerUpsInScene { get; private set; }

        public static float TimeSinceLastSpawnedPowerUp => instance.spawnTime - instance.cooldown;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError($"{nameof(PowerUpManager)} is a singlenton.");
                Destroy(this);
                return;
            }

            instance = this;

            camera = Camera.main;
            cooldown = spawnTime;
            audioSource = GetComponent<AudioSource>();

            EventManager.Subscribe<OnPowerUpPickedEvent>(() => PowerUpsInScene--);

            // For gameplay reasons power ups are not tracked by the rewind feature
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            cooldown -= Time.deltaTime;
            if (cooldown < 0 && powerUp == null)
            {
                cooldown = spawnTime;
                SpawnPowerUp();
            }
        }

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

            int index;
            do
            {
                index = Random.Range(0, templates.Length);
            } while (index == lastTemplate);

            powerUp = templates[index].CreatePickup(audioSource, layer);
            powerUp.layer = layer;
            powerUp.transform.position = position; // Don't update from rigidbody because that has one frame delay
            powerUp.GetComponent<Rigidbody2D>().velocity = -speed;

            PowerUpsInScene++;
            EventManager.Raise(new OnPowerUpSpawnEvent());
        }
    }
}