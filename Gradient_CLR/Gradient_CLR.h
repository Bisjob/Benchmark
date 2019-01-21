#pragma once

using namespace System;
using namespace System::Drawing;

namespace GradientCLR 
{
	public ref class GradientCLR
	{
	private:
		Native::Gradient* native;
		bool disposed;

	public:
		GradientCLR();
		~GradientCLR();
		!GradientCLR();
		Bitmap^ Gradient(Bitmap^ bmp, int factor);

		unsigned char* ImageToByte(Bitmap^ img);
	};
}
