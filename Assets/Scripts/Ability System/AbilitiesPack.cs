﻿using UnityEngine;

namespace Asteroids.AbilitySystem
{
    [CreateAssetMenu(menuName = "Asteroids/Ability System/Abilities/Abilities Pack", fileName = "Ability Package")]
    public class AbilitiesPack : ScriptableObject
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Abilities")]
        private Ability[] abilities;
#pragma warning restore CS0649

        public void Initialize(AbilitiesManager manager)
        {
            foreach (Ability ability in abilities)
                ability.Initialize(manager);
        }

        public void Update()
        {
            foreach (Ability ability in abilities)
                ability.Update();
        }
    }
}
