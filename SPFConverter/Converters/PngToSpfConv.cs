using ImageMagick;
using static SpfPalette;

namespace SPFverter.Converters;

internal abstract class PngToSpfConv
{
    public static SpfPalette? SpfPalette;

    public static void PngToSpf(string outputSpfFilePath, string inputPngFilePath)
    {
        // Initiate SPF write
        using var fileStream = new FileStream(outputSpfFilePath, FileMode.Create);
        using var binaryWriter = new BinaryWriter(fileStream);
        
        // Load and Get palette from Bitmap
        //Bitmap image = BitmapLoader.LoadBitmap(inputPngFilePath);

        // ToDo: Attempting to convert image back from png 48
        var image = LoadImagePng48(inputPngFilePath);

        // Create header
        var header = new SpfFileHeader
        {
            Unknown1 = 0,
            Unknown2 = 1,
            ColorFormat = 0 // Set color format to 8bpp
        };

        // Convert header to bytes
        var headerBytes = SpfFileHeaderToBytes(header);

        // Write file header
        binaryWriter.Write(headerBytes);

        // Write the palette
        SpfPalette.BitmapToBinaryWriterBlock(image, binaryWriter);

        // Write the frame count uint
        binaryWriter.Write((uint)1);

        // Write the frame header
        var frameHeaderBytes = SpfFrameHeaderToBytes(image);
        binaryWriter.Write(frameHeaderBytes);

        // Write the bytesTotal (bitmap width & bitmap height) * 2 for 16bpp
        var bytesTotal = (uint)(image.Width * image.Height);
        binaryWriter.Write(bytesTotal);

        // Write the frame data
        var frameDataBytes = BitmapToFrameData(image);
        binaryWriter.Write(frameDataBytes);
    }

    private static Bitmap LoadImagePng48(string inputPngFilePath)
    {
        using var image = new MagickImage(inputPngFilePath);
        // The image will be read with a bit depth of 16
        image.Depth = 16;
        // Convert the MagickImage to a byte array
        byte[] imageBytes = image.ToByteArray();
        // Convert the byte array to a Bitmap
        return BitmapLoader.LoadBitmap(imageBytes);
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