namespace SPFverter.Converters;

internal abstract class PngToSpfConv
{
    public static void PngToSpf(string outputSpfFilePath, Bitmap loadedBitmap)
    {
        using var fileStream = new FileStream(outputSpfFilePath, FileMode.Create);
        using var binaryWriter = new BinaryWriter(fileStream);

        // Create header
        var header = new SpfFileHeader
        {
            Unknown1 = 0,
            Unknown2 = 1,
            ColorFormat = 1 // Set color format to 16bpp
        };

        // Convert header to bytes
        var headerBytes = SpfFileHeaderToBytes(header);

        // Write file header
        binaryWriter.Write(headerBytes);

        // Write the frame count uint
        binaryWriter.Write((uint)1);

        // Write the frame header
        var frameHeaderBytes = SpfFrameHeaderToBytes(loadedBitmap);
        binaryWriter.Write(frameHeaderBytes);

        // Write the bytesTotal (bitmap width & bitmap height) * 2 for 16bpp
        var bytesTotal = (uint)(loadedBitmap.Width * loadedBitmap.Height) * 2;

        binaryWriter.Write(bytesTotal);

        // Write the frame data
        var frameDataBytes = BitmapToFrameData(loadedBitmap);
        binaryWriter.Write(frameDataBytes);
    }

    private static Bitmap ConvertTo16bppRgb555(Bitmap input)
    {
        var width = input.Width;
        var height = input.Height;

        var output = new Bitmap(width, height, PixelFormat.Format16bppRgb555);
    
        using var graphics = Graphics.FromImage(output);
        graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
        graphics.Clear(Color.Transparent);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = input.GetPixel(x, y);
            
                // Set the alpha channel to black (#000000)
                if (pixelColor.A < 255)
                {
                    pixelColor = Color.FromArgb(0, 0, 0);
                }
            
                output.SetPixel(x, y, pixelColor);
            }
        }
    
        return output;
    }

    private static byte[] BitmapToFrameData(Bitmap bitmap)
    {
        // Check if the input bitmap is already in 16bppRgb555 format, if not, convert it
        if (bitmap.PixelFormat != PixelFormat.Format16bppRgb555)
        {
            bitmap = ConvertTo16bppRgb555(bitmap);
        }

        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format16bppRgb555);

        var ptr = bitmapData.Scan0;
        var size = bitmapData.Stride * bitmap.Height;
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
}