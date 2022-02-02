using Foundation;
using PublicationPlanning.ImageResizer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace PublicationPlanning.iOS
{
    public class ImageResizer : IImageResizer
    {
        public async Task<byte[]> ResizeImage(byte[] imageData, int width, int height, ImageFormat format)
        {
            if (imageData == null || !imageData.Any())
                return new byte[0];

            UIImage originalImage = new UIImage(NSData.FromArray(imageData));

            var originalHeight = originalImage.Size.Height;
            var originalWidth = originalImage.Size.Width;

            nfloat newHeight = 0;
            nfloat newWidth = 0;

            if (originalHeight < height || originalWidth < width)
                return imageData;

            if (originalHeight < originalWidth) // Чтобы обрезалось по рамке. Чтобы полностью помещалось в рамку - поставить тут '>'
            {
                newHeight = height;
                nfloat ratio = originalHeight / (float)height;
                newWidth = originalWidth / ratio;
            }
            else
            {
                newWidth = width;
                nfloat ratio = originalWidth / (float)width;
                newHeight = originalHeight / ratio;
            }

            width = (int)(float)newWidth;
            height = (int)(float)newHeight;

            UIGraphics.BeginImageContext(new SizeF(width, height));
            originalImage.Draw(new RectangleF(0, 0, width, height));
            var resizedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            byte[] bytesImagen = format == ImageFormat.JPEG
                ? resizedImage.AsJPEG().ToArray()
                : resizedImage.AsPNG().ToArray();

            resizedImage.Dispose();
            return bytesImagen;
        }
    }
}