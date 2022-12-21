using System.Drawing;
using System.Drawing.Imaging;

namespace TogiSoft.AtlasDataBase.ArchiveWell.Perspective.Quadrilateral
{
    /// <summary>
    /// Интерфейс фильтра обработки изображений
    /// </summary>
    internal interface IFilter
    {
        /// <summary>
        /// Применить фильтр к изображению
        /// </summary>
        /// <param name="image">Исходное изображение, к которому нужно применить фильтр</param>
        /// <returns>Возвращает результат фильтра, полученный путем применения фильтра к исходному изображению</returns>
        Bitmap Apply(Bitmap image);

        /// <summary>
        /// Применить фильтр к изображению
        /// </summary>
        /// <param name="imageData">Исходное изображение, к которому нужно применить фильтр</param>
        /// <returns>Возвращает результат фильтра, полученный путем применения фильтра к исходному изображению</returns>
        Bitmap Apply(BitmapData imageData);

        /// <summary>
        /// Применить фильтр к изображению
        /// </summary>
        /// <param name="image">Изображение в неуправляемой памяти</param>
        /// <returns>Возвращает результат фильтра, полученный путем применения фильтра к исходному изображению</returns>
        UnmanagedImage Apply(UnmanagedImage image);

        /// <summary>
        /// Применить фильтр к изображению
        /// </summary>
        /// <param name="sourceImage">Исходное изображение для обработки</param>
        /// <param name="destinationImage">Указатель на изображение для хранения результата фильтра</param>
        void Apply(UnmanagedImage sourceImage, UnmanagedImage destinationImage);
    }
}