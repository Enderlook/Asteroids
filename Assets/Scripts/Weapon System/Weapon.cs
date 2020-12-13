﻿using AvalonStudios.Additions.Attributes;

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

        [SerializeField, Tooltip("Time between attacks.")]
        protected float cooldown;

        [field: SerializeField, IsProperty, Tooltip("Whenever the weapon can be held down.")]
        public bool CanBeHoldDown { get; private set; }
#pragma warning restore CS0649

        public bool StopAction { get; protected set; } = false;
        
        protected WeaponsManager manager;

        protected float nextCast;

        public virtual void Initialize(WeaponsManager manager)
        {
            this.manager = manager;
            StopAction = false;
            nextCast = 0;
            Memento.TrackForRewind(this);
        }

        public virtual void Execute(bool canBeHoldDown)
        {
            if (GlobalMementoManager.IsRewinding)
                return;

            bool isPressed = canBeHoldDown ? Input.GetKey(manager.FireInput) : Input.GetKeyDown(manager.FireInput);
            if (Time.time > nextCast && isPressed)
                Fire();
        }

        protected abstract void Fire();

        public virtual void OnDrawGizmos() { }
    }
}