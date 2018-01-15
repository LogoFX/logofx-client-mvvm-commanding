using System;

namespace LogoFX.Client.Mvvm.Commanding
{
    public class DefaultCanExecuteManager : ICanExecuteManager
    {
        public EventHandler CanExecuteHandler { get; private set; }

        public void AddHandler(EventHandler eventHandler)
        {
            CanExecuteHandler += eventHandler;
        }

        public void RemoveHandler(EventHandler eventHandler)
        {
            CanExecuteHandler -= eventHandler;
        }
    }
}