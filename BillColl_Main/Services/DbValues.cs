using System;
using System.Globalization;

namespace BillColl_Main.Services
{
    /// <summary>
    /// SQL Server implicitly converts a string parameter when comparing against an integer
    /// column. PostgreSQL does not (42883: operator does not exist: integer = text), so
    /// identifier values arriving as query strings must be narrowed before binding.
    /// </summary>
    public static class DbValues
    {
        public static object CoerceId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DBNull.Value;
            }

            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
                ? id
                : (object)value;
        }
    }
}
