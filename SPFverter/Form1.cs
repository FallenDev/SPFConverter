using SpfConverter.Spf;

namespace SPFverter;

public partial class Form1 : Form
{
    private OpenFileDialog openFileDialog;
    private FolderBrowserDialog openFolderDialog;
    private SaveFileDialog saveFileDialog;

    public Form1()
    {
        InitializeComponent();
    }

    private void BtnSpfToModern_Click(object sender, EventArgs e)
    {
        openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "SPF Files|*.spf";
        if (openFileDialog.ShowDialog() != DialogResult.OK) return;
        var spfPath = openFileDialog.FileName;

        try
        {
            saveFileDialog = new SaveFileDialog();
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
        openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() != DialogResult.OK) return;
        var imagePath = openFileDialog.FileName;

        try
        {
            saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "SPF Files|*.spf";
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

            using var imageCollection = new MagickImageCollection();
            {
                imageCollection.Add(imagePath);
            }
            var spfImage = SpfImage.FromMagickImageCollection(imageCollection, DitherMethod.FloydSteinberg);
            spfImage.WriteSpf(saveFileDialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($@"Error converting Images: {ex.Message}");
        }
    }

    private void BtnSpfToFolder_Click(object sender, EventArgs e)
    {
        var inputPath = textBox1.Text;
        if (inputPath.Length == 0)
        {
            MessageBox.Show($@"Add an input path, or click Input Path:");
            return;
        }

        var dir = new DirectoryInfo(inputPath);
        var images = dir.GetFiles("*.spf");
        var outputPath = textBox2.Text;
        if (outputPath.Length == 0)
        {
            MessageBox.Show($@"Add an output path, or click Output Path:");
            return;
        }

        //try
        //{
            foreach (var image in images)
            {
                var spfImage = SpfImage.Read(image.FullName);
                if (spfImage == null) continue;
                var nameSplit = image.Name.Split('.');
                spfImage.WriteImg($"{outputPath}\\{nameSplit[0]}.png");
            }
        //}
        //catch (Exception ex)
        //{
        //    MessageBox.Show($@"Error converting Images: {ex.Message}");
        //}
    }

    private void BtnFolderToSpf_Click(object sender, EventArgs e)
    {
        openFolderDialog = new FolderBrowserDialog();
        if (openFolderDialog.ShowDialog() != DialogResult.OK) return;
        var imagePath = openFolderDialog.SelectedPath;
        var dir = new DirectoryInfo(imagePath);
        var imageList = new List<string>();

        foreach (var images in dir.GetFiles("*"))
        {
            imageList.Add(images.FullName);
        }

        try
        {
            saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "SPF Files|*.spf";
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
            using var imageCollection = new MagickImageCollection();

            foreach (var image in imageList)
            {
                imageCollection.Add(image);
            }

            var spfImage = SpfImage.FromMagickImageCollection(imageCollection, DitherMethod.FloydSteinberg);
            spfImage.WriteSpf(saveFileDialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($@"Error converting Images: {ex.Message}");
        }
    }
}