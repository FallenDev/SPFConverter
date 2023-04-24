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

        var btnSpfToTiff = new Button { Text = "SPF to TIFF", AutoSize = true, Location = new Point(30, 15), BackColor = Color.DodgerBlue, ForeColor = Color.White};
        btnSpfToTiff.Click += BtnSpfToTiff_Click;
        Controls.Add(btnSpfToTiff);

        var btnTiffToSpf = new Button { Text = "TIFF to SPF", AutoSize = true, Location = new Point(30, 60), BackColor = Color.DodgerBlue, ForeColor = Color.White};
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

                // ToDo: Create loop here to save multiple frames
                Bitmap frameBitmap = spfFile.Frames[0].FrameBitmap;

                // Create a 32bpp ARGB Bitmap
                Bitmap frameBitmap32bpp = new Bitmap(frameBitmap.Width, frameBitmap.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(frameBitmap32bpp))
                {
                    g.DrawImage(frameBitmap, new Rectangle(0, 0, frameBitmap.Width, frameBitmap.Height));
                }

                // Set the alpha values for each pixel using spfFile.Palette._alpha
                for (int y = 0; y < frameBitmap32bpp.Height; y++)
                {
                    for (int x = 0; x < frameBitmap32bpp.Width; x++)
                    {
                        Color pixelColor = frameBitmap32bpp.GetPixel(x, y);
                        int alphaIndex = pixelColor.A / 2;
                        int alpha = spfFile._mPalette._alpha[alphaIndex];
                        Color colorWithAlpha = Color.FromArgb(alpha, pixelColor.R, pixelColor.G, pixelColor.B);
                        frameBitmap32bpp.SetPixel(x, y, colorWithAlpha);
                    }
                }

                // Set the EncoderParameters to save the TIFF with alpha transparency
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW);

                ImageCodecInfo tiffCodecInfo = GetEncoderInfo("image/tiff");
                frameBitmap32bpp.Save(saveFileDialog.FileName, tiffCodecInfo, encoderParameters);
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
