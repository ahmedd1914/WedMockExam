using Microsoft.Data.SqlClient;
using WedMockExam.Repository.Base;
using WedMockExam.Repository.Helpers;
using WedMockExam.Repository.Interfaces.PreferredLocation;

namespace WedMockExam.Repository.Implementations.PreferredLocation
{
    public class PreferredLocationRepository : BaseRepository<Models.PreferredLocation>, IPreferredLocationRepository
    {
        private const string IdDbFieldName = "UserId";

        protected override string GetTableName()
        {
            return "PreferredLocations";
        }

        protected override string[] GetColumns() => new[] {
            "UserId",
            "WorkplaceId",
            "PreferenceRank"
        };

        protected override Models.PreferredLocation MapEntity(SqlDataReader reader)
        {
            return new Models.PreferredLocation
            {
                UserId = Convert.ToInt32(reader["UserId"]),
                WorkplaceId = Convert.ToInt32(reader["WorkplaceId"]),
                PreferenceRank = Convert.ToInt32(reader["PreferenceRank"])
            };
        }

        public Task<Models.PreferredLocation> RetrieveAsync(int objectId)
        {
            return Task.FromResult<Models.PreferredLocation>(null); // Mapping tables don't use single ID retrieval
        }

        public Task<int> CreateAsync(Models.PreferredLocation entity)
        {
            return base.CreateAsync(entity);
        }

        public async IAsyncEnumerable<Models.PreferredLocation> RetrieveCollectionAsync(PreferredLocationFilter filter)
        {
            Filter commandFilter = new Filter();

            if (filter.UserId.HasValue)
            {
                commandFilter.AddCondition("UserId", filter.UserId.Value);
            }

            if (filter.WorkplaceId.HasValue)
            {
                commandFilter.AddCondition("WorkplaceId", filter.WorkplaceId.Value);
            }

            if (filter.PreferenceRank.HasValue)
            {
                commandFilter.AddCondition("PreferenceRank", filter.PreferenceRank.Value);
            }

            var results = await base.RetrieveCollectionAsync(commandFilter);
            foreach (var result in results)
            {
                yield return result;
            }
        }

        public async Task<bool> UpdateAsync(int objectId, PreferredLocationUpdate update)
        {
            if (!update.WorkplaceId.HasValue)
            {
                throw new ArgumentException("WorkplaceId is required for updating preferred location.");
            }

            var query = $"UPDATE {GetTableName()} SET PreferenceRank = @PreferenceRank WHERE UserId = @UserId AND WorkplaceId = @WorkplaceId";
            var parameters = new[]
            {
                SqlQueryHelper.CreateParameter("UserId", objectId),
                SqlQueryHelper.CreateParameter("WorkplaceId", update.WorkplaceId.Value),
                SqlQueryHelper.CreateParameter("PreferenceRank", update.PreferenceRank.Value)
            };

            try
            {
                var result = await SqlQueryHelper.ExecuteNonQueryAsync(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Error updating entity in {GetTableName()}", ex);
            }
        }

        public async Task<bool> DeleteAsync(int objectId)
        {
            return await base.DeleteAsync(IdDbFieldName, objectId);
        }

        public async Task<bool> DeletePreferredLocationAsync(int userId, int workplaceId)
        {
            var query = $"DELETE FROM {GetTableName()} WHERE UserId = @UserId AND WorkplaceId = @WorkplaceId";
            var parameters = new[]
            {
                SqlQueryHelper.CreateParameter("UserId", userId),
                SqlQueryHelper.CreateParameter("WorkplaceId", workplaceId)
            };

            try
            {
                var result = await SqlQueryHelper.ExecuteNonQueryAsync(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Error deleting entity from {GetTableName()}", ex);
            }
        }
    }
}

