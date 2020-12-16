using UnityEngine;

namespace Asteroids.WeaponSystem
{
    [CreateAssetMenu(menuName = "Asteroids/Weapon System/Weapons/Weapons Pack", fileName = "Weapon Packages")]
    public partial class WeaponsPack : ScriptableObject
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
            //MYA1-P2
            if (TryChangeWeaponCommand() is ChangeWeaponCommand changeWeaponCommand)
                changeWeaponCommand.Execute();
        }

        public void OnDrawGizmos() => weapons[selectedWeapon].OnDrawGizmos();
    }
}
