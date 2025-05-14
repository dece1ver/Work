using System;
using System.Collections.Generic;
using System.Linq;
using libeLog.Base;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using remeLog.Infrastructure.Winnum.Data;
using System.Collections.ObjectModel;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using remeLog.Infrastructure;

namespace remeLog.ViewModels
{
    public class WinnumInfoViewModel : ViewModel
    {

        public WinnumInfoViewModel(string generalInfo, List<PriorityTagDuration> priorityTagDurations, List<TimeInterval> timeIntervals)
        {
            GeneralInfo = generalInfo;

            PriorityTagDurations = priorityTagDurations;

            TimeIntervals = timeIntervals;

            var series = new ObservableCollection<ISeries>();

            var minDate = timeIntervals.Min(x => x.Start);
            var maxDate = timeIntervals.Max(x => x.End);
            (minDate, maxDate) = Util.GetRoundedHourBounds(minDate, maxDate);

            foreach (var interval in timeIntervals)
            {
                series.Add(new StepLineSeries<DateTimePoint>
                {
                    Values = new[]
                    {
                        new DateTimePoint(interval.Start, 1),
                        new DateTimePoint(interval.End, 1)
                    },
                    Fill = new SolidColorPaint(SKColors.Green.WithAlpha(90)),
                    Stroke = null,
                    GeometryFill = null,
                    GeometryStroke = null,
                    XToolTipLabelFormatter = point =>
                    {
                        var x = point.Model?.DateTime ?? DateTime.MinValue;
                        return point.Index switch
                        {
                            0 => $"Начало: {x:HH:mm:ss}",
                            1 => $"Завершение: {x:HH:mm:ss}",
                            _ => $"{x:HH:mm:ss}"
                        };
                    },
                    YToolTipLabelFormatter = point => "Программа выполняется",
                    
                });
            }

            Intervals = series;

            XAxes = new Axis[]
            {
                new DateTimeAxis(TimeSpan.FromHours(1), date => date.ToString("HH:mm:ss"))
                {
                    Name = "Изготовление по ЭЖ",
                    MinStep = 10,
                    LabelsRotation = 45,
                    TextSize = 12,
                    NameTextSize = 14,
                    SeparatorsPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(50)),
                    ShowSeparatorLines = true
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Программа выполняется",
                    NameTextSize = 14,
                    Labels = new List<string>(),
                    MaxLimit = 1
                }
            };
        }
        public ObservableCollection<ISeries> Intervals { get; set; }

        public string GeneralInfo { get; set; }

        public List<PriorityTagDuration> PriorityTagDurations { get; set; }
        public List<TimeInterval> TimeIntervals { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
    }
}
