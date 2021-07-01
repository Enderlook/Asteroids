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
#pragma warning disable CS0649
        [SerializeField, Tooltip("Prefab of score tile to show.")]
        private ScoreField scorePrefab;

        [SerializeField, Tooltip("Total of score and kills.")]
        private ScoreField total;

        [SerializeField, Tooltip("Layout to show scores.")]
        private RectTransform layout;

        [SerializeField, Tooltip("Types of enemies")]
        private SimpleEnemyFlyweight[] enemies;

#pragma warning restore CS0649

        private Dictionary<string, (int kills, int totalScore)> killedEnemies = new Dictionary<string, (int kills, int totalScore)>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            /* We could add all the types of enemies here in killedEnemies with 0 values to avoid the .Concat() LINQ,
             * However, the exercise requested to use that LINQ method so we don't do that. */

            EventManager.Subscribe<EnemySpawner.EnemyDestroyedEvent>(e => {
                killedEnemies.TryGetValue(e.Name, out (int kills, int totalScore) tuple);
                killedEnemies[e.Name] = (tuple.kills + 1, tuple.totalScore + e.Score);
            });
        }

        public void OrderScores()
        {
            foreach ((Sprite sprite, int kills, int score) in killedEnemies
                .Concat(enemies
                    .Select(e => e.name)
                    .Except(killedEnemies.Keys)
                    .Select(e => new KeyValuePair<string, (int kills, int totalScore)>(e, (0, 0))))
                .OrderByDescending(e => e.Value.totalScore)
                .ThenByDescending(e => e.Value.kills)
                .ThenBy(e => e.Key)
                .Join(enemies, e => e.Key, e => e.name, (a, b) => (Resources.Load<Sprite>(b.Sprites[0]), a.Value.kills, a.Value.totalScore))
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