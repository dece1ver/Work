using DocumentFormat.OpenXml.VariantTypes;
using libeLog;
using libeLog.Base;
using remeLog.Infrastructure;
using remeLog.Models;
using remeLog.Views;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace remeLog.ViewModels
{
    public class EditSerialPartsWindowViewModel : ViewModel
    {
        public EditSerialPartsWindowViewModel()
        {
            SaveSerialPartsCommand = new LambdaCommand(OnSaveSerialPartsCommandExecuted, CanSaveSerialPartsCommandExecute);

            _Status = "";
            _SerialParts = new ObservableCollection<SerialPart>();
            SerialParts.CollectionChanged += OnSerialPartsCollectionChanged!;
            LoadSerialPartsAsync();
        }

        private ObservableCollection<SerialPart> _SerialParts;
        public ObservableCollection<SerialPart> SerialParts
        {
            get => _SerialParts;
            set
            {
                if (_SerialParts != null)
                    _SerialParts.CollectionChanged -= OnSerialPartsCollectionChanged!;

                if (Set(ref _SerialParts!, value))
                {
                    _SerialParts.CollectionChanged += OnSerialPartsCollectionChanged!;
                    SubscribeToSerialParts(_SerialParts);
                }
            }
        }

        private string _Status;
        public string Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }

        private bool _IsSaveEnabled;
        public bool IsSaveEnabled
        {
            get => _IsSaveEnabled;
            set => Set(ref _IsSaveEnabled, value);
        }

        private bool _InProgress;
        public bool InProgress
        {
            get => _InProgress;
            set => Set(ref _InProgress, value);
        }

        #region SaveSerialParts
        public ICommand SaveSerialPartsCommand { get; }
        private async void OnSaveSerialPartsCommandExecuted(object p)
        {
            if (string.IsNullOrEmpty(AppSettings.Instance.ConnectionString))
            {
                Status = "Операторы не могут быть сохранены т.к. строка подключения не настроена";
                return;
            }
            InProgress = true;
            await Database.SaveSerialPartsAsync(SerialParts, new Progress<string>(p => Status = p));
            LoadSerialPartsAsync();
            InProgress = false;
            Status = "Обновление завершено";
            await Task.Delay(3000);
            Status = "";
        }

        private bool CanSaveSerialPartsCommandExecute(object p) => !InProgress;
        #endregion

        private async void LoadSerialPartsAsync()
        {
            if (string.IsNullOrEmpty(AppSettings.Instance.ConnectionString))
            {
                Status = "Операторы не могут быть загружены т.к. строка подключения не настроена";
                return;
            }
            var parts = await Database.GetSerialPartsAsync(AppSettings.Instance.ConnectionString, new Progress<string>(p => Status = p));
            SerialParts = new ObservableCollection<SerialPart>(parts);
            ValidateSerialParts();
        }

        private async void OnSerialPartsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (SerialPart p in e.NewItems)
                    SubscribeToSerialPart(p);

            if (e.OldItems != null)
                foreach (SerialPart p in e.OldItems)
                {
                    UnsubscribeFromSerialPart(p);
                    if (p.Id != -1)
                    {
                        await Database.DeleteSerialPartAsync(p.Id, new Progress<string>(p => Status = p));
                    }
                }

            ValidateSerialParts();
        }

        private void SubscribeToSerialParts(ObservableCollection<SerialPart> SerialParts)
        {
            foreach (var op in SerialParts)
                SubscribeToSerialPart(op);
        }

        private void SubscribeToSerialPart(SerialPart op)
        {
            op.PropertyChanged += OnSerialPartPropertyChanged!;
        }

        private void UnsubscribeFromSerialPart(SerialPart op)
        {
            op.PropertyChanged -= OnSerialPartPropertyChanged!;
        }

        private void OnSerialPartPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ValidateSerialParts();
        }

        private void ValidateSerialParts()
        {
            foreach (var p in SerialParts)
            {
                string error = ValidatePart(p);
                if (!string.IsNullOrEmpty(error))
                {
                    Status = error;
                    IsSaveEnabled = false;
                    return;
                }
            }
            IsSaveEnabled = true;
            Status = "";
        }

        private string ValidatePart(SerialPart part)
        {
            if (string.IsNullOrWhiteSpace(part.PartName))
                return "Ошибка: Название детали не может быть пустое.";

            if (SerialParts.Count(o => o.PartName == part.PartName) > 1)
                return $"Ошибка: Деталь '{part.PartName}' уже существует.";

            return null!;
        }
    }
}
