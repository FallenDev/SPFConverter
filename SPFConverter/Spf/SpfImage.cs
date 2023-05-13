using System.Text;
using ImageMagick;
using SpfConverter.Memory;

namespace SpfConverter.Spf;

public sealed class SpfImage
{
    public SpfHeader Header { get; init; }
    public SpfPalette Palette { get; init; }
    public ICollection<SpfFrame> Frames { get; init; }

    public SpfImage(SpfHeader header, SpfPalette palette, ICollection<SpfFrame> frames)
    {
        Header = header;
        Palette = palette;
        Frames = frames;
    }
    
    public static SpfImage FromMagickImageCollection(MagickImageCollection collection)
    {
        var frames = new List<SpfFrame>();
        var header = new SpfHeader
        {
            Unknown2 = 1
        };
        
        //reduce colors to 256, no dithering
        collection.Quantize(
            new QuantizeSettings
            {
                Colors = 256,
                ColorSpace = ColorSpace.sRGB,
                DitherMethod = DitherMethod.No,
            });
        
        //create a mosaic of all images in the collection
        using var mosaic = collection.Mosaic();
        
        //get colors of the mosaic
        var palette = mosaic.Histogram().Keys.ToList();

        //map each image to the palette
        foreach (var image in collection)
        {
            //map the colors of each image to the palette
            image.Map(palette);
            frames.Add(SpfFrame.FromMagickImage((MagickImage)image));
        }

        var spfPalette = new SpfPalette(palette);

        return new SpfImage(header, spfPalette, frames);
    }
    
    public void WriteSpf(string outputPath)
    {
        var writer = new SpanWriter(Encoding.Default, 1024, endianness: Endianness.LittleEndian);
        Header.Write(ref writer);
        Palette.Write(ref writer);
        writer.WriteUInt32((uint)Frames.Count);
        
        foreach (var frame in Frames)
            frame.Write(ref writer);
        
        if(!outputPath.EndsWith(".spf", StringComparison.OrdinalIgnoreCase))
            outputPath += ".spf";

        File.WriteAllBytes(outputPath, writer.ToSpan().ToArray());
    }

    public void WriteImg(string outputPath)
    {
        switch (Frames.Count)
        {
            case 1:
            {
                if (Directory.Exists(outputPath))
                    outputPath = Path.Combine(outputPath, "frame1.png");
                else if (!Path.HasExtension(outputPath))
                    throw new InvalidOperationException("Output path is not a directory and does not have an extension");

                using var collection = ConvertToMagickImageCollection();
                collection[0].Write(outputPath);

                break;
            }
            default:
            {
                if (!Directory.Exists(outputPath))
                    throw new InvalidOperationException("Output path must be a directory when writing multiple frames");

                using var collection = ConvertToMagickImageCollection();
                var index = 1;

                foreach (var image in collection)
                    image.Write(Path.Combine(outputPath, $"frame{index++}.png"));
                
                break;
            }
        }
    }

    private MagickImageCollection ConvertToMagickImageCollection()
    {
        var collection = new MagickImageCollection();

        foreach (var frame in Frames)
        {
            var image = frame.ToImage(Palette);
            
            collection.Add(image);
        }

        return collection;
    }

    public static SpfImage Read(string inputPath)
    {
        if (!inputPath.EndsWith(".spf", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("File is not an spf file (make sure it has the .spf extension)");

        var buffer = File.ReadAllBytes(inputPath).AsSpan();
        var reader = new SpanReader(Encoding.Default, buffer, endianness: Endianness.LittleEndian);
        
        var header = SpfHeader.Read(ref reader);
        var palette = SpfPalette.Read(ref reader);
        var frameCount = reader.ReadUInt32();
        var frames = new List<SpfFrame>();
        
        for (var i = 0; i < frameCount; i++)
            frames.Add(SpfFrame.Read(ref reader));
        
        return new SpfImage(header, palette, frames);
    }
}