namespace Asteroids.Entities.Player
{
    public partial class Player
    {
        public readonly struct State
        {
            // This struct is in charge of storing and setting the player state to save the game.

            private readonly int lifes;
            private readonly int scoreToNextLife;
            private readonly float invulnerabilityTime;
            // Player state is a superset of Player memento, we can take advantage of that to keep DRY
            private readonly Memento memento;

            private State(Player player)
            {
                lifes = player.lifes;
                scoreToNextLife = player.scorePerLife;
                invulnerabilityTime = player.invulnerabilityTime;
                memento = new Memento(player);
            }

            private void Load(Player player)
            {
                player.lifes = lifes;
                player.scoreToNextLife = scoreToNextLife;
                player.invulnerabilityTime = invulnerabilityTime;
                memento.Load(player);
            }
        }
    }
}
