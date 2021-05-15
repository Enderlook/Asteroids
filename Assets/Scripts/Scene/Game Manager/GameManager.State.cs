using System;

namespace Asteroids.Scene
{
    public sealed partial class GameManager
    {
        [Serializable]
        public readonly struct State
        {
            // This struct is in charge of storing and setting the game managaer state to save the game.

            private readonly int level;
            private readonly int score;

            public State(GameManager gameManager)
            {
                level = gameManager.level;
                score = gameManager.score;
            }

            public void Load(GameManager gameManager)
            {
                gameManager.level = level;
                gameManager.score = score;
            }
        }
    }
}