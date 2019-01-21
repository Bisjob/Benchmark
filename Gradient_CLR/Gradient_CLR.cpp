#include "stdafx.h"
#include <vector>
#include <array>
#include <math.h>
#include "Gradient_CLR.h"

using namespace System::Drawing;

GradientCLR::GradientCLR::GradientCLR() : disposed(false), native(new Native::Gradient)
{

}


GradientCLR::GradientCLR::~GradientCLR()
{
	this->!GradientCLR();
}

GradientCLR::GradientCLR::!GradientCLR()
{
	if (disposed) return;
	disposed = true;

	delete native;
}

Bitmap^ GradientCLR::GradientCLR::Gradient(Bitmap^ bmp, int factor)
{
	auto ptr = ImageToByte(bmp);

	auto outPtr = native->Gradient5x5(ptr, bmp->Width, bmp->Height, factor);

	auto res = gcnew Bitmap(bmp->Width, bmp->Height, Imaging::PixelFormat::Format8bppIndexed);
	auto resdata = res->LockBits(Rectangle(0, 0, res->Width, res->Height), Imaging::ImageLockMode::WriteOnly, res->PixelFormat);
	int size = res->Width * res->Height;
	unsigned char* resptr = reinterpret_cast<unsigned char*>(resdata->Scan0.ToPointer());
	for (size_t i = 0; i < res->Width; i++)
	{
		for (size_t j = 0; j < res->Height; j++, ptr++, outPtr++)
		{
			*resptr = *outPtr;
		}
	}

	res->UnlockBits(resdata);

	return res;
}


unsigned char* GradientCLR::GradientCLR::ImageToByte(Bitmap^ img)
{
	auto indata = img->LockBits(Rectangle(0, 0, img->Width, img->Height), Imaging::ImageLockMode::ReadOnly, img->PixelFormat);
	int size = img->Width * img->Height;

	std::vector<unsigned char> outdata(size);
	
	unsigned char* ptr = reinterpret_cast<unsigned char*>(indata->Scan0.ToPointer());

	for (size_t i = 0; i < img->Width; i++)
	{
		for (size_t j = 0; j < img->Height; j++, ptr++)
		{
			outdata[j * img->Width + i] = *ptr;
		}
	}


	img->UnlockBits(indata);

	return 0;
}
