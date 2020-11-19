using System;

namespace Asteroids.Entities.Player
{
    public partial class PlayerController
    {
        [Serializable]
        public readonly struct State
        {
            // This struct is in charge of storing and setting the player state to save the game.

            private readonly int lifes;
            private readonly int scoreToNextLife;
            private readonly float invulnerabilityTime;
            // Player state is a superset of Player memento, we can take advantage of that to keep DRY
            private readonly Memento memento;

            public State(PlayerController playerController)
            {
                lifes = playerController.Lifes;
                scoreToNextLife = playerController.scoreToNextLife;
                invulnerabilityTime = playerController.invulnerabilityTime;
                memento = new Memento(playerController);
            }

            public void Load(PlayerController playerController)
            {
                playerController.Lifes = lifes;
                playerController.scoreToNextLife = scoreToNextLife;
                playerController.invulnerabilityTime = invulnerabilityTime;
                memento.Load(playerController);
            }
        }
    }
}
