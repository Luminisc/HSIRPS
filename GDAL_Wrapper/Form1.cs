using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSGeo.GDAL;

namespace GDAL_Wrapper
{
    public partial class Form1 : Form
    {
        public static string picturePath = @"..\..\Pics\Data_Envi\samson_1.img";
        //public static string picturePath = @"..\..\Pics\Urban_F210\Urban_F210.img";
        public Dataset dataset;
        int max = -10;
        int min = 60000;

        double[] wavelengths = new double[] {
            400.938721, 404.152496, 407.366302, 410.580048, 413.793854, 417.007629,
            420.221405, 423.435181, 426.648956, 429.862732, 433.076508, 436.290314,
            439.504089, 442.717865, 445.931641, 449.145416, 452.359192, 455.572968,
            458.786774, 462.000549, 465.214325, 468.428101, 471.641876, 474.855652,
            478.069427, 481.283234, 484.497009, 487.710785, 490.924561, 494.138336,
            497.352112, 500.565887, 503.779694, 506.993439, 510.207245, 513.421021,
            516.634766, 519.848572, 523.062378, 526.276123, 529.489929, 532.703674,
            535.917480, 539.131226, 542.345032, 545.558838, 548.772583, 551.986389,
            555.200134, 558.413940, 561.627686, 564.841492, 568.055298, 571.269043,
            574.482849, 577.696594, 580.910400, 584.124146, 587.337952, 590.551758,
            593.765503, 596.979309, 600.193054, 603.406860, 606.620605, 609.834412,
            613.048157, 616.261963, 619.475769, 622.689514, 625.903320, 629.117065,
            632.330872, 635.544617, 638.758423, 641.972229, 645.185974, 648.399780,
            651.613525, 654.827332, 658.041077, 661.254883, 664.468689, 667.682434,
            670.896240, 674.109985, 677.323792, 680.537537, 683.751343, 686.965149,
            690.178894, 693.392700, 696.606445, 699.820251, 703.033997, 706.247803,
            709.461548, 712.675354, 715.889160, 719.102905, 722.316711, 725.530457,
            728.744263, 731.958008, 735.171814, 738.385620, 741.599365, 744.813171,
            748.026917, 751.240723, 754.454468, 757.668274, 760.882080, 764.095825,
            767.309631, 770.523376, 773.737183, 776.950928, 780.164734, 783.378540,
            786.592285, 789.806030, 793.019836, 796.233643, 799.447388, 802.661194,
            805.874939, 809.088745, 812.302490, 815.516296, 818.730103, 821.943848,
            825.157654, 828.371399, 831.585205, 834.798950, 838.012756, 841.226562,
            844.440308, 847.654114, 850.867859, 854.081665, 857.295410, 860.509216,
            863.723022, 866.936768, 870.150574, 873.364319, 876.578125, 879.791870,
            883.005676, 886.219421, 889.433228, 892.647034, 895.860779, 899.074585
        };



        public Form1()
        {
            InitializeComponent();
            var dllDirectory = @"GDAL\gdal\csharp";
            var dllDirectory2 = @"GDAL\";
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + dllDirectory + ";" + dllDirectory2);
            Gdal.AllRegister();
            dataset = Gdal.Open(picturePath, Access.GA_ReadOnly);
            numericUpDown1.Maximum = dataset.RasterCount;

            for (int i = 0; i < dataset.RasterCount; i++)
            {
                var band = dataset.GetRasterBand(i + 1);
                
            }
        }

