using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Infrastructure.Extensions
{
    public enum GetDoubleOption
    {

    }

    public static class Object
    {
        public static double GetDouble(this object obj, double defaulValue = 0)
        {
            if (obj is double d) return d;
            return defaulValue;
        }
    }
}
