using SampleMobileApp.StoredModels;
using SampleMobileApp.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using SampleMobileApp.ViewModels;
using SampleMobileApp.Converters;
using System.Linq;
using System.Threading.Tasks;

namespace SampleMobileApp.Services
{
    public abstract class BaseEntityService<TStored, TView> : IEntityService<TView> 
        where TStored : IStoredEntity
        where TView : IViewEntity
    {
        protected readonly IRepository<TStored> repository;
        protected readonly IEntityConverter<TStored, TView> converter;

        public BaseEntityService(
            IRepository<TStored> repository,
            IEntityConverter<TStored, TView> converter)
        {
            this.repository = repository;
            this.converter = converter;
        }

        public virtual async Task<bool> Delete(int id)
        {
            return await repository.Delete(id);
        }

        public virtual async Task<TView> Get(int id)
        {
            return converter.ConvertToViewModel(await repository.Get(id));
        }

        public virtual async Task<IEnumerable<TView>> GetPage(int page, int limit)
        {
            return (await repository.GetPage(page, limit))
                .Select(x => converter.ConvertToViewModel(x));
        }

        public virtual async Task<int> Insert(TView entity)
        {
            return await repository.Insert(converter.ConvertToStoredModel(entity));
        }

        public virtual async Task<int> Update(int id, TView entity)
        {
            return await repository.Update(id, converter.ConvertToStoredModel(entity));
        }
    }
}
