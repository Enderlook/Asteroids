using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public abstract class ExecuteOnCollision : MonoBehaviour
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnCollisionEnter2D(Collision2D collision) => Execute();

        public abstract void Execute();
    }
}