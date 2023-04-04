using System.Drawing.Imaging;

namespace SPFConverter
{
    public partial class Form1 : Form
    {
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;

        public Form1()
        {
            InitializeComponent();

            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();

            var btnSpfToPng = new Button { Text = "SPF to PNG", AutoSize = true, Location = new Point(10, 10) };
            btnSpfToPng.Click += BtnSpfToPng_Click;
            Controls.Add(btnSpfToPng);

            var btnPngToSpf = new Button { Text = "PNG to SPF", AutoSize = true, Location = new Point(10, 50) };
            btnPngToSpf.Click += BtnPngToSpf_Click;
            Controls.Add(btnPngToSpf);
        }

        private void BtnSpfToPng_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "SPF Files|*.spf";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    saveFileDialog.Filter = "PNG Files|*.png";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var spfFile = SpfFile.FromFile(openFileDialog.FileName);
                        spfFile.Frames[0].FrameBitmap.Save(saveFileDialog.FileName, ImageFormat.Png);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error converting SPF to PNG: {ex.Message}");
                }
            }
        }

        private void BtnPngToSpf_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "PNG Files|*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    saveFileDialog.Filter = "SPF Files|*.spf";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        SpfConverter.PngToSpf(openFileDialog.FileName, saveFileDialog.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error converting PNG to SPF: {ex.Message}");
                }
            }
        }
    }
}