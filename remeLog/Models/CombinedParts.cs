using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    class CombinedParts
    {
        public string Machine { get; set; }
        public string Operator => Parts.Count > 0 ? Parts[0].Operator : "";
        public string Shift => Parts.Count > 0 ? Parts[0].Shift : "";
        public DateTime Date { get; set; }
        public List<Part> Parts { get; set; } = new();
        public CombinedParts(string machine)
        {
            Machine = machine;
        }
    }
}
