using AvalonStudios.Additions.Attributes;

using Enderlook.Unity.Components.ScriptableSound;

using Asteroids.Scene;

using UnityEngine;

namespace Asteroids.WeaponSystem
{
    public abstract partial class Weapon : ScriptableObject
    {
#pragma warning disable CS0649
        [StyledHeader("General")]
        [SerializeField, Tooltip("Weapon's name.")]
        private string weaponName;

        [SerializeField, Tooltip("Weapon's description.")]
        private string description;

        [SerializeField, Tooltip("Sound produced on shoot.")]
        protected Sound weaponSound;
#pragma warning restore CS0649

        protected WeaponsManager manager;
        protected KeyCode fireInput = KeyCode.Space;

        protected float cooldown;
        protected float nextCast;

        public virtual void Initialize(WeaponsManager manager)
        {
            this.manager = manager;
            nextCast = 0;
            Memento.TrackForRewind(this);
        }

        public virtual void Update()
        {
            if (GlobalMementoManager.IsRewinding)
                return;

            if (Time.time > nextCast && manager.FireInput.Execute())
                Fire();
        }

        protected abstract void Fire();
    }
}