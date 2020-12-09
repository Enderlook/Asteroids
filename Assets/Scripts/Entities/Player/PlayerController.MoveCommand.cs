using UnityEngine;

namespace Asteroids.Entities.Player
{
    public partial class PlayerController
    {
        private MoveCommand? TryGetMoveCommand()
        {
            float input = Input.GetAxis("Vertical");
            if (input > 0)
                return new MoveCommand(this, input);
            return null;
        }

        private struct MoveCommand
        {
            private PlayerController controller;
            private float strength;

            public MoveCommand(PlayerController controller, float strength)
            {
                this.controller = controller;
                this.strength = strength;
            }

            public void Execute()
            {
                controller.rigidbody.AddRelativeForce(Vector2.up * strength * controller.model.accelerationSpeed, ForceMode2D.Force);
                controller.rigidbody.velocity = Vector2.ClampMagnitude(controller.rigidbody.velocity, controller.model.maximumSpeed);
            }
        }
    }

}