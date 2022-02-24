using PublicationPlanning.StoredModels;
using PublicationPlanning.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.Converters
{
    public class UserConverter : IEntityConverter<User, UserViewModel>
    {
        public User ConvertToStoredModel(UserViewModel view)
        {
            if (view == null)
                return null;

            return new User()
            {
                Id = view.Id,
                ActiveFeedId = view.ActiveFeedId,
            };
        }

        public UserViewModel ConvertToViewModel(User stored)
        {
            if (stored == null)
                return null;

            return new UserViewModel()
            { 
                Id = stored.Id,
                ActiveFeedId = stored.ActiveFeedId
            };
        }
    }
}
