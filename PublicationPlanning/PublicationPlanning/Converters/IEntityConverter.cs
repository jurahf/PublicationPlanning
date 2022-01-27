using PublicationPlanning.StoredModels;
using PublicationPlanning.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.Converters
{
    public interface IEntityConverter<TStored, TView> 
        where TStored : IStoredEntity
        where TView : IViewEntity
    {
        TView ConvertToViewModel(TStored stored);

        TStored ConvertToStoredModel(TView view);
    }
}
