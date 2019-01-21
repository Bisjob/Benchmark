#pragma once

namespace Native
{
	class Gradient
	{
	public:
		Gradient();
		~Gradient();
		unsigned char* Gradient5x5(unsigned char* ptrIn, int width, int height, int facor);
	};
}

