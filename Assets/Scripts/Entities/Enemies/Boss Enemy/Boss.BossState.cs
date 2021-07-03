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
            // The final exam request to have an string/enum as world state,
            // but since in our case it doesn't make sense (we don't use them in any part)
            // the teacher allowed us to use a Vector3 instead as a replacement.

            public Vector3 PlayerPosition;
            public Vector3 BossPosition;
            public int BossHealth;
            public int PlayerHealth;
            public bool HasPowerUpInScene; // An int would be better to track amount of power ups, but the final exam request to have at least one bool.
            public float TimeSinceLastPowerUpWasSpawned;

            public void AdvanceTime(float time)
            {
                TimeSinceLastPowerUpWasSpawned += time;
                if (time > PowerUpManager.SpawnTime)
                {
                    TimeSinceLastPowerUpWasSpawned -= PowerUpManager.SpawnTime;
                    HasPowerUpInScene = true;
                }
            }

            public BossState(Boss boss)
                : this(
                      boss.transform.position, boss.currentLifes,
                      PlayerController.Position, PlayerController.Lifes,
                      PowerUpManager.PowerUpsInScene > 0, PowerUpManager.TimeSinceLastSpawnedPowerUp
                ) { }

            private BossState(Vector3 bossPosition, int bossHealth, Vector3 playerPosition, int playerHealth, bool hasPowerUpInScene, float timeSinceLastPowerUpWasSpawned)
            {
                PlayerPosition = playerPosition;
                BossPosition = bossPosition;
                BossHealth = bossHealth;
                PlayerHealth = playerHealth;
                HasPowerUpInScene = hasPowerUpInScene;
                TimeSinceLastPowerUpWasSpawned = timeSinceLastPowerUpWasSpawned;
            }

            public override string ToString() => $"{{Pl: ({PlayerHealth} {PlayerPosition}) Bo: ({BossHealth} {BossPosition}) Pu: ({HasPowerUpInScene} {TimeSinceLastPowerUpWasSpawned}) D: {Vector3.Distance(PlayerPosition, BossPosition)}}}";

            BossState IWorldState<BossState>.Clone() => this;
        }
    }
}