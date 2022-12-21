using System.Collections.Generic;
using System.Drawing.Imaging;

namespace TogiSoft.AtlasDataBase.ArchiveWell.Perspective.Quadrilateral
{
    /// <summary>
    /// Интерфейс, предоставляющий информацию о фильтре обработки изображений
    /// </summary>
    internal interface IFilterInformation
    {
        /// <summary>
        /// Формат словаря смещений
        /// </summary>
        Dictionary<PixelFormat, PixelFormat> FormatTranslations { get; }
    }
}