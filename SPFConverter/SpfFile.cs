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
        private SpfFileHeader _mHeader;
        private SpfPalette _mPalette;
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
            for (var index = 0; (long)index < (long)FrameCount; ++index)
            {
                var h = SpfFrameHeader.FromBinaryReaderBlock(reader);
                Frames[index] = new SpfFrame(h, ColorFormat, _mPalette);
            }
        }

        private void FrameDataFromReader(BinaryReader reader)
        {
            for (var index = 0; index < FrameCount; ++index)
            {
                var byteCount = (int)Frames[index].ByteCount;
                var numArray = new byte[byteCount];
                var rawBits = reader.ReadBytes(byteCount);
                Frames[index].Render(rawBits);
            }
        }

        public static SpfFile FromFile(string fileName)
        {
            if (!File.Exists(fileName))
                return (SpfFile)null;

            var binaryReader = new BinaryReader((Stream)File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));

            if (binaryReader == null)
                return (SpfFile)null;

            var spfFile = new SpfFile
            {
                _mFileName = fileName,
                _mHeader = SpfFileHeader.FromBinaryReaderBlock(binaryReader)
            };

            if (spfFile.ColorFormat == 0U)
                spfFile._mPalette = SpfPalette.FromBinaryReaderBlock(binaryReader);

            spfFile._mFramecount = binaryReader.ReadUInt32();
            spfFile._mFrames = new SpfFrame[(int)(IntPtr)spfFile.FrameCount];
            spfFile.FrameHeadersFromReader(binaryReader);
            spfFile._mBytetotal = binaryReader.ReadUInt32();
            spfFile.FrameDataFromReader(binaryReader);
            binaryReader.Close();

            return spfFile;
        }

        public uint Width => _mHeader.Width;

        public uint Height => _mHeader.Height;

        public uint ColorFormat => _mHeader.ColorFormat;

        public uint FrameCount => _mFramecount;

        public uint ByteTotal => _mBytetotal;
    }

    public struct SpfFileHeader
    {
        public uint Width;
        public uint Height;
        public uint ColorFormat;

        public static SpfFileHeader FromBinaryReaderBlock(BinaryReader br)
        {
            var gcHandle = GCHandle.Alloc((object)br.ReadBytes(Marshal.SizeOf(typeof(SpfFileHeader))), GCHandleType.Pinned);
            var structure = (SpfFileHeader)(Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(SpfFileHeader)) ?? throw new InvalidOperationException());
            gcHandle.Free();
            return structure;
        }
    }

    public struct SpfPalette
    {
        private byte[] _alpha;
        private byte[] _rgb;
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
                var blue = 8 * ((int)uint16 % 32);
                var green = 8 * ((int)uint16 / 32 % 32);
                var red = 8 * ((int)uint16 / 32 / 32 % 32);
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
            var gcHandle = GCHandle.Alloc((object)br.ReadBytes(Marshal.SizeOf(typeof(SpfFrameHeader))), GCHandleType.Pinned);
            var structure = (SpfFrameHeader)(Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(SpfFrameHeader)) ?? throw new InvalidOperationException());
            gcHandle.Free();
            return structure;
        }
    }

    public sealed class SpfFrame
    {
        private SpfFrameHeader _mHeader;
        private Bitmap _mBitmap;

        public SpfFrame(SpfFrameHeader h, uint format, SpfPalette p)
        {
            _mHeader = h;
            if (ByteCount == 0U)
                return;
            if (format == 0U)
            {
                _mBitmap = new Bitmap(PixelWidth, PixelHeight, PixelFormat.Format8bppIndexed);
                var palette = FrameBitmap.Palette;
                Array.Copy((Array)p._colors, (Array)palette.Entries, 256);
                FrameBitmap.Palette = palette;
            }
            else
                _mBitmap = new Bitmap(PixelWidth, PixelHeight, PixelFormat.Format16bppRgb555);
        }

        public Bitmap FrameBitmap => _mBitmap;

        public void Render(byte[] rawBits)
        {
            if (ByteCount == 0U)
                return;
            if (FrameBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                Render8Bppi(rawBits);
            else
                Render32Bpp(rawBits);
        }

        private void Render8Bppi(byte[] rawBits)
        {
            var num1 = 1;
            if (FrameBitmap.PixelFormat == PixelFormat.Format16bppRgb555)
                num1 = 2;
            var bitmapdata = FrameBitmap.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.ReadWrite, FrameBitmap.PixelFormat);
            var scan0 = bitmapdata.Scan0;
            var num2 = 4 - PixelWidth * num1 % 4;
            if (num2 < 4 || PadWidth > 0 || PadHeight > 0)
            {
                if (num2 == 4)
                    num2 = 0;
                var num3 = PixelWidth * num1 + num2;
                var numArray = new byte[PixelHeight * num3];
                for (var index = 0; index < PixelHeight - PadHeight; ++index)
                {
                    Array.Copy((Array)rawBits, index * num1 * (PixelWidth - PadWidth), (Array)numArray, index * num3, num1 * (PixelWidth - PadWidth));
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
            Array.Copy((Array)rawBits, length, (Array)numArray, 0, length);
            Render8Bppi(numArray);
        }

        public int PadWidth => (int)_mHeader.PadWidth;

        public int PadHeight => (int)_mHeader.PadHeight;

        public int PixelWidth => (int)_mHeader.PixelWidth;

        public int PixelHeight => (int)_mHeader.PixelHeight;

        public uint Unknown => _mHeader.Unknown;

        public uint Reserved => _mHeader.Reserved;

        public uint StartAddress => _mHeader.StartAddress;

        public uint ByteWidth => _mHeader.ByteWidth;

        public uint ByteCount => _mHeader.ByteCount;

        public uint SemiByteCount => _mHeader.SemiByteCount;
    }
}
