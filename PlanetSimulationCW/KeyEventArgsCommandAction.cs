﻿using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace PlanetSimulationCW
{
    public class KeyEventArgsCommandAction : TriggerAction<UIElement>
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(KeyEventArgsCommandAction));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void Invoke(object parameter)
        {
            if (parameter is KeyEventArgs keyEventArgs && Command != null && Command.CanExecute(keyEventArgs))
            {
                Command.Execute(keyEventArgs);
            }
        }
    }
}