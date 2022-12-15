using System.Drawing;

namespace TogiSoft.AtlasDataBase.ArchiveWell.Perspective.Quadrilateral
{
    public class CustomRGB
    {
        //
        // Summary:
        //     Index of red component.
        public const short R = 2;
        //
        // Summary:
        //     Index of green component.
        public const short G = 1;
        //
        // Summary:
        //     Index of blue component.
        public const short B = 0;
        //
        // Summary:
        //     Index of alpha component for ARGB images.
        public const short A = 3;
        //
        // Summary:
        //     Red component.
        public byte Red;
        //
        // Summary:
        //     Green component.
        public byte Green;
        //
        // Summary:
        //     Blue component.
        public byte Blue;
        //
        // Summary:
        //     Alpha component.
        public byte Alpha;

        //
        // Summary:
        //     Initializes a new instance of the AForge.Imaging.RGB class.
        public CustomRGB()
        {
            Red = 0;
            Green = 0;
            Blue = 0;
            Alpha = 1;
        }
        //
        // Summary:
        //     Initializes a new instance of the AForge.Imaging.RGB class.
        //
        // Parameters:
        //   color:
        //     Initialize from specified color.
        public CustomRGB(Color color)
        {
            Red = color.R;
            Green = color.G;
            Blue = color.B;
            Alpha = color.A;
        }
        //
        // Summary:
        //     Initializes a new instance of the AForge.Imaging.RGB class.
        //
        // Parameters:
        //   red:
        //     Red component.
        //
        //   green:
        //     Green component.
        //
        //   blue:
        //     Blue component.
        public CustomRGB(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = 1;
        }
        //
        // Summary:
        //     Initializes a new instance of the AForge.Imaging.RGB class.
        //
        // Parameters:
        //   red:
        //     Red component.
        //
        //   green:
        //     Green component.
        //
        //   blue:
        //     Blue component.
        //
        //   alpha:
        //     Alpha component.
        public CustomRGB(byte red, byte green, byte blue, byte alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        //
        // Summary:
        //     Color value of the class.
        public Color Color { get; set; }
    }
}
