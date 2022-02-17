using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PublicationPlanning.StoredModels;

namespace PublicationPlanning.Repositories
{
    public interface ISettingsRepository : IRepository<Settings>
    {
        Settings GetByUserId(int userId);
    }


    public class SettingsFileRepository : BaseFileRepository<Settings>, ISettingsRepository
    {
        public Settings GetByUserId(int userId)
        {
            // TODO: пока user и userId нет в модели. Этот метод надо изменить, когда появится авторизация
            return allData.FirstOrDefault() ?? GetDefaultSettings();
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
                ImageCompressQuality = 80,
            };
        }
    }
}
