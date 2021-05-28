using Asteroids.Scene;
using Asteroids.Utils;

using System.Collections.Generic;

using UnityEngine;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class SplitEnemyBuilder : IPool<GameObject, (Vector3 position, Vector3 speed)>
    {
        private static readonly BuilderFactoryPool<GameObject, SplitEnemyFlyweight, (Vector3 position, Vector3 speed)>.Initializer initialize = Initialize;
        private static readonly BuilderFactoryPool<GameObject, SplitEnemyFlyweight, (Vector3 position, Vector3 speed)>.Deinitializer deinitialize = Deinitialize;

        private readonly Dictionary<Sprite, string> reverseSprites = new Dictionary<Sprite, string>();
        private readonly BuilderFactoryPool<GameObject, SplitEnemyFlyweight, (Vector3 position, Vector3 speed)> builder;

        public SplitEnemyFlyweight Flyweight {
            get => builder.flyweight;
            set => builder.flyweight = value;
        }

        private string id;

        public SplitEnemyBuilder(string id)
        {
            builder = new BuilderFactoryPool<GameObject, SplitEnemyFlyweight, (Vector3 position, Vector3 speed)>
                {
                    constructor = Construct,
                    commonInitializer = CommonInitialize,
                    initializer = initialize,
                    deinitializer = deinitialize
                };

            this.id = id;

            GameSaver.SubscribeEnemy(
                id,
                (states) =>
                {
                    foreach (SimpleEnemyBuilder.EnemyState state in states)
                        state.Load(this, Create(default));
                }
            );
        }

        public GameObject Construct(in SplitEnemyFlyweight flyweight, in (Vector3 position, Vector3 speed) parameter)
        {
            GameObject enemy = SimpleEnemyBuilder.Construct(flyweight, parameter, this, id, reverseSprites);

            enemy.AddComponent<SplitOnDeath>().flyweight = flyweight;

            return enemy;
        }

        private static void Initialize(in SplitEnemyFlyweight flyweight, GameObject enemy, in (Vector3 position, Vector3 speed) parameter)
            => SimpleEnemyBuilder.Initialize(flyweight, enemy, parameter);

        private void CommonInitialize(in SplitEnemyFlyweight flyweight, GameObject enemy, in (Vector3 position, Vector3 speed) parameter)
            => SimpleEnemyBuilder.CommonInitialize(flyweight, enemy, parameter, reverseSprites);

        private static void Deinitialize(GameObject enemy)
            => SimpleEnemyBuilder.Deinitialize(enemy);

        public GameObject Create((Vector3 position, Vector3 speed) parameter) => builder.Create(parameter);

        public void Store(GameObject obj) => builder.Store(obj);

        public void ExtractIfHas(GameObject obj) => builder.ExtractIfHas(obj);
    }
}