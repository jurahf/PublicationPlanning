using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PublicationPlanning.ImageResizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFormat = PublicationPlanning.ImageResizer.ImageFormat;

namespace PublicationPlanning.Droid
{
    public class ImageResizer : IImageResizer
    {
        public ImageResizer()
        {
        }

        public async Task<byte[]> ResizeImage(byte[] imageData, int width, int height, ImageFormat format, int quality)
        {
            if (imageData == null || !imageData.Any())
                return new byte[0];

            // Load the bitmap
            Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);

            float newHeight = 0;
            float newWidth = 0;
            int originalHeight = originalImage.Height;
            int originalWidth = originalImage.Width;

            if (originalHeight < height || originalWidth < width)
                return imageData;

            if (originalHeight < originalWidth) // Чтобы обрезалось по рамке. Чтобы полностью помещалось в рамку - поставить тут '>'
            {
                newHeight = height;
                float ratio = originalHeight / (float)height;
                newWidth = originalWidth / ratio;
            }
            else
            {
                newWidth = width;
                float ratio = originalWidth / (float)width;
                newHeight = originalHeight / ratio;
            }

            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)newWidth, (int)newHeight, false);
            Bitmap.CompressFormat compressFormat = format == ImageFormat.JPEG
                ? Bitmap.CompressFormat.Jpeg
                : Bitmap.CompressFormat.Png;

            using (MemoryStream ms = new MemoryStream())
            {
                await resizedImage.CompressAsync(compressFormat, quality, ms);
                return ms.ToArray();
            }
        }

    }
}