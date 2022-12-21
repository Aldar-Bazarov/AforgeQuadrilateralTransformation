using System;
using System.Drawing.Imaging;

namespace TogiSoft.AtlasDataBase.ArchiveWell.Perspective.Quadrilateral
{
    /// <summary>
    /// Изображение в неуправляемой памяти
    /// </summary>
    internal class UnmanagedImage
    {
        /// <summary>
        /// Указатель на данные изображения в неуправляемой памяти
        /// </summary>
        private IntPtr imageData;

        /// <summary>
        /// Размер изображения
        /// </summary>
        private int width, height;

        /// <summary>
        /// Шаг изображения (размер линии)
        /// </summary>
        private int stride;

        /// <summary>
        /// Пиксельный формат изображения
        /// </summary>
        private PixelFormat pixelFormat;

        /// <summary>
        /// Инициализация экземпляра
        /// </summary>
        /// <param name="imageData">Указатель на данные изображения в неуправляемой памяти</param>
        /// <param name="width">Ширина изображения в пикселях</param>
        /// <param name="height">Высота изображения в пикселях</param>
        /// <param name="stride">Шаг изображения (размер линии)</param>
        /// <param name="pixelFormat">Пиксельный формат изображения</param>
        public UnmanagedImage(IntPtr imageData, int width, int height, int stride, PixelFormat pixelFormat)
        {
            this.imageData = imageData;
            this.width = width;
            this.height = height;
            this.stride = stride;
            this.pixelFormat = pixelFormat;
        }

        /// <summary>
        /// Инициализация экземпляра
        /// </summary>
        /// <param name="bitmapData">Заблокированные растровые данные</param>
        public UnmanagedImage(BitmapData bitmapData)
        {
            this.imageData = bitmapData.Scan0;
            this.width = bitmapData.Width;
            this.height = bitmapData.Height;
            this.stride = bitmapData.Stride;
            this.pixelFormat = bitmapData.PixelFormat;
        }

        /// <summary>
        /// Указатель на данные изображения в неуправляемой памяти
        /// </summary>
        public IntPtr ImageData
        {
            get { return imageData; }
        }

        /// <summary>
        /// Ширина изображения в пикселях
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Высота изображения в пикселях
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// Шаг изображения (размер строки в байтах)
        /// </summary>
        public int Stride
        {
            get { return stride; }
        }

        /// <summary>
        /// Пиксельный формат изображения
        /// </summary>
        public PixelFormat PixelFormat
        {
            get { return pixelFormat; }
        }

        /// <summary>
        /// Выделить новое изображение в неуправляемой памяти
        /// </summary>
        /// <param name="width">Ширина изображения</param>
        /// <param name="height">Высота изображения</param>
        /// <param name="pixelFormat">Пиксельный формат изображения</param>
        /// <returns>Возврат изображения, выделенного в неуправляемой памяти</returns>
        public static UnmanagedImage Create(int width, int height, PixelFormat pixelFormat)
        {
            var bytesPerPixel = CalculateBytesForPixel(pixelFormat);

            CheckImageSize(width, height);

            var stride = width * bytesPerPixel;

            if (stride % 4 != 0)
            {
                stride += 4 - (stride % 4);
            }

            IntPtr imageData = System.Runtime.InteropServices.Marshal.AllocHGlobal(stride * height);
            //SystemTools.SetUnmanagedMemory(imageData, 0, stride * height);

            UnmanagedImage image = new UnmanagedImage(imageData, width, height, stride, pixelFormat);

            return image;
        }

        private static int CalculateBytesForPixel(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    return 1;
                case PixelFormat.Format16bppGrayScale:
                    return 2;
                case PixelFormat.Format24bppRgb:
                    return 3;
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return 4;
                case PixelFormat.Format48bppRgb:
                    return 6;
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return 8;
                default:
                    throw new Exception("Формат данных о цвете не получен");
            }
        }

        private static void CheckImageSize(int width, int height)
        {
            if ((width <= 0) || (height <= 0))
            {
                throw new Exception("Ширина и/или высота изображения меньше или равна нулю");
            }
        }
    }
}