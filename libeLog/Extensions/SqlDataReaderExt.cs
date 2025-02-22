using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libeLog.Extensions
{
    public static class SqlDataReaderExt
    {
        public static bool? GetNullableBoolean(this SqlDataReader reader, int ordinal)
        {
            if (!reader.IsDBNull(ordinal))
            {
                return reader.GetBoolean(ordinal);
            }
            return null;
        }

        public static double? GetNullableDouble(this SqlDataReader reader, int ordinal)
        {
            if (!reader.IsDBNull(ordinal))
            {
                return reader.GetDouble(ordinal);
            }
            return null;
        }

        public static string GetStringOrEmpty(this SqlDataReader reader, int index) =>
            reader.IsDBNull(index) ? string.Empty : reader.GetString(index);

        public static DateTime GetDateTimeOrMinValue(this SqlDataReader reader, int index) =>
            reader.IsDBNull(index) ? DateTime.MinValue : reader.GetDateTime(index);
    }
}
