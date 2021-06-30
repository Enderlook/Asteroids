using Asteroids.Scene;

using UnityEngine;

namespace Asteroids.Entities.Player
{
    [RequireComponent(typeof(PlayerModel), typeof(SpriteRenderer))]
    public sealed class PlayerView : MonoBehaviour
    {
        private PlayerModel model;

        private new SpriteRenderer renderer;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            model = GetComponent<PlayerModel>();

            renderer = GetComponent<SpriteRenderer>();

            EventManager.Subscribe<PlayerController.VulnerabilityChangedEvent>(OnVulnerabilityChanged);
            EventManager.Subscribe<PlayerController.HealthChangedEvent>(OnHealthChanged);
        }

        private void OnVulnerabilityChanged(PlayerController.VulnerabilityChangedEvent @event)
        {
            if (@event.IsInvulnerable)
                renderer.color = Color.gray;
            else
                renderer.color = Color.white;
        }

        private void OnHealthChanged(PlayerController.HealthChangedEvent @event)
        {
            switch (@event.Reason)
            {
                case PlayerController.HealthChangedEvent.ReasonType.LoseByHit:
                    model.PlayDeathSound();
                    break;
                case PlayerController.HealthChangedEvent.ReasonType.EarnByScore:
                    model.PlayNewLifeSound();
                    break;
            }
        }
    }
}