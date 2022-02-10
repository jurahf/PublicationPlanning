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
        SettingsViewModel GetByUserId(int userId);
    }

    public class SettingsService : BaseEntityService<Settings, SettingsViewModel>, ISettingsService
    {
        protected readonly ISettingsRepository settingsRepository;

        public SettingsService(
            ISettingsRepository repository,
            IEntityConverter<Settings, SettingsViewModel> converter)
            : base(repository, converter)
        {
            this.settingsRepository = repository;
        }

        public SettingsViewModel GetByUserId(int userId)
        {
            Settings settings = settingsRepository.GetByUserId(userId);

            if (settings == null)
                return converter.ConvertToViewModel(GetDefaultSettings());
            else
                return converter.ConvertToViewModel(settings);
        }

        private Settings GetDefaultSettings()
        {
            return new Settings()
            {
                ResizeImages = true,
                ImageResizeWidth = 800,
                ImageResizeHeight = 800,
                ColumnsCount = 3,
                PageSize = 100,
                ImageSpacingPixels = 2,
            };
        }

    }
}
