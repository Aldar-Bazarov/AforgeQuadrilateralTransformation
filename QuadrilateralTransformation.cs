using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace TogiSoft.AtlasDataBase.ArchiveWell.Perspective.Quadrilateral
{
    class QuadrilateralTransformation : BaseTransformationFilter, IDisposable
    {
        private bool automaticSizeCalculaton = true;
        private bool useInterpolation = true;

        private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations
        {
            get { return formatTranslations; }
        }

        protected int newWidth;

        protected int newHeight;

        public bool AutomaticSizeCalculaton
        {
            get { return automaticSizeCalculaton; }
            set
            {
                automaticSizeCalculaton = value;
                if (value)
                {
                    CalculateDestinationSize();
                }
            }
        }

        private List<IntPoint> sourceQuadrilateral;

        public List<IntPoint> SourceQuadrilateral
        {
            get { return sourceQuadrilateral; }
            set
            {
                sourceQuadrilateral = value;
                if (automaticSizeCalculaton)
                {
                    CalculateDestinationSize();
                }
            }
        }

        public int NewWidth
        {
            get { return newWidth; }
            set
            {
                if (!automaticSizeCalculaton)
                {
                    newWidth = Math.Max(1, value);
                }
            }
        }

        public int NewHeight
        {
            get { return newHeight; }
            set
            {
                if (!automaticSizeCalculaton)
                {
                    newHeight = Math.Max(1, value);
                }
            }
        }

        public bool UseInterpolation
        {
            get { return useInterpolation; }
            set { useInterpolation = value; }
        }

        public QuadrilateralTransformation()
        {
            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format24bppRgb] = PixelFormat.Format24bppRgb;
            formatTranslations[PixelFormat.Format32bppRgb] = PixelFormat.Format32bppRgb;
            formatTranslations[PixelFormat.Format32bppArgb] = PixelFormat.Format32bppArgb;
            formatTranslations[PixelFormat.Format32bppPArgb] = PixelFormat.Format32bppPArgb;
        }

        public QuadrilateralTransformation(List<IntPoint> sourceQuadrilateral, int newWidth, int newHeight)
            : this()
        {
            this.automaticSizeCalculaton = false;
            this.sourceQuadrilateral = sourceQuadrilateral;
            this.newWidth = newWidth;
            this.newHeight = newHeight;
        }

        public QuadrilateralTransformation(List<IntPoint> sourceQuadrilateral)
            : this()
        {
            this.automaticSizeCalculaton = true;
            this.sourceQuadrilateral = sourceQuadrilateral;
            CalculateDestinationSize();
        }

        protected override System.Drawing.Size CalculateNewImageSize(UnmanagedImage sourceData)
        {
            if (sourceQuadrilateral == null)
                throw new NullReferenceException("Source quadrilateral was not set.");

            return new Size(newWidth, newHeight);
        }

        private void CalculateDestinationSize()
        {
            if (sourceQuadrilateral == null)
                throw new NullReferenceException("Source quadrilateral was not set.");

            newWidth = (int)Math.Max(sourceQuadrilateral[0].DistanceTo(sourceQuadrilateral[1]),
                                        sourceQuadrilateral[2].DistanceTo(sourceQuadrilateral[3]));
            newHeight = (int)Math.Max(sourceQuadrilateral[1].DistanceTo(sourceQuadrilateral[2]),
                                        sourceQuadrilateral[3].DistanceTo(sourceQuadrilateral[0]));
        }

        protected override unsafe void ProcessFilter(UnmanagedImage sourceData, UnmanagedImage destinationData)
        {
            int srcWidth = sourceData.Width;
            int srcHeight = sourceData.Height;
            int dstWidth = destinationData.Width;
            int dstHeight = destinationData.Height;

            int pixelSize = System.Drawing.Image.GetPixelFormatSize(sourceData.PixelFormat) / 8;
            int srcStride = sourceData.Stride;
            int dstStride = destinationData.Stride;
            int offset = dstStride - dstWidth * pixelSize;

            List<IntPoint> dstRect = new List<IntPoint>();
            dstRect.Add(new IntPoint(0, 0));
            dstRect.Add(new IntPoint(dstWidth - 1, 0));
            dstRect.Add(new IntPoint(dstWidth - 1, dstHeight - 1));
            dstRect.Add(new IntPoint(0, dstHeight - 1));

            double[,] matrix = QuadTransformationCalcs.MapQuadToQuad(dstRect, sourceQuadrilateral);

            byte* ptr = (byte*)destinationData.ImageData.ToPointer();
            byte* baseSrc = (byte*)sourceData.ImageData.ToPointer();

            if (!useInterpolation)
            {
                byte* p;

                for (int y = 0; y < dstHeight; y++)
                {
                    for (int x = 0; x < dstWidth; x++)
                    {
                        double factor = matrix[2, 0] * x + matrix[2, 1] * y + matrix[2, 2];
                        double srcX = (matrix[0, 0] * x + matrix[0, 1] * y + matrix[0, 2]) / factor;
                        double srcY = (matrix[1, 0] * x + matrix[1, 1] * y + matrix[1, 2]) / factor;

                        if ((srcX >= 0) && (srcY >= 0) && (srcX < srcWidth) && (srcY < srcHeight))
                        {
                            p = baseSrc + (int)srcY * srcStride + (int)srcX * pixelSize;
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
                int srcWidthM1 = srcWidth - 1;
                int srcHeightM1 = srcHeight - 1;

                double dx1, dy1, dx2, dy2;
                int sx1, sy1, sx2, sy2;

                byte* p1, p2, p3, p4;

                for (int y = 0; y < dstHeight; y++)
                {
                    for (int x = 0; x < dstWidth; x++)
                    {
                        double factor = matrix[2, 0] * x + matrix[2, 1] * y + matrix[2, 2];
                        double srcX = (matrix[0, 0] * x + matrix[0, 1] * y + matrix[0, 2]) / factor;
                        double srcY = (matrix[1, 0] * x + matrix[1, 1] * y + matrix[1, 2]) / factor;

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

                            p1 = p2 = baseSrc + sy1 * srcStride;
                            p1 += sx1 * pixelSize;
                            p2 += sx2 * pixelSize;

                            p3 = p4 = baseSrc + sy2 * srcStride;
                            p3 += sx1 * pixelSize;
                            p4 += sx2 * pixelSize;

                            for (int i = 0; i < pixelSize; i++, ptr++, p1++, p2++, p3++, p4++)
                            {
                                *ptr = (byte)(
                                    dy2 * (dx2 * (*p1) + dx1 * (*p2)) +
                                    dy1 * (dx2 * (*p3) + dx1 * (*p4)));
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

        public void Dispose()
        {
            this.Dispose();
        }
    }
}
