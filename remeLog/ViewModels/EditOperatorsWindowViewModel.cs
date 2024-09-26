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
    public class EditOperatorsWindowViewModel : ViewModel
    {
        public EditOperatorsWindowViewModel()
        {
            SaveOperatorsCommand = new LambdaCommand(OnSaveOperatorsCommandExecuted, CanSaveOperatorsCommandExecute);

            _Status = "";
            _Operators = new ObservableCollection<OperatorInfo>();
            Operators.CollectionChanged += OnOperatorsCollectionChanged!;
            LoadOperatorsAsync();
        }

        private ObservableCollection<OperatorInfo> _Operators;
        public ObservableCollection<OperatorInfo> Operators
        {
            get => _Operators;
            set
            {
                if (Set(ref _Operators, value))
                {
                    _Operators.CollectionChanged += OnOperatorsCollectionChanged!;
                    SubscribeToOperators(_Operators);
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

        #region SaveOperators
        public ICommand SaveOperatorsCommand { get; }
        private async void OnSaveOperatorsCommandExecuted(object p)
        {
            await Database.SaveOperatorsAsync(Operators, new Progress<string>(p => Status = p));
            LoadOperatorsAsync();
        }

        private static bool CanSaveOperatorsCommandExecute(object p) => true;
        #endregion

        private async void LoadOperatorsAsync()
        {
            var operators = await Database.GetOperatorsAsync(new Progress<string>(p => Status = p));
            Operators = new ObservableCollection<OperatorInfo>(operators);
            ValidateOperators();
        }

        private async void OnOperatorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (OperatorInfo op in e.NewItems)
                    SubscribeToOperator(op);

            if (e.OldItems != null)
                foreach (OperatorInfo op in e.OldItems)
                {
                    UnsubscribeFromOperator(op);
                    if (op.Id != -1)
                    {
                        await Database.DeleteOperatorAsync(op.Id, new Progress<string>(p => Status = p));
                    }
                }

            ValidateOperators();
        }

        private void SubscribeToOperators(ObservableCollection<OperatorInfo> operators)
        {
            foreach (var op in operators)
                SubscribeToOperator(op);
        }

        private void SubscribeToOperator(OperatorInfo op)
        {
            op.PropertyChanged += OnOperatorPropertyChanged!;
        }

        private void UnsubscribeFromOperator(OperatorInfo op)
        {
            op.PropertyChanged -= OnOperatorPropertyChanged!;
        }

        private void OnOperatorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ValidateOperators(); 
        }

        private void ValidateOperators()
        {
            foreach (var op in Operators)
            {
                string error = ValidateOperator(op);
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

        private string ValidateOperator(OperatorInfo op)
        {
            if (string.IsNullOrWhiteSpace(op.Name))
                return "Ошибка: Имя оператора не может быть пустым.";

            if (Operators.Count(o => o.Name == op.Name) > 1)
                return $"Ошибка: Имя оператора '{op.Name}' уже существует.";

            if (op.Qualification < 0 || op.Qualification > 6)
                return $"Ошибка: Разряд оператора '{op.Name}' должен быть от 0 до 6.";

            return null!;
        }
    }
}
