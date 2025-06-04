using System;
using System.Data;
using System.Data.SqlTypes;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using WedMockExam.Models;
using WedMockExam.Repository.Base;
using WedMockExam.Repository.Helpers;
using WedMockExam.Repository.Interfaces.User;

namespace WedMockExam.Repository.Implementations.User
{
    public class UserRepository : BaseRepository<Models.User>, IUserRepository
    {
        private const string IdDbFieldName = "UserId";

        protected override string GetTableName() => "Users";

        protected override string[] GetColumns() => new[]
        {
            IdDbFieldName,
            "Username",
            "Email",
            "PasswordHash",
            "FirstName",
            "LastName",
            "DateOfBirth",
            "CreatedAt",
            "IsActive"
        };

        protected override Models.User MapEntity(SqlDataReader reader)
        {
            return new Models.User
            {
                UserId = Convert.ToInt32(reader[IdDbFieldName]),
                Username = Convert.ToString(reader["Username"]),
                Email = Convert.ToString(reader["Email"]),
                PasswordHash = Convert.ToString(reader["PasswordHash"]),
                FirstName = Convert.ToString(reader["FirstName"]),
                LastName = Convert.ToString(reader["LastName"]),
                DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                IsActive = Convert.ToBoolean(reader["IsActive"])
            };
        }

        public async Task<Models.User> GetByEmailAsync(string email)
        {
            var filter = new Filter().AddCondition("Email", email);
            var users = await RetrieveCollectionAsync(filter);
            return users.FirstOrDefault();
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            var update = new UserUpdate { Password = new SqlString(newPasswordHash) };
            return await UpdateAsync(userId, update);
        }

        public async Task<int> CreateAsync(Models.User entity)
        {
            return await base.CreateAsync(entity, IdDbFieldName);
        }

        public async Task<Models.User> RetrieveAsync(int objectId)
        {
            return await base.RetrieveAsync(IdDbFieldName, objectId);
        }

        public async IAsyncEnumerable<Models.User> RetrieveCollectionAsync(UserFilter filter)
        {
            var users = await base.RetrieveCollectionAsync(filter.ToFilter());
            foreach (var user in users)
            {
                yield return user;
            }
        }

        public async Task<bool> UpdateAsync(int objectId, UserUpdate update)
        {
            var command = update.ToUpdateCommand(GetTableName(), objectId);
            return await command.ExecuteAsync();
        }

        public async Task<bool> DeleteAsync(int objectId)
        {
            return await base.DeleteAsync(IdDbFieldName, objectId);
        }
    }
}
