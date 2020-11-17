using Enderlook.Unity.Utils.Interfaces;
using Enderlook.Unity.Components.ScriptableSound;

using AvalonStudios.Additions.Utils.InputsManager;

using UnityEngine;
using System;

namespace Asteroids.AbilitySystem
{
    public abstract class Ability : ScriptableObject, IInitialize<AbilitiesManager>
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Ability's name.")]
        private string abilityName;

        [SerializeField, Tooltip("Ability's description.")]
        private string description;

        [SerializeField, Tooltip("Ability's cooldown in secods.")]
        private float cooldown = 1;

        [SerializeField, Tooltip("Ability's input.")]
        protected KeyInputManager castInput = new KeyInputManager();

        [SerializeField, Tooltip("Sound produced on shoot.")]
        protected Sound abilitySound;
#pragma warning restore CS0649

        private float nextCast;

        public virtual void Initialize(AbilitiesManager abilitiesManager)
        {
            nextCast = 0;

            GlobalMementoManager.Subscribe(CreateMemento, ConsumeMemento, interpolateMementos);

            float CreateMemento() => nextCast - Time.fixedDeltaTime; // Memento of abilities is only its cooldown

            void ConsumeMemento(float? memento)
            {
                if (memento is float m)
                    nextCast = m + Time.fixedDeltaTime;
            }
        }

        private static readonly Func<float, float, float, float> interpolateMementos = InterpolateMementos;

        private static float InterpolateMementos(float a, float b, float delta) => Mathf.Lerp(a, b, delta);

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
