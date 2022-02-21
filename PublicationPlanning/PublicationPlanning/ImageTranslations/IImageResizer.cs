using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PublicationPlanning.ImageTranslations
{
    public interface IImageResizer
    {
        Task<byte[]> ResizeImage(byte[] imageData, int width, int height, ImageFormat format, int quality);
    }
}
