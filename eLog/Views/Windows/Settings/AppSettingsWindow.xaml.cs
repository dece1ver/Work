﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using eLog.Models;
using Microsoft.Win32;

namespace eLog.Views.Windows.Settings
{
    /// <summary>
    /// Логика взаимодействия для AppSettingsWindow.xaml
    /// </summary>
    public partial class AppSettingsWindow : Window
    {
        public static readonly DependencyProperty AppSettingsProperty =
            DependencyProperty.Register(
                nameof(AppSettings),
                typeof(AppSettingsModel),
                typeof(AppSettingsWindow),
                new PropertyMetadata(default(string)));


        public AppSettingsModel AppSettings
        {
            get => (AppSettingsModel)GetValue(AppSettingsProperty);
            set => SetValue(AppSettingsProperty, value);
        }

        public List<Machine> Machines { get; set; } = new();

        public Machine? CurrentMachine { get; set; }

        public AppSettingsWindow()
        {
            InitializeComponent();
            for (var i = 0; i < 11; i++)
            {
                Machines.Add(new Machine(i));
            }
        }

        private void SetXlPathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Excel таблица (*.xlsx)|*.xlsx";
            openFileDialog.DefaultExt = "xslx";
            if (openFileDialog.ShowDialog() == true)
            {
                AppSettings.XlPath = openFileDialog.FileName;
                XlPathTextBox.Text = AppSettings.XlPath;
            }
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            var handlers = PropertyChanged;
            if (handlers is null) return;

            var invokationList = handlers.GetInvocationList();
            var args = new PropertyChangedEventArgs(PropertyName);

            foreach (var action in invokationList)
            {
                if (action.Target is DispatcherObject dispatcherObject)
                {
                    dispatcherObject.Dispatcher.Invoke(action, this, args);
                }
                else
                {
                    action.DynamicInvoke(this, args);
                }
            }
        }

        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }

        #endregion
    }
}