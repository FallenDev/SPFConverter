using SpfConverter.Spf;

namespace SPFverter;

public partial class Form1 : Form
{
    private OpenFileDialog openFileDialog;
    private SaveFileDialog saveFileDialog;

    public Form1()
    {
        InitializeComponent();

        openFileDialog = new OpenFileDialog();
        saveFileDialog = new SaveFileDialog();

        var btnSpfToPng = new Button { Text = "SPF to Modern", AutoSize = true, Location = new Point(30, 15), BackColor = Color.Red, ForeColor = Color.White };
        btnSpfToPng.Click += BtnSpfToModern_Click;
        Controls.Add(btnSpfToPng);

        var btnImgToSpf = new Button { Text = "Modern to SPF", AutoSize = true, Location = new Point(30, 60), BackColor = Color.DodgerBlue, ForeColor = Color.White };
        btnImgToSpf.Click += BtnModernToSpf_Click;
        Controls.Add(btnImgToSpf);

        var btnMultiToSpf = new Button { Text = "Multi to SPF", AutoSize = true, Location = new Point(30, 105), BackColor = Color.DodgerBlue, ForeColor = Color.White };
        btnMultiToSpf.Click += BtnMultiToSpf_Click;
        Controls.Add(btnMultiToSpf);
    }

    private void BtnSpfToModern_Click(object sender, EventArgs e)
    {
        openFileDialog.Filter = "SPF Files|*.spf";
        if (openFileDialog.ShowDialog() != DialogResult.OK) return;
        var spfPath = openFileDialog.FileName;

        try
        {
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
            var spfImage = SpfImage.Read(spfPath);
            spfImage.WriteImg(saveFileDialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($@"Error converting SPF: {ex.Message}");
        }
    }

    private void BtnModernToSpf_Click(object sender, EventArgs e)
    {
        if (openFileDialog.ShowDialog() != DialogResult.OK) return;
        var imagePath = openFileDialog.FileName;
        var imageList = new List<string> { imagePath };

        try
        {
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
            using var imageCollection = new MagickImageCollection();

            foreach (var image in imageList)
            {
                imageCollection.Add(image);
            }

            var spfImage = SpfImage.FromMagickImageCollection(imageCollection);
            spfImage.WriteSpf(saveFileDialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($@"Error converting Images: {ex.Message}");
        }
    }

    private void BtnMultiToSpf_Click(object sender, EventArgs e)
    {
        if (openFileDialog.ShowDialog() != DialogResult.OK) return;
        var imagePath = openFileDialog.FileNames;
        var imageList = new List<string>();
        imageList.AddRange(imagePath);

        try
        {
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
            using var imageCollection = new MagickImageCollection();

            foreach (var image in imageList)
            {
                imageCollection.Add(image);
            }

            var spfImage = SpfImage.FromMagickImageCollection(imageCollection);
            spfImage.WriteSpf(saveFileDialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($@"Error converting Images: {ex.Message}");
        }
    }
}