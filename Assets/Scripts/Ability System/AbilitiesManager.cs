using Enderlook.Unity.Attributes;

using UnityEngine;

namespace Asteroids.AbilitySystem
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class AbilitiesManager : MonoBehaviour
    {
#pragma warning disable CS0649
        [field: Header("Setup")]
        [field: SerializeField, IsProperty, Tooltip("Cast position")]
        public Transform CastPoint { get; private set; }

        [SerializeField, Tooltip("Abilities.")]
        private AbilitiesPack abilitiesPack;
#pragma warning restore CS0649

        public Rigidbody2D Rigidbody2D { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();
            abilitiesPack.Initialize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            if (abilitiesPack != null)
                abilitiesPack.Update();
        }
    }
}
