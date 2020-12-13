using Asteroids.WeaponSystem;
using Asteroids.Entities.Enemies;
using Asteroids.Entities.Player;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

namespace Asteroids.Scene
{
    public partial class GameSaver
    {
        [Serializable]
        private class GameState
        {
            private static readonly string path = Application.persistentDataPath;
            private static readonly string pathAndName = path + "/save.sav";

            public readonly PlayerController.State player;
            public readonly GameManager.State game;
            public readonly LaserWeapon.State laser;
            public readonly Weapon.State projectile;
            public readonly List<ManualWeapon.ProjectileState> projectiles;
            public readonly BombWeapon.State bomb;
            public readonly List<BombWeapon.Bomb.State> bombs;
            public readonly Dictionary<string, List<SimpleEnemyBuilder.EnemyState>> enemies;

            public GameState(PlayerController.State player, GameManager.State game, LaserWeapon.State laser, Weapon.State projectile, List<ManualWeapon.ProjectileState> projectiles, BombWeapon.State bomb, List<BombWeapon.Bomb.State> bombs, Dictionary<string, List<SimpleEnemyBuilder.EnemyState>> enemies)
            {
                this.player = player;
                this.game = game;
                this.laser = laser;
                this.projectile = projectile;
                this.projectiles = projectiles;
                this.enemies = enemies;
                this.bomb = bomb;
                this.bombs = bombs;
            }

            public void SaveToFile()
            {
                Directory.CreateDirectory(path);
                FileStream fileStream = new FileStream(pathAndName, FileMode.Create);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(fileStream, this);
                fileStream.Flush();
                fileStream.Close();
                fileStream.Dispose();
            }

            public static bool HasSaveFile()
            {
                Directory.CreateDirectory(path);
                return File.Exists(pathAndName);
            }

            public static GameState ReadFile()
            {
                Directory.CreateDirectory(path);
                FileStream fileStream = new FileStream(pathAndName, FileMode.Open);
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