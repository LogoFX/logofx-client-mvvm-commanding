using System;
using System.Windows.Input;
using System.Globalization;
using System.Reflection;
using LogoFX.Client.Core;

#if NET45
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Markup;
using System.Windows.Media;
#endif

#if NETFX_CORE
using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
#endif

#if WINDOWS_APP
using LogoFX.Client.Mvvm.Commanding;
#endif

namespace LogoFX.Client.Mvvm.Commanding
{
    /// <summary>
    /// Provides way to call some named command with notion of visal tree routing
    /// Will search for propety of type <see cref="ICommand"/> with name />
    /// </summary>
#if NET45
    [ContentProperty("Parameter")]
#else
    [ContentProperty(Name = "Parameter")]
#endif
    public class ExecuteNamedCommandAction : TriggerAction<FrameworkElement>
    {
        #region Fields

#if NETFX_CORE
        // ReSharper disable once InconsistentNaming
        private const double INTERACTIVITY_ENABLED = 1d;
        // ReSharper disable once InconsistentNaming
        private const double INTERACTIVITY_DISABLED = 0.5d;
#endif

        private bool _manageEnableState = true;

        #endregion

        #region Events

        /// <summary>
        /// Occurs before the message detaches from the associated object.
        /// </summary>
        public event EventHandler Detaching = delegate { };

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty UseTriggerParameterProperty =
            DependencyProperty.Register(
                "UseTriggerParameter",
                typeof(bool),
                typeof(ExecuteNamedCommandAction),
                new PropertyMetadata(false));

