// Cpp.cpp : Ce fichier contient la fonction 'main'. L'exécution du programme commence et se termine à cet endroit.
//

#include "pch.h"
#include <iostream>
#include <iterator>
#include <fstream>
#include <vector>
#include <string>
#include <math.h>
#include <ctime>

#include "bitmap_image.hpp"

unsigned char* readBMP(const char* filename, int* width, int* height)
{
	int i;
	FILE* f = fopen(filename, "rb");
	unsigned char info[54];
	fread(info, sizeof(unsigned char), 54, f); // read the 54-byte header

	// extract image height and width from header
	*width = *(int*)&info[18];
	*height = *(int*)&info[22];

	int size = 1 * *width * *height;
	unsigned char* data = new unsigned char[size]; // allocate 3 bytes per pixel
	fread(data, sizeof(unsigned char), size, f); // read the rest of the data at once
	fclose(f);

	for (i = 0; i < size; ++i)
	{
		unsigned char tmp = data[i];
		data[i] = data[i + 2];
		data[i + 2] = tmp;
	}

	return data;
}

void saveBMP(const char* filename, unsigned char* buffer, int width, int height)
{
	FILE *f = fopen(filename, "wb");
	unsigned char *img = NULL;
	int filesize = 54 + width * height; 

	//memset(buffer, 0, width * height);

	for (int i = 0; i < width; i++)
	{
		for (int j = 0; j < height; j++, ++buffer, ++img)
		{
			*img = *buffer;
		}
	}

	unsigned char bmpfileheader[14] = { 'B','M', 0,0,0,0, 0,0, 0,0, 54,0,0,0 };
	unsigned char bmpinfoheader[40] = { 40,0,0,0, 0,0,0,0, 0,0,0,0, 1,0, 24,0 };
	unsigned char bmppad[1] = { 0 };

	bmpfileheader[2] = (unsigned char)(filesize);
	bmpfileheader[3] = (unsigned char)(filesize >> 8);
	bmpfileheader[4] = (unsigned char)(filesize >> 16);
	bmpfileheader[5] = (unsigned char)(filesize >> 24);

	bmpinfoheader[4] = (unsigned char)(width);
	bmpinfoheader[5] = (unsigned char)(width >> 8);
	bmpinfoheader[6] = (unsigned char)(width >> 16);
	bmpinfoheader[7] = (unsigned char)(width >> 24);
	bmpinfoheader[8] = (unsigned char)(height);
	bmpinfoheader[9] = (unsigned char)(height >> 8);
	bmpinfoheader[10] = (unsigned char)(height >> 16);
	bmpinfoheader[11] = (unsigned char)(height >> 24);

	fwrite(bmpfileheader, 1, 14, f);
	fwrite(bmpinfoheader, 1, 40, f);
	for (int i = 0; i < height; i++)
	{
		fwrite(buffer + (width*(height - i - 1) * 3), 3, width, f);
		fwrite(bmppad, 1, (4 - (width * 3) % 4) % 4, f);
	}

	free(buffer);
	fclose(f);
}

