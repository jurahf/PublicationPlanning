using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.StoredModels
{
    public class ImageInfo : IStoredEntity
    {
        public int Id { get; set; }

        public string ImageRef { get; set; }

        public int Order { get; set; }

        public Feed Feed { get; set; }

        public ImageSourceType SourceType { get; set; }

        public int DefaultOrder()
        {
            return Order;
        }
    }
}
