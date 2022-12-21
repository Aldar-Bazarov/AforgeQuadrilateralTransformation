using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace TogiSoft.AtlasDataBase.ArchiveWell.Perspective.Quadrilateral
{
    /// <summary>
    /// Класс выполняет четырехугольное преобразование области в заданном исходном изображении
    /// </summary>
    internal class QuadrilateralTransformation : BaseTransformationFilter
    {
        /// <summary>
        /// Новое значение ширины
        /// </summary>
        protected int NewWidth;

        /// <summary>
        /// Новое значение высоты
        /// </summary>
        protected int NewHeight;

        /// <summary>
        /// Флаг - использование интерполяции
        /// </summary>
        private bool useInterpolation = true;

        /// <summary>
        /// Словарь форматов смещений
        /// </summary>
        private readonly Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

        /// <summary>
        /// Исходный четырёхугольник
        /// </summary>
        private List<Point> sourceQuadrilateral;

        /// <summary>
        /// Инициализация экземпляра
        /// </summary>
        public QuadrilateralTransformation()
        {
            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format24bppRgb] = PixelFormat.Format24bppRgb;
            formatTranslations[PixelFormat.Format32bppRgb] = PixelFormat.Format32bppRgb;
            formatTranslations[PixelFormat.Format32bppArgb] = PixelFormat.Format32bppArgb;
            formatTranslations[PixelFormat.Format32bppPArgb] = PixelFormat.Format32bppPArgb;
        }

        /// <summary>
        /// Инициализация экземпляра
        /// </summary>
        /// <param name="sourceQuadrilateral">Исходный четырёхугольник</param>
        /// <param name="newWidth">Новое значение ширины</param>
        /// <param name="newHeight">Новое значение высоты</param>
        public QuadrilateralTransformation(List<Point> sourceQuadrilateral, int newWidth, int newHeight) : this()
        {
            this.sourceQuadrilateral = sourceQuadrilateral;
            this.NewWidth = newWidth;
            this.NewHeight = newHeight;
        }

        /// <summary>
        /// Словарь форматов смещений
        /// </summary>
        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations
        {
            get { return formatTranslations; }
        }

        /// <summary>
        /// Вычислить новый размер изображения
        /// </summary>
        /// <param name="sourceData">Данные исходного изображения</param>
        /// <returns>Новый размер изображения</returns>
        protected override Size CalculateNewImageSize(UnmanagedImage sourceData)
        {
            if (sourceQuadrilateral == null)
            {
                throw new NullReferenceException("Исходный четырехугольник не был установлен");
            }

            return new Size(NewWidth, NewHeight);
        }

        /// <summary>
        /// Обработать фильтр на указанном изображении
        /// </summary>
        /// <param name="sourceData">Данные исходного изображения</param>
        /// <param name="destinationData">Данные изображения смещения</param>
        protected override unsafe void ProcessFilter(UnmanagedImage sourceData, UnmanagedImage destinationData)
        {
            var srcWidth = sourceData.Width;
            var srcHeight = sourceData.Height;
            var dstWidth = destinationData.Width;
            var dstHeight = destinationData.Height;

            var pixelSize = Image.GetPixelFormatSize(sourceData.PixelFormat) / 8;
            var srcStride = sourceData.Stride;
            var dstStride = destinationData.Stride;
            var offset = dstStride - (dstWidth * pixelSize);

            List<Point> dstRect = new List<Point>();

            dstRect.Add(new Point(0, 0));
            dstRect.Add(new Point(dstWidth - 1, 0));
            dstRect.Add(new Point(dstWidth - 1, dstHeight - 1));
            dstRect.Add(new Point(0, dstHeight - 1));

            var matrix = QuadTransformationCalcs.MapQuadToQuad(dstRect, sourceQuadrilateral);

            var ptr = (byte*)destinationData.ImageData.ToPointer();
            var baseSrc = (byte*)sourceData.ImageData.ToPointer();

            if (!useInterpolation)
            {
                byte* p;

                for (int y = 0; y < dstHeight; y++)
                {
                    for (int x = 0; x < dstWidth; x++)
                    {
                        var factor = (matrix[2, 0] * x) + (matrix[2, 1] * y) + matrix[2, 2];
                        var srcX = ((matrix[0, 0] * x) + (matrix[0, 1] * y) + matrix[0, 2]) / factor;
                        var srcY = ((matrix[1, 0] * x) + (matrix[1, 1] * y) + matrix[1, 2]) / factor;

                        if ((srcX >= 0) && (srcY >= 0) && (srcX < srcWidth) && (srcY < srcHeight))
                        {
                            p = baseSrc + ((int)srcY * srcStride) + ((int)srcX * pixelSize);
                            for (int i = 0; i < pixelSize; i++, ptr++, p++)
                            {
                                *ptr = *p;
                            }
                        }
                        else
                        {
                            ptr += pixelSize;
                        }
                    }

                    ptr += offset;
                }
            }
            else
            {
                var srcWidthM1 = srcWidth - 1;
                var srcHeightM1 = srcHeight - 1;

                double dx1;
                double dy1;
                double dx2;
                double dy2;

                int sx1;
                int sy1;
                int sx2;
                int sy2;

                byte* p1;
                byte* p2;
                byte* p3;
                byte* p4;

                for (int y = 0; y < dstHeight; y++)
                {
                    for (int x = 0; x < dstWidth; x++)
                    {
                        var factor = (matrix[2, 0] * x) + (matrix[2, 1] * y) + matrix[2, 2];
                        var srcX = ((matrix[0, 0] * x) + (matrix[0, 1] * y) + matrix[0, 2]) / factor;
                        var srcY = ((matrix[1, 0] * x) + (matrix[1, 1] * y) + matrix[1, 2]) / factor;

                        if ((srcX >= 0) && (srcY >= 0) && (srcX < srcWidth) && (srcY < srcHeight))
                        {
                            sx1 = (int)srcX;
                            sx2 = (sx1 == srcWidthM1) ? sx1 : sx1 + 1;
                            dx1 = srcX - sx1;
                            dx2 = 1.0 - dx1;

                            sy1 = (int)srcY;
                            sy2 = (sy1 == srcHeightM1) ? sy1 : sy1 + 1;
                            dy1 = srcY - sy1;
                            dy2 = 1.0 - dy1;

                            p1 = p2 = baseSrc + (sy1 * srcStride);
                            p1 += sx1 * pixelSize;
                            p2 += sx2 * pixelSize;

                            p3 = p4 = baseSrc + (sy2 * srcStride);
                            p3 += sx1 * pixelSize;
                            p4 += sx2 * pixelSize;

                            for (int i = 0; i < pixelSize; i++, ptr++, p1++, p2++, p3++, p4++)
                            {
                                *ptr = (byte)((dy2 * ((dx2 * (*p1)) + (dx1 * (*p2)))) + (dy1 * ((dx2 * (*p3)) + (dx1 * (*p4)))));
                            }
                        }
                        else
                        {
                            ptr += pixelSize;
                        }
                    }

                    ptr += offset;
                }
            }
        }
    }
}