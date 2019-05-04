using Solid.Bootstrapping;
using Solid.Extensibility;
using Solid.Practices.Middleware;

namespace LogoFX.Client.Mvvm.Commanding
{
    /// <summary>
    /// Bootstrapping extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Uses the platform-specific <see cref="CanExecuteManager"/>.
        /// </summary>        
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <typeparam name="TBootstrapper">The type of the bootstrapper.</typeparam>        
        public static TBootstrapper UseCommanding<TBootstrapper>(
            this TBootstrapper bootstrapper)
            where TBootstrapper : class, IExtensible<TBootstrapper>, IHaveRegistrator            
        {
            return bootstrapper.Use(new InitializeCanExecuteManagerMiddleware<TBootstrapper>());
        }
    }

    /// <summary>
    /// This middleware initializes the <see cref="ICanExecuteManager"/> factory
    /// </summary>
    /// <typeparam name="TBootstrapper">The type of the bootstrapper.</typeparam>
    public class InitializeCanExecuteManagerMiddleware<TBootstrapper> : IMiddleware<TBootstrapper>
        where TBootstrapper : class, IHaveRegistrator
    {
        /// <inheritdoc/>        
        public TBootstrapper Apply(TBootstrapper @object)
        {
            CanExecuteManagerFactoryContext.Current = new CanExecuteManagerFactory<CanExecuteManager>();
            return @object;
        }
    }
}
