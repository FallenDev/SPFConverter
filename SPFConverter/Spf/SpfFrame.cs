using ImageMagick;
using SpfConverter.Memory;

namespace SpfConverter.Spf;

public sealed class SpfFrame
{
    public required SpfFrameHeader Header { get; set; }
    public required byte[] Data { get; set; }

    public static SpfFrame FromMagickImage(MagickImage image)
    {
        var header = new SpfFrameHeader
        {
            PixelWidth = (ushort)image.Width,
            PixelHeight = (ushort)image.Height,
            ByteWidth = (uint)image.Width, //should this not be x2?
            ByteCount = (uint)(image.Width * image.Height * 2),
        };
        
        var pixels = image.GetPixels().ToList();
        var frameData = new byte[pixels.Count];

        for (var i = 0; i < pixels.Count; i++)
        {
            var pixel = pixels[i];
            var index = pixel.GetChannel(3);

            if (index > 256)
                throw new IndexOutOfRangeException("Index is greater than 256");

            frameData[i] = (byte)index;
        }
        
        return new SpfFrame
        {
            Header = header,
            Data = frameData
        };
    }

    public void Write(ref SpanWriter writer)
    {
        Header.Write(ref writer);
        writer.WriteUInt32((uint)Data.Length);
        writer.WriteBytes(Data);
    }

    public static SpfFrame Read(ref SpanReader reader)
    {
        var header = SpfFrameHeader.Read(ref reader);
        var dataLength = reader.ReadUInt32();
        var data = reader.ReadBytes((int)dataLength);
        
        return new SpfFrame
        {
            Header = header,
            Data = data
        };
    }

    public MagickImage ToImage(SpfPalette palette)
    {
        var image = new MagickImage(MagickColors.Transparent, Header.PixelWidth, Header.PixelHeight);
        image.ColorSpace = ColorSpace.sRGB;

        var pixels = image.GetPixelsUnsafe();

        foreach (var set in pixels.Select((p, i) => new { Pixel = p, Index = i }))
        {
            var pixelIndex = set.Index;
            var paletteIndex = Data[pixelIndex];
            var color = palette.Colors.ElementAt(paletteIndex);
            set.Pixel.SetValues(new[] { color.R, color.G, color.B, ushort.MaxValue });
        }

        return image;
    }
}