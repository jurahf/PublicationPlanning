using SampleMobileApp.StoredModels;
using SampleMobileApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SampleMobileApp.Converters
{
    public class ImageInfoConverter : IEntityConverter<ImageInfo, ImageInfoViewModel>
    {
        public ImageInfo ConvertToStoredModel(ImageInfoViewModel view)
        {
            if (view == null)
                return null;

            return new ImageInfo()
            {
                Id = view.Id,
                ImageRef = view.ImageRef,
                Order = view.Order,
                SourceType = view.SourceType
            };
        }

        public ImageInfoViewModel ConvertToViewModel(ImageInfo stored)
        {
            if (stored == null)
                return null;

            return new ImageInfoViewModel()
            {
                Id = stored.Id,
                ImageRef = stored.ImageRef,
                Order = stored.Order,
                SourceType = stored.SourceType,
                ImageSource = GetImageSource(stored.ImageRef, stored.SourceType)
            };
        }

        private ImageSource GetImageSource(string imageRef, ImageSourceType sourceType)
        {
            switch (sourceType)
            {
                case ImageSourceType.Url:
                    return ImageSource.FromUri(new Uri(imageRef));
                case ImageSourceType.FilePath:
                    return ImageSource.FromFile(imageRef);
                default:
                    throw new ArgumentException($"Source type {sourceType} is not supported.");
            }

        }

    }
}
