using static SpfPalette;

namespace SPFverter.Converters;

internal abstract class TiffToSpfConv
{
    public static void TiffToSpf(string outputSpfFilePath, string inputTiffFilePath)
    {
        // Initiate SPF write
        using var fileStream = new FileStream(outputSpfFilePath, FileMode.Create);
        using var binaryWriter = new BinaryWriter(fileStream);
        //var image = BitmapLoader.LoadBitmap(inputTiffFilePath);
        using var image = (Bitmap)Image.FromFile(inputTiffFilePath);

        // Write file header, ColorFormat 1 for 16bpp
        var header = new SpfFileHeader
        {
            Unknown1 = 0,
            Unknown2 = 1,
            ColorFormat = image.PixelFormat == PixelFormat.Format8bppIndexed ? (uint)0 : (uint)1
        };
        var headerBytes = SpfFileHeaderToBytes(header);
        binaryWriter.Write(headerBytes);

        // Write the palette
        //var spfPalette = FromBitmap(image.Palette);
        var spfPalette = FromBitmap(image);
        var spfPaletteByteArray = SpfPaletteToByteArray(spfPalette);
        binaryWriter.Write(spfPaletteByteArray);

        // ToDo: Print Palette
        PrintPalette(spfPalette);

        // Write the frame count uint
        binaryWriter.Write((uint)1);

        // Write the frame header
        var frameHeaderBytes = SpfFrameHeaderToBytes(image);
        binaryWriter.Write(frameHeaderBytes);

        // Write the bytesTotal (bitmap width & bitmap height) * 2 for 16bpp
        var bytesTotal = image.PixelFormat == PixelFormat.Format8bppIndexed ? (uint)(image.Width * image.Height) : (uint)(image.Width * image.Height) * 2;
        binaryWriter.Write(bytesTotal);

        // Write the frame data
        var frameDataBytes = BitmapToFrameData(image);
        binaryWriter.Write(frameDataBytes);
    }
    
    private static byte[] BitmapToFrameData(Bitmap bitmap)
    {
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

        var ptr = bitmapData.Scan0;
        var size = Math.Abs(bitmapData.Stride) * bitmap.Height;
        var frameData = new byte[size];
        Marshal.Copy(ptr, frameData, 0, size);

        bitmap.UnlockBits(bitmapData);

        return frameData;
    }

    private static byte[] SpfFileHeaderToBytes(SpfFileHeader header)
    {
        // Convert the header struct to bytes
        var headerSize = Marshal.SizeOf(header);
        var headerBytes = new byte[headerSize];

        var gcHandle = GCHandle.Alloc(headerBytes, GCHandleType.Pinned);
        Marshal.StructureToPtr(header, gcHandle.AddrOfPinnedObject(), false);
        gcHandle.Free();

        return headerBytes;
    }

    private static byte[] SpfFrameHeaderToBytes(Bitmap bitmap)
    {
        // Create an SpfFrameHeader struct
        SpfFrameHeader spfFrameHeader;
        spfFrameHeader.PadWidth = 0;
        spfFrameHeader.PadHeight = 0;
        spfFrameHeader.PixelWidth = (ushort)bitmap.Width;
        spfFrameHeader.PixelHeight = (ushort)bitmap.Height;
        spfFrameHeader.Unknown = 3435973836; // Every SPF has this value associated with it
        spfFrameHeader.Reserved = 0;
        spfFrameHeader.StartAddress = 0; // Have not seen this change
        spfFrameHeader.ByteWidth = (uint)bitmap.Width;
        spfFrameHeader.SemiByteCount = 0;

        // Write the bytesTotal (bitmap width & bitmap height) *2 if 16bpp
        if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
            spfFrameHeader.ByteCount = (uint)(bitmap.Width * bitmap.Height);
        else
            spfFrameHeader.ByteCount = (uint)(bitmap.Width * bitmap.Height) * 2;

        // Convert the SpfFrameHeader struct to bytes
        var headerBytes = new byte[Marshal.SizeOf(spfFrameHeader)];
        var gcHandle = GCHandle.Alloc(headerBytes, GCHandleType.Pinned);
        Marshal.StructureToPtr(spfFrameHeader, gcHandle.AddrOfPinnedObject(), false);
        gcHandle.Free();

        return headerBytes;
    }

    public static void PrintPalette(SpfPaletteGen spfPalette)
    {
        for (int i = 0; i < 256; ++i)
        {
            ushort alpha = BitConverter.ToUInt16(spfPalette._alpha, 2 * i);
            ushort rgb = BitConverter.ToUInt16(spfPalette._rgb, 2 * i);

            Debug.WriteLine($"Index: {i} | Color: {spfPalette._colors[i]} | Alpha: {alpha} | RGB: {rgb}");
        }
    }

    public static void PrintPalette(SpfPaletteStruct spfPalette)
    {
        for (int i = 0; i < 256; ++i)
        {
            ushort alpha = BitConverter.ToUInt16(spfPalette._alpha, 2 * i);
            ushort rgb = BitConverter.ToUInt16(spfPalette._rgb, 2 * i);

            Debug.WriteLine($"Index: {i} | Color: {spfPalette._colors[i]} | Alpha: {alpha} | RGB: {rgb}");
        }
    }
}