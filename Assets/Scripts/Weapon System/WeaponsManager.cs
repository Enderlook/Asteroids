using AvalonStudios.Additions.Attributes;
using AvalonStudios.Additions.Utils.InputsManager;

using IsProperty = Enderlook.Unity.Attributes.IsPropertyAttribute;

using UnityEngine;

namespace Asteroids.WeaponSystem
{
    public class WeaponsManager : MonoBehaviour
    {
#pragma warning disable CS0649
        [field: StyledHeader("Setup")]
        [field: SerializeField, IsProperty, Tooltip("Cast position")]
        public Transform CastPoint { get; private set; }

        [SerializeField, Tooltip("Weapon package.")]
        private WeaponsPack weaponPack;

        [field: SerializeField, Tooltip("Input to change weapon."), IsProperty]
        public KeyInputManager ChangeWeaponInput { get; private set; }
#pragma warning restore CS0649

        public Rigidbody2D Rigidbody2D { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();
            weaponPack?.Initialize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
            =>  weaponPack?.Update();
    }
}
