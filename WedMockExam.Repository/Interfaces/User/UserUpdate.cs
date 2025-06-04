using System.Data.SqlTypes;
using WedMockExam.Repository.Helpers;

namespace WedMockExam.Repository.Interfaces.User
{
    public class UserUpdate
    {
        public SqlString? FullName { get; set; }
        public SqlString? Password { get; set; }

        public UpdateCommand ToUpdateCommand(string tableName, int userId)
        {
            var command = new UpdateCommand(tableName, "Id", userId);

            if (FullName.HasValue && !FullName.Value.IsNull)
                command.AddUpdate("FullName", FullName.Value.Value);

            if (Password.HasValue && !Password.Value.IsNull)
                command.AddUpdate("Password", Password.Value.Value);

            return command;
        }
    }
}
