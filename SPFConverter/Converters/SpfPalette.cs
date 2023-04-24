using System.Text;

public class SpfPalette
{
    public struct SpfPaletteGen
    {
        public byte[] _alpha;
        public byte[] _rgb;
        public Color[] _colors;
    }

    public static SpfPaletteGen FromBitmap(Bitmap image)
    {
        SpfPaletteGen spfPalette;
        spfPalette._alpha = new byte[512];
        spfPalette._rgb = new byte[512];
        spfPalette._colors = new Color[256];

        Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();
    
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color color = image.GetPixel(x, y);
                if (!colorCounts.ContainsKey(color))
                {
                    colorCounts[color] = 1;
                }
                else
                {
                    colorCounts[color]++;
                }
            }
        }

        var sortedColors = colorCounts.OrderByDescending(c => c.Value).Select(c => c.Key).ToArray();

        for (int i = 0; i < sortedColors.Length && i < 256; i++)
        {
            Color color = sortedColors[i];
            int red = color.R;
            int green = color.G;
            int blue = color.B;
            int alpha = color.A;

            spfPalette._colors[i] = color;

            ushort rgb = (ushort)(((red / 8) * 32 * 32) + ((green / 8) * 32) + (blue / 8));
            BitConverter.GetBytes(rgb).CopyTo(spfPalette._rgb, 2 * i);

            // Set the alpha value to the _alpha array
            spfPalette._alpha[2 * i] = (byte)alpha;
            spfPalette._alpha[2 * i + 1] = 0;
        }

        return spfPalette;
    }

    public static byte[] SpfPaletteToByteArray(SpfPaletteGen spfPalette)
    {
        byte[] result = new byte[1024]; // 512 bytes for alpha and 512 bytes for rgb

        Array.Copy(spfPalette._alpha, 0, result, 0, spfPalette._alpha.Length);
        Array.Copy(spfPalette._rgb, 0, result, spfPalette._alpha.Length, spfPalette._rgb.Length);

        return result;
    }
}

/// <summary>
/// Image loading toolset class which corrects the issue with palette PNG images with transparency from being loaded
/// </summary>
public class BitmapLoader
{
    private static Byte[] PNG_IDENTIFIER = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    /// <summary>
    /// Loads an image, checks if it is a PNG containing palette transparency, and if so, ensures it loads correctly.
    /// The theory can be found at http://www.libpng.org/pub/png/book/chapter08.html
    /// </summary>
    /// <param name="filename">Filename to load</param>
    /// <returns>The loaded image</returns>
    public static Bitmap LoadBitmap(String filename)
    {
        Byte[] data = File.ReadAllBytes(filename);
        return LoadBitmap(data);
    }

    /// <summary>
    /// Loads an image, checks if it is a PNG containing palette transparency, and if so, ensures it loads correctly.
    /// The theory can be found at http://www.libpng.org/pub/png/book/chapter08.html
    /// </summary>
    /// <param name="data">File data to load</param>
    /// <returns>The loaded image</returns>
    public static Bitmap LoadBitmap(Byte[] data)
    {
        Byte[] transparencyData = null;
        if (data.Length > PNG_IDENTIFIER.Length)
        {
            // Check if the image is a PNG.
            Byte[] compareData = new Byte[PNG_IDENTIFIER.Length];
            Array.Copy(data, compareData, PNG_IDENTIFIER.Length);
            if (PNG_IDENTIFIER.SequenceEqual(compareData))
            {
                // Check if it contains a palette.
                // I'm sure it can be looked up in the header somehow, but meh.
                Int32 plteOffset = FindChunk(data, "PLTE");
                if (plteOffset != -1)
                {
                    // Check if it contains a palette transparency chunk.
                    Int32 trnsOffset = FindChunk(data, "tRNS");
                    if (trnsOffset != -1)
                    {
                        // Get chunk
                        Int32 trnsLength = GetChunkDataLength(data, trnsOffset);
                        transparencyData = new Byte[trnsLength];
                        Array.Copy(data, trnsOffset + 8, transparencyData, 0, trnsLength);
                        // filter out the palette alpha chunk, make new data array
                        Byte[] data2 = new Byte[data.Length - (trnsLength + 12)];
                        Array.Copy(data, 0, data2, 0, trnsOffset);
                        Int32 trnsEnd = trnsOffset + trnsLength + 12;
                        Array.Copy(data, trnsEnd, data2, trnsOffset, data.Length - trnsEnd);
                        data = data2;
                    }
                }
            }
        }
        Bitmap loadedImage;
        using (MemoryStream ms = new MemoryStream(data))
        using (Bitmap tmp = new Bitmap(ms))
            loadedImage = CloneImage(tmp);
        ColorPalette pal = loadedImage.Palette;
        if (pal.Entries.Length == 0 || transparencyData == null)
            return loadedImage;
        for (Int32 i = 0; i < pal.Entries.Length; i++)
        {
            if (i >= transparencyData.Length)
                break;
            Color col = pal.Entries[i];
            pal.Entries[i] = Color.FromArgb(transparencyData[i], col.R, col.G, col.B);
        }
        loadedImage.Palette = pal;
        return loadedImage;
    }

