//IA2-P3
// The whole file.
// ^- Don't touch that comment, used by the teacher
using Asteroids.Entities.Enemies;
using Asteroids.Scene;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Resources = Asteroids.Utils.Resources;

namespace Asteroids.UI
{
    public sealed class Scoreboard : MonoBehaviour
    {
        [SerializeField, Tooltip("Prefab of score tile to show.")]
        private ScoreField scorePrefab;

        [SerializeField, Tooltip("Total of score and kills.")]
        private ScoreField total;

        [SerializeField, Tooltip("Layout to show scores.")]
        private RectTransform layout;

        [SerializeField, Tooltip("Types of enemies")]
        private SimpleEnemyFlyweight[] enemies;

        private Dictionary<string, (int kills, int totalScore)> killedEnemies = new Dictionary<string, (int kills, int totalScore)>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            EventManager.Subscribe<EnemySpawner.EnemyDestroyedEvent>(e => {
                killedEnemies.TryGetValue(e.Name, out (int kills, int totalScore) tuple);
                killedEnemies[e.Name] = (tuple.kills + 1, tuple.totalScore + e.Score);
            });
        }

        public void OrderScores()
        {
            IEnumerable<(string name, Sprite sprite)> sprites = enemies.Select(e => (e.name, Resources.Load<Sprite>(e.Sprites[0])));
            foreach ((Sprite sprite, int kills, int score) in killedEnemies
                .Concat(enemies
                    .Select(e => e.name)
                    .Except(killedEnemies.Keys)
                    .Select(e => new KeyValuePair<string, (int kills, int totalScore)>(e, (0, 0))))
                .OrderByDescending(e => e.Value.totalScore)
                .ThenByDescending(e => e.Value.kills)
                .ThenBy(e => e.Key)
                .Join(sprites, e => e.Key, e => e.name, (a, b) => (b.sprite, a.Value.kills, a.Value.totalScore))
                )
            {
                ScoreField scoreField = Instantiate(scorePrefab, layout);
                scoreField.SetTarget(score, kills);
                scoreField.SetSprite(sprite);
            }

            total.SetTarget(killedEnemies.Values.Sum(e => e.totalScore), killedEnemies.Values.Sum(e => e.kills));
        }
    }
}