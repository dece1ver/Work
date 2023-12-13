using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace eLog.Models;

public class CombinedDownTime
{
    public CombinedDownTime(ICollection<DownTime> downtimes)
    {
        if (downtimes == null || downtimes.Count < 1) throw new ArgumentException("Должена быть коллекция простоев хотя бы с одним экземпляром.");
        if (downtimes.All(x => x.Type != downtimes.First().Type)) throw new ArgumentException("Простои должны быть одного типа.");
        if (downtimes.All(x => x.Relation != downtimes.First().Relation)) throw new ArgumentException("Простои должны относиться к одному этапу изготовления.");
        Name = downtimes.First().Name;
        Relation = downtimes.First().Relation;
        Description = $"[{(Relation is DownTime.Relations.Setup ? "н" : "и")}] {Name} - {downtimes.Sum(x => x.Time.TotalMinutes)} мин\n";
        foreach (var downtime in downtimes)
        {
            Description += $" └──{downtime.StartTime:t} ─ {downtime.EndTime:t}\n";
            Time += downtime.Time;
        }
    }
    public DownTime.Types Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DownTime.Relations Relation { get; set; }
    public TimeSpan Time { get; set; }
}
