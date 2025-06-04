using Microsoft.Data.SqlClient;
using WedMockExam.Repository.Base;
using WedMockExam.Repository.Helpers;
using WedMockExam.Repository.Interfaces.Reservation;

namespace WedMockExam.Repository.Implementations.Reservation
{
    public class ReservationRepository : BaseRepository<Models.Reservation>, IReservationRepository
    {
        private const string IdDbFiledEnumeratorName = "ReservationId";

        protected override string GetTableName(){
            return "Reservations";
        }
        protected override string[] GetColumns() => new []{
            IdDbFiledEnumeratorName,
            "UserId",
            "WorkplaceId",
            "BookingDate",
            "IsCancelled"
        };

        protected override Models.Reservation MapEntity(SqlDataReader reader){
            return new Models.Reservation{
                ReservationId = Convert.ToInt32(reader[IdDbFiledEnumeratorName]),
                UserId = Convert.ToInt32(reader["UserId"]),
                WorkplaceId = Convert.ToInt32(reader["WorkplaceId"]),
                BookingDate = Convert.ToDateTime(reader["BookingDate"]),
                IsCancelled = Convert.ToBoolean(reader["IsCancelled"])
            };
        }

        public Task<int> CreateAsync(Models.Reservation entity){
            return base.CreateAsync(entity, IdDbFiledEnumeratorName);
        }

        public Task<Models.Reservation> RetrieveAsync(int objectId){
            return base.RetrieveAsync(IdDbFiledEnumeratorName, objectId);
        }

        public async IAsyncEnumerable<Models.Reservation> RetrieveCollectionAsync(ReservationFilter filter){
            Filter commandFilter = new Filter();

            if(filter.UserId.HasValue){
                commandFilter.AddCondition("UserId", filter.UserId.Value);
            }

            if(filter.WorkplaceId.HasValue){
                commandFilter.AddCondition("WorkplaceId", filter.WorkplaceId.Value);
            }

            if(filter.BookingDate.HasValue){
                commandFilter.AddCondition("BookingDate", filter.BookingDate.Value);
            }

            var results = await base.RetrieveCollectionAsync(commandFilter);
            foreach (var result in results)
            {
                yield return result;
            }
        }

        public async Task<bool> UpdateAsync(int objectId, ReservationUpdate update){
            UpdateCommand updateCommand = new UpdateCommand(GetTableName(), IdDbFiledEnumeratorName, objectId);

            updateCommand.AddUpdate("UserId", update.UserId)
                .AddUpdate("WorkplaceId", update.WorkplaceId)
                .AddUpdate("BookingDate", update.BookingDate)
                .AddUpdate("IsCancelled", update.IsCancelled);

            return await updateCommand.ExecuteAsync();
        }

        public Task<bool> DeleteAsync(int objectId){
            return base.DeleteAsync(IdDbFiledEnumeratorName, objectId);
        }
    }
}
