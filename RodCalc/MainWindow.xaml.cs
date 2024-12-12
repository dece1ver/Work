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
                int partCount = string.IsNullOrWhiteSpace(PartCountTextBox.Text) ? 0 : int.Parse(PartCountTextBox.Text);
                int partLength = int.Parse(PartLengthTextBox.Text);

                // Проверка на корректность ввода
                if (maxRodLength <= leftover + partLength)
                {
                    Variant1TextBox.Text = "Ошибка: длина прута слишком мала для заданных условий.";
                    Variant2TextBox.Text = string.Empty;
                    return;
                }

                // Если количество партии не указано, расчет идет на один полный прут
                if (partCount == 0)
                {
                    int maxPartsPerRod = (maxRodLength - leftover) / partLength;
                    int totalRodLength = maxPartsPerRod * partLength + leftover;

                    Variant1TextBox.Text = $"Вариант 1:\n" +
                                           $"- 1 прут: {totalRodLength} мм ({maxPartsPerRod} деталей с прута)\n" +
                                           $"Общий расход материала: {totalRodLength} мм\n" +
                                           $"Общий остаток: {leftover} мм";

                    Variant2TextBox.Text = Variant1TextBox.Text; // Для одного прута варианты совпадают
                    return;
                }

                // Вариант 1: Максимизация количества заготовок на одном пруте
                int maxPartsPerRodVariant1 = (maxRodLength - leftover) / partLength;
                int fullRodsCountVariant1 = partCount / maxPartsPerRodVariant1;
                int remainingPartsVariant1 = partCount % maxPartsPerRodVariant1;

                int totalRodsVariant1 = fullRodsCountVariant1 + (remainingPartsVariant1 > 0 ? 1 : 0);
                int totalLengthUsedVariant1 = fullRodsCountVariant1 * maxPartsPerRodVariant1 * partLength + remainingPartsVariant1 * partLength;
                int totalLeftoverVariant1 = totalRodsVariant1 * leftover;

                string variant1Details = fullRodsCountVariant1 > 0
                    ? $"- {fullRodsCountVariant1} {GetRodWord(fullRodsCountVariant1)} по {maxPartsPerRodVariant1 * partLength + leftover} мм ({maxPartsPerRodVariant1} {GetPartWord(maxPartsPerRodVariant1)} с прута)\n"
                    : string.Empty;

                if (remainingPartsVariant1 > 0)
                {
                    variant1Details += $"- Последний прут: {remainingPartsVariant1 * partLength + leftover} мм ({remainingPartsVariant1} {GetPartWord(remainingPartsVariant1)} с прута)\n";
                }

                // Вариант 2: Равномерное распределение партии
                int optimalPartsPerRodVariant2 = (int)Math.Ceiling((double)partCount / Math.Ceiling((double)partCount / maxPartsPerRodVariant1));

                if (optimalPartsPerRodVariant2 * partLength + leftover > maxRodLength)
                {
                    Variant2TextBox.Text = "Вариант 2:\nНевозможно равномерно распределить детали по заданным условиям.";
                }
                else
                {
                    int totalRodsVariant2 = (int)Math.Ceiling((double)partCount / optimalPartsPerRodVariant2);
                    int totalLengthUsedVariant2 = totalRodsVariant2 * optimalPartsPerRodVariant2 * partLength;
                    int totalLeftoverVariant2 = totalRodsVariant2 * leftover;

                    string variant2Details = $"- {totalRodsVariant2} {GetRodWord(totalRodsVariant2)} по {optimalPartsPerRodVariant2 * partLength + leftover} мм ({optimalPartsPerRodVariant2} {GetPartWord(optimalPartsPerRodVariant2)} с прута)";

                    Variant2TextBox.Text = $"Вариант 2:\n" +
                                           variant2Details + "\n" +
                                           $"Общий расход материала: {totalLengthUsedVariant2 + totalLeftoverVariant2} мм\n" +
                                           $"Общий остаток: {totalLeftoverVariant2} мм";
                }

                // Результаты варианта 1
                Variant1TextBox.Text = $"Вариант 1:\n" +
                                       variant1Details +
                                       $"Общий расход материала: {totalLengthUsedVariant1 + totalLeftoverVariant1} мм\n" +
                                       $"Общий остаток: {totalLeftoverVariant1} мм";
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

        private string GetRodWord(int count)
        {
            if (count % 10 == 1 && count % 100 != 11) return "прут";
            if (count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 10 || count % 100 >= 20)) return "прута";
            return "прутов";
        }

        private string GetPartWord(int count)
        {
            if (count % 10 == 1 && count % 100 != 11) return "деталь";
            if (count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 10 || count % 100 >= 20)) return "детали";
            return "деталей";
        }
    }
}
