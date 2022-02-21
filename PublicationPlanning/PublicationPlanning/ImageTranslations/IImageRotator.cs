using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PublicationPlanning.ImageTranslations
{
    public interface IImageRotator
    {
        Task<byte[]> Rotate(byte[] imageData, ImageFormat format, float degrees);
    }
}
