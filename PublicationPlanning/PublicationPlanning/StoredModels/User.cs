using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.StoredModels
{
    public class User : IStoredEntity
    {
        public int Id { get; set; }

        public int ActiveFeedId { get; set; }

        public User()
        {
        }

        public int DefaultOrder()
        {
            return Id;
        }
    }
}
