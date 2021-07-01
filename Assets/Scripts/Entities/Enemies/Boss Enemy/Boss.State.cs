using Asteroids.Scene;

using System;
using System.Collections.Generic;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        [Serializable]
        public readonly struct State
        {
            // This class is in charge of storing and setting a boss state to save the game.
            // We don't save GOAP plan because it can be recalculated, so we only save last used action.

            private readonly int lifes;
            private readonly int state;
            private readonly SerializableVector2 position;
            private readonly float rotation;

            public bool IsAlive => lifes > 0;

            public State(Boss boss)
            {
                lifes = boss.currentLifes;
                state = Array.IndexOf(boss.actions, boss.machine.State);
                position = boss.rigidbody.position;
                rotation = boss.rigidbody.rotation;
            }

            public void Load(Boss boss, List<BossShooter.ProjectileState> bossBullets)
            {
                boss.currentLifes = lifes;
                boss.machine.Fire(boss.actions[state]);
                boss.rigidbody.position = position;
                boss.rigidbody.rotation = rotation;
                boss.bossShooter.Load(bossBullets);
            }
        }
    }
}