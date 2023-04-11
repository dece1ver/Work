using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Models
{
    class CombinedDownTime
    {
        public DownTime.Types Type { get; set; }
        public string Name { get; set; }
        public DownTime.Relations Relation { get; set; }
        public TimeSpan Time { get; set; }
    }
}
