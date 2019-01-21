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

unsafe class Program
{
    [DllImport("Gradient5x5", EntryPoint = "?Gradient5x5@@YAPEAEPEAEHHH@Z")]
    static extern IntPtr Gradient5x5(IntPtr imgIN, int width, int height, int factor);

    [DllImport("Gradient5x5", EntryPoint = "?Test@@YAXXZ")]
    static extern IntPtr Test();

    static void Main(string[] args)
    {
        int count = 1;
        if (args != null && args.Length > 0)
            int.TryParse(args[0], out count);

        Test();

        Console.WriteLine("========================================");
        Console.WriteLine("===  C# DLL Benchmark : Gradient 5x5 ===");
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
            IntPtr result;
            fixed (byte* ptr = buffer)
            {
                result = Gradient5x5(new IntPtr((void*)ptr), img.Width, img.Height, 1);
            }

            long processTime = sw.ElapsedMilliseconds;
            Console.WriteLine("Image processed in " + processTime + " ms");

            sw.Restart();
            SaveImage(result, img.Width, img.Height, "CSharp_DLL_res.bmp");
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


            fixed (byte* ptr = buffer)
            {
                Gradient5x5(new IntPtr((void*)ptr), img.Width, img.Height, 1);
            }
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

    static void SaveImage(IntPtr buffer, int width, int height, string filename)
    {
        int padding = (width % 4) != 0 ? 4 - (width % 4) : 0; //determine padding needed for bitmap file
                                                              //if (padding != 0 || selection.Width != this.width || selection.Height != this.height)
        {
            byte[] tempBuffer = new byte[width * height + padding * height];

            unsafe
            {
                fixed (byte* ptrOutF = tempBuffer)
                {
                    byte* ptrIn = (byte*)buffer.ToPointer();
                    byte* ptrOut = ptrOutF;
                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                            *(ptrOut + (height - 1 - y) * width + (height - 1 - y) * padding + x) = *(ptrIn + x + (y) * width);
                }
            }

            GrayBMP.SaveAsGrayScale(tempBuffer, width, height, filename);
        }
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
