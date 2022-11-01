using eLog.Infrastructure.Extensions;
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
        private string firstName;
        private string lastName;
        private string patronymic;

        public string FirstName { get => firstName; set => firstName = value.Capitalize(); }
        public string LastName { get => lastName; set => lastName = value.Capitalize(); }
        public string Patronymic { get => patronymic; set => patronymic = value.Capitalize(); }

        [JsonIgnore]
        public string DisplayName
        {
            get
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
