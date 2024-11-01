using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace PlanetSimulationCW
{
    public class MouseButtonEventArgsCommandAction : TriggerAction<UIElement>
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(MouseButtonEventArgsCommandAction));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void Invoke(object parameter)
        {
            if (parameter is MouseButtonEventArgs keyEventArgs && Command != null && Command.CanExecute(keyEventArgs))
            {
                Command.Execute(keyEventArgs);
            }
        }
    }
}
