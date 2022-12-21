using System.Collections.Generic;
using System.Drawing;

namespace TogiSoft.AtlasDataBase.ArchiveWell.Perspective.Quadrilateral
{
    /// <summary>
    /// Класс описывающий отображение любого четырехугольника на любой другой четырехугольник
    /// </summary>
    internal static class QuadTransformationCalcs
    {
        /// <summary>
        /// Степень точности
        /// </summary>
        private const double epsilon = 1e-13;

        /// <summary>
        /// Вычислить матрицу для общего преобразования четырехугольника в четырехугольник
        /// </summary>
        /// <param name="input">Входящие точки</param>
        /// <param name="output">Выходящие точки</param>
        /// <returns>Вычисленная матрица</returns>
        public static double[,] MapQuadToQuad(List<Point> input, List<Point> output)
        {
            var squareToInpit = MapSquareToQuad(input);

            var squareToOutput = MapSquareToQuad(output);

            if (squareToOutput == null)
            {
                return null;
            }

            return MultiplyMatrix(squareToOutput, AdjugateMatrix(squareToInpit));
        }

        /// <summary>
        /// Вычислить определитель матрицы 2x2
        /// </summary>
        /// <param name="a">Верхний левый элемент</param>
        /// <param name="b">Нижний левый элемент</param>
        /// <param name="c">Верхний правый элемент</param>
        /// <param name="d">Нижний правый элемент</param>
        /// <returns>Определитель</returns>
        private static double Det2(double a, double b, double c, double d)
        {
            return (a * d) - (b * c);
        }

        /// <summary>
        /// Умножить две матрицы 3x3
        /// </summary>
        /// <param name="firstMatrix">Первая матрица</param>
        /// <param name="secondMatrix">Вторая матрица</param>
        /// <returns>Умноженная матрица</returns>
        private static double[,] MultiplyMatrix(double[,] firstMatrix, double[,] secondMatrix)
        {
            var multipliedMatrix = new double[3, 3];

            multipliedMatrix[0, 0] = (firstMatrix[0, 0] * secondMatrix[0, 0]) + (firstMatrix[0, 1] * secondMatrix[1, 0]) + (firstMatrix[0, 2] * secondMatrix[2, 0]);
            multipliedMatrix[0, 1] = (firstMatrix[0, 0] * secondMatrix[0, 1]) + (firstMatrix[0, 1] * secondMatrix[1, 1]) + (firstMatrix[0, 2] * secondMatrix[2, 1]);
            multipliedMatrix[0, 2] = (firstMatrix[0, 0] * secondMatrix[0, 2]) + (firstMatrix[0, 1] * secondMatrix[1, 2]) + (firstMatrix[0, 2] * secondMatrix[2, 2]);
            multipliedMatrix[1, 0] = (firstMatrix[1, 0] * secondMatrix[0, 0]) + (firstMatrix[1, 1] * secondMatrix[1, 0]) + (firstMatrix[1, 2] * secondMatrix[2, 0]);
            multipliedMatrix[1, 1] = (firstMatrix[1, 0] * secondMatrix[0, 1]) + (firstMatrix[1, 1] * secondMatrix[1, 1]) + (firstMatrix[1, 2] * secondMatrix[2, 1]);
            multipliedMatrix[1, 2] = (firstMatrix[1, 0] * secondMatrix[0, 2]) + (firstMatrix[1, 1] * secondMatrix[1, 2]) + (firstMatrix[1, 2] * secondMatrix[2, 2]);
            multipliedMatrix[2, 0] = (firstMatrix[2, 0] * secondMatrix[0, 0]) + (firstMatrix[2, 1] * secondMatrix[1, 0]) + (firstMatrix[2, 2] * secondMatrix[2, 0]);
            multipliedMatrix[2, 1] = (firstMatrix[2, 0] * secondMatrix[0, 1]) + (firstMatrix[2, 1] * secondMatrix[1, 1]) + (firstMatrix[2, 2] * secondMatrix[2, 1]);
            multipliedMatrix[2, 2] = (firstMatrix[2, 0] * secondMatrix[0, 2]) + (firstMatrix[2, 1] * secondMatrix[1, 2]) + (firstMatrix[2, 2] * secondMatrix[2, 2]);

            return multipliedMatrix;
        }

