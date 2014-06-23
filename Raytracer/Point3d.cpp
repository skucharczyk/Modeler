#include "Point3d.h"


#include "Point3d.h"
#include <iostream>
#include <sstream>

namespace raytracer {

	Point3d::Point3d() {
		x = y = z = -1;
	}

	Point3d::Point3d(float x, float y, float z) {
		this->x = x;
		this->y = y;
		this->z = z;
	}

	Point3d::~Point3d() {
	}

	float Point3d::getX() {
		return x;
	}

	float Point3d::getY() {
		return y;
	}

	float Point3d::getZ() {
		return z;
	}
}
