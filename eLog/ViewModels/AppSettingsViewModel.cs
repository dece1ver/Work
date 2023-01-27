using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eLog.Infrastructure.Extensions;
using eLog.Infrastructure;
using System.Windows.Input;
using eLog.Models;
using eLog.Services;
using eLog.ViewModels.Base;
using Microsoft.Win32;
using eLog.Infrastructure.Commands;

namespace eLog.ViewModels
{
    internal class AppSettingsViewModel : ViewModel
    {
        private AppSettingsModel _AppSettings;
        public AppSettingsModel AppSettings
        {
            get => _AppSettings;
            set => Set(ref _AppSettings, value);
        }

        public List<Machine> Machines { get; set; } = Enumerable.Range(0, 11).Select(x => new Machine(x)).ToList();

        public Machine? CurrentMachine { get; set; }


        #region SetXlPath
        public ICommand SetXlPathCommand { get; }
        private void OnSetXlPathCommandExecuted(object p)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Excel таблица (*.xlsx)|*.xlsx";
            openFileDialog.DefaultExt = "xslx";
            if (openFileDialog.ShowDialog() == true)
            {
                AppSettings.XlPath = openFileDialog.FileName;
                OnPropertyChanged(nameof(AppSettings.XlPath));
                OnPropertyChanged(nameof(AppSettings));
            }
        }
        private static bool CanSetXlPathCommandExecute(object p) => true;
        #endregion

        public AppSettingsViewModel()
        {
            SetXlPathCommand = new LambdaCommand(OnSetXlPathCommandExecuted, CanSetXlPathCommandExecute);
        }
    }
}
