using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace TogiSoft.AtlasDataBase.ArchiveWell.Perspective.Quadrilateral
{
    /// <summary>
    /// Базовый класс для фильтров, которые могут создавать новое изображение
    /// другого размера в виде результат обработки изображения.
    /// </summary>
    internal abstract class BaseTransformationFilter
    {
        /// <summary>
        /// Формат словаря смещений
        /// </summary>
        public abstract Dictionary<PixelFormat, PixelFormat> FormatTranslations { get; }

        /// <summary>
        /// Применить фильтр к изображению
        /// </summary>
        /// <param name="image"> Исходное изображение, к которому нужно применить фильтр </param>
        /// <returns> Возвращает результат фильтра, полученный путем применения фильтра к исходному изображению </returns>
        public Bitmap Apply(Bitmap image)
        {
            var srcData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);

            Bitmap dstImage = null;

            try
            {
                dstImage = Apply(srcData);
                dstImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            }
            catch
            {
                throw new Exception("Не удаётся применить фильтр к изображению");
            }
            finally
            {
                image.UnlockBits(srcData);
            }

            return dstImage;
        }

        /// <summary>
        /// Применить фильтр к изображению
        /// </summary>
        /// <param name="imageData"> Исходное изображение, к которому нужно применить фильтр </param>
        /// <returns> Возвращает результат фильтра, полученный путем применения фильтра к исходному изображению </returns>
        public Bitmap Apply(BitmapData imageData)
        {
            CheckSourceFormat(imageData.PixelFormat);

            var dstPixelFormat = FormatTranslations[imageData.PixelFormat];

            var newSize = CalculateNewImageSize(new UnmanagedImage(imageData));

            var dstImage = new Bitmap(newSize.Width, newSize.Height, dstPixelFormat);

            var dstData = dstImage.LockBits(new Rectangle(0, 0, newSize.Width, newSize.Height), ImageLockMode.ReadWrite, dstPixelFormat);

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

        /// <summary>
        /// Применить фильтр к изображению в неуправляемой памяти
        /// </summary>
        /// <param name="image"> Исходное изображение в неуправляемой памяти, к которому нужно применить фильтр </param>
        /// <returns> Возвращает результат фильтра, полученный путем применения фильтра к исходному изображению </returns>
        public UnmanagedImage Apply(UnmanagedImage image)
        {
            CheckSourceFormat(image.PixelFormat);

            var newSize = CalculateNewImageSize(image);

            var dstImage = UnmanagedImage.Create(newSize.Width, newSize.Height, FormatTranslations[image.PixelFormat]);

            ProcessFilter(image, dstImage);

            return dstImage;
        }

        /// <summary>
        /// Применение фильтра к изображению в неуправляемой памяти
        /// </summary>
        /// <param name="sourceImage"> Исходное изображение в неуправляемой памяти, к которому нужно применить фильтр </param>
        /// <param name="destinationImage"> Образ назначения в неуправляемой памяти для помещения результата </param>
        public void Apply(UnmanagedImage sourceImage, UnmanagedImage destinationImage)
        {
            CheckSourceFormat(sourceImage.PixelFormat);

            if (destinationImage.PixelFormat != FormatTranslations[sourceImage.PixelFormat])
            {
                throw new Exception("Не удаётся применить фильтр к изображению");
            }

            var newSize = CalculateNewImageSize(sourceImage);

            if ((destinationImage.Width != newSize.Width) || (destinationImage.Height != newSize.Height))
            {
                throw new Exception("Не удаётся применить фильтр к изображению");
            }

            ProcessFilter(sourceImage, destinationImage);
        }

        /// <summary>
        /// Высисление нового размера изображения
        /// </summary>
        /// <param name="sourceData"> Данные исходного изображения </param>
        /// <returns> Размер нового изображения — размер целевого изображения. </returns>
        protected abstract Size CalculateNewImageSize(UnmanagedImage sourceData);

        /// <summary>
        /// Обработка фильтра на указанном изображении
        /// </summary>
        /// <param name="sourceData"> Данные исходного изображения </param>
        /// <param name="destinationData"> Указатель на данные изображения </param>
        protected abstract unsafe void ProcessFilter(UnmanagedImage sourceData, UnmanagedImage destinationData);

        /// <summary>
        /// Проверить формат пикселей исходного изображения
        /// </summary>
        /// <param name="pixelFormat"> Формат пикселя </param>
        private void CheckSourceFormat(PixelFormat pixelFormat)
        {
            if (!FormatTranslations.ContainsKey(pixelFormat))
            {
                throw new Exception("Не удаётся применить фильтр к изображению");
            }
        }
    }
}