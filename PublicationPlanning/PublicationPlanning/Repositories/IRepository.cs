using PublicationPlanning.StoredModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PublicationPlanning.Repositories
{
    public interface IRepository<T> where T : IStoredEntity
    {
        Task<IEnumerable<T>> GetPage(int page, int limit);

        Task<T> Get(int id);

        Task<int> Update(int id, T entity);

        Task<int> Insert(T entity);

        Task<bool> Delete(int id);
    }
}
