namespace AvalonStudios.Additions.Utils.Interfaces
{
    public interface IEvents<in T>
    {
        void Subscribe(T listener);

        void Unsubscribe(T listener);
    }
}