    /// <summary>
    /// Finds the start of a png chunk. This assumes the image is already identified as PNG.
    /// It does not go over the first 8 bytes, but starts at the start of the header chunk.
    /// </summary>
    /// <param name="data">The bytes of the png image</param>
    /// <param name="chunkName">The name of the chunk to find.</param>
    /// <returns>The index of the start of the png chunk, or -1 if the chunk was not found.</returns>
    private static Int32 FindChunk(Byte[] data, String chunkName)
    {
        if (chunkName.Length != 4)
            throw new ArgumentException("Chunk must be 4 characters!", "chunkName");
        Byte[] chunkNamebytes = Encoding.ASCII.GetBytes(chunkName);
        if (chunkNamebytes.Length != 4)
            throw new ArgumentException("Chunk must be 4 characters!", "chunkName");
        Int32 offset = PNG_IDENTIFIER.Length;
        Int32 end = data.Length;
        Byte[] testBytes = new Byte[4];
        // continue until either the end is reached, or there is not enough space behind it for reading a new chunk
        while (offset + 12 <= end)
        {
            Array.Copy(data, offset + 4, testBytes, 0, 4);
            // Alternative for more visual debugging:
            //String currentChunk = Encoding.ASCII.GetString(testBytes);
            //if (chunkName.Equals(currentChunk))
            //    return offset;
            if (chunkNamebytes.SequenceEqual(testBytes))
                return offset;
            Int32 chunkLength = GetChunkDataLength(data, offset);
            // chunk size + chunk header + chunk checksum = 12 bytes.
            offset += 12 + chunkLength;
        }
        return -1;
    }

    private static Int32 GetChunkDataLength(Byte[] data, Int32 offset)
    {
        if (offset + 4 > data.Length)
            throw new IndexOutOfRangeException("Bad chunk size in png image.");
        // Don't want to use BitConverter; then you have to check platform endianness and all that mess.
        Int32 length = data[offset + 3] + (data[offset + 2] << 8) + (data[offset + 1] << 16) + (data[offset] << 24);
        if (length < 0)
            throw new IndexOutOfRangeException("Bad chunk size in png image.");
        return length;
    }

    /// <summary>
    /// Clones an image object to free it from any backing resources.
    /// Code taken from http://stackoverflow.com/a/3661892/ with some extra fixes.
    /// </summary>
    /// <param name="sourceImage">The image to clone</param>
    /// <returns>The cloned image</returns>
    public static Bitmap CloneImage(Bitmap sourceImage)
    {
        Rectangle rect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
        Bitmap targetImage = new Bitmap(rect.Width, rect.Height, sourceImage.PixelFormat);
        targetImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
        BitmapData sourceData = sourceImage.LockBits(rect, ImageLockMode.ReadOnly, sourceImage.PixelFormat);
        BitmapData targetData = targetImage.LockBits(rect, ImageLockMode.WriteOnly, targetImage.PixelFormat);
        Int32 actualDataWidth = ((Image.GetPixelFormatSize(sourceImage.PixelFormat) * rect.Width) + 7) / 8;
        Int32 h = sourceImage.Height;
        Int32 origStride = sourceData.Stride;
        Boolean isFlipped = origStride < 0;
        origStride = Math.Abs(origStride); // Fix for negative stride in BMP format.
        Int32 targetStride = targetData.Stride;
        Byte[] imageData = new Byte[actualDataWidth];
        IntPtr sourcePos = sourceData.Scan0;
        IntPtr destPos = targetData.Scan0;
        // Copy line by line, skipping by stride but copying actual data width
        for (Int32 y = 0; y < h; y++)
        {
            Marshal.Copy(sourcePos, imageData, 0, actualDataWidth);
            Marshal.Copy(imageData, 0, destPos, actualDataWidth);
            sourcePos = new IntPtr(sourcePos.ToInt64() + origStride);
            destPos = new IntPtr(destPos.ToInt64() + targetStride);
        }
        targetImage.UnlockBits(targetData);
        sourceImage.UnlockBits(sourceData);
        // Fix for negative stride on BMP format.
        if (isFlipped)
            targetImage.RotateFlip(RotateFlipType.Rotate180FlipX);
        // For indexed images, restore the palette. This is not linking to a referenced
        // object in the original image; the getter of Palette creates a new object when called.
        if ((sourceImage.PixelFormat & PixelFormat.Indexed) != 0)
            targetImage.Palette = sourceImage.Palette;
        // Restore DPI settings
        targetImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
        return targetImage;
    }
}