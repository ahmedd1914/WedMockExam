using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WedMockExam.Repository.Base
{
    public interface IBaseRepository<TObj, TFilter, TUpdate>
        where TObj : class
    {
        Task<int> CreateAsync(TObj entity);
        Task<TObj> RetrieveAsync(int objectId);
        IAsyncEnumerable<TObj> RetrieveCollectionAsync(TFilter filter);
        Task<bool> UpdateAsync(int objectId, TUpdate update);
        Task<bool> DeleteAsync(int objectId);
    }
}
