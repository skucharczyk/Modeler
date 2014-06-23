#pragma once
/*
 *  Point3d.h
 *  Raytracer
 *
 *  Created by Marcin on 11-03-02.
 *  Copyright 2011 __MyCompanyName__. All rights reserved.
 *
 */
#include <string>
#include <ostream>
#include <iostream>

namespace raytracer {
	class Point3d {
	public:
		float x, y, z;
		Point3d();
		Point3d(float, float, float);
		~Point3d();
		float getX();
		float getY();
		float getZ();
		void setX(float);
		void setY(float);
		void setZ(float);
		friend std::ostream& operator<<(std::ostream& os, const Point3d& p) 
		{
			return os << p.x << " " << p.y << " " << p.z << "\n";
		};
	};
}