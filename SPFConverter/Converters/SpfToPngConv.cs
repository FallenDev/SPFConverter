using ImageMagick;

namespace SPFverter.Converters;

internal class SpfToPngConv
{
    public SpfFileHeader _mHeader;
    public SpfPaletteStruct _mPalette;
    private uint _mFramecount;
    private uint _mBytetotal;
    private SpfFrame[] _mFrames;
    private string _mFileName;
    private static byte[] _frameData;

    public SpfFrame[] Frames => _mFrames;
    public string FileName => _mFileName;
    public uint ColorFormat => _mHeader.ColorFormat;
    public uint FrameCount => _mFramecount;
    public uint ByteTotal => _mBytetotal;

    public static void SpfToPng(string outputPngFilePath, string inputSpfFilePath)
    {
        SpfToPngConv spf = SpfToPngConv.FromFile(inputSpfFilePath);

        // Assume the first frame is the one you want to convert to PNG
        SpfFrame frame = spf.Frames[0];

        // Get the raw frame data
        byte[] frameData = GetRawBits();

        // Convert the SPF palette and frame data to a Bitmap
        Bitmap pngBitmap = SpfPaletteStruct.SpfPaletteToBitmap(spf._mPalette, (int)frame.PixelWidth, (int)frame.PixelHeight, frameData);
        //Bitmap pngBitmap = SpfPaletteStruct.ToBitmap(spf._mPalette);

        PngToSpfConv.PrintPalette(spf._mPalette);

        // Save the bitmap as a PNG
        pngBitmap.Save(outputPngFilePath, ImageFormat.Png);

        // ToDo: Attempting to convert image to png 48 to save alpha data as 16 bytes
        //SaveImagePng48(outputPngFilePath, pngBitmap);
    }

    private static void SaveImagePng48(string outputPngFilePath, Bitmap map)
    {
        // Convert Bitmap to Byte Array
        var memStream = new MemoryStream();
        map.Save(memStream, ImageFormat.Png);
        byte[] mapBytes = memStream.ToArray();
        using var image = new MagickImage(mapBytes);
        // The image will be saved with a bit depth of 16
        image.Depth = 16;
        image.Write(outputPngFilePath, MagickFormat.Png48);
    }


    public static SpfToPngConv FromFile(string fileName)
    {
        if (!File.Exists(fileName)) return null;

        var binaryReader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));

        // Create and Read file header
        var spfFile = new SpfToPngConv
        {
            _mFileName = fileName,
            _mHeader = SpfFileHeader.FromBinaryReaderBlock(binaryReader)
        };

        // If 8bpp, extract palette
        if (spfFile.ColorFormat == 0)
            spfFile._mPalette = SpfPaletteStruct.FromBinaryReaderBlock(binaryReader);

        // Read frame counts
        spfFile._mFramecount = binaryReader.ReadUInt32();
        spfFile._mFrames = new SpfFrame[(int)(nint)spfFile.FrameCount];

        // Read frame header
        spfFile.FrameHeadersFromReader(binaryReader);

        // Read total bytes
        spfFile._mBytetotal = binaryReader.ReadUInt32();

        // Read frame data
        spfFile.FrameDataFromReader(binaryReader);
        binaryReader.Close();

        return spfFile;
    }

    private void FrameHeadersFromReader(BinaryReader reader)
    {
        for (long index = 0; index < FrameCount; ++index)
        {
            var h = SpfFrameHeader.FromBinaryReaderBlock(reader);
            Frames[index] = new SpfFrame(h, ColorFormat, _mPalette);
        }
    }

    private void FrameDataFromReader(BinaryReader reader)
    {
        for (long index = 0; index < FrameCount; ++index)
        {
            var byteCount = (int)Frames[index].ByteCount;
            var numArray = new byte[byteCount];
            _frameData = reader.ReadBytes(byteCount);
            Frames[index].Render(_frameData);
        }
    }

    private static byte[] GetRawBits()
    {
        return _frameData;
    }
}

public sealed class SpfFrame
{
    private readonly SpfFrameHeader _mHeader;
    public readonly Bitmap FrameBitmap;

    public int PadWidth => _mHeader.PadWidth;
    public int PadHeight => _mHeader.PadHeight;
    public int PixelWidth => _mHeader.PixelWidth;
    public int PixelHeight => _mHeader.PixelHeight;
    public uint Unknown => _mHeader.Unknown;
    public uint Reserved => _mHeader.Reserved;
    public uint StartAddress => _mHeader.StartAddress;
    public uint ByteWidth => _mHeader.ByteWidth;
    public uint ByteCount => _mHeader.ByteCount;
    public uint SemiByteCount => _mHeader.SemiByteCount;

    public SpfFrame(SpfFrameHeader h, uint format, SpfPaletteStruct p)
    {
        _mHeader = h;
        if (ByteCount == 0U) return;
        if (format == 0U)
        {
            FrameBitmap = new Bitmap(PixelWidth, PixelHeight, PixelFormat.Format8bppIndexed);
            var palette = FrameBitmap.Palette;
            Array.Copy(p._colors, palette.Entries, 256);
            FrameBitmap.Palette = palette;
        }
        else
            FrameBitmap = new Bitmap(PixelWidth, PixelHeight, PixelFormat.Format16bppRgb555);
    }

    public void Render(byte[] rawBits)
    {
        if (ByteCount == 0U) return;
        if (FrameBitmap.PixelFormat is PixelFormat.Format8bppIndexed or PixelFormat.Format16bppRgb555)
            Render8BppI16BppRgb(rawBits);
        else
            Render32Bpp(rawBits);
    }

    private void Render8BppI16BppRgb(byte[] rawBits)
    {
        var num1 = 1;
        if (FrameBitmap.PixelFormat == PixelFormat.Format16bppRgb555) num1 = 2;

        var bitmapData = FrameBitmap.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.ReadWrite, FrameBitmap.PixelFormat);
        var scan0 = bitmapData.Scan0;
        var num2 = 4 - PixelWidth * num1 % 4;

        if (num2 < 4 || PadWidth > 0 || PadHeight > 0)
        {
            if (num2 == 4) num2 = 0;
            var num3 = PixelWidth * num1 + num2;
            var numArray = new byte[PixelHeight * num3];

            for (var index = 0; index < PixelHeight - PadHeight; ++index)
            {
                Array.Copy(rawBits, index * num1 * (PixelWidth - PadWidth), numArray, index * num3, num1 * (PixelWidth - PadWidth));
                Marshal.Copy(numArray, 0, scan0, PixelHeight * num3);
            }
        }
        else
            Marshal.Copy(rawBits, 0, scan0, PixelHeight * PixelWidth * num1);

        FrameBitmap.UnlockBits(bitmapData);
    }

    private void Render32Bpp(byte[] rawBits)
    {
        var length = rawBits.Length / 2;
        var numArray = new byte[length];
        Array.Copy(rawBits, length, numArray, 0, length);
        Render8BppI16BppRgb(numArray);
    }
}