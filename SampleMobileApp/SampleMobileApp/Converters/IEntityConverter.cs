using SampleMobileApp.StoredModels;
using SampleMobileApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleMobileApp.Converters
{
    public interface IEntityConverter<TStored, TView> 
        where TStored : IStoredEntity
        where TView : IViewEntity
    {
        TView ConvertToViewModel(TStored stored);

        TStored ConvertToStoredModel(TView view);
    }
}
