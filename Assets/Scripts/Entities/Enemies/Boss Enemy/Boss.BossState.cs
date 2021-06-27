using Asteroids.Entities.Player;
using Asteroids.PowerUps;

using Enderlook.GOAP;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        public struct BossState : IWorldState<BossState>
        {
            public Vector3 PlayerPosition;
            public Vector3 BossPosition;
            public int BossHealth;
            public int PlayerHealth;
            public int PowerUps;
            public float TimeSinceLastPowerUpWasSpawned;

            public void AdvanceTime(float time)
            {
                TimeSinceLastPowerUpWasSpawned += time;
                if (time > PowerUpManager.SpawnTime)
                {
                    TimeSinceLastPowerUpWasSpawned -= PowerUpManager.SpawnTime;
                    PowerUps++;
                }
            }

            public BossState(Boss boss)
                : this(
                      boss.transform.position, boss.currentLifes,
                      PlayerController.Position, PlayerController.Lifes,
                      PowerUpManager.PowerUpsInScene, PowerUpManager.TimeSinceLastSpawnedPowerUp
                ) { }

            private BossState(Vector3 bossPosition, int bossHealth, Vector3 playerPosition, int playerHealth, int powerUps, float timeSinceLastPowerUpWasSpawned)
            {
                PlayerPosition = playerPosition;
                BossPosition = bossPosition;
                BossHealth = bossHealth;
                PlayerHealth = playerHealth;
                PowerUps = powerUps;
                TimeSinceLastPowerUpWasSpawned = timeSinceLastPowerUpWasSpawned;
            }

            public override string ToString() => $"{{Pl: ({PlayerHealth} {PlayerPosition}) Bo: ({BossHealth} {BossPosition}) Pu: ({PowerUps} {TimeSinceLastPowerUpWasSpawned})}}";

            BossState IWorldState<BossState>.Clone() => new BossState(BossPosition, BossHealth, PlayerPosition, PlayerHealth, PowerUps, TimeSinceLastPowerUpWasSpawned);
        }
    }
}