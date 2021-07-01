using Asteroids.Entities.Enemies;
using Asteroids.Scene;

using AvalonStudios.Additions.Attributes;

using Enderlook.Unity.Components.ScriptableSound;

using UnityEngine;

namespace Asteroids.WeaponSystem
{
    [CreateAssetMenu(menuName = "Asteroids/Weapon System/Weapons/Components/Laser Weapon", fileName = "Laser Weapon")]
    public sealed partial class LaserWeapon : Weapon
    {
#pragma warning disable CS0649
        [StyledHeader("Setup")]
        [SerializeField, Tooltip("Raycast distance.")]
        private float distance;

        [SerializeField, Tooltip("Layers to hit.")]
        private LayerMask hitTargets;

        [SerializeField, Tooltip("Laser duration in seconds.")]
        private float duration;
#pragma warning restore CS0649

        private Transform castPoint;
        private LineRenderer lineRenderer;

        private SimpleSoundPlayer soundPlayer;

        private float currentDuration;
        private RaycastHit2D[] results;

        public override void Initialize(WeaponsManager weaponsManager)
        {
            base.Initialize(weaponsManager);
            cooldown += duration;
            castPoint = weaponsManager.CastPoint;
            lineRenderer = castPoint.GetComponent<LineRenderer>();
            soundPlayer = SimpleSoundPlayer.CreateOneTimePlayer(weaponSound, false, false);
            results = new RaycastHit2D[FindObjectOfType<EnemySpawner>().MaximumAmountOfEnemies];

            Memento.TrackForRewind(this);
            GameSaver.SubscribeLaserTrigger(() => new State(this), (state) => ((State)state).Load(this));
        }

        public override void Execute(bool canBeHoldDown)
        {
            base.Execute(canBeHoldDown);

            if (GlobalMementoManager.IsRewinding)
                return;

            if (currentDuration > 0)
            {
                StopAction = true;
                currentDuration -= Time.deltaTime;
                if (currentDuration <= 0)
                {
                    StopAction = false;
                    lineRenderer.enabled = false;
                    return;
                }
                UpdateLaser();
            }
        }

        public override void UpdateNonSelected()
        {
            if (currentDuration != 0)
            {
                currentDuration = 0;
                StopAction = false;
                lineRenderer.enabled = false;
            }
        }

        private void UpdateLaser()
        {
            int amount = Physics2D.RaycastNonAlloc(castPoint.position, castPoint.up, results, 100, hitTargets);
            for (int i = 0; i < amount; i++)
            {
                GameObject gameObject = results[i].collider.gameObject;
                foreach (ExecuteOnCollision toExecute in gameObject.GetComponents<ExecuteOnCollision>())
                    toExecute.Execute();
                if (gameObject.TryGetComponent(out Boss boss))
                    boss.TakeDamage();
            }
            lineRenderer.SetPosition(0, new Vector3(0, distance, 0));
        }

        protected override void Fire()
        {
            nextCast = Time.time + cooldown;
            currentDuration = duration;
            lineRenderer.enabled = true;
            soundPlayer.Play();
            UpdateLaser();
        }
    }
}
