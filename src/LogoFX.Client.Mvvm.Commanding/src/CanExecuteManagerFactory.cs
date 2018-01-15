namespace LogoFX.Client.Mvvm.Commanding
{
    public class CanExecuteManagerFactory<T> : ICanExecuteManagerFactory where T : ICanExecuteManager, new()
    {
        public ICanExecuteManager CreateCanExecuteManager()
        {
            return new T();
        }
    }
}