        void RenderBand(int bandIndex)
        {
            var band = dataset.GetRasterBand(bandIndex);

            var width = band.XSize;
            var height = band.YSize;

            var bandWavelength = wavelengths[bandIndex - 1];
            var color = conversion.wavelength_to_rgb(bandWavelength);

            // Creating a Bitmap to store the GDAL image in
            Bitmap bitmap = new Bitmap(width, height);
            // Creating a C# array to hold the image data
            var r = new short[width * height];
            band.ReadRaster(0, 0, width, height, r, width, height, 0, 0);
            // Copying the pixels into the C# bitmap
            int i, j;
            for (i = 0; i < width; i++)
            {
                for (j = 0; j < height; j++)
                {
                    var pixel = Convert.ToInt16(r[i + j * width]);
                    if (pixel > max) max = pixel;
                    if (pixel < min) min = pixel;
                    if (pixel < 0) pixel = 0;
                    if (pixel > 255) pixel = 255;
                    float coeff = pixel / 255;
                    //Color newColor = Color.FromArgb((byte)(255), (byte)(color.R * coeff), (byte)(color.G * coeff), (byte)(color.B * coeff));
                    Color newColor = Color.FromArgb(pixel, color.R, color.G, color.B);
                    bitmap.SetPixel(i, j, newColor);
                }
            }

            pictureBox1.Image = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.DontCare);
        }

        void RenderSide(int w)
        {
            var band = dataset.GetRasterBand(1);
            var depth = dataset.RasterCount;
            var height = band.YSize;

            //int[] bands = Enumerable.Range(1, depth).ToArray();
            Bitmap bitmap = new Bitmap(depth, height);
            var r = new short[height];
            //band.ReadRaster(0, 0, depth, height, r, depth, height, 0, 0);
            //dataset.ReadRaster(w, 0, 1, height, r, 1, height, 1, new int[] { 1 }, 0, 0, 0);

            

            int i, j;
            for (i = 0; i < depth; i++)
            {
                dataset.ReadRaster(w, 0, 1, height, r, 1, height, 1, new int[] { i+1 }, 0, 0, 0);
                for (j = 0; j < height; j++)
                {
                    var pixel = Convert.ToInt16(r[j]);
                    if (pixel > max) max = pixel;
                    if (pixel < min) min = pixel;
                    if (pixel < 0) pixel = 0;
                    if (pixel > 255) pixel = 255;
                    float coeff = pixel / 255;

                    var bandWavelength = wavelengths[i];
                    var color = conversion.wavelength_to_rgb(bandWavelength);

                    Color newColor = Color.FromArgb(pixel, color.R, color.G, color.B);
                    bitmap.SetPixel(i, j, newColor);
                }
            }

            pictureBox3.Image = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.DontCare);

            //TODO: Exterminate
            if (w == band.YSize) //just because next part will have errors. 
                w = w - 1;

            for (i = 0; i < depth; i++)
            {
                dataset.ReadRaster(w+1, 0, 1, height, r, 1, height, 1, new int[] { i + 1 }, 0, 0, 0);
                for (j = 0; j < height; j++)
                {
                    var pixel = Convert.ToInt16(r[j]);
                    if (pixel > max) max = pixel;
                    if (pixel < min) min = pixel;
                    if (pixel < 0) pixel = 0;
                    if (pixel > 255) pixel = 255;
                    float coeff = pixel / 255;

                    var bandWavelength = wavelengths[i];
                    var color = conversion.wavelength_to_rgb(bandWavelength);

                    Color newColor = Color.FromArgb(pixel, color.R, color.G, color.B);
                    bitmap.SetPixel(i, j, newColor);
                }
            }

            pictureBox4.Image = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.DontCare);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RenderBand((int)numericUpDown1.Value);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            RenderBand((int)numericUpDown1.Value);
        }

        void RenderSignature(int x, int y)
        {
            var histBuffer = new short[dataset.RasterCount];
            dataset.ReadRaster(x, y, 1, 1, histBuffer, 1, 1, dataset.RasterCount, null, 0, 0, 0);

            var hmax = histBuffer.Max();
            var hmin = histBuffer.Min();

            var histHeight = hmax - hmin;
            Bitmap img = new Bitmap(dataset.RasterCount, histHeight + 10);
            using (Graphics g = Graphics.FromImage(img))
            {
                for (int i = 0; i < histBuffer.Length; i++)
                {
                    var bandWavelength = wavelengths[i];
                    var color = conversion.wavelength_to_rgb(bandWavelength);
                    float pct = histBuffer[i] / (float)hmax;   // What percentage of the max is this value?
                    g.DrawLine(new Pen(color),
                        new Point(i, img.Height - 5),
                        new Point(i, img.Height - 5 - (int)(pct * histHeight))  // Use that percentage of the height
                        );
                }
            }
            pictureBox2.Image = img;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            var point = pictureBox1.PointToClient(MousePosition);
            var pw = pictureBox1.Width;
            var ph = pictureBox1.Height;
            
            if (pw > ph)
            {
                var diff = pw - ph;
                var picPoint = point.X - diff / 2;
                if (picPoint < 0 || picPoint > ph) return;

                point = new Point(picPoint * dataset.RasterXSize / ph, point.Y * dataset.RasterYSize / ph);
                RenderSignature(point.X, point.Y);
                RenderSide(point.X);
            }
            else
            {
                var diff = ph - pw;
                var picPoint = point.Y - diff / 2;
                if (picPoint < 0 || picPoint > pw) return;

                point = new Point(point.X * dataset.RasterXSize / pw, picPoint * dataset.RasterYSize / pw);
                RenderSignature(point.X, point.Y);
                RenderSide(point.X);
            }
        }

        private void btnSaveTransitionImage_Click(object sender, EventArgs e)
        {
            DataGenerators.GenerateTransitionImage(dataset, "transition.img");
        }
    }
}
