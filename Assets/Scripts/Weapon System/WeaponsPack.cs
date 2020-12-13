using UnityEngine;

namespace Asteroids.WeaponSystem
{
    [CreateAssetMenu(menuName = "Asteroids/Weapon System/Weapons/Weapons Pack", fileName = "Weapon Packages")]
    public class WeaponsPack : ScriptableObject
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Weapons")]
        private Weapon[] weapons;
#pragma warning restore CS0649

        private int selectedWeapon;
        private WeaponsManager manager;

        public void Initialize(WeaponsManager manager)
        {
            this.manager = manager;
            if (weapons == null)
                return;

            for (int i = 0; i < weapons.Length; i++)
            {
                if (i == 0)
                    selectedWeapon = i;
                weapons[i]?.Initialize(manager);
            }
        }

        public void Update()
        {
            if (!weapons[selectedWeapon].StopAction)
                SwitchWeapon();

            weapons[selectedWeapon]?.Execute(weapons[selectedWeapon].CanBeHoldDown);

            for (int i = 0; i < weapons.Length; i++)
            {
                if (i == selectedWeapon)
                    continue;
                weapons[i].UpdateNonSelected();
            }
        }

        private void SwitchWeapon()
        {
            if (manager.ChangeWeaponInput.Execute())
                selectedWeapon = selectedWeapon >= weapons.Length - 1 ? 0 : selectedWeapon + 1;
        }

        public void OnDrawGizmos() => weapons[selectedWeapon].OnDrawGizmos();
    }
}
