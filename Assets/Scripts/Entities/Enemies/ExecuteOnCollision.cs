using System;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public class ExecuteOnCollision : MonoBehaviour
    {
        private Action OnCollision;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnCollisionEnter2D(Collision2D collision) => OnCollision?.Invoke();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
                OnCollision();
        }

        public void RemoveAllListeners() => OnCollision = null;

        public void Subscribe(Action onCollision) => OnCollision += onCollision;
    }
}