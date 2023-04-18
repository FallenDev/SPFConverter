
public class SpfPalette
{
    public byte[] _argb;
    public Color[] _colors;

    public SpfPalette(int colorCount)
    {
        _argb = new byte[colorCount * 4];
        _colors = new Color[colorCount];
    }

    public static SpfPalette FromBitmap(Bitmap bitmap)
    {
        // Print the loaded bitmap
        Debug.WriteLine("Loaded Bitmap:");
        for (int i = 0; i < bitmap.Palette.Entries.Length; i++)
        {
            Debug.WriteLine($"Color {i}: {bitmap.Palette.Entries[i]}");
        }

        // Extract the palette from the input bitmap
        var colorCount = 256;
        var spfPalette = new SpfPalette(colorCount);

        // We'll use a quantizer to reduce the number of colors in the input image
        var quantizer = new PnnQuant.PnnQuantizer();
        using var quantizedBitmap = quantizer.QuantizeImage(bitmap, PixelFormat.Format8bppIndexed, colorCount, true);
        
        // Copy colors from the quantized bitmap's palette
        var colorPalette = quantizedBitmap.Palette;

        // Print the quantized palette
        //Debug.WriteLine("Quantized Palette:");
        //for (int i = 0; i < colorPalette.Entries.Length; i++)
        //{
        //    Debug.WriteLine($"Color {i}: {colorPalette.Entries[i]}");
        //}
        Debug.WriteLine("Quantized Palette:");
        List<(int, int, int)> rgbPalette = new List<(int, int, int)>();

        for (int i = 0; i < colorPalette.Entries.Length; i++)
        {
            Color color = colorPalette.Entries[i];
            rgbPalette.Add((color.R, color.G, color.B));
            Debug.WriteLine($"Color {i}: {color}");
        }

        Debug.WriteLine("RGB Palette: " + string.Join(", ", rgbPalette));


        for (var i = 0; i < colorCount; i++)
        {
            var color = colorPalette.Entries[i];
            spfPalette._colors[i] = color;

            // Use a single array to store ARGB values for each color
            spfPalette._argb[i * 4] = (byte)color.A;
            spfPalette._argb[i * 4 + 1] = (byte)color.G;
            spfPalette._argb[i * 4 + 2] = (byte)color.B;
            spfPalette._argb[i * 4 + 3] = (byte)color.R;
        }

        return spfPalette;
    }

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
