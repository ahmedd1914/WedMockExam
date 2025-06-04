using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace WedMockExam.Repository.Helpers
{
    public class UpdateCommand
    {
        private readonly Dictionary<string, object> _updates = new();
        private readonly string _tableName;
        private readonly string _idColumn;
        private readonly int _idValue;

        public UpdateCommand(string tableName, string idColumn, int idValue)
        {
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _idColumn = idColumn ?? throw new ArgumentNullException(nameof(idColumn));
            _idValue = idValue;
        }

        public UpdateCommand AddUpdate(string column, object value)
        {
            if (string.IsNullOrWhiteSpace(column))
                throw new ArgumentException("Column name cannot be empty", nameof(column));

            _updates[column] = value;
            return this;
        }

        public async Task<bool> ExecuteAsync()
        {
            if (_updates.Count == 0)
                throw new InvalidOperationException("No fields to update");

            var setClause = SqlQueryHelper.BuildSetClause(_updates);
            var query = SqlQueryHelper.BuildUpdateQuery(_tableName, setClause, _idColumn);

            var parameters = new List<SqlParameter>();
            foreach (var update in _updates)
            {
                parameters.Add(SqlQueryHelper.CreateParameter(update.Key, update.Value));
            }
            parameters.Add(SqlQueryHelper.CreateParameter(_idColumn, _idValue));

            using var connection = await ConnectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var rowsAffected = await SqlQueryHelper.ExecuteNonQueryAsync(query, parameters.ToArray());
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Error updating {_tableName}", ex);
            }
        }
    }
}
