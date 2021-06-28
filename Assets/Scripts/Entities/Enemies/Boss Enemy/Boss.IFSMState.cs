namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        private interface IFSMState
        {
            void OnEntry();
            void OnExit();
            void OnUpdate();
        }
    }
}