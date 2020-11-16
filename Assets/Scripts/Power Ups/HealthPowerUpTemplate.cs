using Asteroids.Entities.Player;

using UnityEngine;

namespace Asteroids.PowerUps
{

    [CreateAssetMenu(menuName = "Asteroids/Power Ups/Health Pack")]
    public sealed class HealthPowerUpTemplate : PowerUpTemplate
    {
        protected override IPickup GetPickup(AudioSource audioSource)
            => new HealthPickupDecorator(base.GetPickup(audioSource));

        private sealed class HealthPickupDecorator : IPickup
        {
            private IPickup decorable;

            public HealthPickupDecorator(IPickup pickup) => decorable = pickup;

            public void PickUp()
            {
                decorable.PickUp();
                FindObjectOfType<Player>().AddNewLife();
            }
        }
    }
}
