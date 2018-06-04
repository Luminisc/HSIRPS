using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSGeo.GDAL;

namespace GDAL_Wrapper
{
    class DataGenerators
    {
        public static void GenerateTransitionImage(Dataset dataset, string output)
        {
            int width = dataset.RasterXSize;
            int height = dataset.RasterYSize;
            int depth = dataset.RasterCount;
            var bandBuffer = new short[width * height];
            var byteBuffer = new byte[width * height * sizeof(short)];

            using (Stream sw = new FileStream(output, FileMode.OpenOrCreate, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(sw))
            {
                bw.Write(width);
                bw.Write(height);
                bw.Write(depth);

                Band band;
                for (int k = 0; k < depth; k++)
                {
                    band = dataset.GetRasterBand(k + 1);
                    band.ReadRaster(0, 0, width, height, bandBuffer, width, height, 0, 0);
                    Buffer.BlockCopy(bandBuffer, 0, byteBuffer, 0, width * height * sizeof(short));
                    bw.Write(byteBuffer);
                }
            }
        }
    }
}
