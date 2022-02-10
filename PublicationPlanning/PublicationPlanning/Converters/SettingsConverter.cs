using System;
using System.Collections.Generic;
using System.Text;
using PublicationPlanning.StoredModels;
using PublicationPlanning.ViewModels;

namespace PublicationPlanning.Converters
{
    public class SettingsConverter : IEntityConverter<Settings, SettingsViewModel>
    {
        public Settings ConvertToStoredModel(SettingsViewModel view)
        {
            if (view == null)
                return null;

            return new Settings()
            {
                Id = view.Id,
                ColumnsCount = view.ColumnsCount,
                PageSize = view.PageSize,
                ImageSpacingPixels = view.ImageSpacingPixels,
                ResizeImages = view.ResizeImages,
                ImageResizeHeight = view.ImageResizeHeight,
                ImageResizeWidth = view.ImageResizeWidth,
            };
        }

        public SettingsViewModel ConvertToViewModel(Settings stored)
        {
            if (stored == null)
                return null;

            return new SettingsViewModel()
            {
                Id = stored.Id,
                ColumnsCount = stored.ColumnsCount,
                PageSize = stored.PageSize,
                ImageSpacingPixels = stored.ImageSpacingPixels,
                ResizeImages = stored.ResizeImages,
                ImageResizeHeight = stored.ImageResizeHeight,
                ImageResizeWidth = stored.ImageResizeWidth,
            };
        }

    }
}
