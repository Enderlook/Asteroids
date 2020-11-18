using Asteroids.Scene;

using UnityEngine;

namespace Asteroids.PowerUps
{
    [CreateAssetMenu(menuName = "Asteroids/Power Ups/Rewind Pack")]
    public sealed class RewindPowerUpTemplate : PowerUpTemplate
    {
        protected override IPickup GetPickup(AudioSource audioSource) => new RewindPickupDecorator(base.GetPickup(audioSource), audioSource);

        private sealed class RewindPickupDecorator : IPickup
        {
            private IPickup decorable;
            private float duration;

            public RewindPickupDecorator(IPickup pickup, AudioSource audioSource)
            {
                decorable = pickup;
                duration = audioSource.clip.length;
            }

            public void PickUp()
            {
                decorable.PickUp();
                GlobalMementoManager.Rewind(duration);
            }
        }
    }
}
