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
        [JsonConstructor]
        public Operator() { }
        public Operator(Operator @operator)
        {
            _FirstName = @operator.FirstName;
            _LastName = @operator.LastName;
            _Patronymic = @operator.Patronymic;
        }

        private string _FirstName = string.Empty;
        private string _LastName = string.Empty;
        private string _Patronymic = string.Empty;

        public string FirstName
        {
            get => _FirstName; 
            set => _FirstName = value.Capitalize();
        }

        public string LastName
        {
            get => _LastName; 
            set => _LastName = value.Capitalize();
        }

        public string Patronymic
        {
            get => _Patronymic; 
            set => _Patronymic = value.Capitalize();
        }

        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                var result = LastName;
                if (string.IsNullOrEmpty(FirstName)) return result;
                result += " " + FirstName[0] + ".";
                if (!string.IsNullOrEmpty(Patronymic))
                {
                    result += " " + Patronymic[0] + ".";
                }
                return result;
            }
        }

        [JsonIgnore]
        public string FullName
        {
            get
            {
                var result = LastName;
                if (string.IsNullOrEmpty(FirstName)) return result;
                result += " " + FirstName;
                if (!string.IsNullOrEmpty(Patronymic))
                {
                    result += " " + Patronymic;
                }
                return result;
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (Operator)obj;
            return FirstName == other.FirstName
                   && LastName == other.LastName
                   && Patronymic == other.Patronymic;
        }

        public override int GetHashCode() => HashCode.Combine(FirstName, LastName, Patronymic);
    }
}
