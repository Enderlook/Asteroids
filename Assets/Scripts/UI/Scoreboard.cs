//IA2-P3
// The whole file.
// ^- Don't touch that comment, used by the teacher
using Asteroids.Entities.Enemies;
using Asteroids.Scene;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Asteroids.UI
{
    public sealed class Scoreboard : MonoBehaviour
    {
        private Dictionary<string, (int kills, int totalScore)> killedEnemies = new Dictionary<string, (int kills, int totalScore)>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            EventManager.Subscribe<EnemySpawner.EnemyDestroyedEvent>(e => {
                killedEnemies.TryGetValue(e.Name, out (int kills, int totalScore) tuple);
                killedEnemies[e.Name] = (tuple.kills + 1, tuple.totalScore + e.Score);
            });
        }

        public void OrderScores(Dictionary<string, (int kills, int score)> enemyScores)
        {
            int totalKills = 0; int totalScore = 0;
            foreach (KeyValuePair<string, (int kills, int score)> enemyDic in enemyScores.OrderByDescending(e => e.Value.score).ThenByDescending(e => e.Value.kills).ThenBy(e => e.Key))
            {
                Debug.Log($"Enemy: {enemyDic.Key}. Killed: {enemyDic.Value.kills}. Score: {enemyDic.Value.score}");
            }
            totalKills = enemyScores.Values.Sum(e => e.kills);
            totalScore = enemyScores.Values.Sum(e => e.score);
            Debug.Log($"Total kills: {totalKills}. Total score: {totalScore}.");
        }
    }
}