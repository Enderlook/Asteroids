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
        AddKilledEnemies("Base Ship", 10, 5);
        AddKilledEnemies("Split Ship", 5, 10);
        AddKilledEnemies("Shooter Ship", 2, 15);
        AddKilledEnemies("Boss Ship", 1, 500);

        OrderScores(killedEnemies);
        #endregion
    }

    public void AddKilledEnemies(string enemyName, int killedAmount, int baseScore)
    {
        killedEnemies.Add(enemyName, (killedAmount, (baseScore * killedAmount))); //(baseScore * killedAmount) could be changed to totalScore if needed.
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
