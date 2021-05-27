using Asteroids.Entities.Enemies;
using Asteroids.Entities.Player;
using Asteroids.WeaponSystem;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Asteroids.Scene
{
    [DefaultExecutionOrder((int)ExecutionOrder.O1_GameSaver)]
    public sealed partial class GameSaver : MonoBehaviour
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
        private Action<ManualWeapon.State, List<ManualWeapon.ProjectileState>> loadProjectileTrigger;
        private List<Func<ManualWeapon.ProjectileState>> saveProjectileTriggerBullets = new List<Func<ManualWeapon.ProjectileState>>();

        private Dictionary<string, List<Func<SimpleEnemyBuilder.EnemyState>>> saveEnemies = new Dictionary<string, List<Func<SimpleEnemyBuilder.EnemyState>>>();
        private Dictionary<string, Action<List<SimpleEnemyBuilder.EnemyState>>> loadEnemyBuilder = new Dictionary<string, Action<List<SimpleEnemyBuilder.EnemyState>>>();

        private Func<BombWeapon.State> saveBombTrigger;
        private Action<BombWeapon.State, List<BombWeapon.Bomb.State>> loadBombTrigger;
        private List<Func<BombWeapon.Bomb.State>> saveBombsTrigger = new List<Func<BombWeapon.Bomb.State>>();

        //IA2-P1
        // ^- Don't touch that comment, used by the teacher
        private Action<List<(SimpleEnemyBuilder.EnemyState, Shooter.ShooterState, List<Shooter.ProjectileState>)>> loadEnemyShooter;
        private List<(Shooter, Func<SimpleEnemyBuilder.EnemyState>)> saveEnemyShooter = new List<(Shooter, Func<SimpleEnemyBuilder.EnemyState>)>();
        private List<(Shooter, Func<Shooter.ShooterState>)> saveEnemyShooterShooter = new List<(Shooter, Func<Shooter.ShooterState>)>();
        private Dictionary<Shooter, List<Func<Shooter.ProjectileState>>> saveShootersProjectiles = new Dictionary<Shooter, List<Func<Shooter.ProjectileState>>>();

        //IA2-P1
        // ^- Don't touch that comment, used by the teacher
        private Action<List<(SimpleEnemyBuilder.EnemyState, Bomber.BomberState, List<Bomber.ProjectileState>)>> loadEnemyBomber;
        private List<(Bomber, Func<SimpleEnemyBuilder.EnemyState>)> saveEnemyBomber = new List<(Bomber, Func<SimpleEnemyBuilder.EnemyState>)>();
        private List<(Bomber, Func<Bomber.BomberState>)> saveEnemyBomberBomber = new List<(Bomber, Func<Bomber.BomberState>)>();
        private Dictionary<Bomber, List<Func<Bomber.ProjectileState>>> saveBombersProjectiles = new Dictionary<Bomber, List<Func<Bomber.ProjectileState>>>();

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

            //IA2-P3
            // ^- Don't touch that comment, used by the teacher
#pragma warning disable RCS1077 // Optimize LINQ method call
            List<ManualWeapon.ProjectileState> projectiles = instance.saveProjectileTriggerBullets.Select(e => e()).ToList();
            Dictionary<string, List<SimpleEnemyBuilder.EnemyState>> enemies = instance.saveEnemies.ToDictionary(e => e.Key, e => e.Value.Select(e2 => e2()).ToList());
            List<BombWeapon.Bomb.State> bombs = instance.saveBombsTrigger.Select(e => e()).ToList();

            //IA2-P1
            // ^- Don't touch that comment, used by the teacher
            //IA2-P3
            // ^- Don't touch that comment, used by the teacher
            List<(SimpleEnemyBuilder.EnemyState, Shooter.ShooterState, List<Shooter.ProjectileState>)> shooters = instance.saveEnemyShooter
                .Select(e => (
                    e.Item2(),
                    instance.saveEnemyShooterShooter.First(e2 => e2.Item1 == e.Item1).Item2(),
                    // Note: This has a time complexity of O(n) instead of O(1) (.TryGetValue(TKey out TValue)) (apart from inherent allocations of LINQ). But TryGetValue is not a LINQ operator and the exercise requested LINQ.
                    (instance.saveShootersProjectiles.FirstOrDefault(e2 => e2.Key == e.Item1).Value ?? Enumerable.Empty<Func<Shooter.ProjectileState>>())
                    .Select(e2 => e2())
                    .ToList())
                )
                .ToList();

            //IA2-P1
            // ^- Don't touch that comment, used by the teacher
            //IA2-P3
            // ^- Don't touch that comment, used by the teacher
            List<(SimpleEnemyBuilder.EnemyState, Bomber.BomberState, List<Bomber.ProjectileState>)> bombers = instance.saveEnemyBomber
                .Select(e => (
                    e.Item2(),
                    instance.saveEnemyBomberBomber.First(e2 => e2.Item1 == e.Item1).Item2(),
                    // Note: This has a time complexity of O(n) instead of O(1) (.TryGetValue(TKey out TValue)) (apart from inherent allocations of LINQ). But TryGetValue is not a LINQ operator and the exercise requested LINQ.
                    (instance.saveBombersProjectiles.FirstOrDefault(e2 => e2.Key == e.Item1).Value ?? Enumerable.Empty<Func<Bomber.ProjectileState>>())
                    .Select(e2 => e2())
                    .ToList())
                )
                .ToList();
#pragma warning restore RCS1077

            GameState gameState = new GameState(player, game, laser, projectile, projectiles, bomb, bombs, enemies, shooters, bombers);
            gameState.SaveToFile();
        }

        public static bool HasSaveFile() => GameState.HasSaveFile();

        private void Load()
        {
            GameState gameState = GameState.ReadFile();
            instance.loadPlayer(gameState.player);
            instance.loadGameManager(gameState.game);
            instance.loadLaserTrigger(gameState.laser);
            instance.loadProjectileTrigger(gameState.projectile, gameState.projectiles);
            instance.loadBombTrigger(gameState.bomb, gameState.bombs);
            foreach (KeyValuePair<string, List<SimpleEnemyBuilder.EnemyState>> enemies in gameState.enemies)
                instance.loadEnemyBuilder[enemies.Key](enemies.Value);
            //IA2-P1
            // ^- Don't touch that comment, used by the teacher
            instance.loadEnemyShooter(gameState.shooterEnemies);
            instance.loadEnemyBomber(gameState.bomberEnemies);
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

        public static void SubscribeProjectileTrigger(Func<ManualWeapon.State> save, Action<ManualWeapon.State, List<ManualWeapon.ProjectileState>> load)
        {
            instance.saveProjectileTrigger = save;
            instance.loadProjectileTrigger = load;
        }

        public static void SubscribeBombTrigger(Func<BombWeapon.State> save, Action<BombWeapon.State, List<BombWeapon.Bomb.State>> load)
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

        //IA2-P1
        // ^- Don't touch that comment, used by the teacher
        public static void SubscribeShooterEnemy(Action<List<(SimpleEnemyBuilder.EnemyState, Shooter.ShooterState, List<Shooter.ProjectileState>)>> load)
            => instance.loadEnemyShooter = load;

        //IA2-P1
        // ^- Don't touch that comment, used by the teacher
        public static void SubscribeShooterEnemy(Shooter shooter, Func<SimpleEnemyBuilder.EnemyState> save)
            => instance.saveEnemyShooter.Add((shooter, save));

        //IA2-P1
        // ^- Don't touch that comment, used by the teacher
        public static void SubscribeShooterEnemy(Shooter shooter, Func<Shooter.ShooterState> save)
            => instance.saveEnemyShooterShooter.Add((shooter, save));

        //IA2-P1
        // ^- Don't touch that comment, used by the teacher
        public static void SubscribeShooterBullet(Shooter shooter, Func<Shooter.ProjectileState> save)
        {
            if (!instance.saveShootersProjectiles.TryGetValue(shooter, out List<Func<Shooter.ProjectileState>> list))
            {
                list = new List<Func<Shooter.ProjectileState>>();
                instance.saveShootersProjectiles.Add(shooter, list);
            }
            list.Add(save);
        }

        //IA2-P1
        // ^- Don't touch that comment, used by the teacher
        public static void SubscribeBomberEnemy(Action<List<(SimpleEnemyBuilder.EnemyState, Bomber.BomberState, List<Bomber.ProjectileState>)>> load)
            => instance.loadEnemyBomber = load;

        //IA2-P1
        // ^- Don't touch that comment, used by the teacher
        public static void SubscribeBomberEnemy(Bomber shooter, Func<SimpleEnemyBuilder.EnemyState> save)
            => instance.saveEnemyBomber.Add((shooter, save));

        //IA2-P1
        // ^- Don't touch that comment, used by the teacher
        public static void SubscribeBomberEnemy(Bomber shooter, Func<Bomber.BomberState> save)
            => instance.saveEnemyBomberBomber.Add((shooter, save));

        //IA2-P1
        // ^- Don't touch that comment, used by the teacher
        public static void SubscribeBomberBullet(Bomber shooter, Func<Bomber.ProjectileState> save)
        {
            if (!instance.saveBombersProjectiles.TryGetValue(shooter, out List<Func<Bomber.ProjectileState>> list))
            {
                list = new List<Func<Bomber.ProjectileState>>();
                instance.saveBombersProjectiles.Add(shooter, list);
            }
            list.Add(save);
        }

        public readonly struct LoadEvent { }
    }
}