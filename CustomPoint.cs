using System;

namespace TogiSoft.AtlasDataBase.ArchiveWell.Perspective.Quadrilateral
{
    [Serializable]
    public struct CustomPoint
    {
        public float X;

        public float Y;

        public CustomPoint(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public float DistanceTo(CustomPoint anotherPoint)
        {
            float dx = X - anotherPoint.X;
            float dy = Y - anotherPoint.Y;

            return (float)System.Math.Sqrt(dx * dx + dy * dy);
        }

        public static CustomPoint operator +(CustomPoint point1, CustomPoint point2)
        {
            return new CustomPoint(point1.X + point2.X, point1.Y + point2.Y);
        }

        public static CustomPoint Add(CustomPoint point1, CustomPoint point2)
        {
            return new CustomPoint(point1.X + point2.X, point1.Y + point2.Y);
        }

        public static CustomPoint operator -(CustomPoint point1, CustomPoint point2)
        {
            return new CustomPoint(point1.X - point2.X, point1.Y - point2.Y);
        }

        public static CustomPoint Subtract(CustomPoint point1, CustomPoint point2)
        {
            return new CustomPoint(point1.X - point2.X, point1.Y - point2.Y);
        }

        public static CustomPoint operator +(CustomPoint point, float valueToAdd)
        {
            return new CustomPoint(point.X + valueToAdd, point.Y + valueToAdd);
        }

        public static CustomPoint Add(CustomPoint point, float valueToAdd)
        {
            return new CustomPoint(point.X + valueToAdd, point.Y + valueToAdd);
        }

        public static CustomPoint operator -(CustomPoint point, float valueToSubtract)
        {
            return new CustomPoint(point.X - valueToSubtract, point.Y - valueToSubtract);
        }

        public static CustomPoint Subtract(CustomPoint point, float valueToSubtract)
        {
            return new CustomPoint(point.X - valueToSubtract, point.Y - valueToSubtract);
        }

        public static CustomPoint operator *(CustomPoint point, float factor)
        {
            return new CustomPoint(point.X * factor, point.Y * factor);
        }

        public static CustomPoint Multiply(CustomPoint point, float factor)
        {
            return new CustomPoint(point.X * factor, point.Y * factor);
        }

        public static CustomPoint operator /(CustomPoint point, float factor)
        {
            return new CustomPoint(point.X / factor, point.Y / factor);
        }

        public static CustomPoint Divide(CustomPoint point, float factor)
        {
            return new CustomPoint(point.X / factor, point.Y / factor);
        }

        public static bool operator ==(CustomPoint point1, CustomPoint point2)
        {
            return ((point1.X == point2.X) && (point1.Y == point2.Y));
        }

        public static bool operator !=(CustomPoint point1, CustomPoint point2)
        {
            return ((point1.X != point2.X) || (point1.Y != point2.Y));
        }

        public override bool Equals(object obj)
        {
            return (obj is CustomPoint) ? (this == (CustomPoint)obj) : false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode();
        }

        public static explicit operator IntPoint(CustomPoint point)
        {
            return new IntPoint((int)point.X, (int)point.Y);
        }

        public static implicit operator DoublePoint(CustomPoint point)
        {
            return new DoublePoint(point.X, point.Y);
        }

        public IntPoint Round()
        {
            return new IntPoint((int)Math.Round(X), (int)Math.Round(Y));
        }

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}, {1}", X, Y);
        }

        public float EuclideanNorm()
        {
            return (float)System.Math.Sqrt(X * X + Y * Y);
        }
    }
}
