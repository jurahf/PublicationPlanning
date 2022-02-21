using Foundation;
using PublicationPlanning.ImageTranslations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace PublicationPlanning.iOS
{
    public class ImageRotator : IImageRotator
    {
        public async Task<byte[]> Rotate(byte[] imageData, ImageFormat format, float degrees)
        {
            if (imageData == null || !imageData.Any())
                return new byte[0];

            UIImage originalImage = new UIImage(NSData.FromArray(imageData));

            float radians = degrees * (float)Math.PI / 180;
            float width = (float)originalImage.Size.Width;
            float height = (float)originalImage.Size.Height;

            UIGraphics.BeginImageContext(new SizeF(width, height));
            originalImage.Draw(new RectangleF(0, 0, width, height));
            CoreGraphics.CGContext context = UIGraphics.GetCurrentContext();
            // возможно, этого не достаточно, и надо делать CGAffineTransform.Rotate
            context.RotateCTM(radians);
            var result = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            byte[] bytesImagen = format == ImageFormat.JPEG
                ? result.AsJPEG(1f).ToArray()
                : result.AsPNG().ToArray();

            result.Dispose();
            return bytesImagen;
        }
    }
}