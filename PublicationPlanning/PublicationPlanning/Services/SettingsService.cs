using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PublicationPlanning.Converters;
using PublicationPlanning.Repositories;
using PublicationPlanning.StoredModels;
using PublicationPlanning.ViewModels;

namespace PublicationPlanning.Services
{
    public interface ISettingsService : IEntityService<SettingsViewModel>
    {
        SettingsViewModel GetByFeed(FeedViewModel feed);
    }

    public class SettingsService : BaseEntityService<Settings, SettingsViewModel>, ISettingsService
    {
        protected readonly ISettingsRepository settingsRepository;
        protected readonly IEntityConverter<Feed, FeedViewModel> feedConverter;

        public SettingsService(
            ISettingsRepository repository,
            IEntityConverter<Settings, SettingsViewModel> converter,
            IEntityConverter<Feed, FeedViewModel> feedConverter)
            : base(repository, converter)
        {
            this.settingsRepository = repository;
            this.feedConverter = feedConverter;
        }

        public SettingsViewModel GetByFeed(FeedViewModel feed)
        {
            Settings settings = settingsRepository.GetByFeed(feedConverter.ConvertToStoredModel(feed));

            return converter.ConvertToViewModel(settings);
        }

    }
}