        public bool UseTriggerParameter
        {
            get { return (bool) GetValue(UseTriggerParameterProperty); }
            set { SetValue(UseTriggerParameterProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(ExecuteNamedCommandAction),
                new PropertyMetadata(
                    null,
                    OnCommandChanged));

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExecuteNamedCommandAction executeNamedCommandAction = (ExecuteNamedCommandAction) d;

            if (e.NewValue != null || string.IsNullOrEmpty(executeNamedCommandAction.CommandName))
            {
                executeNamedCommandAction.InternalCommand = e.NewValue as ICommand;
            }
        }

#if NET45
        [CustomPropertyValueEditor(CustomPropertyValueEditor.PropertyBinding)]
#endif
        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// Represents the method name of an action message.
        /// </summary>
        public static readonly DependencyProperty CommandNameProperty =
            DependencyProperty.Register(
                "CommandName",
                typeof(string),
                typeof(ExecuteNamedCommandAction),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the name of the command to be invoked on the presentation model class.
        /// </summary>
        /// <value>The name of the method.</value>
        public string CommandName
        {
            get { return (string) GetValue(CommandNameProperty); }
            set { SetValue(CommandNameProperty, value); }
        }

        /// <summary>
        /// Represents the parameters of an action message.
        /// </summary>
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register(
                "Parameter",
                typeof(object),
                typeof(ExecuteNamedCommandAction),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the parameters to pass as part of the method invocation.
        /// </summary>
        /// <value>The parameters.</value>
        public object Parameter
        {
            get { return GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }

        public static readonly DependencyProperty TriggerParameterConverterProperty =
            DependencyProperty.Register(
                "TriggerParameterConverter",
                typeof(IValueConverter),
                typeof(ExecuteNamedCommandAction),
                new PropertyMetadata(null));

        public IValueConverter TriggerParameterConverter
        {
            get { return (IValueConverter) GetValue(TriggerParameterConverterProperty); }
            set { SetValue(TriggerParameterConverterProperty, value); }
        }

        #endregion

        #region Public Properties

        public bool ManageEnableState
        {
            get { return _manageEnableState; }
            set { _manageEnableState = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Forces an update of the UI's Enabled/Disabled state based on the the preconditions associated with the method.
        /// </summary>
        public void UpdateAvailability()
        {
            if (AssociatedObject == null)
            {
                return;
            }

            if (InternalCommand == null)
            {
                SetPropertyBinding();
            }

            UpdateAvailabilityCore();
        }

        #endregion

        #region Private Members

        private ICommand _internalCommand;

        private ICommand InternalCommand
        {
            get { return _internalCommand; }
            set
            {
                if (_internalCommand != null)
                {
                    _internalCommand.CanExecuteChanged -= CanexecuteChanged;
                }

                _internalCommand = value;
                if (_internalCommand != null)
                {
                    _internalCommand.CanExecuteChanged += CanexecuteChanged;
                }
            }
        }

        private void CanexecuteChanged(object sender, EventArgs e)
        {
            UpdateAvailabilityCore();
        }

        private void ElementLoaded(object sender, RoutedEventArgs e)
        {
            SetPropertyBinding();
            UpdateAvailabilityCore();
        }

        private bool UpdateAvailabilityCore()
        {
            return !ManageEnableState || ApplyAvailabilityEffect();
        }

        /// <summary>
        /// Applies an availability effect, such as IsEnabled, to an element.
        /// </summary>
        /// <remarks>Returns a value indicating whether or not the action is available.</remarks>
        private bool ApplyAvailabilityEffect()
        {

            if (AssociatedObject == null || InternalCommand == null) return false;

            // we get if it is enabled or not
            bool canExecute = InternalCommand.CanExecute(Parameter);

            // we check if it is a control in SL
#if NETFX_CORE
            var control = AssociatedObject as Control;
            if (control != null)
            {
                var target = control;
                target.IsEnabled = canExecute;
            }
            else
            {
                AssociatedObject.IsHitTestVisible = canExecute;
                AssociatedObject.Opacity = canExecute ? INTERACTIVITY_ENABLED : INTERACTIVITY_DISABLED;
            }
#else
            AssociatedObject.IsEnabled = canExecute;
#endif
            return canExecute;
        }

        /// <summary>
        /// Sets the target, method and view on the context. Uses a bubbling strategy by default.
        /// </summary>
        private void SetPropertyBinding()
        {
            if (string.IsNullOrWhiteSpace(CommandName))
            {
                return;
            }

            DependencyObject currentElement = AssociatedObject;
            PropertyInfo commandProperty = null;
            object currentTarget;
            InternalCommand = null;

            while (currentElement != null && InternalCommand == null)
            {
                currentTarget = currentElement.GetValue(FrameworkElement.DataContextProperty);

                if (currentTarget != null)
                {
#if WINDOWS_APP
                    commandProperty = currentTarget.GetType().GetRuntimeProperty(CommandName);
#else
                    commandProperty = currentTarget.GetType().GetProperty(CommandName);
#endif
                }

                //is have readable property derived from icommand
                if (commandProperty == null ||
                    !commandProperty.CanRead ||
                    !typeof(ICommand).IsAssignableFrom(commandProperty.PropertyType))
                {
                    DependencyObject temp;
#if NET45
                    if (currentElement is ContextMenu)
                    {
                        ContextMenu cm = currentElement as ContextMenu;
                        temp = cm.PlacementTarget;
                    }
                    else
#endif
                    {
                        temp = VisualTreeHelper.GetParent(currentElement);
                    }

                    if (temp == null)
                    {
                        FrameworkElement element = currentElement as FrameworkElement;
                        if (element?.Parent == null)
                        {
                            currentElement = CommonProperties.GetOwner(currentElement) as FrameworkElement;
                        }
                        else
                        {
                            currentElement = element.Parent;
                        }
                    }
                    else
                    {
                        currentElement = temp;
                    }
                }

                else
                {
                    InternalCommand = (ICommand) commandProperty.GetValue(currentTarget, null);
                }
            }

            if (InternalCommand != null)
                return;

            //check associated object itself
            currentTarget = AssociatedObject;

            if (currentTarget != null)
            {
#if WINDOWS_APP
                commandProperty = currentTarget.GetType().GetRuntimeProperty(CommandName);
#else
                commandProperty = currentTarget.GetType().GetProperty(CommandName, BindingFlags.FlattenHierarchy);
#endif
            }

            //is have readable property derived from icommand
            if (commandProperty == null ||
                !commandProperty.CanRead ||
                !typeof(ICommand).IsAssignableFrom(commandProperty.PropertyType))

            {
                return;
            }

            InternalCommand = (ICommand) commandProperty.GetValue(currentTarget, null);
        }

        #endregion

        #region Overrides

        protected override void OnAttached()
        {
            ElementLoaded(null, null);
            AssociatedObject.Loaded += ElementLoaded;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            Detaching(this, EventArgs.Empty);
            AssociatedObject.Loaded -= ElementLoaded;
            if (_internalCommand != null)
            {
                _internalCommand.CanExecuteChanged -= CanexecuteChanged;
            }
            base.OnDetaching();
        }

        protected override void Invoke(object arg)
        {


            if (InternalCommand == null)
            {
                SetPropertyBinding();
                if (!UpdateAvailabilityCore())
                {
                    return;
                }
            }

#if NET45
            var lang = CultureInfo.CurrentCulture;
#else
            var lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
#endif
            // if a trigger parameter converter is specified, then we use that to get the command parameter
            // else we use the given parameter - note_ the parameter can be null
            var parameter = TriggerParameterConverter != null
                ? TriggerParameterConverter.Convert(arg, typeof(object), AssociatedObject, lang)
                : Parameter;

            if (parameter == null && UseTriggerParameter)
            {
                parameter = arg;
            }

            if (InternalCommand != null && InternalCommand.CanExecute(parameter))
            {
                InternalCommand.Execute(parameter);
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return "NamedCommndAction: " + CommandName;
        }

        #endregion
    }
}
