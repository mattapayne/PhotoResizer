using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PhotoResizer.Properties;

namespace PhotoResizer
{
    public partial class Main : Form
    {
        private string _selectedPath;
        private const int DefaultImgWidth = 640;
        private const int DefaultImgHeight = 360;
        private const string FileExtension = "*.jpg";
        private const string DefaultResizedDirectoryName = "resized";

        public Main()
        {
            _selectedPath = String.Empty;
            InitializeComponent();
            txtDirectory.Text = DefaultResizedDirectoryName;
            txtHeight.Text = DefaultImgHeight.ToString();
            txtWidth.Text = DefaultImgWidth.ToString();
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var result = fbd.ShowDialog();

            if(result == DialogResult.OK)
            {
                _selectedPath = fbd.SelectedPath;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(_selectedPath))
            {
                MessageBox.Show(Resources.NoFolderSelectedErrorMessage, 
                    Resources.NoFolderSelectedErrorTitle);

                return;
            }

            ProcessImages();
        }

        private void ProcessImages()
        {
            var folder = new DirectoryInfo(_selectedPath);

            if(!folder.Exists)
            {
                MessageBox.Show(Resources.SelectedFolderDoesNotExistErrorMessage, 
                    Resources.SelectedFolderDoesNotExistErrorTitle);
                return;
            }

            var files = folder.GetFiles(FileExtension);

            if(!files.Any())
            {
                MessageBox.Show(Resources.NoPicturesInSelectedFolderErrorMessage,
                    Resources.NoPicturesInSelectedFolderErrorTitle);
                return;
            }

            ProcessImages(files);
            MessageBox.Show(Resources.SuccessfullyResizedImagesMessage,
                Resources.ResizedImagesSuccessTitle);
        }

        private void ProcessImages(IEnumerable<FileInfo> files)
        {
            var resizedImageFolder = Path.Combine(_selectedPath, ResizedDirectoryName);

            if(Directory.Exists(resizedImageFolder))
            {
                Directory.Delete(resizedImageFolder, true);
            }

            Directory.CreateDirectory(resizedImageFolder);

            var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
            var qualityEncoder = Encoder.Quality;
            var encoderParams = new EncoderParameters(1);
            var encoderParam = new EncoderParameter(qualityEncoder, 90L);
            encoderParams.Param[0] = encoderParam;

            foreach(var imageFile in files)
            {
                var image = Image.FromFile(imageFile.FullName);
                var imageName = imageFile.Name;

                using(var resizedImage = new Bitmap(image, new Size(ImgWidth, ImgHeight)))
                {
                    resizedImage.Save(Path.Combine(resizedImageFolder, imageName), jpegEncoder, encoderParams);     
                }
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        private int ImgHeight
        {
            get
            {
                if(!String.IsNullOrEmpty(txtHeight.Text))
                {
                    int height;

                    if(Int32.TryParse(txtHeight.Text, out height))
                    {
                        return height;
                    }
                }

                return DefaultImgHeight;
            }
        }

        private int ImgWidth
        {
            get
            {
                if (!String.IsNullOrEmpty(txtWidth.Text))
                {
                    int width;

                    if (Int32.TryParse(txtWidth.Text, out width))
                    {
                        return width;
                    }
                }

                return DefaultImgWidth;
            }
        }

        private string ResizedDirectoryName
        {
            get
            {
                return !String.IsNullOrEmpty(txtDirectory.Text) ? txtDirectory.Text : DefaultResizedDirectoryName;
            }
        }
    }
}
