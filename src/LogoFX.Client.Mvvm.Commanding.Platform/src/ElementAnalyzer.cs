#if NET || NETCORE || NETFRAMEWORK
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Markup;
using System.Windows.Media;
#endif

#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
#endif

namespace LogoFX.Client.Mvvm.Commanding
{
    class ElementAnalyzer
    {
        public string CommandName { get; private set;}

        public ElementAnalyzer(string commandName)
        {
            CommandName = commandName;
        }

        internal ElementAnalysisResult Analyze(DependencyObject commandTargetElement)
        {
            PropertyInfo commandProperty;
            var commandTargetDataContext = commandTargetElement.GetValue(FrameworkElement.DataContextProperty);
            if (commandTargetDataContext != null)
            {
                commandProperty = GetCommandProperty(commandTargetDataContext);
            }
            var canUseCommand = CanUseCommandProperty(commandProperty);
            if (!canUseCommand)
            {
                var nextElement = GetNextCommandTargetElement(commandTargetElement);    
                return new ElementAnalysisResult(nextElement);          
            }
            else
            {
                var command = (ICommand)commandProperty.GetValue(commandTargetDataContext, null);  
                return new ElementAnalysisResult(command)              ;
            }            
        }

        private static bool CanUseCommandProperty(PropertyInfo commandProperty)
        {
            return  commandProperty != null &&
                    commandProperty.CanRead &&
                    typeof(ICommand).IsAssignableFrom(commandProperty.PropertyType);
        }
        private PropertyInfo GetCommandProperty(object commandTargetDataContext)
        {
            PropertyInfo commandProperty;
#if WINDOWS_APP || WINDOWS_PHONE_APP
            commandProperty = commandTargetDataContext.GetType().GetRuntimeProperty(CommandName);
#else
            commandProperty = commandTargetDataContext.GetType().GetProperty(CommandName);
#endif
            return commandProperty;
        }

        private DependencyObject GetNextCommandTargetElement(DependencyObject currentCommandTargetElement)
        {
            DependencyObject temp;
#if NET || NETCORE || NETFRAMEWORK
            if (commandTargetElement is ContextMenu)
            {
                ContextMenu cm = commandTargetElement as ContextMenu;
                temp = cm.PlacementTarget;
            }
            else
#endif
            {
                temp = VisualTreeHelper.GetParent(commandTargetElement);
            }
            if (temp == null)
            {
                FrameworkElement element = commandTargetElement as FrameworkElement;
                if (element?.Parent == null)
                {
                    commandTargetElement = CommonProperties.GetOwner(commandTargetElement) as FrameworkElement;
                }
                else
                {
                    commandTargetElement = element.Parent;
                }
            }
            else
            {
                commandTargetElement = temp;
            }
        }             
    }
}