using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SharpDX;

namespace AnnotationTool.Utils
{
    public static class VectorPixelConverter
    {
        public static Vector3 GetPixelFromVector(Vector3 vector, string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return new Vector3();
            }

            FileInfo fileInfo = new FileInfo(imagePath);
            if (!fileInfo.Exists)
            {
                return new Vector3();
            }

            var image = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            int imageWidth = image.PixelWidth;
            int imageHeight = image.PixelHeight;

            double vertical = 5.0;
            double horizontal = imageWidth / (imageHeight / vertical);
            Vector2 center = new Vector2(imageWidth / 2, imageHeight / 2);
            Vector3 computedPoint = new Vector3(0);

            double computedX = Math.Abs(center.X / horizontal * vector.X);
            if (vector.X >= 0)
                computedPoint.X = Convert.ToInt32(center.X + computedX);
            else
                computedPoint.X = Convert.ToInt32(center.X - computedX);

            double computedY = Math.Abs(center.Y / vertical * vector.Y);
            if (vector.Y >= 0)
                computedPoint.Y = Convert.ToInt32(center.Y - computedY);
            else
                computedPoint.Y = Convert.ToInt32(center.Y + computedY);

            return computedPoint;
        }

        public static Vector3 GetVectorFromPixel(Vector3 vector, string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return new Vector3();
            }

            FileInfo fileInfo = new FileInfo(imagePath);
            if (!fileInfo.Exists)
            {
                return new Vector3();
            }

            var image = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            int imageWidth = image.PixelWidth;
            int imageHeight = image.PixelHeight;

            double vertical = 5.0;
            double horizontal = imageWidth / (imageHeight / vertical);
            Vector2 center = new Vector2(imageWidth / 2, imageHeight / 2);
            double computedX = Math.Abs(vector.X - center.X);
            double computedY = Math.Abs(vector.Y - center.Y);

            double computedPointX;
            if (vector.X >= center.X)
                computedPointX = computedX / (center.X / horizontal);
            else
                computedPointX = -computedX / (center.X / horizontal);

            double computedPointY;
            if (vector.Y >= center.Y)
                computedPointY = -computedY / (center.Y / vertical);
            else
                computedPointY = computedY / (center.Y / vertical);

            return new Vector3((float)computedPointX, (float)computedPointY, 0);
        }
    }
}
