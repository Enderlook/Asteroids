using Asteroids.Scene;
using Asteroids.Utils;

using AvalonStudios.Additions.Attributes;

using Enderlook.Unity.Attributes;
using Enderlook.Unity.Components.ScriptableSound;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Resources = Asteroids.Utils.Resources;

namespace Asteroids.WeaponSystem
{
    [CreateAssetMenu(menuName = "Asteroids/Weapon System/Weapons/Components/Bomb Weapon", fileName = "Bomb Weapon")]
    public partial class BombWeapon : Weapon
    {
        private static readonly BuilderFactoryPool<Bomb, BombWeapon, (Vector3 position, Quaternion rotation, Bomb previous)>.Constructor construct = BombConstructor;
        private static readonly BuilderFactoryPool<Bomb, BombWeapon, (Vector3 position, Quaternion rotation, Bomb previous)>.Initializer initialize = BombInitializer;
        private static readonly BuilderFactoryPool<Bomb, BombWeapon, (Vector3 position, Quaternion rotation, Bomb previous)>.Initializer commonInitialize = BombCommonInitializer;
        private static readonly BuilderFactoryPool<Bomb, BombWeapon, (Vector3 position, Quaternion rotation, Bomb previous)>.Deinitializer deinitialize = BombDeinitializer;

#pragma warning disable CS0649
        [StyledHeader("Setup Bomb")]
        [SerializeField, DrawTexture, Tooltip("Sprite of the bomb to fire.")]
        private string spriteBomb;

        [SerializeField, Layer, Tooltip("Layer of the bomb.")]
        private int bombLayer;

        [SerializeField, Tooltip("Bomb sprite scale.")]
        private float bombScale;

        [StyledHeader("Setup Explosion")]
        [SerializeField, Tooltip("Key pressed to explode.")]
        private KeyCode explodeKey;

        [SerializeField, Min(0), Tooltip("Delay between explosions.")]
        private float chainDelay;

        [SerializeField, Tooltip("Animation used to explote the bomb.")]
        private RuntimeAnimatorController explodeAnimation;

        [SerializeField, Tooltip("Explode sprite scale.")]
        private float explodeScale;

        [SerializeField, Layer, Tooltip("Layer of the bomb explosion.")]
        private int explodeLayer;

        [SerializeField, Tooltip("Explode sound.")]
        private AudioClip explodeSound;
#pragma warning restore CS0649

        private BuilderFactoryPool<Bomb, BombWeapon, (Vector3 position, Quaternion rotation, Bomb previous)> builder;

        private SimpleSoundPlayer soundPlayer;

        private Bomb last;

        // Used to fix previous links after load game or rewind
        private HashSet<Bomb> bombs = new HashSet<Bomb>();
        private Dictionary<int, Bomb> lookup = new Dictionary<int, Bomb>();
        private int lastId;
#pragma warning restore CS0649

        public override void Initialize(WeaponsManager weaponsManager)
        {
            //MyA1-P3
            // ^- Don't touch that comment, used by the teacher
            base.Initialize(weaponsManager);

            builder = new BuilderFactoryPool<Bomb, BombWeapon, (Vector3 position, Quaternion rotation, Bomb previous)>
            {
                flyweight = this,
                constructor = construct,
                initializer = initialize,
                commonInitializer = commonInitialize,
                deinitializer = deinitialize
            };

            soundPlayer = SimpleSoundPlayer.CreateOneTimePlayer(weaponSound, false, false);

            // We set this because the application can run several games
            bombs.Clear();
            Bomb.totalID = 0;

            Memento.TrackForRewind(this);
            EventManager.Subscribe<StopRewindEvent>(OnStopRewind);
            GameSaver.SubscribeBombTrigger(() => new State(this), (parameter) => weaponsManager.StartCoroutine(OnLoadGame(parameter)));

            weaponsManager.StartCoroutine(ExplodeChecker());
        }

