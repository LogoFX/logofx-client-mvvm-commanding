using System.Windows.Input;
using System.Globalization;

#if NET45
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Interactivity;
#endif

#if WINDOWS_UWP
using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
#endif

namespace LogoFX.Client.Mvvm.View.Interactivity.Actions
{
#if NET45
    [ContentProperty("Parameter")]
#else
    [ContentProperty(Name = "Parameter")]
#endif
    public class ExecuteCommandAction
#if NET45
        : TriggerAction<UIElement>
#elif WINDOWS_UWP
        : DependencyObject, IAction
#elif WINDOWS_PHONE_APP
        : BindableTriggerAction<FrameworkElement>
#endif
    {
        #region Fields

        private const double INTERACTIVITY_ENABLED = 1d;
        private const double INTERACTIVITY_DISABLED = 0.5d;

        #endregion

        #region Dependency Properties

        /// <summary>
        /// The command property
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(ExecuteCommandAction),
                new PropertyMetadata(
                    null,
                    OnCommandChanged));

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ExecuteCommandAction) d).SetupEnableState(e.NewValue as ICommand, e.OldValue as ICommand);
        }

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
#if NET45 || WINDOWS_UWP
        [CustomPropertyValueEditor(CustomPropertyValueEditor.PropertyBinding)]
#endif
            public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// The parameter property
        /// </summary>
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register(
                "Parameter",
                typeof(object),
                typeof(ExecuteCommandAction),
                new PropertyMetadata(
                    null,
                    OnParameterChanged));

        private static void OnParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ExecuteCommandAction) d).UpdateEnabledState();
        }

        /// <summary>
        /// Gets or sets the parameter.
        /// </summary>
#if WINDOWS_PHONE_APP
        [TypeConverter(typeof(nRoute.Components.TypeConverters.ConvertFromStringConverter))]
#endif
#if NET45 || WINDOWS_UWP
        [CustomPropertyValueEditor(CustomPropertyValueEditor.PropertyBinding)]
#endif
            public object Parameter
        {
            get { return GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }

        /// <summary>
        /// The trigger parameter converter property
        /// </summary>
        public static readonly DependencyProperty TriggerParameterConverterProperty =
            DependencyProperty.Register(
                "TriggerParameterConverter",
                typeof(IValueConverter),
                typeof(ExecuteCommandAction),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the trigger parameter converter.
        /// </summary>
#if NET45 || WINDOWS_UWP
        [CustomPropertyValueEditor(CustomPropertyValueEditor.PropertyBinding)]
#endif
            public IValueConverter TriggerParameterConverter
        {
            get { return (IValueConverter) GetValue(TriggerParameterConverterProperty); }
            set { SetValue(TriggerParameterConverterProperty, value); }
        }


        /// <summary>
        /// The use trigger parameter property
        /// </summary>
        public static readonly DependencyProperty UseTriggerParameterProperty =
            DependencyProperty.Register(
                "UseTriggerParameter",
                typeof(bool),
                typeof(ExecuteCommandAction),
                new PropertyMetadata(false));


        /// <summary>
        /// Gets or sets a value indicating whether to use trigger parameter.
        /// </summary>
        /// <value>
        ///   <c>true</c> if trigger parameter is used; otherwise, <c>false</c>.
        /// </value>
        public bool UseTriggerParameter
        {
            get { return (bool) GetValue(UseTriggerParameterProperty); }
            set { SetValue(UseTriggerParameterProperty, value); }
        }

        #endregion

        #region Public Properties

#if NET45

        private bool _manageEnableState = true;

        public bool ManageEnableState
        {
            get { return _manageEnableState; }
            set { _manageEnableState = value; }
        }

#endif


        #endregion

        #region Overrides

#if NET45

        protected override void OnAttached()
        {
            base.OnAttached();
            UpdateEnabledState();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            DisposeEnableState();
        }

#endif

#if NET45
        protected override void Invoke(object arg)
#elif WINDOWS_UWP
        public object Execute(object sender, object arg)
#endif
        {
            if (Command == null)
            {
                return
#if WINDOWS_UWP
                    false
#endif
                    ;
            }

            // if a trigger parameter converter is specified, then we use that to get the command parameter
            // else we use the given parameter - note_ the parameter can be null
            var parameter = TriggerParameterConverter != null
                ? TriggerParameterConverter.Convert(arg, typeof(object), Parameter, CultureInfo.CurrentCulture
#if WINDOWS_UWP
                    .Name
#endif
                    )
                : Parameter;

            if (parameter == null && UseTriggerParameter)
            {
                parameter = arg;
            }

            if (!Command.CanExecute(parameter))
            {
                return
#if WINDOWS_UWP
                    false
#endif
                    ;
            }

            Command.Execute(parameter);
#if WINDOWS_UWP
            return true;
#endif
        }

        #endregion

        #region Private Members

#if WINDOWS_UWP

        private void SetupEnableState(ICommand newCommand, ICommand oldCommand)
        {

        }

        private void UpdateEnabledState()
        {

        }
#endif

#if NET45

        private void Command_CanExecuteChanged(object sender, EventArgs e)
        {
            UpdateEnabledState();
        }

        private void SetupEnableState(ICommand newCommand, ICommand oldCommand)
        {
            if (!ManageEnableState)
            {
                return;
            }

            // we detach or attach
            if (oldCommand != null)
                oldCommand.CanExecuteChanged -= Command_CanExecuteChanged;
            if (newCommand != null)
                newCommand.CanExecuteChanged += Command_CanExecuteChanged;

            // and update
            UpdateEnabledState();
        }

        private void UpdateEnabledState()
        {
            if (!ManageEnableState || AssociatedObject == null || Command == null) return;

            // we get if it is enabled or not
            var _canExecute = Command.CanExecute(Parameter);

            // we check if it is a control in SL
#if (!NET45)
            if (typeof(Control).IsAssignableFrom(AssociatedObject.GetType()))
            {
                var _target = AssociatedObject as Control;
                _target.IsEnabled = _canExecute;
            }
            else
            {
                AssociatedObject.IsHitTestVisible = _canExecute;
                AssociatedObject.Opacity = _canExecute ? INTERACTIVITY_ENABLED : INTERACTIVITY_DISABLED;
            }
#else
            AssociatedObject.IsEnabled = _canExecute;
#endif
        }

        private void DisposeEnableState()
        {
            if (!ManageEnableState || AssociatedObject == null || Command == null) return;

#if (SILVERLIGHT)
            if (AssociatedObject as Control != null)
#else
            if (AssociatedObject != null)
#endif
            {
                Command.CanExecuteChanged -= Command_CanExecuteChanged;
            }
        }

#endif

        #endregion
    }
}