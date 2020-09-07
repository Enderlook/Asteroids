using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Asteroids.Characters.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("General")]

        [SerializeField]
        private new Rigidbody2D rigidbody2D = null;

        private void FixedUpdate()
        {
            Rotate();
        }

        private void Rotate()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 dir = mousePosition - rigidbody2D.position;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            rigidbody2D.MoveRotation(angle);
        }
    }
}