        private IEnumerator OnLoadGame((State, List<Bomb.State>) parameter)
        {
            yield return null;

            parameter.Item1.Load(this);
            int highest = 0;
            foreach (Bomb.State state in parameter.Item2)
            {
                Bomb bomb = CreateBomb();
                bomb.Initialize();
                state.Load(this, bomb);
                highest = Mathf.Max(highest, bomb.id);
            }
            Bomb.totalID = highest;

            yield return null;
            OnStopRewind();

            // Fix corruption by rewind
            HashSet<Bomb> visited = new HashSet<Bomb>();
            Bomb current = last;
            if (current is null)
                yield break;
            visited.Add(current);

            while (true)
            {
                Bomb previous = current;
                current = last.previous;
                if (current is null)
                    yield break;
                if (visited.Contains(current))
                {
                    Debug.LogWarning("Endless recursion detected due rewind corruption... fixed.");
                    previous.previous = current;

                    foreach (Bomb bomb in bombs)
                    {
                        if (visited.Contains(bomb))
                            continue;
                        builder.Store(bomb);
                    }

                    yield break;
                }
                else
                    visited.Add(current);
            }
        }

        private void OnStopRewind()
        {
            lookup.Clear();

            foreach (Bomb bomb in bombs)
                lookup.Add(bomb.id, bomb);

            foreach (Bomb bomb in bombs)
                bomb.ConfigurePrevious(lookup);

            if (lastId == 0)
                last = null;
            else
                last = lookup[lastId];

            lastId = 0;
        }

        private IEnumerator ExplodeChecker()
        {
            while (true)
            {
                yield return null;
                //MyA1-P2
                // ^- Don't touch that comment, used by the teacher
                if (TryExplodeCommand() is ExplodeCommand command)
                    command.Execute();
            }
        }

        private static Bomb BombConstructor(in BombWeapon flyweight, in (Vector3 position, Quaternion rotation, Bomb previous) parameters)
        {
            GameObject bomb = new GameObject("Bomb")
            {
                layer = flyweight.bombLayer
            };

            SpriteRenderer spriteRenderer = bomb.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load<Sprite>(flyweight.spriteBomb);

            bomb.transform.localScale *= flyweight.bombScale;

            Bomb bombScript = bomb.AddComponent<Bomb>();
            bombScript.flyweight = flyweight;

            GameObject explode = new GameObject("Explode")
            {
                layer = flyweight.explodeLayer,
            };
            explode.SetActive(false);
            explode.transform.SetParent(bomb.transform);
            explode.transform.localScale /= flyweight.bombScale;
            explode.transform.localScale *= flyweight.explodeScale;

            explode.AddComponent<SpriteRenderer>();
            Animator animator = explode.AddComponent<Animator>();
            animator.runtimeAnimatorController = flyweight.explodeAnimation;

            AudioSource audioSource = explode.AddComponent<AudioSource>();
            audioSource.clip = flyweight.explodeSound;

            explode.AddComponent<PolygonCollider2D>();

            GameSaver.SubscribeBombsTrigger(() => new Bomb.State(bombScript));

            flyweight.bombs.Add(bombScript);

            return bombScript;
        }

        private static void BombInitializer(in BombWeapon flyweight, Bomb obj, in (Vector3 position, Quaternion rotation, Bomb previous) parameters)
            => obj.gameObject.SetActive(true);

        private static void BombCommonInitializer(in BombWeapon flyweight, Bomb obj, in (Vector3 position, Quaternion rotation, Bomb previous) parameters)
        {
            Transform transform = obj.transform;
            transform.position = parameters.position;
            transform.rotation = parameters.rotation;

            obj.previous = parameters.previous;
            flyweight.last = obj;
        }

        private static void BombDeinitializer(Bomb obj) => obj.Reset();

        protected override void Fire()
        {
            nextCast = Time.time + cooldown;
            CreateBomb();
        }

        private Bomb CreateBomb()
        {
            Transform castPoint = manager.CastPoint;
            Rigidbody2D playerRigidbody = manager.Rigidbody2D;

            Bomb bomb = builder.Create((
                castPoint.position,
                Quaternion.Euler(new Vector3(0, 0, playerRigidbody.rotation)),
                last
            ));

            soundPlayer.Play();

            return bomb;
        }

        public override void OnDrawGizmos()
        {
            if (last == null)
                return;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(manager.transform.position, last.transform.position);
        }

        public void FallbackExplosion()
        {
            foreach (Bomb bomb in bombs)
                bomb.FallbackExplosion();
        }
    }
}