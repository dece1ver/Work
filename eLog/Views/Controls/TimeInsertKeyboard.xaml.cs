﻿using eLog.Infrastructure;
using eLog.Infrastructure.Extensions;
using eLog.Models;
using libeLog.Extensions;
using libeLog.Models;
using libeLog.WinApi.Windows;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace eLog.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для TimeInsertKeyboard.xaml
    /// </summary>
    public partial class TimeInsertKeyboard : UserControl
    {
        public TimeInsertKeyboard()
        {
            InitializeComponent();
        }

        private void Clear()
        {
            Keyboard.KeyDown(Keys.LControlKey);
            Keyboard.KeyPress(Keys.A);
            Keyboard.KeyUp(Keys.LControlKey);
            Keyboard.KeyPress(Keys.Delete);
        }

        private void InputTime(DateTime dateTime)
        {
            Clear();
            var keys = dateTime.GetKeys();
            for (var i = 0; i < keys.Length; i++)
            {
                if (i == 13)
                {
                    Keyboard.KeyDown(Keys.LShiftKey);
                    Keyboard.KeyPress(keys[i]);
                    Keyboard.KeyUp(Keys.LShiftKey);
                }
                else
                {
                    Keyboard.KeyPress(keys[i]);
                }
            }
        }

        private void StartShiftButton_Click(object sender, RoutedEventArgs e) => InputTime(Util.GetStartShiftTime());

        private void EndShiftButton_Click(object sender, RoutedEventArgs e) => InputTime(Util.GetEndShiftTime());

        private void CurrentTimeButton_Click(object sender, RoutedEventArgs e) => InputTime(DateTime.Now);

        private void PreviousPartEndTimeButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.Instance.Parts.Any(x => x.IsFinished is not Part.State.InProgress))
            {
                InputTime(AppSettings.Instance.Parts.First(x => x.IsFinished is not Part.State.InProgress).EndMachiningTime);
            }
            else
            {
                Keyboard.KeyPress(Keys.OemMinus);
            }
        }
    }
}