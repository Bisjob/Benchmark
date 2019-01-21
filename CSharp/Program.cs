using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 1;
            if (args != null && args.Length > 0)
                int.TryParse(args[0], out count);

            Console.WriteLine("========================================");
            Console.WriteLine("=====  C# Benchmark : Gradient 5x5 =====");
            Console.WriteLine("========================================");

            Stopwatch sw = Stopwatch.StartNew();

            var img = new Bitmap(@"..\..\0.bmp");
            long readingTime = sw.ElapsedMilliseconds;
            Console.WriteLine("Image readed in " + readingTime + " ms");

            sw.Restart();
            var buffer = ImageToByte(img);
            long extractionTime = sw.ElapsedMilliseconds;
            Console.WriteLine("Buffer extracted in " + extractionTime + " ms");
            
            if (count == 1)
            {
                sw.Restart();
                var resdata = Gradient5x5(buffer, img.Width, img.Height, 1);
                long processTime = sw.ElapsedMilliseconds;
                Console.WriteLine("Image processed in " + processTime + " ms");

                sw.Restart();
                SaveImage(resdata, img.Width, img.Height, "CSharp_res.bmp");
                long saveTime = sw.ElapsedMilliseconds;
                Console.WriteLine("Image saved in " + saveTime + " ms");


                long total = readingTime + extractionTime + processTime + saveTime;
                Console.WriteLine("Total : " + total + " ms");

                return;
            }

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
            int padding = (4 - ((1 * width) % 4)) % 4;
            {
                byte[] tmp = new byte[width * height + padding * height];

                unsafe
                {
                    fixed (byte* ptrInF = buffer, ptrOutF = tmp)
                    {
                        byte* ptrOut = ptrOutF;
                        
                        for (int y = 0; y < height; ++y)
                        {
                            byte* rowIn = ptrInF + ((width + padding) * (height - y - 1));
                            for (int x = 0; x < width; x++)
                                *(ptrOut + x + y * (width + padding)) = *(rowIn++);
                        }
                    }
                }
                GrayBMP.SaveAsGrayScale(tmp, width, height, filename);
            }
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
