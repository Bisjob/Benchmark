
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
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

        static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        static void Main(string[] args)
        {
            int count = 20;
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

            var img = new Bitmap(@"C:\src\0-R&D\Multiflash\multiflash\Test_data\1\0.bmp");
            Console.WriteLine("Image readed");
            
            sw.Stop();
            Console.WriteLine("Buffer extracted in " + sw.ElapsedMilliseconds + " ms");

            var data = ImageToByte(img);
            int size = img.Width * img.Height;
            int entete = data.Length - size;
            entete = Math.Max(entete, 0);

            var Originbuffer = data.Skip<byte>(entete).Take<byte>(size).Select(v => (int)v).ToArray<int>();
            var outputBuffer = new int[Originbuffer.Length];

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
                                RunKernel(count, img, Originbuffer, outputBuffer, context, accelerator);
                        }
                        break;
                    case "cpu":
                        using (var accelerator = new CPUAccelerator(context))
                            RunKernel(count, img, Originbuffer, outputBuffer, context, accelerator);
                        break;
                    case "gpu":
                    case "cuda":
                        using (var accelerator = new CudaAccelerator(context))
                            RunKernel(count, img, Originbuffer, outputBuffer, context, accelerator);
                        break;
                }
                
            }
        }

        private static void RunKernel(int count, Bitmap img, int[] Originbuffer, int[] outputBuffer, Context context, Accelerator accelerator)
        {
            Stopwatch sw = Stopwatch.StartNew();
            long time = 0;
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
                    Console.WriteLine("Image processed in " + sw.ElapsedMilliseconds + " ms");
                }
            }
            time /= count;
            Console.WriteLine("Average : " + time + " ms");
            Console.WriteLine("----------------------");
        }



    }
}
