using SpfConverter.Memory;

namespace SpfConverter.Spf;

public sealed class SpfHeader
{
    public uint Unknown1 { get; set; }
    public uint Unknown2 { get; set; }
    public uint ColorFormat { get; set; }
    
    public void Write(ref SpanWriter writer)
    {
        writer.WriteUInt32(Unknown1);
        writer.WriteUInt32(Unknown2);
        writer.WriteUInt32(ColorFormat);
    }

    public static SpfHeader Read(ref SpanReader reader)
    { 
        var unknown1 = reader.ReadUInt32();
        var unknown2 = reader.ReadUInt32();
        var colorFormat = reader.ReadUInt32();

        return new SpfHeader
        {
            Unknown1 = unknown1,
            Unknown2 = unknown2,
            ColorFormat = colorFormat
        };
    }
}