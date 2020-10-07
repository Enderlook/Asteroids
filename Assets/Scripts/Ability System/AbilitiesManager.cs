using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Asteroids.AbilitySystem
{
    public class AbilitiesManager : MonoBehaviour
    {
        public Transform CastPoint => castPoint;

        [Header("Setup")]

        [SerializeField, Tooltip("Cast position")]
        private Transform castPoint;

        [SerializeField, Tooltip("Abilities.")]
        private AbilitiesPack abilitiesPack = null;

        public void Awake()
        {
            abilitiesPack.Initialize(this);
        }

        public void Update()
        {
            if (!abilitiesPack)
                return;

            abilitiesPack.Update();
        }
    }
}
