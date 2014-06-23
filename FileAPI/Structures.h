#ifndef STRUCTURES_H
#define STRUCTIRES_H

#include <vector>
#include <string>

using namespace std;

namespace loader {
	enum LightType { Point, Spot, Goniometric };

	class Vector3D
	{
	public:
		float x, y, z;

		Vector3D(float x, float y, float z)
		{
			this->x = x;
			this->y = y;
			this->z = z;
		}

		Vector3D()
		{
			x = y = z = 0;
		}

		Vector3D(const Vector3D& point)
		{
			x = point.x;
			y = point.y;
			z = point.z;
		}
	};

	class Triangle
	{
	public:
		unsigned p1, p2, p3; // indeksy punktów

		Triangle(unsigned p1 = 0, unsigned p2 = 0, unsigned p3 = 0)
		{
			this->p1 = p1;
			this->p2 = p2;
			this->p3 = p3;
		}

		Triangle(const Triangle& triangle)
		{
			p1 = triangle.p1;
			p2 = triangle.p2;
			p3 = triangle.p3;
		}
	};

	class Material
	{
	public:
		string name;
		float colorR, colorG, colorB;
		float kdcR, kdcG, kdcB;
		float kscR, kscG, kscB;
		float krcR, krcG, krcB;
		float kacR, kacG, kacB;
		float g;
		float n;

		Material(string name, float colorR, float colorG, float colorB, float kdcR, float kdcG, float kdcB, float kscR, float kscG,
		float kscB, float krcR, float krcG, float krcB, float kacR, float kacG, float kacB, float g, float n)
		{
			this->name = name;
			this->colorR = colorR;
			this->colorG = colorG;
			this->colorB = colorB;
			this->kdcR = kdcR;
			this->kdcG = kdcG;
			this->kdcB = kdcB;
			this->kscR = kscR;
			this->kscG = kscG;
			this->kscB = kscB;
			this->krcR = krcR;
			this->krcG = krcG;
			this->krcB = krcB;
			this->kacR = kacR;
			this->kacG = kacG;
			this->kacB = kacB;
			this->g = g;
			this->n = n;
		}

		Material (const Material& copy)
		{
			this->name = copy.name;
			this->colorR = copy.colorR;
			this->colorG = copy.colorG;
			this->colorB = copy.colorB;
			this->kdcR = copy.kdcR;
			this->kdcG = copy.kdcG;
			this->kdcB = copy.kdcB;
			this->kscR = copy.kscR;
			this->kscG = copy.kscG;
			this->kscB = copy.kscB;
			this->krcR = copy.krcR;
			this->krcG = copy.krcG;
			this->krcB = copy.krcB;
			this->kacR = copy.kacR;
			this->kacG = copy.kacG;
			this->kacB = copy.kacB;
			this->g = copy.g;
			this->n = copy.n;
		}
	};

	class Part
	{
	public:
		vector<int> triangles;

		Part(vector<int>& triangles)
		{
			this->triangles = triangles;
		}
	};

	class Light
	{
	public:
		 string name;
		 LightType type;
		 bool enabled;
		 float colorR, colorG, colorB;
		 float power;
		 Vector3D position;
		 Vector3D direction;
		 float innerAngle;
		 float outerAngle;
		 vector<pair<float, float> > goniometric;

		 Light()
		{
			name = "default";
			type = Spot;
			enabled = true;
			colorR = 1;
			colorG = 1;
			colorB = 1;
			power = 20;
			position = Vector3D(0, 0, 0);
			direction = Vector3D(1, 0, 0);
			innerAngle = 30;
			outerAngle = 60;
			goniometric.push_back(make_pair(0, 1));
			goniometric.push_back(make_pair(180, 1));
		}

		 Light(string name, LightType type, bool enabled, float r,
						float g, float b, float power, Vector3D& position)
		{
			this->name = name;
			this->type = type;
			this->enabled = enabled;
			colorR = r;
			colorG = g;
			colorB = b;
			this->power = power;
			this->position = position;
			direction = Vector3D(1, 0, 0);
			innerAngle = -1;
			outerAngle = -1;
			goniometric.push_back(make_pair(0, 1));
			goniometric.push_back(make_pair(180, 1));
		}

		 Light(string name, LightType type, bool enabled, float r,
						float g, float b, float power, Vector3D& position,
						Vector3D& direction, float innerAngle, float outerAngle,
						vector<pair<float, float> >& goniometric)
		{
			this->name = name;
			this->type = type;
			this->enabled = enabled;
			colorR = r;
			colorG = g;
			colorB = b;
			this->power = power;
			this->position = position;
			this->direction = direction;
			this->innerAngle = innerAngle;
			this->outerAngle = outerAngle;
			this->goniometric = goniometric;
		}

		 Light(const Light& copy)
		{
			this->name = copy.name;
			this->type = copy.type;
			this->enabled = copy.enabled;
			colorR = copy.colorR;
			colorG = copy.colorG;
			colorB = copy.colorB;
			this->power = copy.power;
			position = Vector3D(copy.position.x, copy.position.y, copy.position.z);
			direction = Vector3D(copy.direction.x, copy.direction.y, copy.direction.z);
			innerAngle = copy.innerAngle;
			outerAngle = copy.outerAngle;
			goniometric = copy.goniometric;
		}
	};

	class Camera
	{
	public:
		string name;
		int resolutionX, resolutionY;
		Vector3D position, lookAt;
		float fovAngle;
		float rotateAngle;

		Camera()
		{
			name = "";
			resolutionX = 800;
			resolutionY = 600;
			position = Vector3D(0, 0, 0);
			lookAt = Vector3D(1, 0, 0);
			fovAngle = 60;
			rotateAngle = 0; 
		}

		Camera(string name, int resolutionX, int resolutionY, Vector3D& position, Vector3D& lookAt, float fovAngle, float rotateAngle)
		{
			this->name = name;
			this->resolutionX = resolutionX;
			this->resolutionY = resolutionY;
			this->position = position;
			this->lookAt = lookAt;
			this->fovAngle = fovAngle;
			this->rotateAngle = rotateAngle;
		}

		Camera(const Camera& copy)
		{
			this->name = copy.name;
			this->resolutionX = copy.resolutionX;
			this->resolutionY = copy.resolutionY;
			this->position = copy.position;
			this->lookAt = copy.lookAt;
			this->fovAngle = copy.fovAngle;
			this->rotateAngle = copy.rotateAngle;
		}
	};
}
	

#endif