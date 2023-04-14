﻿namespace SPFverter.Structs;

public struct SpfPaletteStruct
{
    public byte[] _alpha;
    public byte[] _rgb;
    public Color[] _colors;

    /// <summary>
    /// Reads 8bppIndexed Palette, and converts #00000 black to alpha channel
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
            var alpha = (red == 0 && green == 0 && blue == 0) ? 0 : 255; // Alpha is #000000, so make this transparent
            spfPalette._colors[index] = Color.FromArgb(alpha, red, green, blue);
        }

        return spfPalette;
    }
}