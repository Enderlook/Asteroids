using Asteroids.Utils;

using UnityEngine;

namespace Asteroids.WeaponSystem
{
    public partial class BombWeapon : Weapon
    {
        private ExplodeCommand? TryExplodeCommand()
        {
            if (Input.GetKeyDown(explodeKey) && last != null)
                return new ExplodeCommand(this);
            return null;
        }

        private struct ExplodeCommand : ICommand
        {
            private BombWeapon weapon;

            public ExplodeCommand(BombWeapon weapon) => this.weapon = weapon;

            public void Execute()
            {
                weapon.last.Explode(0);
                weapon.last = null;
            }
        }
    }
}