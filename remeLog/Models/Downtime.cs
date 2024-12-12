using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    public enum Downtime
    {
        Maintenance, 
        ToolSearching,
        ToolChanging,
        Mentoring,
        ContactingDepartments,
        FixtureMaking,
        HardwareFailure
    }
}
