using UnityEngine;

namespace Asteroids.Entities.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Acceleration per second.")]
        private float accelerationSpeed;

        [SerializeField, Tooltip("Maximum allowed speed.")]
        private float maximumSpeed;

        [SerializeField, Tooltip("Angles rotated per second.")]
        private float rotationSpeed;
#pragma warning restore CS0649

        private new Rigidbody2D rigidbody;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake() => rigidbody = GetComponent<Rigidbody2D>();

        private void FixedUpdate()
        {
            Move();
            Rotate();
        }

        private void Move()
        {
            rigidbody.AddRelativeForce(Vector2.up * Mathf.Max(Input.GetAxis("Vertical"), 0) * accelerationSpeed, ForceMode2D.Force);
            rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, maximumSpeed);
        }

        private void Rotate() => rigidbody.SetRotation(rigidbody.rotation - Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime);
    }
}
