using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eLog.Infrastructure;

namespace eLog.Models
{
    public class Machine
    {
        public Machine(int id)
        {
            if (id >= 0 && id <= 10)
            {
                Id = id;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
            
        }

        public int Id { get; }
        public string Name { get => Id switch
                {
                    0 => "Goodway GS-1500",
                    1 => "Hyundai L320A",
                    2 => "Hyundai WIA SKT21 #1",
                    3 => "Hyundai WIA SKT21 #2",
                    4 => "Hyundai XH6300",
                    5 => "Mazak QTS200ML",
                    6 => "Mazak QTS350",
                    7 => "Mazak Integrex i200",
                    8 => "Mazak Nexus 5000",
                    9 => "Quaser MV143",
                    10 => "Victor A110",

                    _ => throw new ArgumentOutOfRangeException(),
                }; 
            }

        public string LogPath { get => Path.Combine(AppSettings.LogBasePath, Name); }
    }
}
