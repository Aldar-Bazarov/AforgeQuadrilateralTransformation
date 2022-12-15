using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace TogiSoft.AtlasDataBase.ArchiveWell.Perspective.Quadrilateral
{
    public class UnmanagedImage : IDisposable
    {
        // pointer to image data in unmanaged memory
        public IntPtr imageData;
        // image size
        public int width, height;
        // image stride (line size)
        public int stride;
        // image pixel format
        public PixelFormat pixelFormat;
        // flag which indicates if the image should be disposed or not
        public bool mustBeDisposed = false;

        public IntPtr ImageData
        {
            get { return imageData; }
        }
        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public int Stride
        {
            get { return stride; }
        }

        public PixelFormat PixelFormat
        {
            get { return pixelFormat; }
        }

        public UnmanagedImage(IntPtr imageData, int width, int height, int stride, PixelFormat pixelFormat)
        {
            this.imageData = imageData;
            this.width = width;
            this.height = height;
            this.stride = stride;
            this.pixelFormat = pixelFormat;
        }

        public UnmanagedImage(BitmapData bitmapData)
        {
            this.imageData = bitmapData.Scan0;
            this.width = bitmapData.Width;
            this.height = bitmapData.Height;
            this.stride = bitmapData.Stride;
            this.pixelFormat = bitmapData.PixelFormat;
        }

        ~UnmanagedImage()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            // remove me from the Finalization queue 
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
            }
            // free image memory if the image was allocated using this class
            if ((mustBeDisposed) && (imageData != IntPtr.Zero))
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(imageData);
                imageData = IntPtr.Zero;
            }
        }

        public UnmanagedImage Clone()
        {
            // allocate memory for the image
            IntPtr newImageData = System.Runtime.InteropServices.Marshal.AllocHGlobal(stride * height);

            UnmanagedImage newImage = new UnmanagedImage(newImageData, width, height, stride, pixelFormat);
            newImage.mustBeDisposed = true;

            SystemTools.CopyUnmanagedMemory(newImageData, imageData, stride * height);

            return newImage;
        }
        public void Copy(UnmanagedImage destImage)
        {
            if (
                (width != destImage.width) || (height != destImage.height) ||
                (pixelFormat != destImage.pixelFormat))
            {
                throw new Exception();
            }

            if (stride == destImage.stride)
            {
                // copy entire image
                SystemTools.CopyUnmanagedMemory(destImage.imageData, imageData, stride * height);
            }
            else
            {
                unsafe
                {
                    int dstStride = destImage.stride;
                    int copyLength = (stride < dstStride) ? stride : dstStride;

                    byte* src = (byte*)imageData.ToPointer();
                    byte* dst = (byte*)destImage.imageData.ToPointer();

                    // copy line by line
                    for (int i = 0; i < height; i++)
                    {
                        SystemTools.CopyUnmanagedMemory(dst, src, copyLength);

                        dst += dstStride;
                        src += stride;
                    }
                }
            }
        }

        public static UnmanagedImage Create(int width, int height, PixelFormat pixelFormat)
        {
            int bytesPerPixel = 0;

            // calculate bytes per pixel
            switch (pixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    bytesPerPixel = 1;
                    break;
                case PixelFormat.Format16bppGrayScale:
                    bytesPerPixel = 2;
                    break;
                case PixelFormat.Format24bppRgb:
                    bytesPerPixel = 3;
                    break;
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    bytesPerPixel = 4;
                    break;
                case PixelFormat.Format48bppRgb:
                    bytesPerPixel = 6;
                    break;
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    bytesPerPixel = 8;
                    break;
                default:
                    throw new Exception();
            }

            // check image size
            if ((width <= 0) || (height <= 0))
            {
                throw new Exception();
            }

            // calculate stride
            int stride = width * bytesPerPixel;

            if (stride % 4 != 0)
            {
                stride += (4 - (stride % 4));
            }

            // allocate memory for the image
            IntPtr imageData = System.Runtime.InteropServices.Marshal.AllocHGlobal(stride * height);
            SystemTools.SetUnmanagedMemory(imageData, 0, stride * height);

            UnmanagedImage image = new UnmanagedImage(imageData, width, height, stride, pixelFormat);
            image.mustBeDisposed = true;

            return image;
        }

        public Bitmap ToManagedImage()
        {
            return ToManagedImage(true);
        }

        public Bitmap ToManagedImage(bool makeCopy)
        {
            Bitmap dstImage = null;

            try
            {
                if (!makeCopy)
                {
                    dstImage = new Bitmap(width, height, stride, pixelFormat, imageData);
                    if (pixelFormat == PixelFormat.Format8bppIndexed)
                    {
                        CustomImage.SetGrayscalePalette(dstImage);
                    }
                }
                else
                {
                    dstImage = (pixelFormat == PixelFormat.Format8bppIndexed) ?
                        CustomImage.CreateGrayscaleImage(width, height) :
                        new Bitmap(width, height, pixelFormat);

                    BitmapData dstData = dstImage.LockBits(
                        new Rectangle(0, 0, width, height),
                        ImageLockMode.ReadWrite, pixelFormat);

                    int dstStride = dstData.Stride;
                    int lineSize = Math.Min(stride, dstStride);

                    unsafe
                    {
                        byte* dst = (byte*)dstData.Scan0.ToPointer();
                        byte* src = (byte*)imageData.ToPointer();

                        if (stride != dstStride)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                SystemTools.CopyUnmanagedMemory(dst, src, lineSize);
                                dst += dstStride;
                                src += stride;
                            }
                        }
                        else
                        {
                            SystemTools.CopyUnmanagedMemory(dst, src, stride * height);
                        }
                    }

                    dstImage.UnlockBits(dstData);
                }

                return dstImage;
            }
            catch (Exception)
            {
                if (dstImage != null)
                {
                    dstImage.Dispose();
                }

                throw new Exception();
            }
        }

        public static UnmanagedImage FromManagedImage(Bitmap image)
        {
            UnmanagedImage dstImage = null;

            BitmapData sourceData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, image.PixelFormat);

            try
            {
                dstImage = FromManagedImage(sourceData);
            }
            finally
            {
                image.UnlockBits(sourceData);
            }

            return dstImage;
        }

        public static UnmanagedImage FromManagedImage(BitmapData imageData)
        {
            PixelFormat pixelFormat = imageData.PixelFormat;

            // check source pixel format
            if (
                (pixelFormat != PixelFormat.Format8bppIndexed) &&
                (pixelFormat != PixelFormat.Format16bppGrayScale) &&
                (pixelFormat != PixelFormat.Format24bppRgb) &&
                (pixelFormat != PixelFormat.Format32bppRgb) &&
                (pixelFormat != PixelFormat.Format32bppArgb) &&
                (pixelFormat != PixelFormat.Format32bppPArgb) &&
                (pixelFormat != PixelFormat.Format48bppRgb) &&
                (pixelFormat != PixelFormat.Format64bppArgb) &&
                (pixelFormat != PixelFormat.Format64bppPArgb))
            {
                throw new Exception();
            }

            IntPtr dstImageData = System.Runtime.InteropServices.Marshal.AllocHGlobal(imageData.Stride * imageData.Height);

            UnmanagedImage image = new UnmanagedImage(dstImageData, imageData.Width, imageData.Height, imageData.Stride, pixelFormat);
            SystemTools.CopyUnmanagedMemory(dstImageData, imageData.Scan0, imageData.Stride * imageData.Height);
            image.mustBeDisposed = true;

            return image;
        }

        public byte[] Collect8bppPixelValues(List<IntPoint> points)
        {
            int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;

            if ((pixelFormat == PixelFormat.Format16bppGrayScale) || (pixelSize > 4))
            {
                throw new Exception();
            }

            byte[] pixelValues = new byte[points.Count * ((pixelFormat == PixelFormat.Format8bppIndexed) ? 1 : 3)];

            unsafe
            {
                byte* basePtr = (byte*)imageData.ToPointer();
                byte* ptr;

                if (pixelFormat == PixelFormat.Format8bppIndexed)
                {
                    int i = 0;

                    foreach (IntPoint point in points)
                    {
                        ptr = basePtr + stride * point.Y + point.X;
                        pixelValues[i++] = *ptr;
                    }
                }
                else
                {
                    int i = 0;

                    foreach (IntPoint point in points)
                    {
                        ptr = basePtr + stride * point.Y + point.X * pixelSize;
                        pixelValues[i++] = ptr[CustomRGB.R];
                        pixelValues[i++] = ptr[CustomRGB.G];
                        pixelValues[i++] = ptr[CustomRGB.B];
                    }
                }
            }

            return pixelValues;
        }

        public List<IntPoint> CollectActivePixels()
        {
            return CollectActivePixels(new Rectangle(0, 0, width, height));
        }

        public List<IntPoint> CollectActivePixels(Rectangle rect)
        {
            List<IntPoint> pixels = new List<IntPoint>();

            int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;

            // correct rectangle
            rect.Intersect(new Rectangle(0, 0, width, height));

            int startX = rect.X;
            int startY = rect.Y;
            int stopX = rect.Right;
            int stopY = rect.Bottom;

            unsafe
            {
                byte* basePtr = (byte*)imageData.ToPointer();

                if ((pixelFormat == PixelFormat.Format16bppGrayScale) || (pixelSize > 4))
                {
                    int pixelWords = pixelSize >> 1;

                    for (int y = startY; y < stopY; y++)
                    {
                        ushort* ptr = (ushort*)(basePtr + y * stride + startX * pixelSize);

                        if (pixelWords == 1)
                        {
                            // grayscale images
                            for (int x = startX; x < stopX; x++, ptr++)
                            {
                                if (*ptr != 0)
                                {
                                    pixels.Add(new IntPoint(x, y));
                                }
                            }
                        }
                        else
                        {
                            // color images
                            for (int x = startX; x < stopX; x++, ptr += pixelWords)
                            {
                                if ((ptr[CustomRGB.R] != 0) || (ptr[CustomRGB.G] != 0) || (ptr[CustomRGB.B] != 0))
                                {
                                    pixels.Add(new IntPoint(x, y));
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int y = startY; y < stopY; y++)
                    {
                        byte* ptr = basePtr + y * stride + startX * pixelSize;

                        if (pixelSize == 1)
                        {
                            // grayscale images
                            for (int x = startX; x < stopX; x++, ptr++)
                            {
                                if (*ptr != 0)
                                {
                                    pixels.Add(new IntPoint(x, y));
                                }
                            }
                        }
                        else
                        {
                            // color images
                            for (int x = startX; x < stopX; x++, ptr += pixelSize)
                            {
                                if ((ptr[CustomRGB.R] != 0) || (ptr[CustomRGB.G] != 0) || (ptr[CustomRGB.B] != 0))
                                {
                                    pixels.Add(new IntPoint(x, y));
                                }
                            }
                        }
                    }
                }
            }

            return pixels;
        }

        public void SetPixels(List<IntPoint> coordinates, Color color)
        {
            unsafe
            {
                int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;
                byte* basePtr = (byte*)imageData.ToPointer();

                byte red = color.R;
                byte green = color.G;
                byte blue = color.B;
                byte alpha = color.A;

                switch (pixelFormat)
                {
                    case PixelFormat.Format8bppIndexed:
                        {
                            byte grayValue = (byte)(0.2125 * red + 0.7154 * green + 0.0721 * blue);

                            foreach (IntPoint point in coordinates)
                            {
                                if ((point.X >= 0) && (point.Y >= 0) && (point.X < width) && (point.Y < height))
                                {
                                    byte* ptr = basePtr + point.Y * stride + point.X;
                                    *ptr = grayValue;
                                }
                            }
                        }
                        break;

                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppRgb:
                        {


                            foreach (IntPoint point in coordinates)
                            {
                                if ((point.X >= 0) && (point.Y >= 0) && (point.X < width) && (point.Y < height))
                                {
                                    byte* ptr = basePtr + point.Y * stride + point.X * pixelSize;
                                    ptr[CustomRGB.R] = red;
                                    ptr[CustomRGB.G] = green;
                                    ptr[CustomRGB.B] = blue;
                                }
                            }
                        }
                        break;

                    case PixelFormat.Format32bppArgb:
                        {
                            foreach (IntPoint point in coordinates)
                            {
                                if ((point.X >= 0) && (point.Y >= 0) && (point.X < width) && (point.Y < height))
                                {
                                    byte* ptr = basePtr + point.Y * stride + point.X * pixelSize;
                                    ptr[CustomRGB.R] = red;
                                    ptr[CustomRGB.G] = green;
                                    ptr[CustomRGB.B] = blue;
                                    ptr[CustomRGB.A] = alpha;
                                }
                            }
                        }
                        break;

                    case PixelFormat.Format16bppGrayScale:
                        {
                            ushort grayValue = (ushort)((ushort)(0.2125 * red + 0.7154 * green + 0.0721 * blue) << 8);

                            foreach (IntPoint point in coordinates)
                            {
                                if ((point.X >= 0) && (point.Y >= 0) && (point.X < width) && (point.Y < height))
                                {
                                    ushort* ptr = (ushort*)(basePtr + point.Y * stride) + point.X;
                                    *ptr = grayValue;
                                }
                            }
                        }
                        break;

                    case PixelFormat.Format48bppRgb:
                        {
                            ushort red16 = (ushort)(red << 8);
                            ushort green16 = (ushort)(green << 8);
                            ushort blue16 = (ushort)(blue << 8);

                            foreach (IntPoint point in coordinates)
                            {
                                if ((point.X >= 0) && (point.Y >= 0) && (point.X < width) && (point.Y < height))
                                {
                                    ushort* ptr = (ushort*)(basePtr + point.Y * stride + point.X * pixelSize);
                                    ptr[CustomRGB.R] = red16;
                                    ptr[CustomRGB.G] = green16;
                                    ptr[CustomRGB.B] = blue16;
                                }
                            }
                        }
                        break;

                    case PixelFormat.Format64bppArgb:
                        {
                            ushort red16 = (ushort)(red << 8);
                            ushort green16 = (ushort)(green << 8);
                            ushort blue16 = (ushort)(blue << 8);
                            ushort alpha16 = (ushort)(alpha << 8);

                            foreach (IntPoint point in coordinates)
                            {
                                if ((point.X >= 0) && (point.Y >= 0) && (point.X < width) && (point.Y < height))
                                {
                                    ushort* ptr = (ushort*)(basePtr + point.Y * stride + point.X * pixelSize);
                                    ptr[CustomRGB.R] = red16;
                                    ptr[CustomRGB.G] = green16;
                                    ptr[CustomRGB.B] = blue16;
                                    ptr[CustomRGB.A] = alpha16;
                                }
                            }
                        }
                        break;

                    default:
                        throw new Exception();
                }
            }
        }

        public void SetPixel(IntPoint point, Color color)
        {
            SetPixel(point.X, point.Y, color);
        }

        public void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.R, color.G, color.B, color.A);
        }

        public void SetPixel(int x, int y, byte value)
        {
            SetPixel(x, y, value, value, value, 255);
        }

        public void SetPixel(int x, int y, byte r, byte g, byte b, byte a)
        {
            if ((x >= 0) && (y >= 0) && (x < width) && (y < height))
            {
                unsafe
                {
                    int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;
                    byte* ptr = (byte*)imageData.ToPointer() + y * stride + x * pixelSize;
                    ushort* ptr2 = (ushort*)ptr;

                    switch (pixelFormat)
                    {
                        case PixelFormat.Format8bppIndexed:
                            *ptr = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);
                            break;

                        case PixelFormat.Format24bppRgb:
                        case PixelFormat.Format32bppRgb:
                            ptr[CustomRGB.R] = r;
                            ptr[CustomRGB.G] = g;
                            ptr[CustomRGB.B] = b;
                            break;

                        case PixelFormat.Format32bppArgb:
                            ptr[CustomRGB.R] = r;
                            ptr[CustomRGB.G] = g;
                            ptr[CustomRGB.B] = b;
                            ptr[CustomRGB.A] = a;
                            break;

                        case PixelFormat.Format16bppGrayScale:
                            *ptr2 = (ushort)((ushort)(0.2125 * r + 0.7154 * g + 0.0721 * b) << 8);
                            break;

                        case PixelFormat.Format48bppRgb:
                            ptr2[CustomRGB.R] = (ushort)(r << 8);
                            ptr2[CustomRGB.G] = (ushort)(g << 8);
                            ptr2[CustomRGB.B] = (ushort)(b << 8);
                            break;

                        case PixelFormat.Format64bppArgb:
                            ptr2[CustomRGB.R] = (ushort)(r << 8);
                            ptr2[CustomRGB.G] = (ushort)(g << 8);
                            ptr2[CustomRGB.B] = (ushort)(b << 8);
                            ptr2[CustomRGB.A] = (ushort)(a << 8);
                            break;

                        default:
                            throw new Exception();
                    }
                }
            }
        }

        public Color GetPixel(IntPoint point)
        {
            return GetPixel(point.X, point.Y);
        }

        public Color GetPixel(int x, int y)
        {
            if ((x < 0) || (y < 0))
            {
                throw new ArgumentOutOfRangeException("x", "The specified pixel coordinate is out of image's bounds.");
            }

            if ((x >= width) || (y >= height))
            {
                throw new ArgumentOutOfRangeException("y", "The specified pixel coordinate is out of image's bounds.");
            }

            Color color = new Color();

            unsafe
            {
                int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;
                byte* ptr = (byte*)imageData.ToPointer() + y * stride + x * pixelSize;

                switch (pixelFormat)
                {
                    case PixelFormat.Format8bppIndexed:
                        color = Color.FromArgb(*ptr, *ptr, *ptr);
                        break;

                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppRgb:
                        color = Color.FromArgb(ptr[CustomRGB.R], ptr[CustomRGB.G], ptr[CustomRGB.B]);
                        break;

                    case PixelFormat.Format32bppArgb:
                        color = Color.FromArgb(ptr[CustomRGB.A], ptr[CustomRGB.R], ptr[CustomRGB.G], ptr[CustomRGB.B]);
                        break;

                    default:
                        throw new Exception();
                }
            }

            return color;
        }

        public ushort[] Collect16bppPixelValues(List<IntPoint> points)
        {
            int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;

            if ((pixelFormat == PixelFormat.Format8bppIndexed) || (pixelSize == 3) || (pixelSize == 4))
            {
                throw new Exception();
            }

            ushort[] pixelValues = new ushort[points.Count * ((pixelFormat == PixelFormat.Format16bppGrayScale) ? 1 : 3)];

            unsafe
            {
                byte* basePtr = (byte*)imageData.ToPointer();
                ushort* ptr;

                if (pixelFormat == PixelFormat.Format16bppGrayScale)
                {
                    int i = 0;

                    foreach (IntPoint point in points)
                    {
                        ptr = (ushort*)(basePtr + stride * point.Y + point.X * pixelSize);
                        pixelValues[i++] = *ptr;
                    }
                }
                else
                {
                    int i = 0;

                    foreach (IntPoint point in points)
                    {
                        ptr = (ushort*)(basePtr + stride * point.Y + point.X * pixelSize);
                        pixelValues[i++] = ptr[CustomRGB.R];
                        pixelValues[i++] = ptr[CustomRGB.G];
                        pixelValues[i++] = ptr[CustomRGB.B];
                    }
                }
            }

            return pixelValues;
        }
    }
}
