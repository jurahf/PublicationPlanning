using System;
using System.Collections.Generic;
using System.Text;

namespace SampleMobileApp.StoredModels
{
    public class ImageInfo : IStoredEntity
    {
        public int Id { get; set; }

        public string ImageRef { get; set; }

        public int Order { get; set; }

        public ImageSourceType SourceType { get; set; }

        public int DefaultOrder()
        {
            return Order;
        }
    }
}
