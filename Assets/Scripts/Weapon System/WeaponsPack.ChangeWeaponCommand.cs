using Asteroids.Utils;

using UnityEngine;

namespace Asteroids.WeaponSystem
{
    public partial class WeaponsPack
    {
        private ChangeWeaponCommand TryChangeWeaponCommand()
        {
            bool input = Input.GetKeyDown(manager.ChangeWeaponInput);
            if (input)
                return new ChangeWeaponCommand(this);
            return null;
        }

        private class ChangeWeaponCommand : ICommand
        {
            private WeaponsPack pack;

            public ChangeWeaponCommand(WeaponsPack pack)
            {
                this.pack = pack;
            }

            public void Execute() =>
                pack.selectedWeapon = pack.selectedWeapon >= pack.weapons.Length - 1 ? 0 : pack.selectedWeapon + 1;
        }
    }
}
