using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.StoredModels
{
    public class Feed : IStoredEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public User Owner { get; set; }

        public Settings Settings { get; set; }

        public Feed()
        {
        }

        public int DefaultOrder()
        {
            return Id;      // TODO: Name?
        }
    }
}
