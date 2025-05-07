using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace remeLog.Views
{
    /// <summary>
    /// Логика взаимодействия для WinnumInfoWindow.xaml
    /// </summary>
    public partial class WinnumInfoWindow : Window
    {
        public WinnumInfoWindow()
        {
            InitializeComponent();
        }

        public void SetData(List<Dictionary<string, string>> dictList)
        {
            var allKeys = dictList.SelectMany(d => d.Keys).Distinct().ToList();

            var table = new DataTable();
            foreach (var key in allKeys)
                table.Columns.Add(key);

            foreach (var dict in dictList)
            {
                var row = table.NewRow();
                foreach (var key in allKeys)
                    row[key] = dict.TryGetValue(key, out var value) ? Uri.UnescapeDataString(value) : "";
                table.Rows.Add(row);
            }

            DataGrid.ItemsSource = table.DefaultView;
        }
    }
}
