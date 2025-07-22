using remeLog.Validation;
using System;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace remeLog.Views
{
    public partial class UserInputDialogWindow : Window
    {
        public static new readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(UserInputDialogWindow), new PropertyMetadata("Ввод"));

        public static readonly DependencyProperty PromptTextProperty =
            DependencyProperty.Register(nameof(PromptText), typeof(string), typeof(UserInputDialogWindow), new PropertyMetadata("Введите значение:"));

        public static readonly DependencyProperty UserInputProperty =
            DependencyProperty.Register(nameof(UserInput), typeof(string), typeof(UserInputDialogWindow), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ExpectedTypeProperty =
            DependencyProperty.Register(nameof(ExpectedType), typeof(Type), typeof(UserInputDialogWindow), new PropertyMetadata(typeof(string)));

        public new string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string PromptText
        {
            get => (string)GetValue(PromptTextProperty);
            set => SetValue(PromptTextProperty, value);
        }

        public string UserInput
        {
            get => (string)GetValue(UserInputProperty);
            set => SetValue(UserInputProperty, value);
        }

        public Type? ExpectedType
        {
            get => (Type)GetValue(ExpectedTypeProperty);
            set => SetValue(ExpectedTypeProperty, value);
        }

        public UserInputDialogWindow(string title = "Ввод", string prompt = "Введите значение:", string? defaultInput = null, Type? expectedType = null)
        {
            InitializeComponent();

            Title = title;
            PromptText = prompt;
            UserInput = defaultInput ?? string.Empty;
            ExpectedType = expectedType;
            if (ExpectedType != null)
            {
                var binding = BindingOperations.GetBinding(InputBox, TextBox.TextProperty);
                binding?.ValidationRules.Add(new InputTypeValidationRule { ExpectedType = expectedType });
            }

            DataContext = this;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InputBox.Focus();
        }
    }
}
