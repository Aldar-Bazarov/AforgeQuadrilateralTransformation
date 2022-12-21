/// <summary>
/// Обработчик события отпускания кнопки мыши в SourcePictureBox
/// </summary>
/// <param name="sender"> Источник </param>
/// <param name="e"> Параметры </param>
/// <remarks> Трансформирует исходное изображение под заданную рамку </remarks>
private void SourcePictureBox_MouseUp(object sender, MouseEventArgs e)
{
    if (PerspectiveBorder.MouseIsUp)
    {
        var corners = new List<Point>
        {
            new Point(PerspectiveBorder.TopLeft.X, PerspectiveBorder.TopLeft.Y),
            new Point(PerspectiveBorder.TopRight.X, PerspectiveBorder.TopRight.Y),
            new Point(PerspectiveBorder.BottomRight.X, PerspectiveBorder.BottomRight.Y),
            new Point(PerspectiveBorder.BottomLeft.X, PerspectiveBorder.BottomLeft.Y)
        };

        var filter = new QuadrilateralTransformation(corners, sourcePictureBox.Width, sourcePictureBox.Height);
        transformedPictureBox.Image = filter.Apply((Bitmap)sourcePictureBox.Image)
        EditedImage = new Bitmap(transformedPictureBox.Image, SourceImage.Width, SourceImage.Height)
        PerspectiveBorder.MouseIsUp = false;
    }
}