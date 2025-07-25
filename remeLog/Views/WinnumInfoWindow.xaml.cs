﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using remeLog.Infrastructure.Winnum.Data;
using remeLog.ViewModels;

namespace remeLog.Views
{
    /// <summary>
    /// Логика взаимодействия для WinnumInfoWindow.xaml
    /// </summary>
    public partial class WinnumInfoWindow : Window
    {
        public WinnumInfoWindow(string generalInfo, string ncProgramFolder, List<PriorityTagDuration> priorityTagDurations, List<TimeInterval> timeIntervals)
        {
            DataContext = new WinnumInfoViewModel(generalInfo, ncProgramFolder, priorityTagDurations, timeIntervals);
            InitializeComponent();
        }
    }
}
