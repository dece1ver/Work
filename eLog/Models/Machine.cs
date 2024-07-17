using Newtonsoft.Json;

namespace eLog.Models
{
    public class Machine
    {
        [JsonConstructor]
        public Machine(int id)
        {
            Id = id;
            switch (id)
            {
                case 0:
                    Name = "Goodway GS-1500";
                    SafeName = "Goodway GS-1500";
                    break;
                case 1:
                    Name = "Hyundai L230A";
                    SafeName = "Hyundai L230A";
                    break;
                case 2:
                    Name = "Hyundai WIA SKT21 №105";
                    SafeName = "Hyundai WIA SKT21 (105)";
                    break;
                case 3:
                    Name = "Hyundai WIA SKT21 №104";
                    SafeName = "Hyundai WIA SKT21 (104)";
                    break;
                case 4:
                    Name = "Hyundai XH6300";
                    SafeName = "Hyundai XH6300";
                    break;
                case 5:
                    Name = "Mazak QTS200ML";
                    SafeName = "Mazak QTS200ML";
                    break;
                case 6:
                    Name = "Mazak QTS350";
                    SafeName = "Mazak QTS350";
                    break;
                case 7:
                    Name = "Mazak Integrex i200";
                    SafeName = "Mazak Integrex i200";
                    break;
                case 8:
                    Name = "Mazak Nexus 5000";
                    SafeName = "Mazak Nexus 5000";
                    break;
                case 9:
                    Name = "Quaser MV134";
                    SafeName = "Quaser MV134";
                    break;
                case 10:
                    Name = "Victor A110";
                    SafeName = "Victor A110";
                    break;
                case 11:
                    Name = "Rontek HTC550MY";
                    SafeName = "Rontek HTC550MY";
                    break;
                case 12:
                    Name = "Rontek HTC650M";
                    SafeName = "Rontek HTC650M";
                    break;
                case 13:
                    Name = "Rontek HTC420 №1";
                    SafeName = "Rontek HTC420 (1)";
                    break;
                case 14:
                    Name = "Rontek HTC420 №2";
                    SafeName = "Rontek HTC420 (2)";
                    break;
                case 15:
                    Name = "Rontek VMC40C";
                    SafeName = "Rontek VMC40C";
                    break;
                case 16:
                    Name = "Rontek VMC90C";
                    SafeName = "Rontek VMC90C";
                    break;
                case 17:
                    Name = "Rontek HB1316";
                    SafeName = "Rontek HB1316";
                    break;
                default:
                    Name = "-//-";
                    SafeName = "---";
                    break;
            }
        }

        public int Id { get; }

        public string Name { get; }
        [JsonIgnore]
        public string SafeName { get; }
    }
}