using Asteroids.Events;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public class Enemy : MonoBehaviour
    {
        private EnemyDestroyedEvent destroyedEvent;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnCollisionEnter2D(Collision2D collision) => EventManager.Raise(destroyedEvent);

        public void SetDestroyedEvent(EnemyDestroyedEvent destroyedEvent) => this.destroyedEvent = destroyedEvent;
    }
}