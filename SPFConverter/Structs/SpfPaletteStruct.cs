namespace SPFverter.Structs;

public struct SpfPaletteStruct
{
    public byte[] _alpha;
    public byte[] _rgb;
    public Color[] _colors;

    /// <summary>
    /// Reads 8bppIndexed Palette, and converts #00000 black to alpha channel
    /// </summary>
    public static SpfPaletteStruct FromBinaryReaderBlock(BinaryReader br)
    {
        SpfPaletteStruct spfPalette;
        spfPalette._alpha = br.ReadBytes(512);
        spfPalette._rgb = br.ReadBytes(512);
        spfPalette._colors = new Color[256];

        for (var index = 0; index < 256; ++index)
        {
            var uint16 = BitConverter.ToUInt16(spfPalette._rgb, 2 * index);
            var blue = 8 * (uint16 % 32);
            var green = 8 * (uint16 / 32 % 32);
            var red = 8 * (uint16 / 32 / 32 % 32);
            //var alpha = (red == 0 && green == 0 && blue == 0) ? 0 : 255; // Alpha is #000000, so make this transparent
            var alpha = spfPalette._alpha[2 * index]; // Use original alpha channel
            spfPalette._colors[index] = Color.FromArgb(alpha, red, green, blue);
        }

        return spfPalette;
    }

    public static Bitmap SpfPaletteToBitmap(SpfPaletteStruct spfPalette, int width, int height, byte[] frameData)
    {
        Bitmap result = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

        // Create a color palette from the SpfPaletteStruct
        ColorPalette pngPalette = result.Palette;
        for (int i = 0; i < spfPalette._colors.Length; i++)
        {
            pngPalette.Entries[i] = spfPalette._colors[i];
        }
        result.Palette = pngPalette;

        // Create a BitmapData object to lock the bitmap
        BitmapData bmpData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, result.PixelFormat);

        // Get the address of the first line
        IntPtr ptr = bmpData.Scan0;

        // Calculate the number of bytes required and create an array to hold the data
        int numBytes = Math.Abs(bmpData.Stride) * height;
        byte[] rgbaValues = new byte[numBytes];

        // Copy the frameData to the rgbaValues array
        for (int y = 0; y < height; y++)
        {
            int rowOffset = y * bmpData.Stride;
            for (int x = 0; x < width; x++)
            {
                int index = frameData[y * width + x];
                rgbaValues[rowOffset + x] = (byte)index;
            }
        }

        // Copy the RGBA values back to the bitmap
        Marshal.Copy(rgbaValues, 0, ptr, numBytes);

        // Unlock the bits
        result.UnlockBits(bmpData);

        return result;
    }
}