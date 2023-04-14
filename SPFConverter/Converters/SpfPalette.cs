using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace SPFverter.Converters
{
    public class SpfPalette
    {
        private Color[] _colors;

        private SpfPalette()
        {
        }

        public static SpfPalette CreateFromBitmap(Bitmap bitmap)
        {
            SpfPalette spfPalette = new SpfPalette();
            ColorPalette colorPalette = bitmap.Palette;
            spfPalette._colors = colorPalette.Entries;

            return spfPalette;
        }

        public ColorPalette ToColorPalette()
        {
            using (var bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                ColorPalette colorPalette = bmp.Palette;

                for (int i = 0; i < _colors.Length; i++)
                {
                    colorPalette.Entries[i] = _colors[i];
                }

                return colorPalette;
            }
        }

        public byte[] ToByteArray()
        {
            var bytes = new byte[_colors.Length * 4];

            for (int i = 0; i < _colors.Length; i++)
            {
                bytes[i * 4] = _colors[i].A;
                bytes[i * 4 + 1] = _colors[i].R;
                bytes[i * 4 + 2] = _colors[i].G;
                bytes[i * 4 + 3] = _colors[i].B;
            }

            return bytes;
        }

        public Color GetClosestColor(Color color)
        {
            int minDistance = int.MaxValue;
            Color closestColor = Color.Black;
            for (int i = 0; i < _colors.Length; i++)
            {
                int distance = ColorDistance(color, _colors[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestColor = _colors[i];
                }
            }

            return closestColor;
        }

        private int ColorDistance(Color c1, Color c2)
        {
            int rDiff = c1.R - c2.R;
            int gDiff = c1.G - c2.G;
            int bDiff = c1.B - c2.B;
            int aDiff = c1.A - c2.A;

            return rDiff * rDiff + gDiff * gDiff + bDiff * bDiff + aDiff * aDiff;
        }
    }
}
