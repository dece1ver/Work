using System;

namespace eLog.Models;

class CombinedDownTime
{
    public DownTime.Types Type { get; set; }
    public string Name { get; set; }
    public DownTime.Relations Relation { get; set; }
    public TimeSpan Time { get; set; }
}
