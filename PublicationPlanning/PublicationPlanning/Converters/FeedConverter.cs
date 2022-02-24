using PublicationPlanning.StoredModels;
using PublicationPlanning.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.Converters
{
    public class FeedConverter : IEntityConverter<Feed, FeedViewModel>
    {
        private readonly IEntityConverter<User, UserViewModel> userConverter;
        private readonly IEntityConverter<Settings, SettingsViewModel> settingsConverter;

        public FeedConverter(
            IEntityConverter<User, UserViewModel> userConverter,
            IEntityConverter<Settings, SettingsViewModel> settingsConverter)
        {
            this.userConverter = userConverter;
            this.settingsConverter = settingsConverter;
        }

        public Feed ConvertToStoredModel(FeedViewModel view)
        {
            if (view == null)
                return null;

            return new Feed()
            {
                Id = view.Id,
                Name = view.Name,
                Owner = userConverter.ConvertToStoredModel(view.Owner),
                Settings = settingsConverter.ConvertToStoredModel(view.Settings),
            };
        }

        public FeedViewModel ConvertToViewModel(Feed stored)
        {
            if (stored == null)
                return null;

            return new FeedViewModel()
            {
                Id = stored.Id,
                Name = stored.Name,
                Owner = userConverter.ConvertToViewModel(stored.Owner),
                Settings = settingsConverter.ConvertToViewModel(stored.Settings),
            };
        }
    }
}
