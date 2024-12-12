using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace RodCalc
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string GetCalculationFormula(int partLength, int leftover, int maxRodLength, int partCount, int bestPartsPerRod = 0, int bestRodCount = 0)
        {
            StringBuilder formula = new StringBuilder();
            formula.AppendLine("\n\nПуть расчета:");
            formula.AppendLine($"{partLength} × n + {leftover} ≤ {maxRodLength} (где n - количество деталей на прут)");
            formula.AppendLine($"{partLength} × n ≤ {maxRodLength - leftover}");
            formula.AppendLine($"N ≤ {(maxRodLength - leftover) / (double)partLength:0.###}");
            formula.AppendLine($"Максимум деталей на прут: n = {(maxRodLength - leftover) / partLength:0.###}");

            if (bestPartsPerRod > 0 && bestRodCount > 0)
            {
                formula.AppendLine($"Для равномерного распределения: {bestPartsPerRod} деталей на прут");
                formula.AppendLine($"Количество прутов: {partCount}/{bestPartsPerRod} = {bestRodCount}");
            }

            return formula.ToString();
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int maxRodLength = string.IsNullOrWhiteSpace(MaxRodLengthTextBox.Text) ? 500 : int.Parse(MaxRodLengthTextBox.Text);
                int leftover = string.IsNullOrWhiteSpace(LeftoverTextBox.Text) ? 30 : int.Parse(LeftoverTextBox.Text);
                int partCount = string.IsNullOrWhiteSpace(PartCountTextBox.Text) ? 0 : int.Parse(PartCountTextBox.Text);
                int partLength = int.Parse(PartLengthTextBox.Text);

                if (maxRodLength <= leftover + partLength)
                {
                    Variant1TextBox.Text = "Ошибка: длина прута слишком мала для заданных условий.";
                    Variant2TextBox.Text = string.Empty;
                    return;
                }

                string calculationFormula = GetCalculationFormula(partLength, leftover, maxRodLength, partCount);

                // Если количество партии не указано, расчет идет на один полный прут
                if (partCount == 0)
                {
                    int maxPartsPerRod = (maxRodLength - leftover) / partLength;
                    int totalRodLength = maxPartsPerRod * partLength + leftover;

                    string result = $"- 1 прут: {totalRodLength} мм ({maxPartsPerRod} {GetPartWord(maxPartsPerRod)} с прута)\n" +
                                  $"Общий расход материала: {totalRodLength} мм\n" +
                                  $"Общий остаток: {leftover} мм" +
                                  calculationFormula;

                    Variant1TextBox.Text = result;
                    Variant2TextBox.Text = result;
                    return;
                }

                // Вариант 1: Максимизация количества заготовок на одном пруте
                int maxPartsPerRodVariant1 = (maxRodLength - leftover) / partLength;
                int fullRodsCountVariant1 = partCount / maxPartsPerRodVariant1;
                int remainingPartsVariant1 = partCount % maxPartsPerRodVariant1;

                int totalRodsVariant1 = fullRodsCountVariant1 + (remainingPartsVariant1 > 0 ? 1 : 0);
                int totalLengthUsedVariant1 = (fullRodsCountVariant1 * maxPartsPerRodVariant1 + remainingPartsVariant1) * partLength;
                int totalLeftoverVariant1 = (fullRodsCountVariant1 * leftover) + (remainingPartsVariant1 > 0 ? leftover : 0);

                string variant1Details;
                if (totalRodsVariant1 == 1)
                {
                    variant1Details = $"- 1 прут: {partCount * partLength + leftover} мм ({partCount} {GetPartWord(partCount)} с прута)\n";
                }
                else
                {
                    variant1Details = fullRodsCountVariant1 > 0
                        ? $"- {fullRodsCountVariant1} {GetRodWord(fullRodsCountVariant1)} по {maxPartsPerRodVariant1 * partLength + leftover} мм ({maxPartsPerRodVariant1} {GetPartWord(maxPartsPerRodVariant1)} с прута)\n"
                        : string.Empty;

                    if (remainingPartsVariant1 > 0)
                    {
                        variant1Details += $"- 1 прут: {remainingPartsVariant1 * partLength + leftover} мм ({remainingPartsVariant1} {GetPartWord(remainingPartsVariant1)} с прута)\n";
                    }
                }

                // Вариант 2: Равномерное распределение
                int bestPartsPerRod = 0;
                int bestRodCount = int.MaxValue;

                for (int partsPerRod = 1; partsPerRod <= maxPartsPerRodVariant1; partsPerRod++)
                {
                    if (partsPerRod * partLength + leftover <= maxRodLength)
                    {
                        int neededRods = (int)Math.Ceiling((double)partCount / partsPerRod);

                        if (neededRods * partsPerRod == partCount)
                        {
                            if (neededRods < bestRodCount ||
                                (neededRods == bestRodCount && partsPerRod > bestPartsPerRod))
                            {
                                bestRodCount = neededRods;
                                bestPartsPerRod = partsPerRod;
                            }
                        }
                    }
                }

                if (bestPartsPerRod > 0)
                {
                    int totalLength = bestPartsPerRod * partLength + leftover;
                    int totalMaterial = bestRodCount * totalLength;
                    int totalLeftover = bestRodCount * leftover;

                    string variant2Formula = GetCalculationFormula(partLength, leftover, maxRodLength, partCount, bestPartsPerRod, bestRodCount);

                    Variant2TextBox.Text = $"- {bestRodCount} {GetRodWord(bestRodCount)} по {totalLength} мм ({bestPartsPerRod} {GetPartWord(bestPartsPerRod)} с прута)\n" +
                                         $"Общий расход материала: {totalMaterial} мм\n" +
                                         $"Общий остаток: {totalLeftover} мм" +
                                         variant2Formula;
                }
                else
                {
                    Variant2TextBox.Text = "Вариант 2:\n" +
                                         "Невозможно равномерно распределить детали по прутам.\n" +
                                         "Рекомендуется использовать Вариант 1." +
                                         calculationFormula;
                }

                Variant1TextBox.Text = $"Вариант 1:\n" +
                                      variant1Details +
                                      $"Общий расход материала: {totalLengthUsedVariant1 + totalLeftoverVariant1} мм\n" +
                                      $"Общий остаток: {totalLeftoverVariant1} мм" +
                                      calculationFormula;
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
