using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.ViewModels
{
    public class FeedViewModel : IViewEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public UserViewModel Owner { get; set; }

        public SettingsViewModel Settings { get; set; }

        public FeedViewModel()
        {
        }
    }
}
