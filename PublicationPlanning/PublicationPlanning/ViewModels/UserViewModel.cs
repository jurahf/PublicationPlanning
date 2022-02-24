using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.ViewModels
{
    public class UserViewModel : IViewEntity
    {
        public int Id { get; set; }

        public int ActiveFeedId { get; set; }

        public UserViewModel()
        {
        }
    }
}
