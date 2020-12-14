namespace Asteroids.Scene
{
    public partial class GlobalMementoManager
    {
        private interface IMementoManager
        {
            void Store();

            void StartRewind();

            // We don't use Coroutines because they produced an insane bottleneck,
            // this is extremely more performant and doesn't allocate
            void UpdateRewind(float deltaTime);
        }
    }
}