using Asteroids.AbilitySystem;
using Asteroids.Entities.Enemies;
using Asteroids.Entities.Player;

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

namespace Asteroids.Scene
{
    public partial class GameSaver
    {
        private class GameState
        {
            private static readonly string path = Application.persistentDataPath + "save.data";

            public readonly Player.State player;
            public readonly GameManager.State game;
            public readonly LaserTrigger.State laser;
            public readonly Ability.State projectile;
            public readonly List<ProjectileTrigger.ProjectileState> projectiles;
            public readonly Dictionary<string, List<SimpleEnemyBuilder.EnemyState>> enemies;

            public GameState(Player.State player, GameManager.State game, LaserTrigger.State laser, Ability.State projectile, List<ProjectileTrigger.ProjectileState> projectiles, Dictionary<string, List<SimpleEnemyBuilder.EnemyState>> enemies)
            {
                this.player = player;
                this.game = game;
                this.laser = laser;
                this.projectile = projectile;
                this.projectiles = projectiles;
                this.enemies = enemies;
            }

            public void SaveToFile()
            {
                FileStream fileStream = new FileStream(path, FileMode.Create);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(fileStream, this);
                fileStream.Flush();
                fileStream.Close();
                fileStream.Dispose();
            }

            public static bool HasSaveFile() => File.Exists(path);

            public static GameState ReadFile()
            {
                FileStream fileStream = new FileStream(path, FileMode.Open);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                GameState gameState = (GameState)binaryFormatter.Deserialize(fileStream);
                fileStream.Flush();
                fileStream.Close();
                fileStream.Dispose();
                return gameState;
            }
        }
    }
}