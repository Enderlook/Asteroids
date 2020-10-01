using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public class EnemyGeneratorSimple : EnemyGeneratorSimple<EnemyGeneratorSimple, EnemyGeneratorSimple.Handler>
    {
        protected override Handler Constructor((Vector2 position, Vector2 speed) arguments) => ConstructorBase(this, arguments);

        public new class Handler : EnemyGeneratorSimple<EnemyGeneratorSimple, Handler>.Handler
        {
            public override void ReturnToPool() => StoreInPool(this);
        }
    }
}