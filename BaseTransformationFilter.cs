using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace TogiSoft.AtlasDataBase.ArchiveWell.Perspective.Quadrilateral
{
    public abstract class BaseTransformationFilter : IFilter, IFilterInformation
    {
        public abstract Dictionary<PixelFormat, PixelFormat> FormatTranslations { get; }

        public Bitmap Apply(Bitmap image)
        {
            BitmapData srcData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, image.PixelFormat);

            Bitmap dstImage = null;

            try
            {
                dstImage = Apply(srcData);
                dstImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            }
            finally
            {
                image.UnlockBits(srcData);
            }

            return dstImage;
        }

        public Bitmap Apply(BitmapData imageData)
        {
            CheckSourceFormat(imageData.PixelFormat);

            PixelFormat dstPixelFormat = FormatTranslations[imageData.PixelFormat];

            Size newSize = CalculateNewImageSize(new UnmanagedImage(imageData));

            Bitmap dstImage = (dstPixelFormat == PixelFormat.Format8bppIndexed) ?
                CustomImage.CreateGrayscaleImage(newSize.Width, newSize.Height) :
                new Bitmap(newSize.Width, newSize.Height, dstPixelFormat);

            BitmapData dstData = dstImage.LockBits(
                new Rectangle(0, 0, newSize.Width, newSize.Height),
                ImageLockMode.ReadWrite, dstPixelFormat);

            try
            {
                ProcessFilter(new UnmanagedImage(imageData), new UnmanagedImage(dstData));
            }
            finally
            {
                dstImage.UnlockBits(dstData);
            }

            return dstImage;
        }

        public UnmanagedImage Apply(UnmanagedImage image)
        {
            CheckSourceFormat(image.PixelFormat);

            Size newSize = CalculateNewImageSize(image);

            UnmanagedImage dstImage = UnmanagedImage.Create(newSize.Width, newSize.Height, FormatTranslations[image.PixelFormat]);

            ProcessFilter(image, dstImage);

            return dstImage;
        }

        public void Apply(UnmanagedImage sourceImage, UnmanagedImage destinationImage)
        {
            CheckSourceFormat(sourceImage.PixelFormat);

            if (destinationImage.PixelFormat != FormatTranslations[sourceImage.PixelFormat])
            {
                throw new Exception();
            }

            Size newSize = CalculateNewImageSize(sourceImage);

            if ((destinationImage.Width != newSize.Width) || (destinationImage.Height != newSize.Height))
            {
                throw new Exception();
            }

            ProcessFilter(sourceImage, destinationImage);
        }

        protected abstract System.Drawing.Size CalculateNewImageSize(UnmanagedImage sourceData);


        protected abstract unsafe void ProcessFilter(UnmanagedImage sourceData, UnmanagedImage destinationData);

        private void CheckSourceFormat(PixelFormat pixelFormat)
        {
            if (!FormatTranslations.ContainsKey(pixelFormat))
                throw new Exception();
        }
    }
}
