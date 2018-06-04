using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDAL_Wrapper
{
    public class BufferedImage
    {
        public static int TYPE_INT_RGB = 4;

        int _width = 1, _height = 1;
        int[] image;

        public BufferedImage(int width, int height, int colorType)
        {
            _width = width;
            _height = height;
            image = new int[_width * _height];
        }

        public int getWidth()
        {
            return _width;
        }

        public int getHeight()
        {
            return _height;
        }
        public int getRGB(int x, int y)
        {
            return image[y * _width + x];
        }

        public void setRGB(int x, int y, int color)
        {
            image[y * _width + x] = color;
        }

    }
    public class ColorCannyEdgeDetector
    {

        private static double GAUSSIAN_CUT_OFF = 0.005;
        private int W;
        private int H;
        private int[] r, g, b;
        private double[] yrConv;
        private double[] xrConv;
        private double[] ygConv;
        private double[] xgConv;
        private double[] ybConv;
        private double[] xbConv;


        private double[] xGradient;
        private double[] yGradient;
        private double[] vMagnitude;

        private int[] data;


        private double[] magnitude;

        double gaussianKernelRadius = 1;
        int gaussianKernelWidth = 16;
        private static double lowThreshold = 3;
        private static double highThreshold = 7;

        public ColorCannyEdgeDetector(BufferedImage image)
        {

            this.W = image.getWidth();
            this.H = image.getHeight();
            int p = 0;
            r = new int[W * H];
            g = new int[W * H];
            b = new int[W * H];

            for (int x = 0; x < W; x++)
            {
                for (int y = 0; y < H; y++)
                {
                    int rgb = image.getRGB(x, y);
                    r[p] = (rgb >> 16) & 0xff;
                    g[p] = (rgb >> 8) & 0xff;
                    b[p++] = rgb & 0xff;
                }
            }

            yrConv = new double[r.Length];
            xrConv = new double[r.Length];
            ygConv = new double[r.Length];
            xgConv = new double[r.Length];
            ybConv = new double[r.Length];
            xbConv = new double[r.Length];

            xGradient = new double[r.Length];
            yGradient = new double[r.Length];
            vMagnitude = new double[r.Length];

            data = new int[r.Length];
            magnitude = new double[r.Length];

        }




        public BufferedImage findEdges()
        {
            computeColorGradients(gaussianKernelRadius, gaussianKernelWidth);
            double low = lowThreshold;
            double high = highThreshold;
            hysterezis(low, high);
            threshold();
            //        double max = 0;
            //        for(int i=0; i<magnitude.length; i++){
            //            max = Math.max(max, magnitude[i]);
            //        }
            //        for(int i=0; i<data.length; i++){
            //            data[i] = (int) (255 * magnitude[i] / max);
            //        }
            return image(data, data, data, W, H);
        }

        private static BufferedImage image(int[] r, int[] g, int[] b, int W, int H)
        {
            BufferedImage image = new BufferedImage(W, H, BufferedImage.TYPE_INT_RGB);
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    image.setRGB(x, y, r[x * H + y] << 16 | g[x * H + y] << 8 | b[x * H + y]);
                }
            }
            return image;
        }

        private void computeColorGradients(double kernelRadius, int kernelWidth)
        {
            double[] kernel = new double[kernelWidth];
            double[] diffKernel = new double[kernelWidth];
            int kWidth;
            for (kWidth = 0; kWidth < kernelWidth; kWidth++)
            {
                double g1 = gaussian(kWidth, kernelRadius);
                if (g1 <= GAUSSIAN_CUT_OFF && kWidth >= 2) break;
                double g2 = gaussian(kWidth - 0.5, kernelRadius);
                double g3 = gaussian(kWidth + 0.5, kernelRadius);
                kernel[kWidth] = (g1 + g2 + g3) / 3.0 / (2 * Math.PI * kernelRadius * kernelRadius);
                diffKernel[kWidth] = g3 - g2;
            }

            int initX = kWidth - 1;
            int maxX = W - (kWidth - 1);
            int initY = (kWidth - 1);
            int maxY = (H - (kWidth - 1));

            for (int x = initX; x < maxX; x++)
            {
                for (int y = initY; y < maxY; y++)
                {
                    double sumrX = r[x * H + y] * kernel[0];
                    double sumrY = r[x * H + y] * kernel[0];
                    double sumgX = g[x * H + y] * kernel[0];
                    double sumgY = g[x * H + y] * kernel[0];
                    double sumbX = b[x * H + y] * kernel[0];
                    double sumbY = b[x * H + y] * kernel[0];

                    for (int ri = 1; ri < kWidth; ri++)
                    {
                        sumrY += kernel[ri] * (r[x * H + y - ri] + r[x * H + y + ri]);
                        sumrX += kernel[ri] * (r[(x - ri) * H + y] + r[(x + ri) * H + y]);
                        sumgY += kernel[ri] * (g[x * H + y - ri] + g[x * H + y + ri]);
                        sumgX += kernel[ri] * (g[(x - ri) * H + y] + g[(x + ri) * H + y]);
                        sumbY += kernel[ri] * (b[x * H + y - ri] + b[x * H + y + ri]);
                        sumbX += kernel[ri] * (b[(x - ri) * H + y] + b[(x + ri) * H + y]);

                    }

                    yrConv[x * H + y] = sumrY;
                    xrConv[x * H + y] = sumrX;
                    ygConv[x * H + y] = sumgY;
                    xgConv[x * H + y] = sumgX;
                    ybConv[x * H + y] = sumbY;
                    xbConv[x * H + y] = sumbX;
                }

            }


            initX = kWidth;
            maxX = W - kWidth;
            initY = kWidth;
            maxY = (H - kWidth);
            for (int x = initX; x < maxX; x++)
            {
                for (int y = initY; y < maxY; y++)
                {
                    double rx = (xrConv[(x + 1) * H + y] - xrConv[(x - 1) * H + y]) / 2.0;
                    double gx = (xgConv[(x + 1) * H + y] - xgConv[(x - 1) * H + y]) / 2.0;
                    double bx = (xbConv[(x + 1) * H + y] - xbConv[(x - 1) * H + y]) / 2.0;

                    double ry = (yrConv[x * H + y - 1] - yrConv[x * H + y + 1]) / 2.0;
                    double gy = (ygConv[x * H + y - 1] - ygConv[x * H + y + 1]) / 2.0;
                    double by = (ybConv[x * H + y - 1] - ybConv[x * H + y + 1]) / 2.0;


                    double q1 = rx * rx + gx * gx + bx * bx;
                    double q2 = rx * ry + gx * gy + bx * by;
                    double q4 = ry * ry + gy * gy + by * by;

                    double[] eigen = Eigen(q1, q2, q2, q4);
                    double fe1 = eigen[0];
                    double fe2 = eigen[1];
                    double fv = eigen[2];

                    xGradient[x * H + y] = fe1;
                    yGradient[x * H + y] = fe2;
                    vMagnitude[x * H + y] = Math.Sqrt(fv);
                }

            }


            for (int x = initX + 1; x < maxX - 1; x++)
            {
                for (int y = initY + 1; y < maxY - 1; y++)
                {

                    double a1 = vMagnitude[(x - 1) * H + y - 1];
                    double a2 = vMagnitude[x * H + y - 1];
                    double a3 = vMagnitude[(x + 1) * H + y - 1];

                    double a4 = vMagnitude[(x - 1) * H + y];
                    double a5 = vMagnitude[x * H + y];
                    double a6 = vMagnitude[(x + 1) * H + y];

                    double a7 = vMagnitude[(x - 1) * H + y + 1];
                    double a8 = vMagnitude[x * H + y + 1];
                    double a9 = vMagnitude[(x + 1) * H + y + 1];

                    double gx = xGradient[x * H + y];
                    double gy = yGradient[x * H + y];

                    double angle = Math.Atan2(gy, gx);
                    double h, v;

                    if (ishorizonatal(angle))
                    {
                        h = a4;
                        v = a6;
                    }
                    else if (isvertical(angle))
                    {
                        h = a2;
                        v = a8;
                    }
                    else if (isdiag1(angle))
                    {
                        h = a3;
                        v = a7;
                    }
                    else if (isdiag2(angle))
                    {
                        h = a1;
                        v = a9;
                    }
                    else
                    {
                        throw new Exception("Incorrect angle");
                    }

                    double val = 0;
                    if (a5 > h && a5 >= v)
                        val = a5;

                    magnitude[x * H + y] = val;
                }
            }
        }

        private static int conv(byte col)
        {
            return col >= 0 ? col : (int)col + 256;
        }

        private void hysterezis(double low, double high)
        {
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    if (data[x * H + y] == 0 && magnitude[x * H + y] >= high)
                    {
                        track(x, y, low);
                    }
                }
            }
        }

        private void threshold()
        {
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    data[x * H + y] = data[x * H + y] > 0 ? 255 : 0;
                }
            }
        }

        private void track(int x_, int y_, double threshold)
        {
            Stack<Point> stack = new Stack<Point>();
            stack.Push(new Point(x_, y_));
            data[x_ * H + y_] = (int)magnitude[x_ * H + y_];
            while (stack.Count > 0)
            {
                Point n = stack.Pop();
                int x1 = n.X;
                int y1 = n.Y;
                int x0 = x1 == 0 ? 0 : x1 - 1;
                int x2 = x1 == W - 1 ? W - 1 : x1 + 1;
                int y0 = y1 == 0 ? 0 : y1 - 1;
                int y2 = y1 == H - 1 ? H - 1 : y1 + 1;

                for (int x = x0; x <= x2; x++)
                {
                    for (int y = y0; y <= y2; y++)
                    {
                        if ((y != y1 || x != x1) && data[x * H + y] == 0 && magnitude[x * H + y] >= threshold)
                        {
                            data[x * H + y] = (int)magnitude[x * H + y] + 1;
                            stack.Push(new Point(x, y));
                        }
                    }
                }
            }
        }

        private double[] Eigen(double q1, double q2, double q3, double q4)
        {

            double a = 1;
            double b = -(q1 + q4);
            double c = q1 * q4 - q2 * q3;
            double v = ((-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a));
            double e1 = -q2;
            double e2 = q1 - v;

            if (v < 0)
            {
                v = -v;
                e1 = -e1;
                e2 = -e2;
            }

            return new double[] { e1, e2, v };
        }

        private bool isdiag2(double angle)
        {
            return ((angle >= -3 * Math.PI / 8) && (angle < -(Math.PI / 8)))
                    || ((angle >= 5 * Math.PI / 8) && (angle < 7 * Math.PI / 8));
        }

        private bool isdiag1(double angle)
        {
            return ((angle >= -7 * Math.PI / 8) && (angle < -5 * Math.PI / 8))
                    || ((angle >= Math.PI / 8) && (angle < 3 * Math.PI / 8));
        }

        private bool isvertical(double angle)
        {
            return ((angle >= -5 * Math.PI / 8) && (angle < -3 * Math.PI / 8))
                    || ((angle >= 3 * Math.PI / 8) && (angle < 5 * Math.PI / 8));
        }

        private bool ishorizonatal(double angle)
        {
            return (angle < -7 * Math.PI / 8) || ((angle >= -(Math.PI / 8))
                    && (angle < Math.PI / 8)) || (angle >= 7 * Math.PI / 8);
        }


        private double gaussian(double x, double sigma)
        {
            return Math.Exp(-(x * x) / (2f * sigma * sigma));
        }
    }
}
