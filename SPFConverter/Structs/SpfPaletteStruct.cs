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
            var blue = (uint16 & 31) << 3; //take the first 5 bits and shift them left 3
            var green = ((uint16 >> 5) & 31) << 3; //take the next 5 bits, shift them left 3
            var red = ((uint16 >> 10) & 31) << 3; //take the next 5 bits, shift them left 3
            
            // ToDo: Full Transparency - Sets anything that's not a color to transparent
            //var alpha = (red == 0 && green == 0 && blue == 0) ? 0 : 255;
            // ToDo: Alpha Transparency - Sets as another channel, but perhaps it should be a layer?
            //var alpha = spfPalette._alpha[2 * index];

            // Read the 16-bit alpha value from the _alpha array
            var alphaUint16 = BitConverter.ToUInt16(spfPalette._alpha, 2 * index);
            var alpha = alphaUint16 >> 8;

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

    public static Bitmap ToBitmap(SpfPaletteStruct spfPalette)
    {
        Bitmap result = new Bitmap(256, 1, PixelFormat.Format8bppIndexed);
        ColorPalette outputPalette = result.Palette;

        for (int i = 0; i < 256; ++i)
        {
            ushort alphaValue = BitConverter.ToUInt16(spfPalette._alpha, 2 * i);
            byte alpha = (byte)(alphaValue & 0xFF); // Extract the alpha channel from the value
            Color baseColor = spfPalette._colors[i];

            // Create a new color with the proper alpha channel
            outputPalette.Entries[i] = Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);
        }

        result.Palette = outputPalette;

        BitmapData bmpData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height),
            ImageLockMode.WriteOnly, result.PixelFormat);

        for (int x = 0; x < result.Width; ++x)
        {
            Marshal.WriteByte(bmpData.Scan0, x, (byte)x);
        }

        result.UnlockBits(bmpData);

        return result;
    }
}