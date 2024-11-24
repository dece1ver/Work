using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Infrastructure.Types
{
    public class SearchPattern
    {
        public string Value { get; }
        public string Operator { get; }

        public SearchPattern(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                Value = input;
                Operator = "LIKE";
                return;
            }

            if (input.StartsWith('='))
            {
                Value = input[1..];
                Operator = "=";
                return;
            }

            Operator = "LIKE";

            if (input.Contains('*'))
            {
                Value = input.Replace('*', '%');
                return;
            }

            Value = $"%{input}%";
        }



        public override string ToString() => $"{Operator} '{Value}'";
    }
}
