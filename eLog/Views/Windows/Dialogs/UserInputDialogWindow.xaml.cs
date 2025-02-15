using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для UserInputDialogWindow.xaml.
    /// </summary>
    public partial class UserInputDialogWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Конструктор окна диалога.
        /// </summary>
        /// <param name="message">Сообщение для отображения в окне.</param>
        /// <param name="optionalLabel">
        /// Опциональная подпись для ComboBox. Если null или пустая, а также если options пуст, опциональный блок не отображается.
        /// </param>
        /// <param name="options">
        /// Массив значений для ComboBox (например, значения перечисления). Если null или пуст, опциональный блок не отображается.
        /// </param>
        public UserInputDialogWindow(string message, string? optionalLabel = null, IEnumerable<string> options = null)
        {
            InitializeComponent();
            Message = message;
            OptionalLabel = optionalLabel;
            Options = (List<string>)(options ?? new List<string>());
        }

        #region DependencyProperty для Message

        /// <summary>
        /// DependencyProperty для свойства Message.
        /// </summary>
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(UserInputDialogWindow),
                new PropertyMetadata(default(string)));

        /// <summary>
        /// Сообщение для отображения в окне.
        /// </summary>
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        #endregion

        #region Свойства с уведомлением (INotifyPropertyChanged)

        private string? _userInput;
        /// <summary>
        /// Введённый пользователем текст.
        /// </summary>
        public string? UserInput
        {
            get => _userInput;
            set
            {
                if (_userInput != value)
                {
                    _userInput = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanConfirm));
                }
            }
        }

        private string? _optionalLabel;
        /// <summary>
        /// Текст подписи для опционального блока (ComboBox).
        /// </summary>
        public string? OptionalLabel
        {
            get => _optionalLabel;
            set
            {
                if (_optionalLabel != value)
                {
                    _optionalLabel = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShowOptionalBlock));
                    OnPropertyChanged(nameof(CanConfirm));
                }
            }
        }

        private List<string> _options = new List<string>();
        /// <summary>
        /// Массив значений для ComboBox.
        /// </summary>
        public List<string> Options
        {
            get => _options;
            set
            {
                if (_options != value)
                {
                    _options = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShowOptionalBlock));
                    OnPropertyChanged(nameof(CanConfirm));
                }
            }
        }

        private string _selectedOption;
        /// <summary>
        /// Выбранное значение из опционального ComboBox.
        /// </summary>
        public string SelectedOption
        {
            get => _selectedOption;
            set
            {
                if (_selectedOption != value)
                {
                    _selectedOption = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanConfirm));
                }
            }
        }

        /// <summary>
        /// Вычисляемое свойство, определяющее, следует ли отображать опциональный блок.
        /// Блок отображается, если OptionalLabel не пустой или Options содержит хотя бы один элемент.
        /// </summary>
        public bool ShowOptionalBlock => !string.IsNullOrWhiteSpace(OptionalLabel) && (Options?.Count > 0);

        /// <summary>
        /// Вычисляемое свойство, определяющее, можно ли подтвердить диалог.
        /// Если ни текст не введён, ни (при наличии опционального блока) не выбран элемент, подтверждение невозможно.
        /// </summary>
        public bool CanConfirm => !string.IsNullOrWhiteSpace(UserInput) && ((ShowOptionalBlock && SelectedOption != null) || !ShowOptionalBlock);

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}
