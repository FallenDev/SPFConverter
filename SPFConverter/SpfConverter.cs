using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SPFConverter
{
    internal class SpfConverter
    {
        public static void PngToSpf(string inputPngFilePath, string outputSpfFilePath)
        {
            using (var bitmap = new Bitmap(inputPngFilePath))
            {
                if (bitmap.PixelFormat != PixelFormat.Format8bppIndexed)
                {
                    throw new ArgumentException("Input PNG file must have an 8-bit indexed color palette.");
                }

                var header = new SpfFileHeader
                {
                    Width = (ushort)bitmap.Width,
                    Height = (ushort)bitmap.Height
                };

                var palette = new SpfPalette
                {
                    _colors = bitmap.Palette.Entries
                };

                using (var binaryWriter = new BinaryWriter(File.Open(outputSpfFilePath, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    WriteSpfHeader(binaryWriter, header);
                    WriteSpfPalette(binaryWriter, palette);
                    WriteSpfData(binaryWriter, bitmap);
                }
            }
        }

        private static void WriteSpfHeader(BinaryWriter bw, SpfFileHeader header)
        {
            bw.Write(header.Width);
            bw.Write(header.Height);
            bw.Write(header.ColorFormat);
        }

        private static void WriteSpfPalette(BinaryWriter bw, SpfPalette palette)
        {
            for (int i = 0; i < palette._colors.Length; ++i)
            {
                ushort rgb = (ushort)(((palette._colors[i].R / 8) << 10) | ((palette._colors[i].G / 8) << 5) | (palette._colors[i].B / 8));
                bw.Write(rgb);
            }
        }

        private static void WriteSpfData(BinaryWriter bw, Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            byte[] rawData = new byte[bitmap.Width * bitmap.Height];
            Marshal.Copy(bitmapData.Scan0, rawData, 0, rawData.Length);
            bitmap.UnlockBits(bitmapData);

            bw.Write(rawData);
        }

        public static Bitmap SpfToPng(string inputSpfFilePath)
        {
            if (!File.Exists(inputSpfFilePath))
            {
                throw new FileNotFoundException("Input SPF file not found.");
            }

            using (var binaryReader = new BinaryReader(File.Open(inputSpfFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                // Read SPF_File_Header
                var header = SpfFileHeader.FromBinaryReaderBlock(binaryReader);

                // Read and create SPF_Palette
                var palette = SpfPalette.FromBinaryReaderBlock(binaryReader);

                // Create a bitmap with the appropriate dimensions and pixel format
                var bitmap = new Bitmap((int)header.Width, (int)header.Height, PixelFormat.Format8bppIndexed);

                // Set the bitmap's color palette
                var colorPalette = bitmap.Palette;
                for (int i = 0; i < palette._colors.Length; i++)
                {
                    colorPalette.Entries[i] = palette._colors[i];
                }
                bitmap.Palette = colorPalette;

                // Read and set bitmap data
                var rawData = binaryReader.ReadBytes((int)header.Width * (int)header.Height);
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, (int)header.Width, (int)header.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                Marshal.Copy(rawData, 0, bitmapData.Scan0, rawData.Length);
                bitmap.UnlockBits(bitmapData);

                return bitmap;
            }
        }
    }
}
