using UnityEngine;

namespace Asteroids.PowerUps
{
    [CreateAssetMenu(menuName = "Asteroids/Power Ups/Rewind Pack")]
    public sealed class RewindPowerUpTemplate : PowerUpTemplate
    {
        protected override IPickup GetPickup(AudioSource audioSource) => new RewindPickupDecorator(base.GetPickup(audioSource));

        private sealed class RewindPickupDecorator : IPickup
        {
            private IPickup decorable;

            public RewindPickupDecorator(IPickup pickup) => decorable = pickup;

            public void PickUp()
            {
                decorable.PickUp();
                GlobalMementoManager.Rewind();
            }
        }
    }
}
