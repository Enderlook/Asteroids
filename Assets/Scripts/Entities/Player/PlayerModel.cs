using Enderlook.Unity.Attributes;
using Enderlook.Unity.Components.ScriptableSound;

using UnityEngine;

namespace Asteroids.Entities.Player
{
    public class PlayerModel : MonoBehaviour
    {
#pragma warning disable CS0649
        [field: SerializeField, IsProperty, Tooltip("Amount of lifes the player start with.")]
        public int startingLifes { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Maximum amount of lifes the player can have.")]
        public int maxLifes { get; private set; }

        [field: SerializeField, IsProperty, Tooltip("Duration of invulnerability after lose a life.")]
        public float invulnerabilityDuration { get; private set; }

        [field: SerializeField, IsProperty, Min(1), Tooltip("Amount of points required to earn a new life. Up to a maximum of one life can be get per score increase.")]
        public int scorePerLife { get; private set; }

        [SerializeField, Tooltip("Sound played on death.")]
        private SimpleSoundPlayer deathSound;

        [SerializeField, Tooltip("Sound played on get new life by score.")]
        private SimpleSoundPlayer newLifeSound;
#pragma warning restore CS0649

        public void PlayDeathSound() => deathSound.Play();

        public void PlayNewLifeSound() => newLifeSound.Play();



        [SerializeField, Tooltip("Acceleration per second.")]
        public float accelerationSpeed;

        [SerializeField, Tooltip("Maximum allowed speed.")]
        public float maximumSpeed;

        [SerializeField, Tooltip("Angles rotated per second.")]
        public float rotationSpeed;
    }
}