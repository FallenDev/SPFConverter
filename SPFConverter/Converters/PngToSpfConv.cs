using System.Security.Cryptography.X509Certificates;

namespace SPFverter.Converters;

internal abstract class PngToSpfConv
{
    public static SpfPalette? SpfPalette;

    public static void PngToSpf(string outputSpfFilePath, Bitmap loadedBitmap)
    {
        using var fileStream = new FileStream(outputSpfFilePath, FileMode.Create);
        using var binaryWriter = new BinaryWriter(fileStream);

        // Create header
        var header = new SpfFileHeader
        {
            Unknown1 = 0,
            Unknown2 = 1,
            //ColorFormat = 1 // Set color format to 16bpp
            ColorFormat = 0 // Set color format to 8bpp
        };

        // Convert header to bytes
        var headerBytes = SpfFileHeaderToBytes(header);

        // Write file header
        binaryWriter.Write(headerBytes);

        // Write the palette
        SpfPalette = SpfPalette.FromBitmap(loadedBitmap);

        // Print the SpfPalette
        Debug.WriteLine("SPF Palette:");
        for (int i = 0; i < SpfPalette._colors.Length; i++)
        {
            Debug.WriteLine($"Color {i}: {SpfPalette._colors[i]}");
        }

        binaryWriter.Write(SpfPalette.ToArray());

        // Write the frame count uint
        binaryWriter.Write((uint)1);

        // Write the frame header
        var frameHeaderBytes = SpfFrameHeaderToBytes(loadedBitmap);
        binaryWriter.Write(frameHeaderBytes);

        // Write the bytesTotal (bitmap width & bitmap height) * 2 for 16bpp
        var bytesTotal = (uint)(loadedBitmap.Width * loadedBitmap.Height)/* * 2*/;
        binaryWriter.Write(bytesTotal);

        // Write the frame data
        var frameDataBytes = BitmapToFrameData(loadedBitmap);
        binaryWriter.Write(frameDataBytes);
    }

    private static Bitmap ConvertTo8bppIndexed(Bitmap input)
    {
        int width = input.Width;
        int height = input.Height;

        // Create a temporary 32bppArgb bitmap
        using Bitmap tempBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        // Draw the input image on the temporary bitmap
        using (Graphics graphics = Graphics.FromImage(tempBitmap))
        {
            graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            graphics.DrawImage(input, 0, 0, width, height);
        }

        // Create the output 8bppIndexed bitmap
        Bitmap output = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

        // Get the palette from the input image and find the nearest color index for each pixel
        //SpfPalette spfPalette = SpfPalette.FromBitmap(input);
        ColorPalette colorPalette = output.Palette;
        Array.Copy(SpfPalette._colors, colorPalette.Entries, SpfPalette._colors.Length);
        output.Palette = colorPalette;

        BitmapData outputData = output.LockBits(new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
        BitmapData tempData = tempBitmap.LockBits(new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        unsafe
        {
            byte* outputPtr = (byte*)outputData.Scan0;
            byte* tempPtr = (byte*)tempData.Scan0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = Color.FromArgb(*(tempPtr + 3), *(tempPtr + 2), *(tempPtr + 1), *tempPtr);
                    int nearestIndex = SpfPalette.FindNearestColorIndex(pixelColor);
                    *(outputPtr + x) = (byte)nearestIndex;

                    tempPtr += 4;
                }

                outputPtr += outputData.Stride;
                tempPtr += tempData.Stride - width * 4;
            }
        }

        output.UnlockBits(outputData);
        tempBitmap.UnlockBits(tempData);

        return output;
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
        // Check if the input bitmap is already in 8bppIndexed format, if not, convert it
        if (bitmap.PixelFormat != PixelFormat.Format8bppIndexed)
        {
            bitmap = ConvertTo8bppIndexed(bitmap);
        }

        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

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