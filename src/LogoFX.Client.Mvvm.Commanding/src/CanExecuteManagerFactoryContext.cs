namespace LogoFX.Client.Mvvm.Commanding
{
    public static class CanExecuteManagerFactoryContext
    {
        private static ICanExecuteManagerFactory _canExecuteManagerFactory = new DefaultCanExecuteManagerFactory();

        public static ICanExecuteManagerFactory Current
        {
            get => _canExecuteManagerFactory;
            set => _canExecuteManagerFactory = value;
        }
    }
}