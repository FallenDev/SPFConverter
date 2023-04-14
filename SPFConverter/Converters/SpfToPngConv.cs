namespace SPFverter.Converters;

internal class SpfToPngConv
{
    public SpfFileHeader _mHeader;
    public SpfPaletteStruct _mPalette;
    private uint _mFramecount;
    private uint _mBytetotal;
    private SpfFrame[] _mFrames;
    private string _mFileName;

    public SpfFrame[] Frames => _mFrames;
    public string FileName => _mFileName;
    public uint ColorFormat => _mHeader.ColorFormat;
    public uint FrameCount => _mFramecount;
    public uint ByteTotal => _mBytetotal;

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
            var rawBits = reader.ReadBytes(byteCount);
            Frames[index].Render(rawBits);
        }
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