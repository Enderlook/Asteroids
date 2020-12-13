using Asteroids.WeaponSystem;
using Asteroids.Entities.Enemies;
using Asteroids.Entities.Player;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Asteroids.Scene
{
    [DefaultExecutionOrder((int)ExecutionOrder.O1_GameSaver)]
    public partial class GameSaver : MonoBehaviour
    {
        public static bool requestLoad;

        private static GameSaver instance;

        private Func<PlayerController.State> savePlayer;
        private Action<PlayerController.State> loadPlayer;

        private Func<GameManager.State> saveGameManager;
        private Action<GameManager.State> loadGameManager;

        private Func<LaserWeapon.State> saveLaserTrigger;
        private Action<LaserWeapon.State> loadLaserTrigger;

        private Func<ManualWeapon.State> saveProjectileTrigger;
        private Action<(ManualWeapon.State, List<ManualWeapon.ProjectileState>)> loadProjectileTrigger;
        private List<Func<ManualWeapon.ProjectileState>> saveProjectileTriggerBullets = new List<Func<ManualWeapon.ProjectileState>>();

        private Dictionary<string, List<Func<SimpleEnemyBuilder.EnemyState>>> saveEnemies = new Dictionary<string, List<Func<SimpleEnemyBuilder.EnemyState>>>();
        private Dictionary<string, Action<List<SimpleEnemyBuilder.EnemyState>>> loadEnemyBuilder = new Dictionary<string, Action<List<SimpleEnemyBuilder.EnemyState>>>();
        
        private Func<BombWeapon.State> saveBombTrigger;
        private Action<(BombWeapon.State, List<BombWeapon.Bomb.State>)> loadBombTrigger;
        private List<Func<BombWeapon.Bomb.State>> saveBombsTrigger = new List<Func<BombWeapon.Bomb.State>>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Debug.LogError($"Can only have one instance of {nameof(GameSaver)}.");
                Destroy(this);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Start()
        {
            // We can't load during Awake because we don't have registered loaders yet

            if (requestLoad)
            {
                requestLoad = false;
                Load();
            }
        }

        public void Save()
        {
            // We don't save power ups because that would require also saving memento states
            // That would make the saving file bigger, not sure if it is worthly.

            PlayerController.State player = instance.savePlayer();
            GameManager.State game = instance.saveGameManager();
            LaserWeapon.State laser = instance.saveLaserTrigger();
            Weapon.State projectile = instance.saveProjectileTrigger();
            BombWeapon.State bomb = instance.saveBombTrigger();

            List<ManualWeapon.ProjectileState> projectiles = new List<ManualWeapon.ProjectileState>(instance.saveProjectileTriggerBullets.Count);
            foreach (Func<ManualWeapon.ProjectileState> save in instance.saveProjectileTriggerBullets)
                projectiles.Add(save());

            Dictionary<string, List<SimpleEnemyBuilder.EnemyState>> enemies = new Dictionary<string, List<SimpleEnemyBuilder.EnemyState>>(instance.saveEnemies.Count);
            foreach (KeyValuePair<string, List<Func<SimpleEnemyBuilder.EnemyState>>> save in instance.saveEnemies)
            {
                List<SimpleEnemyBuilder.EnemyState> enemies_ = new List<SimpleEnemyBuilder.EnemyState>(save.Value.Count);
                enemies.Add(save.Key, enemies_);

                foreach (Func<SimpleEnemyBuilder.EnemyState> enemy in save.Value)
                    enemies_.Add(enemy());
            }

            List<BombWeapon.Bomb.State> bombs = new List<BombWeapon.Bomb.State>(instance.saveBombsTrigger.Count);
            foreach (Func<BombWeapon.Bomb.State> save in instance.saveBombsTrigger)
                bombs.Add(save());

            GameState gameState = new GameState(player, game, laser, projectile, projectiles, bomb, bombs, enemies);
            gameState.SaveToFile();
        }

        public static bool HasSaveFile() => GameState.HasSaveFile();

        private void Load()
        {
            GameState gameState = GameState.ReadFile();
            instance.loadPlayer(gameState.player);
            instance.loadGameManager(gameState.game);
            instance.loadLaserTrigger(gameState.laser);
            instance.loadProjectileTrigger((gameState.projectile, gameState.projectiles));
            instance.loadBombTrigger((gameState.bomb, gameState.bombs));
            foreach (KeyValuePair<string, List<SimpleEnemyBuilder.EnemyState>> enemies in gameState.enemies)
                instance.loadEnemyBuilder[enemies.Key](enemies.Value);

            EventManager.Raise(new LoadEvent());
        }

        public static void SubscribePlayer(Func<PlayerController.State> save, Action<PlayerController.State> load)
        {
            instance.savePlayer = save;
            instance.loadPlayer = load;
        }

        public static void SubscribeGameManager(Func<GameManager.State> save, Action<GameManager.State> load)
        {
            instance.saveGameManager = save;
            instance.loadGameManager = load;
        }

        public static void SubscribeLaserTrigger(Func<LaserWeapon.State> save, Action<LaserWeapon.State> load)
        {
            instance.saveLaserTrigger = save;
            instance.loadLaserTrigger = load;
        }

        public static void SubscribeProjectileTrigger(Func<ManualWeapon.State> save, Action<(ManualWeapon.State, List<ManualWeapon.ProjectileState>)> load)
        {
            instance.saveProjectileTrigger = save;
            instance.loadProjectileTrigger = load;
        }

        public static void SubscribeBombTrigger(Func<BombWeapon.State> save, Action<(BombWeapon.State, List<BombWeapon.Bomb.State>)> load)
        {
            instance.saveBombTrigger = save;
            instance.loadBombTrigger = load;
        }

        public static void SubscribeProjectileTriggerBullet(Func<ManualWeapon.ProjectileState> save)
            => instance.saveProjectileTriggerBullets.Add(save);

        public static void SubscribeBombsTrigger(Func<BombWeapon.Bomb.State> save)
            => instance.saveBombsTrigger.Add(save);

        public static void SubscribeEnemy(string id, Action<List<SimpleEnemyBuilder.EnemyState>> load)
            => instance.loadEnemyBuilder.Add(id, load);

        public static void SubscribeEnemy(string id, Func<SimpleEnemyBuilder.EnemyState> save)
        {
            if (!instance.saveEnemies.TryGetValue(id, out List<Func<SimpleEnemyBuilder.EnemyState>> list))
            {
                list = new List<Func<SimpleEnemyBuilder.EnemyState>>();
                instance.saveEnemies.Add(id, list);
            }
            list.Add(save);
        }

        public readonly struct LoadEvent { }
    }
}