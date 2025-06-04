using Microsoft.Data.SqlClient;
using WedMockExam.Repository.Base;
using WedMockExam.Repository.Helpers;
using WedMockExam.Repository.Interfaces.Workplace;
using System.Collections.Generic;
using System.Linq;

namespace WedMockExam.Repository.Implementations.Workplace
{
    public class WorkplaceRepository : BaseRepository<Models.Workplace> , IWorkplaceRepository
    {
        private const string IdDbFiledEnumeratorName = "WorkplaceId";

        protected override string GetTableName(){
            return "Workplaces";
        }
        protected override string[] GetColumns() => new []{
            IdDbFiledEnumeratorName,
            "Floor",
            "Zone",
            "HasMonitor",
            "HasDocking",
            "HasWindow",    
            "HasPrinter"
        };

        protected override Models.Workplace MapEntity(SqlDataReader reader){
            return new Models.Workplace{
                WorkplaceId = Convert.ToInt32(reader[IdDbFiledEnumeratorName]),
                Floor = Convert.ToInt32(reader["Floor"]),
                Zone = reader["Zone"] as string,
                HasMonitor = Convert.ToBoolean(reader["HasMonitor"]),
                HasDocking = Convert.ToBoolean(reader["HasDocking"]),
                HasWindow = Convert.ToBoolean(reader["HasWindow"]),
                HasPrinter = Convert.ToBoolean(reader["HasPrinter"])
            };
        }

        public Task<int> CreateAsync(Models.Workplace entity){
            return base.CreateAsync(entity, IdDbFiledEnumeratorName);
        }

        public Task<Models.Workplace> RetrieveAsync(int objectId){
            return base.RetrieveAsync(IdDbFiledEnumeratorName, objectId);
        }

        public async IAsyncEnumerable<Models.Workplace> RetrieveCollectionAsync(WorkplaceFilter filter){
            Filter commandFilter = new Filter();

            if(filter.Floor.HasValue){
                commandFilter.AddCondition("Floor", filter.Floor.Value);
            }

            if(filter.Zone.HasValue){
                commandFilter.AddCondition("Zone", filter.Zone.Value);
            }

            if(filter.HasMonitor.HasValue){
                commandFilter.AddCondition("HasMonitor", filter.HasMonitor.Value);
            }

            if(filter.HasDocking.HasValue){
                commandFilter.AddCondition("HasDocking", filter.HasDocking.Value);
            }

            if(filter.HasWindow.HasValue){
                commandFilter.AddCondition("HasWindow", filter.HasWindow.Value);
            }

            if(filter.HasPrinter.HasValue){
                commandFilter.AddCondition("HasPrinter", filter.HasPrinter.Value);
            }

            var results = await base.RetrieveCollectionAsync(commandFilter);
            foreach (var result in results)
            {
                yield return result;
            }
        }

        public async Task<bool> UpdateAsync(int objectId, WorkplaceUpdate update){
            UpdateCommand updateCommand = new UpdateCommand(GetTableName(), IdDbFiledEnumeratorName, objectId);

            updateCommand.AddUpdate("Floor", update.Floor)
                .AddUpdate("Zone", update.Zone)
                .AddUpdate("HasMonitor", update.HasMonitor)
                .AddUpdate("HasDocking", update.HasDocking)
                .AddUpdate("HasWindow", update.HasWindow)
                .AddUpdate("HasPrinter", update.HasPrinter);

            return await updateCommand.ExecuteAsync();
        }

        public Task<bool> DeleteAsync(int objectId){
            return base.DeleteAsync(IdDbFiledEnumeratorName, objectId);
        }      
    }
}

