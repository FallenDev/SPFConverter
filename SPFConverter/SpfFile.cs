using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SPFConverter
{
    internal class SpfFile
    {
        public SpfFileHeader _mHeader;
        public SpfPalette _mPalette;
        private uint _mFramecount;
        private uint _mBytetotal;
        private SpfFrame[] _mFrames;
        private string _mFileName;

        public SpfFrame this[int index]
        {
            get => Frames[index];
            set => Frames[index] = value;
        }

        public SpfFrame[] Frames => _mFrames;

        public string FileName => _mFileName;

        private void FrameHeadersFromReader(BinaryReader reader)
        {
            for (long index = 0; index < FrameCount; ++index)
            {
                var h = SpfFrameHeader.FromBinaryReaderBlock(reader);
                Frames[index] = new SpfFrame(h, ColorFormat, _mPalette);
            }
        }

        private void FrameDataFromReader(BinaryReader reader)
        {
            for (long index = 0; index < FrameCount; ++index)
            {
                var byteCount = (int)Frames[index].ByteCount;
                var numArray = new byte[byteCount];
                var rawBits = reader.ReadBytes(byteCount);
                Frames[index].Render(rawBits);
            }
        }

        public static SpfFile FromFile(string fileName)
        {
            if (!File.Exists(fileName)) return null;

            var binaryReader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));
            var spfFile = new SpfFile
            {
                _mFileName = fileName,
                _mHeader = SpfFileHeader.FromBinaryReaderBlock(binaryReader)
            };

            if (spfFile.ColorFormat == 0U)
                spfFile._mPalette = SpfPalette.FromBinaryReaderBlock(binaryReader);

            spfFile._mFramecount = binaryReader.ReadUInt32();
            spfFile._mFrames = new SpfFrame[(int)(nint)spfFile.FrameCount];

            spfFile.FrameHeadersFromReader(binaryReader);
            spfFile._mBytetotal = binaryReader.ReadUInt32();
            spfFile.FrameDataFromReader(binaryReader);
            binaryReader.Close();

            return spfFile;
        }
        
        public uint ColorFormat => _mHeader.ColorFormat;

        public uint FrameCount => _mFramecount;

        public uint ByteTotal => _mBytetotal;
    }

    public struct SpfFileHeader
    {
        public uint Unknown1;
        public uint Unknown2;
        public uint ColorFormat;

        public static SpfFileHeader FromBinaryReaderBlock(BinaryReader br)
        {
            var gcHandle = GCHandle.Alloc(br.ReadBytes(Marshal.SizeOf(typeof(SpfFileHeader))), GCHandleType.Pinned);
            var structure = (SpfFileHeader)(Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(SpfFileHeader)) ?? throw new InvalidOperationException());
            gcHandle.Free();
            return structure;
        }
    }

    public struct SpfPalette
    {
        public byte[] _alpha;
        public byte[] _rgb;
        public Color[] _colors;

        public static SpfPalette FromBinaryReaderBlock(BinaryReader br)
        {
            SpfPalette spfPalette;
            spfPalette._alpha = br.ReadBytes(512);
            spfPalette._rgb = br.ReadBytes(512);
            spfPalette._colors = new Color[256];

            for (var index = 0; index < 256; ++index)
            {
                var uint16 = BitConverter.ToUInt16(spfPalette._rgb, 2 * index);
                var blue = 8 * (uint16 % 32);
                var green = 8 * (uint16 / 32 % 32);
                var red = 8 * (uint16 / 32 / 32 % 32);
                spfPalette._colors[index] = Color.FromArgb(red, green, blue);
            }

            return spfPalette;
        }
    }

    public struct SpfFrameHeader
    {
        public ushort PadWidth;
        public ushort PadHeight;
        public ushort PixelWidth;
        public ushort PixelHeight;
        public uint Unknown;
        public uint Reserved;
        public uint StartAddress;
        public uint ByteWidth;
        public uint ByteCount;
        public uint SemiByteCount;

        public static SpfFrameHeader FromBinaryReaderBlock(BinaryReader br)
        {
            var gcHandle = GCHandle.Alloc(br.ReadBytes(Marshal.SizeOf(typeof(SpfFrameHeader))), GCHandleType.Pinned);
            var structure = (SpfFrameHeader)(Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(SpfFrameHeader)) ?? throw new InvalidOperationException());
            gcHandle.Free();
            return structure;
        }
    }

    public sealed class SpfFrame
    {
        private SpfFrameHeader _mHeader;
        public Bitmap FrameBitmap;

        public SpfFrame(SpfFrameHeader h, uint format, SpfPalette p)
        {
            _mHeader = h;
            if (ByteCount == 0U) return;
            if (format == 0U)
            {
                FrameBitmap = new Bitmap(PixelWidth, PixelHeight, PixelFormat.Format8bppIndexed);
                var palette = FrameBitmap.Palette;
                Array.Copy(p._colors, palette.Entries, 256);
                FrameBitmap.Palette = palette;
            }
            else
                FrameBitmap = new Bitmap(PixelWidth, PixelHeight, PixelFormat.Format16bppRgb555);
        }

        public void Render(byte[] rawBits)
        {
            if (ByteCount == 0U) return;
            if (FrameBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                Render8Bppi(rawBits);
            else
                Render32Bpp(rawBits);
        }

        private void Render8Bppi(byte[] rawBits)
        {
            var num1 = 1;
            if (FrameBitmap.PixelFormat == PixelFormat.Format16bppRgb555) num1 = 2;

            var bitmapdata = FrameBitmap.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.ReadWrite, FrameBitmap.PixelFormat);
            var scan0 = bitmapdata.Scan0;
            var num2 = 4 - PixelWidth * num1 % 4;

            if (num2 < 4 || PadWidth > 0 || PadHeight > 0)
            {
                if (num2 == 4) num2 = 0;
                var num3 = PixelWidth * num1 + num2;
                var numArray = new byte[PixelHeight * num3];

                for (var index = 0; index < PixelHeight - PadHeight; ++index)
                {
                    Array.Copy(rawBits, index * num1 * (PixelWidth - PadWidth), numArray, index * num3, num1 * (PixelWidth - PadWidth));
                    Marshal.Copy(numArray, 0, scan0, PixelHeight * num3);
                }
            }
            else
                Marshal.Copy(rawBits, 0, scan0, PixelHeight * PixelWidth * num1);

            FrameBitmap.UnlockBits(bitmapdata);
        }

        private void Render32Bpp(byte[] rawBits)
        {
            var length = rawBits.Length / 2;
            var numArray = new byte[length];
            Array.Copy(rawBits, length, numArray, 0, length);
            Render8Bppi(numArray);
        }

        public byte[] GetRawBits()
        {
            var bitmapdata = FrameBitmap.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.ReadOnly, FrameBitmap.PixelFormat);
            var rawDataLength = bitmapdata.Stride * bitmapdata.Height;
            var rawData = new byte[rawDataLength];
            Marshal.Copy(bitmapdata.Scan0, rawData, 0, rawDataLength);
            FrameBitmap.UnlockBits(bitmapdata);
            return rawData;
        }

        public int PadWidth => _mHeader.PadWidth;

        public int PadHeight => _mHeader.PadHeight;

        public int PixelWidth => _mHeader.PixelWidth;

        public int PixelHeight => _mHeader.PixelHeight;

        public uint Unknown => _mHeader.Unknown;

        public uint Reserved => _mHeader.Reserved;

        public uint StartAddress => _mHeader.StartAddress;

        public uint ByteWidth => _mHeader.ByteWidth;

        public uint ByteCount => _mHeader.ByteCount;

        public uint SemiByteCount => _mHeader.SemiByteCount;
    }
}
