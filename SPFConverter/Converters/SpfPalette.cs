
public class SpfPalette
{
    public byte[] _argb;
    public byte[] _alpha;
    public byte[] _rgb;
    public Color[] _colors;

    public SpfPalette(int colorCount)
    {
        _argb = new byte[colorCount * 4];
        //_alpha = new byte[colorCount];
        //_rgb = new byte[colorCount * 3];
        _colors = new Color[colorCount];
    }

    public static SpfPalette FromBitmap(Bitmap bitmap)
    {
        // Extract the palette from the input bitmap
        var colorCount = 256;
        var spfPalette = new SpfPalette(colorCount);

        // We'll use a quantizer to reduce the number of colors in the input image
        var quantizer = new PnnQuant.PnnQuantizer();
        using var quantizedBitmap = quantizer.QuantizeImage(bitmap, PixelFormat.Format8bppIndexed, colorCount, true);
        
        // Copy colors from the quantized bitmap's palette
        var colorPalette = quantizedBitmap.Palette;

        // Do the nQuant.Master conversion, then:
        // Print the indexed image's palette
        Debug.WriteLine("Indexed Image Palette:");
        for (int i = 0; i < colorPalette.Entries.Length; i++)
        {
            Debug.WriteLine($"Color {i}: {colorPalette.Entries[i]}");
        }

        //for (var i = 0; i < colorCount; i++)
        //{
        //    var color = colorPalette.Entries[i];
        //    spfPalette._colors[i] = color;
        //    spfPalette._alpha[i] = (byte)color.A;
        //    spfPalette._rgb[i * 3] = (byte)color.R;
        //    spfPalette._rgb[i * 3 + 1] = (byte)color.G;
        //    spfPalette._rgb[i * 3 + 2] = (byte)color.B;
        //}

        for (var i = 0; i < colorCount; i++)
        {
            var color = colorPalette.Entries[i];
            spfPalette._colors[i] = color;

            // Use a single array to store ARGB values for each color
            spfPalette._argb[i * 4] = (byte)color.A;
            spfPalette._argb[i * 4 + 1] = (byte)color.R;
            spfPalette._argb[i * 4 + 2] = (byte)color.G;
            spfPalette._argb[i * 4 + 3] = (byte)color.B;
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

    //public byte[] ToArray()
    //{
    //    var paletteBytes = new byte[_alpha.Length + _rgb.Length];
    //    Array.Copy(_alpha, 0, paletteBytes, 0, _alpha.Length);
    //    Array.Copy(_rgb, 0, paletteBytes, _alpha.Length, _rgb.Length);

    //    return paletteBytes;
    //}

    public byte[] ToArray()
    {
        int colorCount = _colors.Length;
        var paletteBytes = new byte[colorCount * 4];

        for (int i = 0; i < colorCount; i++)
        {
            paletteBytes[i * 4] = _argb[i * 4];         // Alpha
            paletteBytes[i * 4 + 1] = _argb[i * 4 + 1]; // Red
            paletteBytes[i * 4 + 2] = _argb[i * 4 + 2]; // Green
            paletteBytes[i * 4 + 3] = _argb[i * 4 + 3]; // Blue
        }

        return paletteBytes;
    }
}
