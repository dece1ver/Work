using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eLog.Infrastructure;
using Newtonsoft.Json;

namespace eLog.Models
{
    public class Machine
    {
        public Machine(int id)
        {
            Id = id;
        }

        public int Id { get; }
        public string Name =>
            Id switch
            {
                0 => "Goodway GS-1500",
                1 => "Hyundai L320A",
                2 => "Hyundai WIA SKT21 №105",
                3 => "Hyundai WIA SKT21 №104",
                4 => "Hyundai XH6300",
                5 => "Mazak QTS200ML",
                6 => "Mazak QTS350",
                7 => "Mazak Integrex i200",
                8 => "Mazak Nexus 5000",
                9 => "Quaser MV143",
                10 => "Victor A110",

                _ => "-//-",
            };

        [JsonIgnore]
        public string XlReservedPath => Path.Combine(AppSettings.XlReservedPath, Name);
    }
}
