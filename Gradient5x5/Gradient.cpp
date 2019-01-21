#include <math.h>
#include <ctime>
#include <vector>
#include <iterator>
#include <string>
#include <iostream>

__declspec(dllexport) void Test()
{
	std::cout << "hello" << std::endl;
}

__declspec(dllexport) unsigned char* Gradient5x5(unsigned char* imgIN, int imgWidth, int imgHeight, int factor)
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
