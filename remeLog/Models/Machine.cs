using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    public class Machine
    {
        public Machine(string name, int wnId, Guid wnUuid)
        {
            Name = name;
            WnId = wnId;
            WnUuid = wnUuid;
        }

        public string Name { get; set; }
        public int WnId { get; set; }
        public Guid WnUuid { get; set; }
    }
}