        /// <summary>
        /// Вычислить сопряженную матрицу 3x3
        /// </summary>
        /// <param name="a">Матрица 2х2</param>
        /// <returns>Вычисленная сопряжённая матрица</returns>
        private static double[,] AdjugateMatrix(double[,] a)
        {
            var b = new double[3, 3];

            b[0, 0] = Det2(a[1, 1], a[1, 2], a[2, 1], a[2, 2]);
            b[1, 0] = Det2(a[1, 2], a[1, 0], a[2, 2], a[2, 0]);
            b[2, 0] = Det2(a[1, 0], a[1, 1], a[2, 0], a[2, 1]);
            b[0, 1] = Det2(a[2, 1], a[2, 2], a[0, 1], a[0, 2]);
            b[1, 1] = Det2(a[2, 2], a[2, 0], a[0, 2], a[0, 0]);
            b[2, 1] = Det2(a[2, 0], a[2, 1], a[0, 0], a[0, 1]);
            b[0, 2] = Det2(a[0, 1], a[0, 2], a[1, 1], a[1, 2]);
            b[1, 2] = Det2(a[0, 2], a[0, 0], a[1, 2], a[1, 0]);
            b[2, 2] = Det2(a[0, 0], a[0, 1], a[1, 0], a[1, 1]);

            return b;
        }

        /// <summary>
        /// Вычислить матрицу для преобразования единичного четырехугольника в четырехугольник
        /// </summary>
        /// <param name="quad">Четырехугольник</param>
        /// <returns>Вычисленная матрица</returns>
        private static double[,] MapSquareToQuad(List<Point> quad)
        {
            var sq = new double[3, 3];

            double px;
            double py;

            px = quad[0].X - quad[1].X + quad[2].X - quad[3].X;
            py = quad[0].Y - quad[1].Y + quad[2].Y - quad[3].Y;

            if ((px < epsilon) && (px > -epsilon) &&
                 (py < epsilon) && (py > -epsilon))
            {
                sq[0, 0] = quad[1].X - quad[0].X;
                sq[0, 1] = quad[2].X - quad[1].X;
                sq[0, 2] = quad[0].X;

                sq[1, 0] = quad[1].Y - quad[0].Y;
                sq[1, 1] = quad[2].Y - quad[1].Y;
                sq[1, 2] = quad[0].Y;

                sq[2, 0] = 0.0;
                sq[2, 1] = 0.0;
                sq[2, 2] = 1.0;
            }
            else
            {
                double dx1;
                double dx2;
                double dy1;
                double dy2;
                double del;

                dx1 = quad[1].X - quad[2].X;
                dx2 = quad[3].X - quad[2].X;
                dy1 = quad[1].Y - quad[2].Y;
                dy2 = quad[3].Y - quad[2].Y;

                del = Det2(dx1, dx2, dy1, dy2);

                if (del == 0.0)
                {
                    return null;
                }

                sq[2, 0] = Det2(px, dx2, py, dy2) / del;
                sq[2, 1] = Det2(dx1, px, dy1, py) / del;
                sq[2, 2] = 1.0;

                sq[0, 0] = quad[1].X - quad[0].X + (sq[2, 0] * quad[1].X);
                sq[0, 1] = quad[3].X - quad[0].X + (sq[2, 1] * quad[3].X);
                sq[0, 2] = quad[0].X;

                sq[1, 0] = quad[1].Y - quad[0].Y + (sq[2, 0] * quad[1].Y);
                sq[1, 1] = quad[3].Y - quad[0].Y + (sq[2, 1] * quad[3].Y);
                sq[1, 2] = quad[0].Y;
            }

            return sq;
        }
    }
}