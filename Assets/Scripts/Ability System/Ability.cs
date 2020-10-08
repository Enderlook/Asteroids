using Enderlook.Unity.Utils.Interfaces;
using AvalonStudios.Additions.Utils.InputsManager;

using UnityEngine;

namespace Asteroids.AbilitySystem
{
    public abstract class Ability : ScriptableObject, IInitialize<AbilitiesManager>
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Weapon's name.")]
        private string weaponName;

        [SerializeField, Tooltip("Weapon's description.")]
        private string description;

        [SerializeField, Tooltip("Weapon's cooldown in secods.")]
        private float cooldown = 1;

        [SerializeField, Tooltip("Ability's input.")]
        private KeyInputManager castInput = new KeyInputManager();
#pragma warning restore CS0649

        private float nextCast;

        public virtual void Initialize(AbilitiesManager abilitiesManager) => nextCast = 0;

        public virtual void Update()
        {
            if (Time.time > nextCast && castInput.Execute())
            {
                nextCast = Time.time + cooldown;
                Execute();
            }
        }

        public abstract void Execute();
    }
}
