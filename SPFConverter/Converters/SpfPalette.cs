using nquant.NET;

public class SpfPalette
{
    public byte[] _alpha;
    public byte[] _rgb;
    public Color[] _colors;

    public SpfPalette(int colorCount)
    {
        _alpha = new byte[colorCount];
        _rgb = new byte[colorCount * 3];
        _colors = new Color[colorCount];
    }

    public static SpfPalette FromBitmap(Bitmap bitmap)
    {
        // Extract the palette from the input bitmap
        var colorCount = 256;
        var spfPalette = new SpfPalette(colorCount);

        // We'll use a quantizer to reduce the number of colors in the input image
        var quantizer = new WuQuantizer();

        using (var quantizedBitmap = quantizer.QuantizeImage(bitmap, 10, 70))
        {
            // Copy colors from the quantized bitmap's palette
            var colorPalette = quantizedBitmap.Palette;
            for (var i = 0; i < colorCount; i++)
            {
                var color = colorPalette.Entries[i];
                spfPalette._colors[i] = color;
                spfPalette._alpha[i] = (byte)color.A;
                spfPalette._rgb[i * 3] = (byte)color.R;
                spfPalette._rgb[i * 3 + 1] = (byte)color.G;
                spfPalette._rgb[i * 3 + 2] = (byte)color.B;
            }
        }

        return spfPalette;
    }

    public int FindNearestColorIndex(Color color)
    {
        var closestIndex = 0;
        var closestDistance = int.MaxValue;

        for (var i = 0; i < _colors.Length; i++)
        {
            var paletteColor = _colors[i];
            var distance = ColorDistance(color, paletteColor);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private static int ColorDistance(Color a, Color b)
    {
        var dr = a.R - b.R;
        var dg = a.G - b.G;
        var db = a.B - b.B;
        var da = a.A - b.A;

        return dr * dr + dg * dg + db * db + da * da;
    }

    public byte[] ToArray()
    {
        var paletteBytes = new byte[_alpha.Length + _rgb.Length];
        Array.Copy(_alpha, 0, paletteBytes, 0, _alpha.Length);
        Array.Copy(_rgb, 0, paletteBytes, _alpha.Length, _rgb.Length);

        return paletteBytes;
    }
}
