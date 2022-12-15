using System.Drawing;
using System.Drawing.Imaging;

namespace TogiSoft.AtlasDataBase.ArchiveWell.Perspective.Quadrilateral
{
    interface IFilter
    {
        Bitmap Apply(Bitmap image);
        Bitmap Apply(BitmapData imageData);
        UnmanagedImage Apply(UnmanagedImage image);
        void Apply(UnmanagedImage sourceImage, UnmanagedImage destinationImage);
    }
}
