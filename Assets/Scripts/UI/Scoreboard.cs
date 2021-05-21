using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    private Dictionary<string, (int kills, int score)> killedEnemies = new Dictionary<string, (int kills, int score)>();

    private void Awake()
    {
        #region Test
        killedEnemies.Add("Feral Dog", (5, 50));
        killedEnemies.Add("Spirit", (2, 50));
        killedEnemies.Add("Bandit", (5, 100));
        killedEnemies.Add("Bandit Boss", (1, 250));

        OrderScores(killedEnemies);
        #endregion
    }

    public void GetIndividualEnemy(IEnumerable<string> enemyName)
    {
        enemyName.GroupBy(e => e).Select(e => (e.Key, e.Count())).OrderBy(e => e.Item2);
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
