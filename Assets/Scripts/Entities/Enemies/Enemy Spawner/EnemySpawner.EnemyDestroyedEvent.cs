namespace Asteroids.Entities.Enemies
{
    public sealed partial class EnemySpawner
    {
        public readonly struct EnemyDestroyedEvent
        {
            //IA2-P3
            // ^- Don't touch that comment, used by the teacher
            public readonly string Name;
            public readonly int Score;

            //IA2-P3
            // ^- Don't touch that comment, used by the teacher
            public EnemyDestroyedEvent(string name, int score)
            {
                Name = name;
                Score = score;
            }
        }
    }
}