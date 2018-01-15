namespace LogoFX.Client.Mvvm.Commanding
{
    public class DefaultCanExecuteManagerFactory : ICanExecuteManagerFactory
    {
        public ICanExecuteManager CreateCanExecuteManager()
        {
            return new DefaultCanExecuteManager();
        }
    }
}