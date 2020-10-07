using Enderlook.Unity.Utils.Interfaces;
using AvalonStudios.Additions.Utils.InputsManager;

using UnityEngine;

namespace Asteroids.AbilitySystem
{
    public abstract class Ability : ScriptableObject, IInitialize<AbilitiesManager>
    {
        [SerializeField, Tooltip("Weapon's name.")]
        private string weaponName = "";

        [SerializeField, Tooltip("Weapon's description.")]
        private string description = "";

        [SerializeField, Tooltip("Weapon's cooldown.")]
        private float cooldown = 1;

        [SerializeField, Tooltip("Ability's input")]
        private KeyInputManager castInput = new KeyInputManager();

        private float nextCast;

        public virtual void Initialize(AbilitiesManager abilitiesManager) 
        {
            nextCast = 0;
        }

        public virtual void Update()
        {
            if (castInput.Execute() && Time.time > nextCast)
            {
                nextCast = Time.time + cooldown;
                Execute();
            }
        }

        public abstract void Execute();
    }
}
