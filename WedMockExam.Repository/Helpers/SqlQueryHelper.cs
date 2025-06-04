using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace WedMockExam.Repository.Helpers
{
    public static class SqlQueryHelper
    {
        public const string SelectAll = "SELECT {0} FROM {1}";
        public const string SelectById = "SELECT {0} FROM {1} WHERE {2} = @{2}";
        public const string Insert = "INSERT INTO {0} ({1}) VALUES ({2}); SELECT SCOPE_IDENTITY();";
        public const string Update = "UPDATE {0} SET {1} WHERE {2} = @{2}";
        public const string Delete = "DELETE FROM {0} WHERE {1} = @{1}";
        public const string Where = " WHERE {0}";
        public const string And = " AND ";
        


        private static readonly Dictionary<Type, SqlDbType> TypeToSqlDbType = new()
        {
            { typeof(int), SqlDbType.Int },
            { typeof(long), SqlDbType.BigInt },
            { typeof(string), SqlDbType.NVarChar },
            { typeof(DateTime), SqlDbType.DateTime2 },
            { typeof(bool), SqlDbType.Bit },
            { typeof(decimal), SqlDbType.Decimal },
            { typeof(double), SqlDbType.Float },
            { typeof(Guid), SqlDbType.UniqueIdentifier }
        };

        public static string BuildSelectAllQuery(string columns, string tableName)
        {
            if (string.IsNullOrWhiteSpace(columns)) throw new ArgumentException("Columns cannot be empty", nameof(columns));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be empty", nameof(tableName));
            
            return string.Format(SelectAll, columns, tableName);
        }

        public static string BuildSelectByIdQuery(string columns, string tableName, string idColumn)
        {
            if (string.IsNullOrWhiteSpace(columns)) throw new ArgumentException("Columns cannot be empty", nameof(columns));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be empty", nameof(tableName));
            if (string.IsNullOrWhiteSpace(idColumn)) throw new ArgumentException("ID column cannot be empty", nameof(idColumn));
            
            return string.Format(SelectById, columns, tableName, idColumn);
        }

        public static string BuildInsertQuery(string tableName, string columns, string parameters)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be empty", nameof(tableName));
            if (string.IsNullOrWhiteSpace(columns)) throw new ArgumentException("Columns cannot be empty", nameof(columns));
            if (string.IsNullOrWhiteSpace(parameters)) throw new ArgumentException("Parameters cannot be empty", nameof(parameters));
            
            return string.Format(Insert, tableName, columns, parameters);
        }

        public static string BuildUpdateQuery(string tableName, string setClause, string idColumn)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be empty", nameof(tableName));
            if (string.IsNullOrWhiteSpace(setClause)) throw new ArgumentException("SET clause cannot be empty", nameof(setClause));
            if (string.IsNullOrWhiteSpace(idColumn)) throw new ArgumentException("ID column cannot be empty", nameof(idColumn));
            
            return string.Format(Update, tableName, setClause, idColumn);
        }

        public static string BuildDeleteQuery(string tableName, string idColumn)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be empty", nameof(tableName));
            if (string.IsNullOrWhiteSpace(idColumn)) throw new ArgumentException("ID column cannot be empty", nameof(idColumn));
            
            return string.Format(Delete, tableName, idColumn);
        }

        public static string BuildWhereClause(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) throw new ArgumentException("Condition cannot be empty", nameof(condition));
            return string.Format(Where, condition);
        }

        public static string BuildSetClause(Dictionary<string, object> updates)
        {
            if (updates == null || updates.Count == 0)
                throw new ArgumentException("Updates dictionary cannot be null or empty", nameof(updates));

            var setClauses = new List<string>();
            foreach (var update in updates)
            {
                if (string.IsNullOrWhiteSpace(update.Key))
                    throw new ArgumentException("Update key cannot be empty");
                    
                setClauses.Add($"{update.Key} = @{update.Key}");
            }
            return string.Join(", ", setClauses);
        }

        public static async Task<T> ExecuteScalarAsync<T>(string query, SqlParameter[] parameters = null)
        {
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException("Query cannot be empty", nameof(query));

            using var connection = await ConnectionFactory.CreateConnectionAsync();
            using var command = new SqlCommand(query, connection);
            
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            try
            {
                var result = await command.ExecuteScalarAsync();
                return result == DBNull.Value ? default : (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing scalar query: {ex.Message}", ex);
            }
        }

        public static async Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null)
        {
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException("Query cannot be empty", nameof(query));

            using var connection = await ConnectionFactory.CreateConnectionAsync();
            using var command = new SqlCommand(query, connection);
            
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            try
            {
                return await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing non-query: {ex.Message}", ex);
            }
        }

        public static async Task<SqlDataReader> ExecuteReaderAsync(string query, SqlParameter[] parameters = null)
        {
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException("Query cannot be empty", nameof(query));

            var connection = await ConnectionFactory.CreateConnectionAsync();
            var command = new SqlCommand(query, connection);
            
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            try
            {
                return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                throw new Exception($"Error executing reader: {ex.Message}", ex);
            }
        }
        public static SqlParameter CreateParameter(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter name cannot be empty", nameof(name));

            var parameter = new SqlParameter(name, value ?? DBNull.Value);
            
            if (value != null && TypeToSqlDbType.TryGetValue(value.GetType(), out var sqlDbType))
            {
                parameter.SqlDbType = sqlDbType;
            }

            return parameter;
        }
    }
}
