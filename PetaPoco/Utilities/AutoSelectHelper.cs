using System;
using System.Linq;
using System.Text.RegularExpressions;
using PetaPoco.Core;

namespace PetaPoco.Internal
{
    internal static class AutoSelectHelper
    {
        private static Regex rxSelect = new Regex(@"\A\s*(SELECT|EXECUTE|CALL|WITH|SET|DECLARE)\s",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static Regex rxFrom = new Regex(@"\A\s*FROM\s", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static string AddSelectClause<T>(IProvider provider, string sql, IMapper defaultMapper, int top = 0)
        {
            if (sql.StartsWith(";"))
                return sql.Substring(1);

            if (!rxSelect.IsMatch(sql))
            {
                var pd = PocoData.ForType(typeof(T), defaultMapper);

                string cols = GetColumnsString<T>(provider, defaultMapper, pd.TableInfo.TableName);
                string selectstr = top > 0 ? $"SELECT TOP {top}" : "SELECT";

                if (!rxFrom.IsMatch(sql)) {
                    var tableName = provider.EscapeTableName(pd.TableInfo.TableName);
                    sql = $"{selectstr} {cols} FROM {tableName} {sql}";
                }
                else {
                    sql = $"{selectstr} {cols} {sql}";
                }
            }

            return sql;
        }

        public static string GetColumnsString<T>(IProvider provider, IMapper defaultMapper, string table = null)
        {
            var pd = PocoData.ForType(typeof(T), defaultMapper);

            Func<string, string> format;
            if (!string.IsNullOrEmpty(table)) {
                var escaped = provider.EscapeSqlIdentifier(table);
                format = c => $"{escaped}.{provider.EscapeSqlIdentifier(c)}";
            }
            else {
                format = c => provider.EscapeSqlIdentifier(c);
            }

            return pd.Columns.Count != 0 ? string.Join(", ", pd.QueryColumns.Select(format)) : "NULL";
        }
    }
}