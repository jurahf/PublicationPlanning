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

            return converter.ConvertToViewModel(settings);
        }

    }
}
