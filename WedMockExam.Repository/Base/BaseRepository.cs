using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using WedMockExam.Repository.Helpers;

namespace WedMockExam.Repository.Base
{
    public class RepositoryException : Exception
    {
        public RepositoryException(string message, Exception inner = null) : base(message, inner) { }
    }

    public abstract class BaseRepository<TObj>
    {
        protected abstract string GetTableName();
        protected abstract string[] GetColumns();
        protected abstract TObj MapEntity(SqlDataReader reader);
        protected virtual string GetIdColumnName() => "Id";

        protected async Task<int> CreateAsync(TObj entity, string idDbFieldEnumeratorName = null)
        {
            try
            {
                var properties = typeof(TObj).GetProperties()
                    .Where(p => p.Name != idDbFieldEnumeratorName)
                    .ToList();

                string columns = string.Join(", ", properties.Select(p => p.Name));
                string parameters = string.Join(", ", properties.Select(p => "@" + p.Name));

                var query = SqlQueryHelper.BuildInsertQuery(GetTableName(), columns, parameters);
                var sqlParams = properties.Select(p => 
                    SqlQueryHelper.CreateParameter(p.Name, p.GetValue(entity))).ToArray();

                return await SqlQueryHelper.ExecuteScalarAsync<int>(query, sqlParams);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Error creating entity in {GetTableName()}", ex);
            }
        }

        protected async Task<TObj> RetrieveAsync(string idDbFieldName, int idDbFieldValue)
        {
            try
            {
                var columns = string.Join(", ", GetColumns());
                var query = SqlQueryHelper.BuildSelectByIdQuery(columns, GetTableName(), idDbFieldName);
                var parameter = SqlQueryHelper.CreateParameter(idDbFieldName, idDbFieldValue);

                using var reader = await SqlQueryHelper.ExecuteReaderAsync(query, new[] { parameter });
                
                if (!reader.Read())
                {
                    return default;
                }

                var result = MapEntity(reader);
                return result;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Error retrieving entity from {GetTableName()}", ex);
            }
        }

        protected async Task<List<TObj>> RetrieveCollectionAsync(Filter filter = null)
        {
            try
            {
                var columns = string.Join(", ", GetColumns());
                var query = SqlQueryHelper.BuildSelectAllQuery(columns, GetTableName());
                var parameters = new List<SqlParameter>();

                if (filter?.Conditions?.Any() == true)
                {
                    var conditions = filter.Conditions.Select(c => $"{c.Key} = @{c.Key}");
                    query += SqlQueryHelper.BuildWhereClause(string.Join(SqlQueryHelper.And, conditions));
                    parameters.AddRange(filter.Conditions.Select(c => 
                        SqlQueryHelper.CreateParameter(c.Key, c.Value)));
                }

                var results = new List<TObj>();
                using var reader = await SqlQueryHelper.ExecuteReaderAsync(query, parameters.ToArray());
                while (await reader.ReadAsync())
                {
                    results.Add(MapEntity(reader));
                }
                return results;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Error retrieving collection from {GetTableName()}", ex);
            }
        }

        protected async Task<bool> DeleteAsync(string idDbFieldName, int idDbFieldValue)
        {
            using var connection = await ConnectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();
            
            try
            {
                var query = SqlQueryHelper.BuildDeleteQuery(GetTableName(), idDbFieldName);
                var parameter = SqlQueryHelper.CreateParameter(idDbFieldName, idDbFieldValue);

                var rowsAffected = await SqlQueryHelper.ExecuteNonQueryAsync(query, new[] { parameter });
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new RepositoryException($"Error deleting entity from {GetTableName()}", ex);
            }
        }
    }
}
