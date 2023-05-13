using ImageMagick;
using SpfConverter.Memory;
using SpfConverter.Utility;

namespace SpfConverter.Spf;

public sealed class SpfPalette
{
    public ICollection<IMagickColor<ushort>> Colors { get; init; }
    public int Padding { get; init; }
    private const ushort FIVE_BIT_MASK = 0b11111;
    private const ushort SIX_BIT_MASK = 0b111111;

    public SpfPalette(ICollection<IMagickColor<ushort>> colors)
    {
        if (colors.Count > 256)
            throw new ArgumentException("Palette can only contain 256 colors");
        
        Colors = colors;
        Padding = 256 - colors.Count;
    }
    
    public void Write(ref SpanWriter writer)
    {
        Write565(ref writer);
        Write1555(ref writer);
    }

    public static SpfPalette Read(ref SpanReader reader)
    {
        var rgb565 = Read565(ref reader);
        // ReSharper disable once UnusedVariable
        var rgb1555 = Read1555(ref reader);

        return new SpfPalette(rgb565);
    }

    private static ICollection<IMagickColor<ushort>> Read565(ref SpanReader reader)
    {
        var colors = new List<IMagickColor<ushort>>();

        for (var i = 0; i < 256; i++)
        {
            var color = reader.ReadUInt16();
            //@formatter:off
            var r = MathEx.ScaleRange<int, ushort>(color >> 11, 0, FIVE_BIT_MASK, 0, ushort.MaxValue);
            var g = MathEx.ScaleRange<int, ushort>((color >> 5) & SIX_BIT_MASK, 0, SIX_BIT_MASK, 0, ushort.MaxValue);
            var b = MathEx.ScaleRange<int, ushort>(color & FIVE_BIT_MASK, 0, FIVE_BIT_MASK, 0, ushort.MaxValue);
            //@formatter:on

            var magickColor = new MagickColor(r, g, b);
            colors.Add(magickColor);
        }

        return colors;
    }
    
    private static ICollection<IMagickColor<ushort>> Read1555(ref SpanReader reader)
    {
        var colors = new List<IMagickColor<ushort>>();
        

        for (var i = 0; i < 256; i++)
        {
            var color = reader.ReadUInt16();
            //@formatter:off
            //TODO: do i bother reading the alpha? not sure what use it would be
            var r = MathEx.ScaleRange<int, ushort>((color >> 10) & FIVE_BIT_MASK, 0, FIVE_BIT_MASK, 0, ushort.MaxValue);
            var g = MathEx.ScaleRange<int, ushort>((color >> 5) & FIVE_BIT_MASK, 0, FIVE_BIT_MASK, 0, ushort.MaxValue);
            var b = MathEx.ScaleRange<int, ushort>(color & FIVE_BIT_MASK, 0, FIVE_BIT_MASK, 0, ushort.MaxValue);
            //@formatter:on

            var magickColor = new MagickColor(r, g, b);
            colors.Add(magickColor);
        }

        return colors;
    }
    
    private void Write565(ref SpanWriter writer)
    {
        foreach (var color in Colors)
        {
            //@formatter:off
            var r = MathEx.ScaleRange(color.R, 0, ushort.MaxValue, 0, 0b11111);
            var g = MathEx.ScaleRange(color.G, 0, ushort.MaxValue, 0, 0b111111);
            var b = MathEx.ScaleRange(color.B, 0, ushort.MaxValue, 0, 0b11111);
            //@formatter:on
            var rgb565 = (ushort)((r << 11) | (g << 5) | b);
            writer.WriteUInt16(rgb565);
        }

        if (Padding > 0)
            writer.WriteBytes(new byte[Padding * 2]);
    }
    
    private void Write1555(ref SpanWriter writer)
    {
        foreach (var color in Colors)
        {
            //@formatter:off
            var r = MathEx.ScaleRange<ushort, byte>(color.R, 0, ushort.MaxValue, 0, 0b11111);
            var g = MathEx.ScaleRange<ushort, byte>(color.G, 0, ushort.MaxValue, 0, 0b11111);
            var b = MathEx.ScaleRange<ushort, byte>(color.B, 0, ushort.MaxValue, 0, 0b11111);
            //@formatter:on
            
            var rgb1555 = (ushort) ((r << 10) | (g << 5) | b);
            
            //if there is alpha, set alpha bit i guess?
            if(color.A < ushort.MaxValue)
                rgb1555 |= 0x8000;
            
            writer.WriteUInt16(rgb1555);
        }
        
        if (Padding > 0)
            writer.WriteBytes(new byte[Padding * 2]);
    }
}