using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHAP_Manager
{
    internal static class BitmapExtensions
    {
        public static Bitmap RotateBitmap(this Bitmap source, float angle)
        {
            float radians = (float)(angle * Math.PI / 180.0);
            float cos = Math.Abs((float)Math.Cos(radians));
            float sin = Math.Abs((float)Math.Sin(radians));
            int newWidth = (int)(source.Width * cos + source.Height * sin);
            int newHeight = (int)(source.Width * sin + source.Height * cos);

            Bitmap rotatedBitmap = new Bitmap(newWidth, newHeight);

            using (Graphics g = Graphics.FromImage(rotatedBitmap))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.TranslateTransform(newWidth / 2.0f, newHeight / 2.0f);
                g.RotateTransform(angle);
                g.DrawImage(source, -source.Width / 2.0f, -source.Height / 2.0f);
            }

            return rotatedBitmap;
        }
        public static Bitmap Tint(this Bitmap original, Color tint)
        {
            Bitmap tintedBitmap = new Bitmap(original.Width, original.Height);

            // Create the color matrix
            float[][] colorMatrixElements = {
            new float[] {tint.R / 255f, 0, 0, 0, 0},
            new float[] {0, tint.G / 255f, 0, 0, 0},
            new float[] {0, 0, tint.B / 255f, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {0, 0, 0, 0, 1}
        };

            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);

            // Create image attributes
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            // Draw the original image on the new image using the color matrix
            using (Graphics graphics = Graphics.FromImage(tintedBitmap))
            {
                graphics.DrawImage(
                    original,
                    new Rectangle(0, 0, original.Width, original.Height),
                    0, 0, original.Width, original.Height,
                    GraphicsUnit.Pixel,
                    imageAttributes
                );
            }

            return tintedBitmap;
        }
    }
}
