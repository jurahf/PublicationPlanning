using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PublicationPlanning.StoredModels;

namespace PublicationPlanning.Repositories
{
    public interface ISettingsRepository : IRepository<Settings>
    {
        Settings GetByFeed(Feed feed);
    }


    public class SettingsFileRepository : BaseFileRepository<Settings>, ISettingsRepository
    {
        public Settings GetByFeed(Feed feed)
        {
            if (feed?.Settings == null)
                return GetDefaultSettings(feed);

            return allData.FirstOrDefault(x => x.Id == feed.Settings.Id) ?? GetDefaultSettings(feed);
        }

        private Settings GetDefaultSettings(Feed feed)
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
