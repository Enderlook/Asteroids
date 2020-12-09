using UnityEngine;

namespace Asteroids.Entities.Player
{
    public partial class PlayerController
    {
        private RotateCommand? TryGetRotateCommand()
        {
            float input = Input.GetAxis("Horizontal");
            if (input != 0)
                return new RotateCommand(this, input * Time.deltaTime);
            return null;
        }

        private struct RotateCommand
        {
            private PlayerController controller;
            private float strength;

            public RotateCommand(PlayerController controller, float strength)
            {
                this.controller = controller;
                this.strength = strength;
            }

            public void Execute()
                => controller.rigidbody.SetRotation(controller.rigidbody.rotation - (strength * controller.model.rotationSpeed));
        }
    }

}