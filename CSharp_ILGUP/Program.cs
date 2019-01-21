
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_ILGUP
{
    class Program
    {
        static void MyKernel(Index index, ArrayView<int> inputBuffer, ArrayView<int> outpuBuffer, int imgWidth, int imgHeight, int factor)
        {
            // 'Allocate' a single shared memory variable of type int (= 4 bytes)
            ref int maxValue = ref ILGPU.SharedMemory.Allocate<int>();

            // Initialize shared memory
            if (index.IsFirst)
                maxValue = 0;

            // Wait for the initialization to complete
            Group.Barrier();


            //   pxl :
            //
            // |09|10|11|12|13|
            // |24|01|02|03|14|
            // |23|08|00|04|15|
            // |22|07|06|05|16|
            // |21|20|19|18|17|

            int pxlRes = 0;
            int x = index % imgWidth;
            int y = index / imgWidth;

            if (x < 0 || x >= imgWidth)
                return;


            if (y < 0 || y >= imgHeight)
                return;

            int curIndex = 0;

            var val0 = inputBuffer[index];

            bool OnBorderTop3 = false;
            bool OnBorderBot3 = false;
            bool OnBorderLeft3 = false;
            bool OnBorderRight3 = false;

            bool OnBorderTop5 = false;
            bool OnBorderBot5 = false;
            bool OnBorderLeft5 = false;
            bool OnBorderRight5 = false;

            if (x - 1 < 0)
                OnBorderLeft3 = true;
            if (y - 1 < 0)
                OnBorderTop3 = true;
            if (y + 1 >= imgHeight)
                OnBorderBot3 = true;
            if (x + 1 >= imgWidth)
                OnBorderRight3 = true;

            if (x - 2 < 0)
                OnBorderLeft5 = true;
            if (y - 2 < 0)
                OnBorderTop5 = true;
            if (y + 2 >= imgHeight)
                OnBorderBot5 = true;
            if (x + 2 >= imgWidth)
                OnBorderRight5 = true;


            #region noyeau 3x3
            if (!OnBorderTop3)
            {
                //pxl 2
                curIndex = (y - 1) * imgWidth + (x);
                pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);

                if (!OnBorderLeft3)
                {
                    //if (cbx8Connect.Checked)
                    {
                        //pxl 1
                        curIndex = (y - 1) * imgWidth + (x - 1);
                        pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                    }
                }

                if (!OnBorderRight3)
                {
                    //if (cbx8Connect.Checked)
                    {
                        //pxl 3
                        curIndex = (y - 1) * imgWidth + (x + 1);
                        pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                    }
                }
            }

            if (!OnBorderBot3)
            {
                //pxl 6
                curIndex = (y + 1) * imgWidth + (x);
                pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);

                if (!OnBorderLeft3)
                {
                    //if (cbx8Connect.Checked)
                    {
                        //pxl 7
                        curIndex = (y + 1) * imgWidth + (x - 1);
                        pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                    }
                }

                if (!OnBorderRight3)
                {
                    //if (cbx8Connect.Checked)
                    {
                        //pxl 5
                        curIndex = (y + 1) * imgWidth + (x + 1);
                        pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                    }
                }
            }

            if (!OnBorderLeft3)
            {
                //pxl 8
                curIndex = (y) * imgWidth + (x - 1);
                pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
            }

            if (!OnBorderRight3)
            {
                //pxl 4
                curIndex = (y) * imgWidth + (x + 1);
                pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
            }
            #endregion noyeau 3x3

            #region noyeau 5x5
            if (!OnBorderTop5)
            {
                if (!OnBorderLeft3)
                {
                    //pxl 10
                    curIndex = (y - 2) * imgWidth + (x - 1);
                    pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                }

                //pxl 11
                curIndex = (y - 2) * imgWidth + (x);
                pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);

                //pxl 12
                if (!OnBorderRight3)
                {
                    curIndex = (y - 2) * imgWidth + (x + 1);
                    pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                }

                if (!OnBorderLeft5)
                {
                    //pxl 9
                    curIndex = (y - 2) * imgWidth + (x - 2);
                    pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                }

                if (!OnBorderRight5)
                {
                    //pxl 13
                    curIndex = (y - 2) * imgWidth + (x + 2);
                    pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                }
            }

            if (!OnBorderBot5)
            {
                if (!OnBorderLeft3)
                {
                    //pxl 20
                    curIndex = (y + 2) * imgWidth + (x - 1);
                    pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                }

                //pxl 19
                curIndex = (y + 2) * imgWidth + (x);
                pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);

                if (!OnBorderRight3)
                {
                    //pxl 18
                    curIndex = (y + 2) * imgWidth + (x + 1);
                    pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                }

                if (!OnBorderLeft5)
                {
                    //pxl 21
                    curIndex = (y + 2) * imgWidth + (x - 2);
                    pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                }

                if (!OnBorderRight5)
                {
                    //pxl 17
                    curIndex = (y + 2) * imgWidth + (x + 2);
                    pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                }
            }

            if (!OnBorderLeft5)
            {
                if (!OnBorderTop3)
                {
                    //pxl 24
                    curIndex = (y - 1) * imgWidth + (x - 2);
                    pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                }

                //pxl 23
                curIndex = (y) * imgWidth + (x - 2);
                pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);

                if (!OnBorderBot3)
                {
                    //pxl 22
                    curIndex = (y + 1) * imgWidth + (x - 2);
                    pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                }
            }

            if (!OnBorderRight5)
            {
                if (!OnBorderTop3)
                {
                    //pxl 14
                    curIndex = (y - 1) * imgWidth + (x + 2);
                    pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                }

                //pxl 15
                curIndex = (y) * imgWidth + (x + 2);
                pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);

                if (!OnBorderBot3)
                {
                    //pxl 16
                    curIndex = (y + 1) * imgWidth + (x + 2);
                    pxlRes += Math.Abs(val0 - inputBuffer[curIndex]);
                }
            }

            #endregion noyeau 5x5

            pxlRes *= factor;

            if (pxlRes > 255)
                pxlRes = 255;
            if (pxlRes < 0)
                pxlRes = 0;

            if (pxlRes > maxValue)
                maxValue = pxlRes;

            // Wait for all threads to complete the maximum computation process
            Group.Barrier();

            outpuBuffer[index] = (byte)pxlRes;
        }

        static byte[] ImageToByte(Bitmap img)
        {
            var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, img.PixelFormat);
            byte[] buffer = new byte[img.Height * data.Stride];
            Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);

            img.UnlockBits(data);
            return buffer;
        }

        static void SaveImage(byte[] buffer, int width, int height, string filename)
        {
            int padding = (width % 4) != 0 ? 4 - (width % 4) : 0; //determine padding needed for bitmap file
                                                                  //if (padding != 0 || selection.Width != this.width || selection.Height != this.height)
            byte[] tempBuffer = new byte[width * height + padding * height];

            unsafe
            {
                fixed (byte* ptrInF = buffer, ptrOutF = tempBuffer)
                {
                    byte* ptrIn = ptrInF;
                    byte* ptrOut = ptrOutF;
                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                            *(ptrOut + (height - 1 - y) * width + (height - 1 - y) * padding + x) = *(ptrIn + x + (y) * width);
                }
            }

            GrayBMP.SaveAsGrayScale(tempBuffer, width, height, filename);
        }

        static void Main(string[] args)
        {
            int count = 1;
            string accel = "both";
            if (args != null && args.Length > 0)
            {
                int.TryParse(args[0], out count);
                if (args.Length > 1)
                {
                    accel = args[1].ToLower();
                }
            }

            Console.WriteLine("========================================");
            Console.WriteLine("==  C# ILGPU Benchmark : Gradient 5x5 ==");
            Console.WriteLine("========================================");

            Stopwatch sw = Stopwatch.StartNew();

            var img = new Bitmap(@"..\..\0.bmp");
            long readingTime = sw.ElapsedMilliseconds;
            Console.WriteLine("Image readed in " + readingTime + " ms");

            sw.Restart();
            var buffer = ImageToByte(img);
            long extractionTime = sw.ElapsedMilliseconds;
            Console.WriteLine("Buffer extracted in " + extractionTime + " ms");

            sw.Restart();
            var data = ImageToByte(img);
            var Originbuffer = data.Select(v => (int)v).ToArray<int>();
            var outputBuffer = new int[Originbuffer.Length];
            long allocatingCache = sw.ElapsedMilliseconds;
            Console.WriteLine("Allocating Cache in " + allocatingCache + " ms");

            if (count == 1)
            {
                sw.Restart();
                long allocatingAccel = 0;
                long processTime = 0;
                long saveTime = 0;
                using (var context = new Context())
                {
                    long allocatingContext = sw.ElapsedMilliseconds;
                    Console.WriteLine("Allocating Context in " + allocatingContext + " ms");
                    sw.Restart();
                    switch (accel)
                    {
                        case "both":
                            // For each available accelerator...
                            foreach (var acceleratorId in Accelerator.Accelerators)
                            {
                                using (var accelerator = Accelerator.Create(context, acceleratorId))
                                {
                                    allocatingAccel = sw.ElapsedMilliseconds;
                                    Console.WriteLine("Allocating Accelerator in " + allocatingAccel + " ms");

                                    sw.Restart();
                                    RunKernel(1, img, Originbuffer, ref outputBuffer, context, accelerator, false);

                                    processTime = sw.ElapsedMilliseconds;
                                    Console.WriteLine("Image processed for " + acceleratorId.AcceleratorType + " in " + processTime + " ms");

                                }

                                sw.Restart();
                                SaveImage(outputBuffer.Select(v => (byte)v).ToArray<byte>(), img.Width, img.Height, "CSharp_ILGPU_" + acceleratorId.AcceleratorType + ".bmp");
                                saveTime = sw.ElapsedMilliseconds;
                                Console.WriteLine("Image saved in " + saveTime + " ms");
                                long total = readingTime + extractionTime + allocatingCache + allocatingContext + allocatingAccel + processTime + saveTime;
                                Console.WriteLine("Total : " + total + " ms");
                            }
                            break;
                        case "cpu":
                            {
                                using (var accelerator = new CPUAccelerator(context))
                                    RunKernel(1, img, Originbuffer, ref outputBuffer, context, accelerator, false);

                                processTime = sw.ElapsedMilliseconds;
                                Console.WriteLine("Image processed for cpu in " + processTime + " ms");

                                sw.Restart();
                                SaveImage(outputBuffer.Select(v => (byte)v).ToArray<byte>(), img.Width, img.Height, "CSharp_ILGPU_" + accel + ".bmp");
                                saveTime = sw.ElapsedMilliseconds;
                                Console.WriteLine("Image saved in " + saveTime + " ms");
                                long total = readingTime + extractionTime + processTime + saveTime;
                                Console.WriteLine("Total : " + total + " ms");
                            }
                            break;
                        case "gpu":
                        case "cuda":
                            {
                                using (var accelerator = new CudaAccelerator(context))
                                    RunKernel(1, img, Originbuffer, ref outputBuffer, context, accelerator, false);

                                processTime = sw.ElapsedMilliseconds;
                                Console.WriteLine("Image processed for gpu in " + processTime + " ms");

                                sw.Restart();
                                SaveImage(outputBuffer.Select(v => (byte)v).ToArray<byte>(), img.Width, img.Height, "CSharp_ILGPU_" + accel + ".bmp");
                                saveTime = sw.ElapsedMilliseconds;
                                Console.WriteLine("Image saved in " + saveTime + " ms");
                                long total = readingTime + extractionTime + processTime + saveTime;
                                Console.WriteLine("Total : " + total + " ms");
                            }
                            break;
                    }
                }
                return;
            }


            // Create the required ILGPU context
            using (var context = new Context())
            {
                switch (accel)
                {
                    case "both":
                        // For each available accelerator...
                        foreach (var acceleratorId in Accelerator.Accelerators)
                        {
                            using (var accelerator = Accelerator.Create(context, acceleratorId))
                                RunKernel(count, img, Originbuffer, ref outputBuffer, context, accelerator);
                        }
                        break;
                    case "cpu":
                        using (var accelerator = new CPUAccelerator(context))
                            RunKernel(count, img, Originbuffer, ref outputBuffer, context, accelerator);
                        break;
                    case "gpu":
                    case "cuda":
                        using (var accelerator = new CudaAccelerator(context))
                            RunKernel(count, img, Originbuffer, ref outputBuffer, context, accelerator);
                        break;
                }

            }
        }

        private static void RunKernel(int count, Bitmap img, int[] Originbuffer, ref int[] outputBuffer, Context context, Accelerator accelerator, bool verbose = true)
        {
            Stopwatch sw = Stopwatch.StartNew();
            long time = 0;
            if (verbose)
                Console.WriteLine("------ Test on " + accelerator + " ------");

            // accelerator.LoadAutoGroupedStreamKernel creates a typed launcher
            // that implicitly uses the default accelerator stream.
            // In order to create a launcher that receives a custom accelerator stream
            // use: accelerator.LoadAutoGroupedKernel<Index, ArrayView<int> int>(...)
            var kernel = accelerator.LoadAutoGroupedStreamKernel<Index, ArrayView<int>, ArrayView<int>, int, int, int>(MyKernel);

            for (int i = 0; i < count; i++)
            {
                // Allocate some memory
                using (var buffer = accelerator.Allocate<int>(Originbuffer.Length))
                using (var output = accelerator.Allocate<int>(Originbuffer.Length))
                {
                    sw.Restart();

                    // Initialize data source
                    //buffer.CopyFrom(Originbuffer.Select(v => (int)v).ToArray(), 0, 0, Originbuffer.Length);
                    buffer.CopyFrom(Originbuffer, 0, 0, Originbuffer.Length);

                    // Init output buffer
                    output.MemSetToZero();

                    // Launch buffer.Length many threads and pass a view to buffer
                    kernel(buffer.Length, buffer.View, output.View, img.Width, img.Height, 1);

                    // Wait for the kernel to finish...
                    accelerator.Synchronize();

                    // Resolve data
                    output.CopyTo(outputBuffer, 0, 0, Originbuffer.Length);

                    sw.Stop();
                    time += sw.ElapsedMilliseconds;
                    if (verbose)
                        Console.WriteLine("Image processed in " + sw.ElapsedMilliseconds + " ms");
                }
            }
            time /= count;
            if (verbose)
                Console.WriteLine("Average : " + time + " ms");
        }



    }

    internal static class GrayBMP
    {
        static byte[] BMP_File_Header = new byte[14];
        static byte[] DIB_header = new byte[40];
        static byte[] Color_palette = new byte[1024]; //a palette containing 256 colors
        static byte[] Bitmap_Data;

        //creates byte array of 256 color grayscale palette
        static byte[] create_palette()
        {
            byte[] color_palette = new byte[1024];
            for (int i = 0; i < 256; i++)
            {
                color_palette[i * 4 + 0] = (byte)(i); //bule
                color_palette[i * 4 + 1] = (byte)(i); //green
                color_palette[i * 4 + 2] = (byte)(i); //red
                color_palette[i * 4 + 3] = (byte)0; //padding
            }
            return color_palette;
        }
        //create different part of a bitmap file
        static void create_parts(byte[] buffer, int imageWidth, int imageHeight)
        {
            //Create Bitmap Data
            Bitmap_Data = buffer;
            //Create Bitmap File Header (populate BMP_File_Header array)
            Copy_to_Index(BMP_File_Header, new byte[] { (byte)'B', (byte)'M' }, 0); //magic number
            Copy_to_Index(BMP_File_Header, BitConverter.GetBytes(BMP_File_Header.Length
                            + DIB_header.Length + Color_palette.Length + Bitmap_Data.Length), 2); //file size
            Copy_to_Index(BMP_File_Header, new byte[] { (byte)'M', (byte)'C', (byte)'A', (byte)'T' }, 6); //reserved for application generating the bitmap file (not imprtant)
            Copy_to_Index(BMP_File_Header, BitConverter.GetBytes(BMP_File_Header.Length
                            + DIB_header.Length + Color_palette.Length), 10); //bitmap raw data offset
                                                                              //Create DIB Header (populate DIB_header array)
            Copy_to_Index(DIB_header, BitConverter.GetBytes(DIB_header.Length), 0); //DIB header length
            Copy_to_Index(DIB_header, BitConverter.GetBytes(imageWidth), 4); //image width
            Copy_to_Index(DIB_header, BitConverter.GetBytes(imageHeight), 8); //image height
            Copy_to_Index(DIB_header, new byte[] { (byte)1, (byte)0 }, 12); //color planes. N.B. Must be set to 1
            Copy_to_Index(DIB_header, new byte[] { (byte)8, (byte)0 }, 14); //bits per pixel
            Copy_to_Index(DIB_header, BitConverter.GetBytes(0), 16); //compression method N.B. BI_RGB = 0
            Copy_to_Index(DIB_header, BitConverter.GetBytes(Bitmap_Data.Length), 20); //lenght of raw bitmap data
            Copy_to_Index(DIB_header, BitConverter.GetBytes(1000), 24); //horizontal reselution N.B. not important
            Copy_to_Index(DIB_header, BitConverter.GetBytes(1000), 28); //vertical reselution N.B. not important
            Copy_to_Index(DIB_header, BitConverter.GetBytes(256), 32); //number of colors in the palette
            Copy_to_Index(DIB_header, BitConverter.GetBytes(0), 36); //number of important colors used N.B. 0 = all colors are imprtant
                                                                     //Create Color palette
            Color_palette = create_palette();
        }

        static public bool SaveAsGrayScale(byte[] buffer, int imageWidth, int imageHeight, string path)
        {
            try
            {
                create_parts(buffer, imageWidth, imageHeight);
                //Write to file
                FileStream oFileStream;
                oFileStream = new FileStream(path, System.IO.FileMode.OpenOrCreate);
                oFileStream.Write(BMP_File_Header, 0, BMP_File_Header.Length);
                oFileStream.Write(DIB_header, 0, DIB_header.Length);
                oFileStream.Write(Color_palette, 0, Color_palette.Length);
                oFileStream.Write(Bitmap_Data, 0, Bitmap_Data.Length);
                oFileStream.Close();
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        static public bool CreateGrayScaleBmp(byte[] buffer, int imageWidth, int imageHeight, out Bitmap bmp)
        {
            try
            {
                create_parts(buffer, imageWidth, imageHeight);
                //Write to file
                MemoryStream oFileStream;
                oFileStream = new MemoryStream();
                oFileStream.Write(BMP_File_Header, 0, BMP_File_Header.Length);
                oFileStream.Write(DIB_header, 0, DIB_header.Length);
                oFileStream.Write(Color_palette, 0, Color_palette.Length);
                oFileStream.Write(Bitmap_Data, 0, Bitmap_Data.Length);
                bmp = new Bitmap(oFileStream);
                oFileStream.Close();
                oFileStream.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                bmp = null;
                return false;
            }
        }

        //adds dtata of Source array to Destinition array at the Index
        static bool Copy_to_Index(byte[] destination, byte[] source, int index)
        {
            try
            {
                for (int i = 0; i < source.Length; i++)
                {
                    destination[i + index] = source[i];
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
