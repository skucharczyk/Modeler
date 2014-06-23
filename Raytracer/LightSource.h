#pragma once
#include "Point3d.h"

namespace raytracer {
	class LightSource
	{
	private:

	public:
		Point3d position;
		float flux;
		float r, g, b;
		LightSource(void);
		LightSource(float, float, float, float, float, float, float);
		~LightSource(void);
		friend std::ostream& operator<<(std::ostream& os, const LightSource& l) 
		{
			return os << l.position << l.flux << " " << l.r << " " << l.g << " " << l.b << "\n";
		};
	};
}