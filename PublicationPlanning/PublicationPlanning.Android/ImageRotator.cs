using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PublicationPlanning.ImageTranslations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFormat = PublicationPlanning.ImageTranslations.ImageFormat;

namespace PublicationPlanning.Droid
{
    public class ImageRotator : IImageRotator
    {
        public async Task<byte[]> Rotate(byte[] imageData, ImageFormat format, float degrees)
        {
            if (imageData == null || !imageData.Any())
                return new byte[0];

            Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);

            Matrix matrix = new Matrix();
            matrix.PostRotate(degrees);
            Bitmap rotatedImage = Bitmap.CreateBitmap(originalImage, 0, 0, originalImage.Width, originalImage.Height, matrix, false);

            Bitmap.CompressFormat compressFormat = format == ImageFormat.JPEG
                ? Bitmap.CompressFormat.Jpeg
                : Bitmap.CompressFormat.Png;

            using (MemoryStream ms = new MemoryStream())
            {
                await rotatedImage.CompressAsync(compressFormat, 100, ms);
                return ms.ToArray();
            }
        }
    }
}