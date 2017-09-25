using System;
using System.Windows;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text;

namespace ImageConverterToASCII
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int redThreshold = 128;
        private const int greenThreshold = 128;
        private const int blueThreshold = 128;

        private const string procentSym = "%";
        private const string atSym = "@";

        private string fileName = "";


        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image Files (*.png; *.jpg) | *.png; *.jpg;";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                fileName = dlg.FileName;
                sourceImage.Source = new BitmapImage(new Uri(fileName));
                // Remove converted image
                convertedImage.Source = new BitmapImage();
            }
        }

        private void Convert_Image_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsFileNameSet()) return;

                var bitmapOfImage = new Bitmap(fileName);
                string[] symbolsFromImage = ProcessPixels(bitmapOfImage);
                DrawImageFromSymbols(symbolsFromImage, bitmapOfImage);
                SaveToFile(symbolsFromImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private bool IsFileNameSet()
        {
            if (fileName != "") return true;
            else
            {
                MessageBox.Show("Image was not chosen.");
                return false;
            }
        }



        /// <summary>
        /// Sets symbol for each pixel depending on its color.
        /// </summary>
        /// <param name="image"> Bitmap of image to process </param>
        /// <returns> Image, converted to symbols </returns>
        private string[] ProcessPixels(Bitmap image)
        {
            string[] convertedSymbolicImage = new string[image.Height];
            int currentRowIndex = 0;
            StringBuilder newConvertedLine = new StringBuilder(image.Width);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++) 
                {
                    Color pixelColor = image.GetPixel(x, y);

                    bool lessThanThreshold = AreChannelsLessThanThreshold(pixelColor);

                    if (lessThanThreshold) newConvertedLine.Append(atSym);
                    else newConvertedLine.Append(procentSym);
                }
                convertedSymbolicImage[currentRowIndex] = newConvertedLine.ToString();
                newConvertedLine.Clear();
                currentRowIndex++;
            }

            return convertedSymbolicImage;
        }

        /// <summary>
        /// Decides which symbol to set for given pixel.
        /// Depends on being bigger or less than threshold value.
        /// </summary>
        /// <param name="pixelColor"> Color of given pixel </param>
        /// <returns>
        /// True: pixel color is lower than threshold.
        /// False: pixel color is higher than threshold.
        /// </returns>
        private bool AreChannelsLessThanThreshold(Color pixelColor)
        {
            int channelsLessThanThreshold = 0;

            if (pixelColor.R < redThreshold) channelsLessThanThreshold++;
            if (pixelColor.G < redThreshold) channelsLessThanThreshold++;
            if (pixelColor.B < redThreshold) channelsLessThanThreshold++;

            // A least 2 channels should be lower than threshold to choose lower side symbol.
            if (channelsLessThanThreshold >= 2) return true;
            else return false;
        }



        /// <summary>
        /// Makes a preview of converted symbolic image.
        /// </summary>
        private void DrawImageFromSymbols(string[] convertedSymbolicImage, Bitmap image)
        {
            String drawString = PrepareSymbolsForImageConversion(convertedSymbolicImage, image);
            Font drawFont = new Font("Arial", 2);
            convertedImage.Source = ConvertBitmapToBitmapImage(ConvertTextToImage(drawString, drawFont));
        }

        private Bitmap ConvertTextToImage(string txt, Font font)
        {
            Bitmap bm = new Bitmap(1, 1);
            Graphics graphics = Graphics.FromImage(bm);

            SizeF stringSize = graphics.MeasureString(txt, font);
            bm = new Bitmap(bm, (int)stringSize.Width, (int)stringSize.Height);
            graphics = Graphics.FromImage(bm);

            graphics.DrawString(txt, font, Brushes.White, 0, 0);

            font.Dispose();
            graphics.Flush();
            graphics.Dispose();

            return bm;     
        }

        private string PrepareSymbolsForImageConversion(string[] convertedSymbolicImage, Bitmap image)
        {
            StringBuilder allConvertedLinesKeeper = new StringBuilder(convertedSymbolicImage.Length * image.Width);
            foreach (string line in convertedSymbolicImage)
            {
                allConvertedLinesKeeper.Append(line + "\n");
            }

            return allConvertedLinesKeeper.ToString();
        }



        /// <summary>
        /// Saves converted image to txt file.
        /// </summary>
        private void SaveToFile(string[] convertedSymbolicImage)
        {
            using (StreamWriter outputFile = new StreamWriter(@"../../Output/imageInASCII.txt"))
            {
                foreach (string line in convertedSymbolicImage) outputFile.WriteLine(line);
            }
        }



        /// <summary>
        /// Takes a bitmap and converts it to an image
        /// </summary>
        private BitmapImage ConvertBitmapToBitmapImage(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
