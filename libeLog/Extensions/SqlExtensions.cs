using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libeLog.Extensions
{
    public static class SqlExtensions
    {
        public static void AddNullableParameter(this SqlParameterCollection parameters, string name, object? value, SqlDbType? type = null)
        {
            var parameter = new SqlParameter(name, value ?? DBNull.Value);
            if (type.HasValue)
                parameter.SqlDbType = type.Value;
            parameters.Add(parameter);
        }
    }
}
