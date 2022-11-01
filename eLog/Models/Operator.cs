using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Models
{
    public class Operator
    {
        public Operator(string lastName, string firstName = "", string patronymic = "")
        {
            LastName = lastName;
            FirstName = firstName;
            Patronymic = patronymic;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }

        [JsonIgnore]
        public string DisplayName { get
                {
                    var result = LastName;
                    if (!string.IsNullOrEmpty(FirstName))
                    {
                        result += " " + FirstName[0] + ".";
                        if (!string.IsNullOrEmpty(Patronymic))
                        {
                            result += " " + Patronymic[0] + ".";
                        }
                    }
                    return result;
                } 
            }
        
    }
}
