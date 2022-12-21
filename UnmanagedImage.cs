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
        /// <param name="bitmapData"> Заблокированные растровые данные /param>
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
    }
}