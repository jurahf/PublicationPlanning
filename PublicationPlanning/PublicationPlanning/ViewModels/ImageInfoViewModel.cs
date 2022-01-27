using PublicationPlanning.StoredModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace PublicationPlanning.ViewModels
{
    public class ImageInfoViewModel : IViewEntity
    {
        public int Id { get; set; }

        public string ImageRef { get; set; }

        public int Order { get; set; }

        public ImageSourceType SourceType { get; set; }

        public ImageSource ImageSource { get; set; }
    }
}
