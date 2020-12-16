using Asteroids.Utils;

using UnityEngine;

namespace Asteroids.WeaponSystem
{
    public partial class Weapon
    {
        private FireCommand TryFireCommand(bool canBeHoldDown)
        {
            bool input = canBeHoldDown ? Input.GetKey(manager.FireInput) : Input.GetKeyDown(manager.FireInput);
            if (Time.time > nextCast && input)
                return new FireCommand(this);
            return null;
        }

        private class FireCommand : ICommand
        {
            private Weapon weapon;

            public FireCommand(Weapon weapon)
            {
                this.weapon = weapon;
            }

            public void Execute() => weapon.Fire();
        }
    }
}
