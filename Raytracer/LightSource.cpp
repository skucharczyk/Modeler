#include "LightSource.h"

namespace raytracer {

	LightSource::LightSource(void)
	{
	}

	LightSource::LightSource(float x, float y, float z, float flux, float r, float g, float b)
	{
		position = Point3d(x, y, z);
		this->flux = flux;
		this->r = r;
		this->g = g;
		this->b = b;
	}


	LightSource::~LightSource(void)
	{
	}
}