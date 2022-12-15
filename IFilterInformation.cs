using System.Collections.Generic;
using System.Drawing.Imaging;

namespace TogiSoft.AtlasDataBase.ArchiveWell.Perspective.Quadrilateral
{
    interface IFilterInformation
    {
        Dictionary<PixelFormat, PixelFormat> FormatTranslations { get; }
    }
}
