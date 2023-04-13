﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SPFConverter
{
    internal class SpfConverter
    {
        public static void PngToSpf(string outputSpfFilePath, Bitmap loadedBitmap)
        {
            using var fileStream = new FileStream(outputSpfFilePath, FileMode.Create);
            using var binaryWriter = new BinaryWriter(fileStream);

            // Create header
            var header = new SpfFileHeader
            {
                Unknown1 = 0,
                Unknown2 = 1,
                ColorFormat = loadedBitmap.PixelFormat == PixelFormat.Format8bppIndexed ? (uint)0 : (uint)1
            };

            // Convert header to bytes
            var headerBytes = SpfFileHeaderToBytes(header);

            // Convert to palette
            var palette = SpfPaletteFromBitmap(loadedBitmap);
            var paletteBytes = SpfPaletteToBytes(palette);

            // Concatenate header with palette
            var combinedBytes = headerBytes.Concat(paletteBytes).ToArray();

            // Write file header and palette
            binaryWriter.Write(combinedBytes);

            // Write the frame count uint
            binaryWriter.Write((uint)1);

            // Write the frame header
            var frameHeaderBytes = SpfFrameHeaderToBytes(loadedBitmap);
            binaryWriter.Write(frameHeaderBytes);

            // Write the bytesTotal (bitmap width & bitmap height) *2 if 16bpp
            uint bytesTotal;
            if (loadedBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                bytesTotal = (uint)(loadedBitmap.Width * loadedBitmap.Height);
            else
                bytesTotal = (uint)(loadedBitmap.Width * loadedBitmap.Height) * 2;

            binaryWriter.Write(bytesTotal);

            // Write the frame data
            var frameDataBytes = BitmapToFrameData(loadedBitmap, palette);
            binaryWriter.Write(frameDataBytes);
        }

        private static SpfPalette SpfPaletteFromBitmap(Bitmap bitmap)
        {
            // If the image is not 8bppIndexed, convert it first
            var bitmapIndexed = bitmap.PixelFormat != PixelFormat.Format8bppIndexed ? ConvertTo8bppIndexed(bitmap) : bitmap;

            // Create an empty SpfPalette struct
            var spfPalette = new SpfPalette
            {
                _alpha = new byte[512],
                _rgb = new byte[512],
                _colors = new Color[256]
            };

            // Extract colors from the Bitmap
            var colorPalette = bitmapIndexed.Palette;

            var colorCount = colorPalette.Entries.Length;

            // Fill the _colors array with the extracted colors
            for (var i = 0; i < colorCount; i++)
                spfPalette._colors[i] = colorPalette.Entries[i];

            // Convert colors to RGB 555 format and store them in the _rgb and _alpha array
            for (var i = 0; i < colorCount; i++)
            {
                var color = spfPalette._colors[i];
                var red = color.R / 8;
                var green = color.G / 8;
                var blue = color.B / 8;
                var rgb555 = (ushort)((red << 10) | (green << 5) | blue);
                var rgbBytes = BitConverter.GetBytes(rgb555);

                spfPalette._rgb[i * 2] = rgbBytes[0];
                spfPalette._rgb[i * 2 + 1] = rgbBytes[1];

                // Check if the color is #000000 and set the alpha value to 0
                if (red == 0 && green == 0 && blue == 0)
                {
                    spfPalette._alpha[i * 2] = 0;
                    spfPalette._alpha[i * 2 + 1] = 0;
                }
                else
                {
                    spfPalette._alpha[i * 2] = 255;
                    spfPalette._alpha[i * 2 + 1] = 0;
                }
            }

            return spfPalette;
        }

        private static Bitmap ConvertTo8bppIndexed(Bitmap input)
        {
            // First, create a temporary 32-bit image
            var bitmap32bpp = new Bitmap(input.Width, input.Height, PixelFormat.Format32bppArgb);

            // Draw the input image on the 32-bit image
            using (var g = Graphics.FromImage(bitmap32bpp))
            {
                g.DrawImage(input, new Rectangle(0, 0, input.Width, input.Height), 0, 0, input.Width, input.Height, GraphicsUnit.Pixel);
            }

            var bitmap8bpp = new Bitmap(input.Width, input.Height, PixelFormat.Format8bppIndexed);
            var palette = bitmap8bpp.Palette;

            for (int i = 0; i < palette.Entries.Length; i++)
            {
                int alpha = (i * 0xFF) / (palette.Entries.Length - 1);
                palette.Entries[i] = Color.FromArgb(alpha, Color.Black);
            }

            bitmap8bpp.Palette = palette;

            var rect = new Rectangle(0, 0, bitmap8bpp.Width, bitmap8bpp.Height);
            var bitmapData = bitmap8bpp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            using (var g = Graphics.FromImage(bitmap32bpp))
            {
                var imageAttributes = new ImageAttributes();

                ColorMap[] colorMap = new ColorMap[palette.Entries.Length];
                for (int i = 0; i < palette.Entries.Length; i++)
                {
                    colorMap[i] = new ColorMap
                    {
                        OldColor = Color.FromArgb(i, Color.Black),
                        NewColor = palette.Entries[i]
                    };
                }

                imageAttributes.SetRemapTable(colorMap);
                g.DrawImage(bitmap32bpp, rect, 0, 0, input.Width, input.Height, GraphicsUnit.Pixel, imageAttributes);
            }

            IntPtr ptr = bitmapData.Scan0;
            int size = bitmapData.Stride * bitmap8bpp.Height;
            byte[] imageData = new byte[size];
            System.Runtime.InteropServices.Marshal.Copy(ptr, imageData, 0, size);
            System.Runtime.InteropServices.Marshal.Copy(imageData, 0, ptr, size);
            bitmap8bpp.UnlockBits(bitmapData);

            return bitmap8bpp;
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

        private static byte[] SpfPaletteToBytes(SpfPalette palette)
        {
            // Convert the palette struct to bytes
            var combinedArray = new byte[palette._alpha.Length + palette._rgb.Length];
            Array.Copy(palette._alpha, 0, combinedArray, 0, palette._alpha.Length);
            Array.Copy(palette._rgb, 0, combinedArray, palette._alpha.Length, palette._rgb.Length);
            return combinedArray;
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

        private static byte[] BitmapToFrameData(Bitmap bitmap, SpfPalette spfPalette)
        {
            // Convert the bitmap data to the SPF frame data format
            SpfFrameHeader frameHeader;
            frameHeader.PadWidth = 0;
            frameHeader.PadHeight = 0;
            frameHeader.PixelWidth = (ushort)bitmap.Width;
            frameHeader.PixelHeight = (ushort)bitmap.Height;
            frameHeader.Unknown = 0;
            frameHeader.Reserved = 0;
            frameHeader.StartAddress = 0;
            frameHeader.ByteWidth = (uint)bitmap.Width;
            frameHeader.SemiByteCount = 0;

            // Write the bytesTotal (bitmap width & bitmap height) *2 if 16bpp
            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                frameHeader.ByteCount = (uint)(bitmap.Width * bitmap.Height);
            else
                frameHeader.ByteCount = (uint)(bitmap.Width * bitmap.Height) * 2;

            var spfFrame = new SpfFrame(frameHeader, 0, spfPalette);
            spfFrame.FrameBitmap = (Bitmap)bitmap.Clone();
            var frameData = spfFrame.GetRawBits();

            return frameData;
        }
    }
}