unsigned char* Gradient5x5(unsigned char* imgIN, int imgWidth, int imgHeight, int factor)
{
	//Image OUT
	unsigned char* imgOut = new unsigned char[imgWidth * imgHeight];

	int maxValue = 0;

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

				unsigned char* ptrSrc = imgIN + y * imgWidth + x;
				unsigned char val0 = *ptrSrc;

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


				if (!OnBorderTop3)
				{
					//pxl 2
					ptrSrc = imgIN + (y - 1) * imgWidth + (x);
					pxlRes += std::abs(val0 - *ptrSrc);

					if (!OnBorderLeft3)
					{
						//if (cbx8Connect.Checked)
						{
							//pxl 1
							ptrSrc = imgIN + (y - 1) * imgWidth + (x - 1);
							pxlRes += std::abs(val0 - *ptrSrc);
						}
					}

					if (!OnBorderRight3)
					{
						//if (cbx8Connect.Checked)
						{
							//pxl 3
							ptrSrc = imgIN + (y - 1) * imgWidth + (x + 1);
							pxlRes += std::abs(val0 - *ptrSrc);
						}
					}
				}

				if (!OnBorderBot3)
				{
					//pxl 6
					ptrSrc = imgIN + (y + 1) * imgWidth + (x);
					pxlRes += std::abs(val0 - *ptrSrc);

					if (!OnBorderLeft3)
					{
						//if (cbx8Connect.Checked)
						{
							//pxl 7
							ptrSrc = imgIN + (y + 1) * imgWidth + (x - 1);
							pxlRes += std::abs(val0 - *ptrSrc);
						}
					}

					if (!OnBorderRight3)
					{
						//if (cbx8Connect.Checked)
						{
							//pxl 5
							ptrSrc = imgIN + (y + 1) * imgWidth + (x + 1);
							pxlRes += std::abs(val0 - *ptrSrc);
						}
					}
				}

				if (!OnBorderLeft3)
				{
					//pxl 8
					ptrSrc = imgIN + (y)* imgWidth + (x - 1);
					pxlRes += std::abs(val0 - *ptrSrc);
				}

				if (!OnBorderRight3)
				{
					//pxl 4
					ptrSrc = imgIN + (y)* imgWidth + (x + 1);
					pxlRes += std::abs(val0 - *ptrSrc);
				}

				if (!OnBorderTop5)
				{
					//pxl 10
					ptrSrc = imgIN + (y - 2) * imgWidth + (x - 1);
					pxlRes += std::abs(val0 - *ptrSrc);

					//pxl 11
					ptrSrc = imgIN + (y - 2) * imgWidth + (x);
					pxlRes += std::abs(val0 - *ptrSrc);

					//pxl 12
					ptrSrc = imgIN + (y - 2) * imgWidth + (x + 1);
					pxlRes += std::abs(val0 - *ptrSrc);

					if (!OnBorderLeft5)
					{
						//pxl 9
						ptrSrc = imgIN + (y - 2) * imgWidth + (x - 2);
						pxlRes += std::abs(val0 - *ptrSrc);
					}

					if (!OnBorderRight5)
					{
						//pxl 13
						ptrSrc = imgIN + (y - 2) * imgWidth + (x + 2);
						pxlRes += std::abs(val0 - *ptrSrc);
					}
				}

				if (!OnBorderBot5)
				{
					//pxl 20
					ptrSrc = imgIN + (y + 2) * imgWidth + (x - 1);
					pxlRes += std::abs(val0 - *ptrSrc);

					//pxl 19
					ptrSrc = imgIN + (y + 2) * imgWidth + (x);
					pxlRes += std::abs(val0 - *ptrSrc);

					//pxl 18
					ptrSrc = imgIN + (y + 2) * imgWidth + (x + 1);
					pxlRes += std::abs(val0 - *ptrSrc);

					if (!OnBorderLeft5)
					{
						//pxl 21
						ptrSrc = imgIN + (y + 2) * imgWidth + (x - 2);
						pxlRes += std::abs(val0 - *ptrSrc);
					}

					if (!OnBorderRight5)
					{
						//pxl 17
						ptrSrc = imgIN + (y + 2) * imgWidth + (x + 2);
						pxlRes += std::abs(val0 - *ptrSrc);
					}
				}

				if (!OnBorderLeft5)
				{
					if (!OnBorderTop3)
					{
						//pxl 24
						ptrSrc = imgIN + (y - 1) * imgWidth + (x - 2);
						pxlRes += std::abs(val0 - *ptrSrc);
					}

					//pxl 23
					ptrSrc = imgIN + (y)* imgWidth + (x - 2);
					pxlRes += std::abs(val0 - *ptrSrc);

					if (!OnBorderBot3)
					{
						//pxl 22
						ptrSrc = imgIN + (y + 1) * imgWidth + (x - 2);
						pxlRes += std::abs(val0 - *ptrSrc);
					}
				}

				if (!OnBorderRight5)
				{
					if (!OnBorderTop3)
					{
						//pxl 14
						ptrSrc = imgIN + (y - 1) * imgWidth + (x + 2);
						pxlRes += std::abs(val0 - *ptrSrc);
					}

					//pxl 15
					ptrSrc = imgIN + (y)* imgWidth + (x + 2);
					pxlRes += std::abs(val0 - *ptrSrc);

					if (!OnBorderBot3)
					{
						//pxl 16
						ptrSrc = imgIN + (y + 1) * imgWidth + (x + 2);
						pxlRes += std::abs(val0 - *ptrSrc);
					}
				}


				pxlRes *= factor;

				if (pxlRes > 255)
					pxlRes = 255;
				if (pxlRes < 0)
					pxlRes = 0;

				if (pxlRes > maxValue)
					maxValue = pxlRes;

				unsigned char* ptrDst = imgOut + y * imgWidth + x;
				*ptrDst = (unsigned char)pxlRes;

			}//End for X

		}//End for Y

		for (unsigned char* ptr = imgOut; ptr != imgOut + imgWidth * imgHeight; ++ptr)
			*ptr = (unsigned char)((*ptr) * (255 / maxValue));

	}//End fixed

	return imgOut;

}

