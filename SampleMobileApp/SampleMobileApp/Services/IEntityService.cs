﻿using SampleMobileApp.StoredModels;
using SampleMobileApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleMobileApp.Services
{
    public interface IEntityService<TView>
        where TView : IViewEntity
    {
        Task<IEnumerable<TView>> GetPage(int page, int limit);

        Task<TView> Get(int id);

        Task<int> Update(int id, TView entity);

        Task<int> Insert(TView entity);

        Task<bool> Delete(int id);
    }
}
