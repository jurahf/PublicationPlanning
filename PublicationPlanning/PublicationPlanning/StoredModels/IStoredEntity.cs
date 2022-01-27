using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.StoredModels
{
    public interface IStoredEntity
    {
        int Id { get; set; } // TODO: может явно не задавать int?

        int DefaultOrder(); // для сортировки при постраничном чтении
    }
}
