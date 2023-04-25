using SPFverter.Converters;
using System.Drawing.Imaging;

namespace SPFverter;

public partial class Form1 : Form
{
    private OpenFileDialog openFileDialog;
    private SaveFileDialog saveFileDialog;
    private Bitmap _loadedBitmap;

    public Form1()
    {
        InitializeComponent();

        openFileDialog = new OpenFileDialog();
        saveFileDialog = new SaveFileDialog();

        var btnSpfToTiff = new Button { Text = "SPF to TIFF", AutoSize = true, Location = new System.Drawing.Point(30, 15), BackColor = System.Drawing.Color.DodgerBlue, ForeColor = System.Drawing.Color.White};
        btnSpfToTiff.Click += BtnSpfToTiff_Click;
        Controls.Add(btnSpfToTiff);

        var btnTiffToSpf = new Button { Text = "TIFF to SPF", AutoSize = true, Location = new System.Drawing.Point(30, 60), BackColor = System.Drawing.Color.DodgerBlue, ForeColor = System.Drawing.Color.White};
        btnTiffToSpf.Click += BtnTiffToSpf_Click;
        Controls.Add(btnTiffToSpf);
    }

private void BtnSpfToTiff_Click(object sender, EventArgs e)
{
    openFileDialog.Filter = "SPF Files|*.spf";
    if (openFileDialog.ShowDialog() == DialogResult.OK)
    {
        try
        {
            saveFileDialog.Filter = "TIFF Files|*.tiff";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var spfFile = SpfReadAndConversion.FromFile(openFileDialog.FileName);
                Bitmap frameBitmap = spfFile.Frames[0].FrameBitmap;

                // Create a new 32bpp Bitmap to store the ARGB values
                Bitmap argbBitmap = new Bitmap(frameBitmap.Width, frameBitmap.Height, PixelFormat.Format32bppArgb);

                // Copy the indexed image to the new 32bpp Bitmap while preserving the alpha channel
                for (int y = 0; y < frameBitmap.Height; y++)
                {
                    for (int x = 0; x < frameBitmap.Width; x++)
                    {
                        System.Drawing.Color indexedColor = frameBitmap.GetPixel(x, y);
                        int alpha = spfFile._mPalette._colors[indexedColor.A].A;
                        System.Drawing.Color argbColor = System.Drawing.Color.FromArgb(alpha, indexedColor);
                        argbBitmap.SetPixel(x, y, argbColor);
                    }
                }

                // Set the EncoderParameters to save the TIFF with alpha transparency
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionNone);

                ImageCodecInfo tiffCodecInfo = GetEncoderInfo("image/tiff");
                argbBitmap.Save(saveFileDialog.FileName, tiffCodecInfo, encoderParameters);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error converting SPF to TIFF: {ex.Message}");
        }
    }
}

    private void BtnTiffToSpf_Click(object sender, EventArgs e)
    {
        openFileDialog.Filter = "TIFF Files|*.tiff";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            //try
            //{
                saveFileDialog.Filter = "SPF Files|*.spf";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    TiffToSpfConv.TiffToSpf(saveFileDialog.FileName, openFileDialog.FileName);
                }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Error converting TIFF to SPF: {ex.Message} {ex.StackTrace}");
            //}
        }
    }

    // Get the ImageCodecInfo for the specified mimeType
    private static ImageCodecInfo GetEncoderInfo(string mimeType)
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
        foreach (ImageCodecInfo codec in codecs)
        {
            if (codec.MimeType == mimeType)
                return codec;
        }
        return null;
    }
}
