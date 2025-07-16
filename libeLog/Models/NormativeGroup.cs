using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libeLog.Models
{
    /// <summary>
    /// Группа нормативов одного типа (наладка или изготовление)
    /// </summary>
    public class NormativeGroup
    {
        public string Label { get; set; }
        public IEnumerable<NormativeEntry> Entries { get; set; }

        public NormativeGroup(string label, IEnumerable<NormativeEntry> entries)
        {
            Label = label;
            Entries = entries;
        }

        public override string ToString() => Label;
    }
}
