using System;

namespace LogoFX.Client.Mvvm.Commanding
{
    public interface ICanExecuteManager
    {
        EventHandler CanExecuteHandler { get; }
        void AddHandler(EventHandler eventHandler);
        void RemoveHandler(EventHandler eventHandler);
    }
}
