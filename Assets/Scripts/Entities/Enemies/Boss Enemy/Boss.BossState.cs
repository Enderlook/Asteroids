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

            private Boss flyweight;

            public bool IsBossTooHurt => (BossHealth / (float)flyweight.lifes) > flyweight.tooHurtFactor;

            public float BossMovementSpeed => flyweight.movementSpeed;

            public int BossMaxHealth => flyweight.lifes;

            public float AverageTimeToSpawnPickup => flyweight.powerUpManager.SpawnTime;

            public void AdvanceTime(float time)
            {
                TimeSinceLastPowerUpWasSpawned += time;
                if (time > flyweight.powerUpManager.SpawnTime)
                {
                    TimeSinceLastPowerUpWasSpawned -= flyweight.powerUpManager.SpawnTime;
                    PowerUps++;
                }
            }

            //public BossState(Boss boss) : this(boss, PlayerController.Position, boss.transform.position, boss.lifes, PlayerController.Lifes, Object.FindObjectOfType<IPickup>(), ) { }

            private BossState(Boss flyweight, Vector3 playerPosition, Vector3 bossPosition, int bossHealth, int playerHealth, int powerUps, float timeSinceLastHealthPackWasSpawned)
            {
                this.flyweight = flyweight;
                PlayerPosition = playerPosition;
                BossPosition = bossPosition;
                BossHealth = bossHealth;
                PlayerHealth = playerHealth;
                PowerUps = powerUps;
                TimeSinceLastPowerUpWasSpawned = timeSinceLastHealthPackWasSpawned;
            }

            public override string ToString() => $"{{P: ({PlayerHealth} {PlayerPosition}) B: ({BossHealth} {BossPosition}) P: ({PowerUps} {TimeSinceLastPowerUpWasSpawned})}}";

            BossState IWorldState<BossState>.Clone() => new BossState(flyweight, PlayerPosition, BossPosition, BossHealth, PlayerHealth, PowerUps, TimeSinceLastPowerUpWasSpawned);
        }
    }
}