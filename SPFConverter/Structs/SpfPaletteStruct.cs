namespace SPFverter.Structs;

public struct SpfPaletteStruct
{
    public byte[] _alpha;
    public byte[] _rgb;
    public Color[] _colors;

    /// <summary>
    /// Reads SPF Image and converts to Palette
    /// </summary>
    public static SpfPaletteStruct FromBinaryReaderBlock(BinaryReader br)
    {
        SpfPaletteStruct spfPalette;
        spfPalette._alpha = br.ReadBytes(512);
        spfPalette._rgb = br.ReadBytes(512);
        spfPalette._colors = new Color[256];

        for (var index = 0; index < 256; ++index)
        {
            var uint16 = BitConverter.ToUInt16(spfPalette._rgb, 2 * index);
            var blue = 8 * (uint16 % 32);
            var green = 8 * (uint16 / 32 % 32);
            var red = 8 * (uint16 / 32 / 32 % 32);
            
            // Use the alpha value from the _alpha array
            var alpha = spfPalette._alpha[2 * index];
            
            spfPalette._colors[index] = Color.FromArgb(alpha, red, green, blue);
        }

        return spfPalette;
    }
}