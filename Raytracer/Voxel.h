#pragma once
#include <list>
#include "Triangle.h"
#include "Point3d.h"
#include "Vector3d.h"

namespace raytracer {
	class Voxel
	{
	public:
		Point3d center;
		Triangle** trianList;
		bool empty;
		int trian_num;
		Voxel(void);
		~Voxel(void);
	};
}