using PublicationPlanning.Repositories;
using PublicationPlanning.StoredModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CommonTests
{
    public class SettingsFileRepositoryTest
    {
        private readonly SettingsFileRepository repository;

        public SettingsFileRepositoryTest()
        {
            repository = new SettingsFileRepository();
        }

        [Fact]
        public void GetSettings_EmptyFeedRef()
        {
            // Возвращается настройка по-умолчанию
            Feed feed = new Feed() { Id = 1 };
            Settings settings = repository.GetByFeed(feed);

            Assert.NotNull(settings);
            Assert.Equal(true, settings.ResizeImages);
            Assert.Equal(800, settings.ImageResizeWidth);
            Assert.Equal(800, settings.ImageResizeHeight);
            Assert.Equal(3, settings.ColumnsCount);
            Assert.Equal(100, settings.PageSize);
            Assert.Equal(2, settings.ImageSpacingPixels);
            Assert.Equal(80, settings.ImageCompressQuality);
        }

    }
}
