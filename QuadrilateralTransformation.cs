using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace TogiSoft.AtlasDataBase.ArchiveWell.Perspective.Quadrilateral
{
    /// <summary>
    /// Класс выполняет четырехугольное преобразование области в заданном исходном изображении
    /// </summary>
    internal class QuadrilateralTransformation
    {
        /// <summary>
        /// Новое значение ширины
        /// </summary>
        private int newWidth;

        /// <summary>
        /// Новое значение высоты
        /// </summary>
        private int newHeight;

        /// <summary>
        /// Углы исходного четырёхугольника
        /// </summary>
        private List<Point> sourceQuadrilateralCorners;

        /// <summary>
        /// Словарь форматов смещений
        /// </summary>
        private readonly Dictionary<PixelFormat, PixelFormat> formatTranslations;

        /// <summary>
        /// Инициализация экземпляра
        /// </summary>
        /// <param name="sourceQuadrilateral">Исходный четырёхугольник</param>
        /// <param name="newWidth">Новое значение ширины</param>
        /// <param name="newHeight">Новое значение высоты</param>
        public QuadrilateralTransformation(List<Point> sourceQuadrilateralCorners, int newWidth, int newHeight)
        {
            formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format24bppRgb] = PixelFormat.Format24bppRgb;
            formatTranslations[PixelFormat.Format32bppRgb] = PixelFormat.Format32bppRgb;
            formatTranslations[PixelFormat.Format32bppArgb] = PixelFormat.Format32bppArgb;
            formatTranslations[PixelFormat.Format32bppPArgb] = PixelFormat.Format32bppPArgb;

            this.sourceQuadrilateralCorners = sourceQuadrilateralCorners;
            this.newWidth = newWidth;
            this.newHeight = newHeight;
        }

        /// <summary>
        /// Применить фильтр к изображению
        /// </summary>
        /// <param name="image"> Исходное изображение, к которому нужно применить фильтр </param>
        /// <returns> Возвращает результат фильтра, полученный путем применения фильтра к исходному изображению </returns>
        public Bitmap Apply(Bitmap image)
        {
            var srcData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);

            var dstPixelFormat = formatTranslations[srcData.PixelFormat];

            var newSize = new Size(newWidth, newHeight);

            var dstImage = new Bitmap(newSize.Width, newSize.Height, dstPixelFormat);

            var dstData = dstImage.LockBits(new Rectangle(0, 0, newSize.Width, newSize.Height), ImageLockMode.ReadWrite, dstPixelFormat);

            DoPerspective(new UnmanagedImage(srcData), new UnmanagedImage(dstData));

            dstImage.UnlockBits(dstData);

            dstImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            image.UnlockBits(srcData);

            return dstImage;
        }

        /// <summary>
        /// Применить перспективу
        /// </summary>
        /// <param name="sourceData"> Данные исходного изображения </param>
        /// <param name="destinationData"> Данные изображения смещения </param>
        protected unsafe void DoPerspective(UnmanagedImage sourceData, UnmanagedImage destinationData)
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

            var matrix = QuadTransformationCalcs.MapQuadToQuad(dstRect, sourceQuadrilateralCorners);

            var ptr = (byte*)destinationData.ImageData.ToPointer();
            var baseSrc = (byte*)sourceData.ImageData.ToPointer();


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