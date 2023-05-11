using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SPFverter.Converters;

public static class TiffConverter
{
    public static void SaveBitmapsAsTiff(Bitmap bitmap1, Bitmap bitmap2, string outputPath1, string outputPath2)
    {
        // Create an ImageCodecInfo for the TIFF format
        ImageCodecInfo codecInfo = GetEncoderInfo("image/tiff");

        // Create an EncoderParameters object to store the compression type
        EncoderParameters encoderParameters = new EncoderParameters(1);
        encoderParameters.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionNone);

        // Save the first bitmap as a TIFF file
        bitmap1.Save(outputPath1, codecInfo, encoderParameters);

        // Save the second bitmap as a TIFF file
        bitmap2.Save(outputPath2, codecInfo, encoderParameters);
    }

    public static void CombineBitmapsToTiff(Bitmap bitmap1, Bitmap bitmap2, string outputPath)
    {
        // Create an ImageCodecInfo for the TIFF format
        ImageCodecInfo codecInfo = GetEncoderInfo("image/tiff");

        // Create an EncoderParameters object to store the compression type and save flags
        EncoderParameters encoderParameters = new EncoderParameters(2);
        encoderParameters.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionNone);
        encoderParameters.Param[1] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame);

        // Save the first frame
        bitmap1.Save(outputPath, codecInfo, encoderParameters);

        // Update the save flag to AppendFrame
        encoderParameters.Param[1] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);

        // Save the second frame
        bitmap1.SaveAdd(bitmap2, encoderParameters);

        // Update the save flag to Flush
        encoderParameters.Param[1] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);

        // Flush the changes
        bitmap1.SaveAdd(encoderParameters);
    }

    private static ImageCodecInfo GetEncoderInfo(string mimeType)
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

        foreach (ImageCodecInfo codec in codecs)
        {
            if (codec.MimeType == mimeType)
            {
                return codec;
            }
        }

        return null;
    }
}
