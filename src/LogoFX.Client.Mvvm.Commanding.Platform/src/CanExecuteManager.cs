using System;
#if NET
using System.Windows.Input;
#endif

namespace LogoFX.Client.Mvvm.Commanding
{
    class CanExecuteManager : ICanExecuteManager
    {
        public EventHandler CanExecuteHandler { get; private set; }

#if WINDOWS_UWP || NETFX_CORE

        public void AddHandler(EventHandler eventHandler)
        {            
            CanExecuteHandler += eventHandler;
        }

        public void RemoveHandler(EventHandler eventHandler)
        {            
            CanExecuteHandler -= eventHandler;
        }
#endif

#if NET
        public void AddHandler(EventHandler eventHandler)
        {
            CommandManager.RequerySuggested += eventHandler;
            CanExecuteHandler += eventHandler;
        }

        public void RemoveHandler(EventHandler eventHandler)
        {
            CommandManager.RequerySuggested -= eventHandler;
            CanExecuteHandler -= eventHandler;
        }
#endif
    }    
}
