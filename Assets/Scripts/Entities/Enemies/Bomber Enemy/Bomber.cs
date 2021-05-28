//IA2-P1
// The whole file.
// ^- Don't touch that comment, used by the teacher
using Asteroids.Entities.Player;
using Asteroids.Scene;
using Asteroids.Utils;

using Enderlook.Unity.Components.ScriptableSound;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Resources = Asteroids.Utils.Resources;

namespace Asteroids.Entities.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed partial class Bomber : MonoBehaviour
    {
        private static readonly BuilderFactoryPool<Bomb, Bomber, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Constructor construct = ProjectileConstructor;
        private static readonly BuilderFactoryPool<Bomb, Bomber, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Initializer initialize = ProjectileInitializer;
        private static readonly BuilderFactoryPool<Bomb, Bomber, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Initializer commonInitialize = ProjectileCommonInitializer;
        private static readonly BuilderFactoryPool<Bomb, Bomber, (Vector3 position, Quaternion rotation, Vector3 velocity)>.Deinitializer deinitialize = ProjectileDeinitializer;

        private BomberEnemyFlyweight flyweight;
        private Transform shootPoint;

        private BuilderFactoryPool<Bomb, Bomber, (Vector3 position, Quaternion rotation, Vector3 velocity)> builder;

        private SimpleSoundPlayer soundPlayer;
        private float nextCast;
        private new Rigidbody2D rigidbody2D;
        private new SpriteRenderer renderer;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            BomberMemento.TrackForRewind(this);

            builder = new BuilderFactoryPool<Bomb, Bomber, (Vector3 position, Quaternion rotation, Vector3 velocity)>
            {
                flyweight = this,
                constructor = construct,
                initializer = initialize,
                commonInitializer = commonInitialize,
                deinitializer = deinitialize
            };

            rigidbody2D = GetComponent<Rigidbody2D>();
            renderer = GetComponent<SpriteRenderer>();

            GameSaver.SubscribeBomberEnemy(this, () => new BomberState(this));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]    
        private void Update()
        {
            if (GlobalMementoManager.IsRewinding)
                return;

            if (Time.time >= nextCast)
                Fire();
        }

        public void Construct(BomberEnemyFlyweight flyweight, Transform shootPoint)
        {
            this.flyweight = flyweight;
            this.shootPoint = shootPoint;
            soundPlayer = SimpleSoundPlayer.CreateOneTimePlayer(flyweight.ShootSound, false, false);
        }

        public void Load(BomberState state, List<ProjectileState> projectileStates) => StartCoroutine(OnLoadGame(state, projectileStates));

        private IEnumerator OnLoadGame(BomberState state, List<ProjectileState> projectileStates)
        {
            yield return null;
            state.Load(this);
            foreach (ProjectileState state_ in projectileStates)
                state_.Load(this, CreateBomb());
        }

        private static Bomb ProjectileConstructor(in Bomber flyweight, in (Vector3 position, Quaternion rotation, Vector3 velocity) parameters)
        {
            BomberEnemyFlyweight flyweight2 = flyweight.flyweight;

            GameObject projectile = new GameObject("Bomb")
            {
                layer = flyweight2.BombLayer
            };

            projectile.transform.localScale *= flyweight2.SpriteScale;

            SpriteRenderer spriteRenderer = projectile.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load<Sprite>(flyweight2.Sprite);

            projectile.AddComponent<PolygonCollider2D>();

            Bomb bomb = projectile.AddComponent<Bomb>();
            bomb.flyweight = flyweight;

            GameObject explode = new GameObject("Explode")
            {
                layer = flyweight2.ExplodeLayer,
            };
            explode.SetActive(false);
            explode.transform.SetParent(bomb.transform);
            explode.transform.localScale /= flyweight2.SpriteScale;
            explode.transform.localScale *= flyweight2.ExplodeScale;

            explode.AddComponent<SpriteRenderer>();
            Animator animator = explode.AddComponent<Animator>();
            animator.runtimeAnimatorController = flyweight2.ExplodeAnimation;

            AudioSource audioSource = explode.AddComponent<AudioSource>();
            audioSource.clip = flyweight2.ExplodeSound;

            explode.AddComponent<PolygonCollider2D>();

            Bomb.ProjectileMemento.TrackForRewind(flyweight, bomb);

            GameSaver.SubscribeBomberBullet(flyweight, () => new ProjectileState(bomb));

            return bomb;
        }

        private static void ProjectileInitializer(in Bomber flyweight, Bomb bomb, in (Vector3 position, Quaternion rotation, Vector3 velocity) parameters) {}

        private static void ProjectileDeinitializer(Bomb bomb) => bomb.gameObject.SetActive(false);

        private static void ProjectileCommonInitializer(in Bomber flyweight, Bomb bomb, in (Vector3 position, Quaternion rotation, Vector3 velocity) parameters)
        {
            // We enable the gameObject here instead in ProjectileInitializer, because that method is executed later
            // and so it produces a bug because rigibodies doesn't work when their gameObjects are disabled
            bomb.gameObject.SetActive(true);

            // Don't use Rigidbody2D because it takes a frame to update and produces a visual bug
            Transform transform = bomb.transform;
            transform.position = parameters.position;
            transform.rotation = parameters.rotation;
        }

        private void Fire() 
        {
            if (!renderer.isVisible)
                return;

            nextCast = Time.time + flyweight.Cooldown;
            CreateBomb(); 
        }

        private Bomb CreateBomb()
        {
            Bomb bomb = builder.Create((
                shootPoint.position,
                Quaternion.Euler(new Vector3(0, 0, rigidbody2D.rotation)),
                (Vector2)(-shootPoint.up * flyweight.Force) + rigidbody2D.velocity
            ));

            soundPlayer.Play();

            return bomb;
        }

        public sealed partial class Bomb : MonoBehaviour
        {
            private static readonly List<Vector2> physicsShape = new List<Vector2>();

            public Bomber flyweight;

            private Transform child;
            private SpriteRenderer spriteRenderer;
            private SpriteRenderer childSpriteRenderer;
            private new PolygonCollider2D collider;
            private AudioSource audioSource;
            private Animator animator;

            private Sprite last;

            private bool isExploding;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
            private void Start()
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                child = transform.GetChild(0);
                audioSource = child.GetComponent<AudioSource>();
                childSpriteRenderer = child.GetComponent<SpriteRenderer>();
                collider = child.GetComponent<PolygonCollider2D>();
                animator = child.GetComponent<Animator>();

                last = childSpriteRenderer.sprite;
                if (last == null)
                    collider.enabled = false;
                else
                {
                    collider.enabled = true;

                    int count = last.GetPhysicsShapeCount();
                    for (int i = 0; i < count; i++)
                    {
                        last.GetPhysicsShape(i, physicsShape);
                        collider.SetPath(i, physicsShape);
                    }
                }

                EventManager.Subscribe<GameManager.LevelTerminationEvent>(e =>
                {
                    if (e.HasWon)
                        Return();
                });
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
            private void Update()
            {
                if (isExploding)
                {
                    UpdateCollider();
                    if (!audioSource.isPlaying && childSpriteRenderer.sprite == null)
                        Return();
                }
                else
                {
                    float explosionDistance = flyweight.flyweight.ExplosionDistance;
                    //IA2-P2
                    // ^- Don't touch that comment, used by the teacher
#if SPATIAL_GRID
                    /* The only place in the game when we do a range check that doesn't use colliders
                     * and so it can be made with an spatial grid without losing precision.
                     * Thought the query only check for the existence of a single object. */
                    Vector2 position = transform.position;
                    float squaredExplosionDistance = explosionDistance * explosionDistance;
                    Vector2 squaredExplosionDistanceVector = Vector2.one * squaredExplosionDistance;
                    if (GameManager.SpatialGrid.Query(
                        position - squaredExplosionDistanceVector,
                        position + squaredExplosionDistanceVector,
                        e => (position - e).sqrMagnitude <= squaredExplosionDistance)
                        .OfType<PlayerController>().Any())
                        GotoStateExploding();
#else
                    if ((transform.position - PlayerController.Position).sqrMagnitude <= explosionDistance * explosionDistance)
                        GotoStateExploding();
#endif
                }
            }

            private void GotoStateExploding()
            {
                isExploding = true;
                spriteRenderer.enabled = false;
                child.gameObject.SetActive(true);
                audioSource.Play();
            }

            private void GotoStateNormal()
            {
                isExploding = false;
                spriteRenderer.enabled = true;
                child.gameObject.SetActive(false);
            }

            private void Return()
            {
                flyweight.builder.Store(this);
                GotoStateNormal();
            }

            private void UpdateCollider()
            {
                Sprite sprite = childSpriteRenderer.sprite;
                if (last != sprite)
                {
                    last = sprite;
                    if (sprite == null)
                        collider.enabled = false;
                    else
                    {
                        collider.enabled = true;

                        int count = sprite.GetPhysicsShapeCount();
                        collider.pathCount = count;
                        for (int i = 0; i < count; i++)
                        {
                            sprite.GetPhysicsShape(i, physicsShape);
                            collider.SetPath(i, physicsShape);
                        }
                    }
                }
            }
        }
    }
}