int main(int argc, char* argv[])
{
	int count = 1;

	if (argc >= 2) 
	{
		std::string arg = argv[1];
		try 
		{
			std::size_t pos;
			int x = std::stoi(arg, &pos);
			if (pos < arg.size()) 
			{
				std::cerr << "Trailing characters after number: " << arg << '\n';
			}
			count = x;
		}
		catch (std::invalid_argument const &ex) 
		{
			std::cerr << "Invalid number: " << arg << '\n';
		}
		catch (std::out_of_range const &ex) 
		{
			std::cerr << "Number out of range: " << arg << '\n';
		}
	}


	std::cout << "========================================" << std::endl;
	std::cout << "===== C++ Benchmark : Gradient 5x5 =====" << std::endl;
	std::cout << "========================================" << std::endl;

	clock_t begin_time = clock();

	//int width = 0;
	//int height = 0;
	//auto data = readBMP("..\\..\\0.bmp", &width, &height);

	bitmap_image img("C:\\src\\_gistore\\BenchmarkCode\\0.bmp"); //"..\\..\\0.bmp"

	img.save_image("test.bmp");

	auto readTime = float(clock() - begin_time) / CLOCKS_PER_SEC * 1000;
	std::cout << "Image readed in " << readTime << " ms" << std::endl;
	if (count == 1)
	{
		begin_time = clock();
		auto res = Gradient5x5(img.data(), img.width(), img.height(), 1);
		auto processTime = float(clock() - begin_time) / CLOCKS_PER_SEC * 1000;
		std::cout << "Image processed in : " << processTime << " ms" << std::endl;
		
		begin_time = clock();
		bitmap_image resImg(res, img.width(), img.height(), 1);
		resImg.save_image("Cpp.bmp");
		auto saveTime = float(clock() - begin_time) / CLOCKS_PER_SEC * 1000;
		std::cout << "Image saaved in : " << processTime << " ms" << std::endl;

		auto total = readTime + processTime + saveTime;
		std::cout << "Total : " << total << " ms" << std::endl;
		return 0;
	}


	long time = 0;
	for (size_t i = 0; i < count; i++)
	{
		begin_time = clock();
		Gradient5x5(img.data(), img.width(), img.height(), 1);
		time += float(clock() - begin_time) / CLOCKS_PER_SEC * 1000;
		std::cout << "Image processed in : " << float(clock() - begin_time) / CLOCKS_PER_SEC * 1000 << " ms" << std::endl;
	}

	time /= count;
	std::cout << "Average : " << time << " ms" << std::endl;

	std::cout << "========================================" << std::endl;
	
}
