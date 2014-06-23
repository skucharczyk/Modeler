#pragma once
/*
 *  Scene.h
 *  Raytracer
 *
 *  Created by Marcin on 11-03-03.
 *  Copyright 2011 __MyCompanyName__. All rights reserved.
 *
 */
#include <string>
#include <vector>
#include <Windows.h>
#include "Triangle.h"
#include "Camera.h"
#include "LightSource.h"
#include "Attrib.h"
#include "Voxel.h"

#define TILE_SIZE 36

const int MAX_DEPTH = 5;
namespace raytracer {
	typedef struct Color  
	{
		float red;
		float green;
		float blue;
		Color operator+(Color &other) {
			Color col;
			col.red = red+other.red;
			col.green = green+other.green;
			col.blue = blue+other.blue;
			return col;
		};
		Color operator*(float number) {
			Color col;
			col.red = red*number;
			col.green = green*number;
			col.blue = blue*number;
			return col;
		};
	} Color;

	class Scene {
	//private:
	public:
		std::vector<Point3d> vertices;
		std::vector<Point3d> trianVertIdx;
		Triangle* triangles;
		Camera camera;
		LightSource* lights;
		Attrib* attributes;
		Voxel*** voxels;
		Vector3d voxel_half_legths;
		float** zbuffer;
		int trianSize;
		int lightNumber;
		int attributesNuber;
		int mx, my, mz;
		int x_tiles, y_tiles;
		int tile_x_cntr, tile_y_cntr;
		float dx, dy, dz;
		float x_max_domain, y_max_domain, z_max_domain;
		float x_min_domain, y_min_domain, z_min_domain;
		float domain_height, domain_width, domain_depth;
		bool zbuf_flag;
		long facePlane(const Point3d&);
		long bevel2d(const Point3d&);
		long bevel3d(const Point3d&);
		long checkLine(const Point3d&, const Point3d&, long);
		long checkPoint(const Point3d&, const Point3d&, float, long);
	//public:
		Scene(Triangle*, Camera);
		Scene();
		~Scene();
		void readGeometry(std::string);
		void readCamera(std::string);
		void readAttributes(std::string);
		void readLighting(std::string);
		void readAttributesAlt(std::string);
		bool loadFromSceneLoader(std::string path);
		void saveWithNewFormat(std::string path);
		void voxelize();
		void print(void);
		void printLight(void);
		void printAttributes(void);
		//void render(std::string, bool, float, float);
		bool renderTile(BYTE* buffer, int psw);
		void render(BYTE* buffer, int psw);
		bool triangleVoxelIntersection(const Triangle* const, const Voxel* const);
		float isVisibleSub(Point3d&, Vector3d&, float, int);
		Color calculateColorSub(Point3d&, Vector3d&, int, int, float&);
		BYTE* convertRGBToBMPBuffer(BYTE*, int, int, long*);
		static bool saveBMP(BYTE*, int, int, long, LPCTSTR);
		void printMinMax(void);
	private:
		void calcCoordsImp(float x, float y, Point3d pos, Point3d lookAt,
						   Vector3d& outUpLeftFst, Vector3d& outUpRightFst, 
						   Vector3d& outLoLeftFst, Vector3d& outLoRightFst);
		void rotatePoint(float x, float y, float xRes, float yRes, float rectSizeX, float rectSizeY,
						 float angle, Vector3d& upLeftFst, Vector3d& upRightFst, 
						 Vector3d& loLeftFst, Vector3d& loRightFst, Point3d& outPoint);
	};
}