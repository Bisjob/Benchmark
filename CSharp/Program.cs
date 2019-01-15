using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 20;
            if (args != null && args.Length > 0)
                int.TryParse(args[0], out count);

            Console.WriteLine("========================================");
            Console.WriteLine("=====  C# Benchmark : Gradient 5x5 =====");
            Console.WriteLine("========================================");

            Stopwatch sw = Stopwatch.StartNew();

            var img = new Bitmap(@"..\..\0.bmp");
            Console.WriteLine("Image readed");

            var buffer = ImageToByte(img);
            sw.Stop();
            Console.WriteLine("Buffer extracted in " + sw.ElapsedMilliseconds + " ms");

            long time = 0;
            for (int i = 0; i < count; i++)
            {
                sw.Restart();
                Gradient5x5(buffer, img.Width, img.Height, 1);
                sw.Stop();
                time += sw.ElapsedMilliseconds;
                Console.WriteLine("Image processed in " + sw.ElapsedMilliseconds + " ms");
            }

            time /= count;
            Console.WriteLine("Average : " + time + " ms");

            Console.WriteLine("----------------------");
            Console.WriteLine("========================================");
        }

        static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
        
        static unsafe byte[] Gradient5x5(byte[] imgIN, int imgWidth, int imgHeight, int factor)
        {
            //Image OUT
            byte[] imgOut = new byte[imgWidth * imgHeight * 1];
            
            int maxValue = 0;

            fixed (byte* ptrSource = imgIN)
            fixed (byte* ptrDest = imgOut)
            {
                int x = 0;
                int y = 0;

                for (; y < imgHeight; ++y)
                {
                    x = 0;
                    for (; x < imgWidth; ++x)
                    {
                        //   pxl :
                        //
                        // |09|10|11|12|13|
                        // |24|01|02|03|14|
                        // |23|08|00|04|15|
                        // |22|07|06|05|16|
                        // |21|20|19|18|17|

                        int pxlRes = 0;

                        byte* ptrSrc = ptrSource + y * imgWidth + x;
                        byte val0 = *ptrSrc;

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
                            ptrSrc = ptrSource + (y - 1) * imgWidth + (x);
                            pxlRes += Math.Abs(val0 - *ptrSrc);

                            if (!OnBorderLeft3)
                            {
                                //if (cbx8Connect.Checked)
                                {
                                    //pxl 1
                                    ptrSrc = ptrSource + (y - 1) * imgWidth + (x - 1);
                                    pxlRes += Math.Abs(val0 - *ptrSrc);
                                }
                            }

                            if (!OnBorderRight3)
                            {
                                //if (cbx8Connect.Checked)
                                {
                                    //pxl 3
                                    ptrSrc = ptrSource + (y - 1) * imgWidth + (x + 1);
                                    pxlRes += Math.Abs(val0 - *ptrSrc);
                                }
                            }
                        }

                        if (!OnBorderBot3)
                        {
                            //pxl 6
                            ptrSrc = ptrSource + (y + 1) * imgWidth + (x);
                            pxlRes += Math.Abs(val0 - *ptrSrc);

                            if (!OnBorderLeft3)
                            {
                                //if (cbx8Connect.Checked)
                                {
                                    //pxl 7
                                    ptrSrc = ptrSource + (y + 1) * imgWidth + (x - 1);
                                    pxlRes += Math.Abs(val0 - *ptrSrc);
                                }
                            }

                            if (!OnBorderRight3)
                            {
                                //if (cbx8Connect.Checked)
                                {
                                    //pxl 5
                                    ptrSrc = ptrSource + (y + 1) * imgWidth + (x + 1);
                                    pxlRes += Math.Abs(val0 - *ptrSrc);
                                }
                            }
                        }

                        if (!OnBorderLeft3)
                        {
                            //pxl 8
                            ptrSrc = ptrSource + (y) * imgWidth + (x - 1);
                            pxlRes += Math.Abs(val0 - *ptrSrc);
                        }

                        if (!OnBorderRight3)
                        {
                            //pxl 4
                            ptrSrc = ptrSource + (y) * imgWidth + (x + 1);
                            pxlRes += Math.Abs(val0 - *ptrSrc);
                        }
                        #endregion noyeau 3x3

                        #region noyeau 5x5
                        if (!OnBorderTop5)
                        {
                            //pxl 10
                            ptrSrc = ptrSource + (y - 2) * imgWidth + (x - 1);
                            pxlRes += Math.Abs(val0 - *ptrSrc);

                            //pxl 11
                            ptrSrc = ptrSource + (y - 2) * imgWidth + (x);
                            pxlRes += Math.Abs(val0 - *ptrSrc);

                            //pxl 12
                            ptrSrc = ptrSource + (y - 2) * imgWidth + (x + 1);
                            pxlRes += Math.Abs(val0 - *ptrSrc);

                            if (!OnBorderLeft5)
                            {
                                //pxl 9
                                ptrSrc = ptrSource + (y - 2) * imgWidth + (x - 2);
                                pxlRes += Math.Abs(val0 - *ptrSrc);
                            }

                            if (!OnBorderRight5)
                            {
                                //pxl 13
                                ptrSrc = ptrSource + (y - 2) * imgWidth + (x + 2);
                                pxlRes += Math.Abs(val0 - *ptrSrc);
                            }
                        }

                        if (!OnBorderBot5)
                        {
                            //pxl 20
                            ptrSrc = ptrSource + (y + 2) * imgWidth + (x - 1);
                            pxlRes += Math.Abs(val0 - *ptrSrc);

                            //pxl 19
                            ptrSrc = ptrSource + (y + 2) * imgWidth + (x);
                            pxlRes += Math.Abs(val0 - *ptrSrc);

                            //pxl 18
                            ptrSrc = ptrSource + (y + 2) * imgWidth + (x + 1);
                            pxlRes += Math.Abs(val0 - *ptrSrc);

                            if (!OnBorderLeft5)
                            {
                                //pxl 21
                                ptrSrc = ptrSource + (y + 2) * imgWidth + (x - 2);
                                pxlRes += Math.Abs(val0 - *ptrSrc);
                            }

                            if (!OnBorderRight5)
                            {
                                //pxl 17
                                ptrSrc = ptrSource + (y + 2) * imgWidth + (x + 2);
                                pxlRes += Math.Abs(val0 - *ptrSrc);
                            }
                        }

                        if (!OnBorderLeft5)
                        {
                            if (!OnBorderTop3)
                            {
                                //pxl 24
                                ptrSrc = ptrSource + (y - 1) * imgWidth + (x - 2);
                                pxlRes += Math.Abs(val0 - *ptrSrc);
                            }

                            //pxl 23
                            ptrSrc = ptrSource + (y) * imgWidth + (x - 2);
                            pxlRes += Math.Abs(val0 - *ptrSrc);

                            if (!OnBorderBot3)
                            {
                                //pxl 22
                                ptrSrc = ptrSource + (y + 1) * imgWidth + (x - 2);
                                pxlRes += Math.Abs(val0 - *ptrSrc);
                            }
                        }

                        if (!OnBorderRight5)
                        {
                            if (!OnBorderTop3)
                            {
                                //pxl 14
                                ptrSrc = ptrSource + (y - 1) * imgWidth + (x + 2);
                                pxlRes += Math.Abs(val0 - *ptrSrc);
                            }

                            //pxl 15
                            ptrSrc = ptrSource + (y) * imgWidth + (x + 2);
                            pxlRes += Math.Abs(val0 - *ptrSrc);

                            if (!OnBorderBot3)
                            {
                                //pxl 16
                                ptrSrc = ptrSource + (y + 1) * imgWidth + (x + 2);
                                pxlRes += Math.Abs(val0 - *ptrSrc);
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

                        byte* ptrDst = ptrDest + y * imgWidth + x;
                        *ptrDst = (byte)pxlRes;

                    }//End for X

                }//End for Y

                for (byte* ptr = ptrDest; ptr != ptrDest + imgWidth * imgHeight; ++ptr)
                    *ptr = (byte)((*ptr) * (255 / maxValue));

            }//End fixed

            return imgOut;

        }
    }
}
