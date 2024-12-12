using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RodCalc
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ввод данных
                int maxRodLength = string.IsNullOrWhiteSpace(MaxRodLengthTextBox.Text) ? 500 : int.Parse(MaxRodLengthTextBox.Text);
                int leftover = string.IsNullOrWhiteSpace(LeftoverTextBox.Text) ? 30 : int.Parse(LeftoverTextBox.Text);
                int partLength = int.Parse(PartLengthTextBox.Text);
                int partCount = string.IsNullOrWhiteSpace(PartCountTextBox.Text) ? 0 : int.Parse(PartCountTextBox.Text);

                // Проверка на корректность ввода
                if (maxRodLength <= leftover + partLength)
                {
                    Variant1TextBox.Text = "Ошибка: длина прута слишком мала для заданных условий.";
                    Variant2TextBox.Text = string.Empty;
                    return;
                }

                // Если количество партии не указано, просто считаем длину прута
                if (partCount == 0)
                {
                    int maxPartsPerRod = (maxRodLength - leftover) / partLength;
                    Variant1TextBox.Text = $"Длина прута для максимального использования материала: {maxPartsPerRod * partLength + leftover} мм ({maxPartsPerRod} деталей с прута).";
                    Variant2TextBox.Text = string.Empty;
                    return;
                }

                // Вариант 1: Максимизация количества заготовок на одном пруте
                int maxPartsPerRodVariant1 = (maxRodLength - leftover) / partLength;
                int fullRodsCountVariant1 = partCount / maxPartsPerRodVariant1;
                int remainingPartsVariant1 = partCount % maxPartsPerRodVariant1;
                int totalRodsVariant1 = fullRodsCountVariant1 + (remainingPartsVariant1 > 0 ? 1 : 0);
                int totalLengthUsedVariant1 = fullRodsCountVariant1 * maxPartsPerRodVariant1 * partLength + remainingPartsVariant1 * partLength;
                int totalLeftoverVariant1 = totalRodsVariant1 * leftover;

                string variant1Details = remainingPartsVariant1 > 0
                    ? $"- {fullRodsCountVariant1} прутов по {maxPartsPerRodVariant1 * partLength + leftover} мм ({maxPartsPerRodVariant1} деталей с прута)\n" +
                      $"- Последний прут: {remainingPartsVariant1 * partLength + leftover} мм ({remainingPartsVariant1} деталь с прута)"
                    : $"- {fullRodsCountVariant1} прутов по {maxPartsPerRodVariant1 * partLength + leftover} мм ({maxPartsPerRodVariant1} деталей с прута)";

                // Вариант 2: Равномерное распределение партии
                int optimalPartsPerRodVariant2 = (int)Math.Ceiling((double)partCount / (partCount / maxPartsPerRodVariant1));
                int totalRodsVariant2 = (int)Math.Ceiling((double)partCount / optimalPartsPerRodVariant2);
                int totalLengthUsedVariant2 = totalRodsVariant2 * optimalPartsPerRodVariant2 * partLength;
                int totalLeftoverVariant2 = totalRodsVariant2 * leftover;

                string variant2Details = $"- {totalRodsVariant2} прутов по {optimalPartsPerRodVariant2 * partLength + leftover} мм ({optimalPartsPerRodVariant2} деталей с прута)";

                // Результаты
                Variant1TextBox.Text = $"Вариант 1:\n" +
                                       variant1Details + "\n" +
                                       $"Общий расход материала: {totalLengthUsedVariant1 + totalLeftoverVariant1} мм\n" +
                                       $"Общий остаток: {totalLeftoverVariant1} мм";

                Variant2TextBox.Text = $"Вариант 2:\n" +
                                       variant2Details + "\n" +
                                       $"Общий расход материала: {totalLengthUsedVariant2 + totalLeftoverVariant2} мм\n" +
                                       $"Общий остаток: {totalLeftoverVariant2} мм";
            }
            catch (Exception ex)
            {
                Variant1TextBox.Text = $"Ошибка: {ex.Message}";
                Variant2TextBox.Text = string.Empty;
            }
        }

        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is TextBox textBox)
            {
                Clipboard.SetText(textBox.Text);
            }
        }
    }
}
