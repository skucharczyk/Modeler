
#include "Scene.h"
#include "Vector3d.h"
#include "SceneLoader.h"
#include "File.h"
#include <iostream>
#include <sstream>
#include <fstream>
#include <limits>
#include <stdlib.h>
#include <time.h>
#include <ppl.h>
#include <map>
#include <math.h>
#include <algorithm>

#define EPS 10e-5
#define EPS1 10e-3
#define PI 3.14159265

//#define PARALLEL
//#define DOF
#define LOOP

using namespace std;
using namespace loader;
namespace raytracer {

	Scene::Scene() {
		zbuf_flag = false;
		x_tiles = 0;
		y_tiles = 0;
		tile_x_cntr = 0;
		tile_y_cntr = 0;
		camera = Camera();
	}

	Scene::~Scene() {
		if (!triangles)
			delete[] triangles;
		if (!lights)
			delete[] lights;
		if (!attributes)
			delete[] attributes;

		if (zbuf_flag)
		{
			for (int i=0; i<camera.xResolution; i++) {
				delete[] zbuffer[i];
			}
			delete[] zbuffer;
		}
		
		if (!voxels) {
			for (int i=0; i<mx; i++)
			{
				for (int j=0; j<my; j++)
				{
					delete[] voxels[i][j];
				}
				delete[] voxels[i];
			}
			delete[] voxels;
		}
	}

	void Scene::readGeometry(string filePath) {
		int vertSize = NULL;
		ifstream file;
		string line, token;
		stringstream lineStream;
		//vector<Point3d> vertices;
		x_min_domain = y_min_domain = z_min_domain = numeric_limits<float>::infinity();
		x_max_domain = y_max_domain = z_max_domain = - numeric_limits<float>::infinity();
	
		file.open(filePath.c_str());

		if (file.is_open()) {
		
			while (!file.eof()) {
			
				getline(file, line);
				lineStream.clear();
				lineStream.str(line);
			
				while (lineStream >> token) {
				
					if (token == "vertices" && !lineStream.eof()) {
						lineStream >> vertSize;
						vertices = vector<Point3d>(vertSize);
					
						for (int i=0; i<vertSize; i++) {
							getline(file, line);
							lineStream.clear();
							lineStream.str(line);
						
							float t1, t2, t3;
							lineStream >> t1;
							lineStream >> t2;
							lineStream >> t3;

							x_min_domain = t1<x_min_domain?t1:x_min_domain;
							y_min_domain = t2<y_min_domain?t2:y_min_domain;
							z_min_domain = t3<z_min_domain?t3:z_min_domain;

							x_max_domain = t1>x_max_domain?t1:x_max_domain;
							y_max_domain = t2>y_max_domain?t2:y_max_domain;
							z_max_domain = t3>z_max_domain?t3:z_max_domain;
						
							vertices[i] = Point3d(t1, t2, t3);
						}
					}
				
					if (token == "triangles" && !lineStream.eof()) {
						lineStream >> trianSize;
						trianVertIdx = vector<Point3d>(trianSize);
						triangles = new Triangle [trianSize];
					
						for (int i=0; i<trianSize; i++) {
							getline(file, line);
							lineStream.clear();
							lineStream.str(line);
							int v1, v2, v3;
							lineStream >> v1;
							lineStream >> v2;
							lineStream >> v3;

							trianVertIdx[i] = Point3d(v1, v2, v3);
						
							triangles[i] = Triangle(vertices[v1], vertices[v2], vertices[v3], i);
						}
					}

					if (token == "parts" && !lineStream.eof()) {

						int i=0;
						int ind;

						while (i<trianSize) {
							getline(file, line);
							lineStream.clear();
							lineStream.str(line);

							while (lineStream >> ind) {
								triangles[i].setAtributeIndex(ind);
								i++;
							}
						}
					}
				}			
			}
		}
		file.close();

		domain_depth = z_max_domain - z_min_domain;
		domain_height = y_max_domain - y_min_domain;
		domain_width = x_max_domain - x_min_domain;
	}

	void Scene::readCamera(string filePath) {
		ifstream file;
		string line, token;
		stringstream lineStream;
		Point3d cameraCenter, topLeft, bottomLeft, topRight;
		int xRes=-1, yRes=-1;

		file.open(filePath.c_str());

		if (file.is_open()) {
			while(!file.eof()) {

				getline(file, line);
				lineStream.clear();
				lineStream.str(line);
				while (lineStream >> token) {

					if (token == "viewpoint")
					{
						float x, y, z;
						lineStream >> x;
						lineStream >> y;
						lineStream >> z;
						cameraCenter = Point3d(x, y, z);
					}

					if (token == "screen")
					{
						getline(file, line);
						lineStream.clear();
						lineStream.str(line);
						float x, y, z;
						lineStream >> x;
						lineStream >> y;
						lineStream >> z;
						topLeft = Point3d(x, y, z);

						getline(file, line);
						lineStream.clear();
						lineStream.str(line);
						lineStream >> x;
						lineStream >> y;
						lineStream >> z;
						topRight = Point3d(x, y, z);

						getline(file, line);
						lineStream.clear();
						lineStream.str(line);
						lineStream >> x;
						lineStream >> y;
						lineStream >> z;
						bottomLeft = Point3d(x, y, z);
					}

					if (token == "resolution")
					{
						lineStream >> xRes;
						lineStream >> yRes;
					}
				}
			}
		}
		file.close();
		camera = Camera(cameraCenter, topLeft, bottomLeft, topRight, xRes, yRes);
	}

	void Scene::readLighting(string filePath)
	{
		ifstream file;
		string line, token;
		stringstream lineStream;
		float x, y, z, flux, r, g, b;

		file.open(filePath.c_str());

		if (file.is_open()) {
			while(!file.eof()) {

				getline(file, line);
				lineStream.clear();
				lineStream.str(line);
				while(lineStream >> token) {

					if (token == "lights")
					{
						getline(file, line);
						lineStream.clear();
						lineStream.str(line);

						lineStream >> lightNumber;
						lights = new LightSource[lightNumber];
					}

					if (token == "Position")
					{
						for (int i=0; i<lightNumber; i++)
						{
							getline(file, line);
							lineStream.clear();
							lineStream.str(line);

							lineStream >> x;
							lineStream >> y;
							lineStream >> z;
							lineStream >> flux;
							lineStream >> r;
							lineStream >> g;
							lineStream >> b;

							lights[i] = LightSource(x, y, z, flux, r, g, b);
						}
					}
				}
			}
		}
		file.close();
	}

	void Scene::readAttributes(string filePath) {
		ifstream file;
		string line, token;
		stringstream lineStream;

		float kd, ks, wg, ka, r, g, b, kt, eta, kr;

		file.open(filePath.c_str());

		if (file.is_open())
		{
			while (!file.eof())
			{
				getline(file, line);
				lineStream.clear();
				lineStream.str(line);
				while (lineStream >> token)
				{
					if (token == "attribute")
					{
						getline(file, line);
						lineStream.clear();
						lineStream.str(line);

						lineStream >> attributesNuber;

						attributes = new Attrib[attributesNuber];
					}

					if (token == "attributes")
					{
						for (int i=0; i<attributesNuber; i++) {
							getline(file, line);
							lineStream.clear();
							lineStream.str(line);

							lineStream >> kd; lineStream >> ks; lineStream >> wg; lineStream >> ka;
							lineStream >> r; lineStream >> g; lineStream >> b;

							getline(file, line);
							lineStream.clear();
							lineStream.str(line);

							lineStream >> kt; lineStream >> eta; lineStream >> kr;

							attributes[i] = Attrib(kd, ks, wg, ka, r, g, b, kt, eta, kr);
							attributes[i].kdcG = kd;
							attributes[i].kdcG = kd;

							attributes[i].kscG = ks;
							attributes[i].kscB = ks;

							attributes[i].krcG = kt;
							attributes[i].krcB = kt;

						}
					}
				}
			}
		}
		file.close();
	}

	void Scene::readAttributesAlt(string filePath) {
		ifstream file;
		string line, token;
		stringstream lineStream;

		float kd, ks, wg, ka, r, g, b, kt, eta, kr;

		file.open(filePath.c_str());

		if (file.is_open())
		{
			getline(file, line);
			lineStream.clear();
			lineStream.str(line);

			lineStream >> attributesNuber;
			attributes = new Attrib[attributesNuber];
			getline(file, line);
			while(!file.eof())
			{
				for (int i=0; i<attributesNuber; i++) {
					getline(file, line);
					getline(file, line);
					getline(file, line);
					lineStream.clear();
					lineStream.str(line);
					while(lineStream>>token) {
							   if (token=="kd")		{
							lineStream >> kd;
						} else if (token=="ks")		{
							lineStream >> ks;
						} else if (token=="gs")		{
							lineStream >> wg;
						} else if (token=="color")	{
							lineStream >> r;
							lineStream >> g;
							lineStream >> b;
						} else if (token=="kts")	{
							lineStream >> kt;
						} else if (token=="eta")	{
							lineStream >> eta;
						} else if (token=="kf")		{
							lineStream >> kr;
						} else if (token=="ka")		{
							lineStream >> ka;
						} else if (token=="enddef") {
							attributes[i] = Attrib(kd, ks, wg, ka, r, g, b, kt, eta, kr);
							attributes[i].kdcG = kd;
							attributes[i].kdcB = kd;

							attributes[i].kscG = ks;
							attributes[i].kscB = ks;

							attributes[i].krcG = kt;
							attributes[i].krcB = kt;
							break;
						}
						getline(file, line);
						lineStream.clear();
						lineStream.str(line);
					}
					getline(file, line);
				}
				break;
			}
		}
		file.close();
	}

	bool Scene::loadFromSceneLoader(string filePath) {
		vector<loader::Vector3D> outPoints;
		vector<loader::Triangle> outTriangles;
		vector<loader::Part> outParts;
		vector<loader::Material> outMaterials;
		vector<string> matAssigns;
		vector<loader::Light> outLights;
		unsigned activeCamera;
		vector<loader::Camera> outCams;

		if (!LoadScene(filePath, outPoints, outTriangles, outParts, outMaterials, matAssigns, outLights, activeCamera, outCams))
			return false;

		
		x_min_domain = y_min_domain = z_min_domain = numeric_limits<float>::infinity();
		x_max_domain = y_max_domain = z_max_domain = - numeric_limits<float>::infinity();

		for (int i = 0; i < outPoints.size(); i++) {
			x_min_domain = outPoints[i].x < x_min_domain ? outPoints[i].x : x_min_domain;
			y_min_domain = outPoints[i].y < y_min_domain ? outPoints[i].y : y_min_domain;			
			z_min_domain = outPoints[i].z < z_min_domain ? outPoints[i].z : z_min_domain;
			
			x_max_domain = outPoints[i].x > x_max_domain ? outPoints[i].x : x_max_domain;
			y_max_domain = outPoints[i].y > y_max_domain ? outPoints[i].y : y_max_domain;			
			z_max_domain = outPoints[i].z > z_max_domain ? outPoints[i].z : z_max_domain;

		}

		trianSize = outTriangles.size();
		triangles = new Triangle[trianSize];
		Point3d p1, p2, p3;
		for (int i = 0; i < trianSize; i++) {
			p1.x = outPoints[outTriangles[i].p1].x;
			p1.y = outPoints[outTriangles[i].p1].y;
			p1.z = outPoints[outTriangles[i].p1].z;
			
			p2.x = outPoints[outTriangles[i].p2].x;
			p2.y = outPoints[outTriangles[i].p2].y;
			p2.z = outPoints[outTriangles[i].p2].z;
			
			p3.x = outPoints[outTriangles[i].p3].x;
			p3.y = outPoints[outTriangles[i].p3].y;
			p3.z = outPoints[outTriangles[i].p3].z;

			triangles[i] = Triangle(p1, p2, p3, i);
		}

		map<string, int> mat_names = map<string, int>();
		for (int i = 0; i < outMaterials.size(); i++) {
			//mat_names.insert(pair<string, int>(outMaterials[i].name, i));
			mat_names[outMaterials[i].name] = i;
		}

		int *tr_parts = new int [outTriangles.size()];
		for (int i = 0; i < outParts.size(); i++) {
			for (int j = 0; j < outParts[i].triangles.size(); j++) {
				tr_parts[outParts[i].triangles[j]] = i;
			}
		}

		for (int i = 0; i < trianSize; i++) {
			//if (tr_parts[i]<outMaterials.size()) {
				triangles[i].atributeIndex = mat_names[matAssigns[tr_parts[i]]];
			/*} else {
				cout << "błąd\n";
			}*/
		}

		domain_depth = z_max_domain - z_min_domain;
		domain_height = y_max_domain - y_min_domain;
		domain_width = x_max_domain - x_min_domain;

		lightNumber = outLights.size();
		lights = new LightSource[outLights.size()];
		for (int i = 0; i < outLights.size(); i++) {
			lights[i] = LightSource(outLights[i].position.x,
									outLights[i].position.y,
									outLights[i].position.z,
									outLights[i].power,
									outLights[i].colorR,
									outLights[i].colorG,
									outLights[i].colorB);
		}

		//Camera newCam = Camera();

		camera.cameraCenter.x = outCams[activeCamera].position.x;
		camera.cameraCenter.y = outCams[activeCamera].position.y;
		camera.cameraCenter.z = outCams[activeCamera].position.z;

		camera.lookAt.x = outCams[activeCamera].lookAt.x;
		camera.lookAt.y = outCams[activeCamera].lookAt.y;
		camera.lookAt.z = outCams[activeCamera].lookAt.z;

		camera.fovX = outCams[activeCamera].fovAngle;
		camera.rotation = outCams[activeCamera].rotateAngle;

		camera.xResolution = outCams[activeCamera].resolutionX;
		camera.yResolution = outCams[activeCamera].resolutionY;

		//float angle = 2.0f * (float)Math.Atan(Math.Tan(fovAngle / 2.0f) / aspect);

		//camera = raytracer::Camera(newCam);

		float radFovX = camera.fovX * PI / 180;
		float aspect = (float)camera.xResolution / camera.yResolution;
		float radFovY = 2.0f * (atan((tan((radFovX / 2.0f))) / aspect));

		camera.fovY = radFovY * 180 / PI;

		Point3d oldLookAt = camera.lookAt;

		camera.cameraCenter.x -= camera.lookAt.x;
		camera.cameraCenter.y -= camera.lookAt.y;
		camera.cameraCenter.z -= camera.lookAt.z;

		camera.lookAt.x -= camera.lookAt.x;
		camera.lookAt.y -= camera.lookAt.y;
		camera.lookAt.z -= camera.lookAt.z;

		Vector3d dir = Vector3d(camera.cameraCenter, camera.lookAt, false);
		float dir_lngth = dir.length();
		float rect_size_x = 2.0 * dir_lngth * tan(radFovX / 2);
		float rect_size_y = (float)(rect_size_x * camera.yResolution) / camera.xResolution;

		Vector3d upFstLeft, upFstRight, loFstLeft, loFstRight;
		calcCoordsImp(rect_size_x, rect_size_y, camera.cameraCenter, camera.lookAt, upFstLeft, upFstRight, loFstLeft, loFstRight);
		rotatePoint(0, 0, camera.xResolution, camera.yResolution, rect_size_x, rect_size_y, camera.rotation, 
					upFstLeft, upFstRight, loFstLeft, loFstRight, camera.topLeft);
		rotatePoint(camera.xResolution, 0, camera.xResolution, camera.yResolution, rect_size_x, rect_size_y, camera.rotation, 
					upFstLeft, upFstRight, loFstLeft, loFstRight, camera.topRight);
		rotatePoint(0, camera.yResolution, camera.xResolution, camera.yResolution, rect_size_x, rect_size_y, camera.rotation, 
					upFstLeft, upFstRight, loFstLeft, loFstRight, camera.bottomLeft);

		camera.cameraCenter.x += oldLookAt.x;
		camera.cameraCenter.y += oldLookAt.y;
		camera.cameraCenter.z += oldLookAt.z;

		camera.lookAt.x += oldLookAt.x;
		camera.lookAt.y += oldLookAt.y;
		camera.lookAt.z += oldLookAt.z;

		camera.topLeft.x += camera.lookAt.x;
		camera.topLeft.y += camera.lookAt.y;
		camera.topLeft.z += camera.lookAt.z;

		camera.topRight.x += camera.lookAt.x;
		camera.topRight.y += camera.lookAt.y;
		camera.topRight.z += camera.lookAt.z;

		camera.bottomLeft.x += camera.lookAt.x;
		camera.bottomLeft.y += camera.lookAt.y;
		camera.bottomLeft.z += camera.lookAt.z;

		
		//camera = Camera(newCam.cameraCenter, newCam.topLeft, newCam.bottomLeft, newCam.topRight, newCam.xResolution, newCam.yResolution);

		/*camera.topLeft = Point3d(8.623569f, 10.19726f, 17.23943f);
		camera.topRight = Point3d(10.02134f, 10.19726f,  16.8144f);
		camera.bottomLeft = Point3d(8.729696f, 9.433238f, 17.58845f);*/
		//camera.topLeft = Point3d(3.849229, 2.096238, 4.665725);
		//camera.topRight = Point3d(4.665725, 2.096238, 3.849229);
		//camera.bottomLeft = Point3d(3.95778, 1.44493, 4.774277);
		attributes = new Attrib[outMaterials.size()];

		attributesNuber = outMaterials.size();
		for (int i = 0; i < attributesNuber; i++) {
			attributes[i] = Attrib();
			attributes[i].r = outMaterials[i].colorR * 255;
			attributes[i].g = outMaterials[i].colorG * 255;
			attributes[i].b = outMaterials[i].colorB * 255;

			attributes[i].kdcR = outMaterials[i].kdcR;
			attributes[i].kdcG = outMaterials[i].kdcG;
			attributes[i].kdcB = outMaterials[i].kdcB;

			attributes[i].kscR = outMaterials[i].kscR;
			attributes[i].kscG = outMaterials[i].kscG;
			attributes[i].kscB = outMaterials[i].kscB;

			attributes[i].krcR = outMaterials[i].krcR;
			attributes[i].krcG = outMaterials[i].krcG;
			attributes[i].krcB = outMaterials[i].krcB;

			attributes[i].kacR = outMaterials[i].kacR;
			attributes[i].kacG = outMaterials[i].kacG;
			attributes[i].kacB = outMaterials[i].kacB;

			attributes[i].wg = outMaterials[i].g;
			attributes[i].eta = outMaterials[i].n;
		}

		return true;
	}

	void Scene::saveWithNewFormat(string filePath)
	{
		vector<string> text;
		stringstream sstr;
		text.push_back("");
		text.push_back("// punkty");
		sstr << vertices.size();
		text.push_back("points_count " + sstr.str());
		sstr.str("");
		string x, y, z;
		for (int i = 0; i< vertices.size(); i++) {
			sstr << vertices[i].z;
			x = sstr.str();
			sstr.str("");
			sstr << vertices[i].y;
			y = sstr.str();
			sstr.str("");
			sstr << vertices[i].x;
			z = sstr.str();
			sstr.str("");
			text.push_back(x + " " + y + " " + z);
		}
		text.push_back("");
		text.push_back("");

		text.push_back("// trojkaty");
		sstr << trianSize;
		text.push_back("triangles_count " + sstr.str());
		sstr.str("");
		for (int i = 0; i < trianSize; i++) {			
			sstr << trianVertIdx[i].x;
			x = sstr.str();
			sstr.str("");
			sstr << trianVertIdx[i].y;
			y = sstr.str();
			sstr.str("");
			sstr << trianVertIdx[i].z;
			z = sstr.str();
			sstr.str("");
			text.push_back(x + " " + y + " " + z);
		}

		vector<vector<int>*> wat;
		wat.resize(attributesNuber);
		for (int i = 0; i < wat.size(); i++) {
			wat[i] = new vector<int>();
		}
		for (int i = 0; i < trianSize; i++) {
			wat[triangles[i].atributeIndex]->push_back(i);
		}

		int emptyPartsCount = 0;
		vector<int> emptyMatListIdx;
		for (int i = 0; i < wat.size(); i++){
			if (wat[i]->size() == 0) {
				emptyPartsCount++;
				emptyMatListIdx.push_back(i);
			}
		}


		text.push_back("// czesci");
		sstr << (attributesNuber - emptyPartsCount);
		text.push_back("parts_count " + sstr.str());
		sstr.str("");
		
		int offset = 0;
		string parts;
		for (int i = 0; i < trianSize; i++) {
			offset = 0;
			for (int j = 0; j < emptyMatListIdx.size(); j++) {
				if (triangles[i].atributeIndex > emptyMatListIdx[j] - offset) {
					triangles[i].atributeIndex--;
					offset++;
				}
				else {
					break;
				}
			}
			sstr << triangles[i].atributeIndex;
			parts.append(sstr.str()).append(" ");
			sstr.str("");
		}
		text.push_back(parts);

		text.push_back("");
		text.push_back("");

		text.push_back("// materialy");
		sstr << (attributesNuber - emptyPartsCount);
		text.push_back("materials_count " + sstr.str());
		sstr.str("");
		offset = 0;
		for (int i = 0; i < attributesNuber - emptyPartsCount; i++) {
			if (find(emptyMatListIdx.begin() + offset, emptyMatListIdx.end(), i) != emptyMatListIdx.end()) {
				++offset;
				--i;
				continue;
			}
			text.push_back("");

			sstr << i;
			text.push_back("mat_name mat" + sstr.str());
			sstr.str("");

			sstr << ((float)attributes[i].r / 255);
			x = sstr.str();
			sstr.str("");
			sstr << ((float)attributes[i].g / 255);
			y = sstr.str();
			sstr.str("");
			sstr << ((float)attributes[i].b / 255);
			z = sstr.str();
			sstr.str("");
			text.push_back("rgb " + x + " " + y + " " + z);

			sstr << attributes[i].kdcR;
			text.push_back("kdCr " + sstr.str());
			sstr.str("");
			sstr << attributes[i].kdcG;
			text.push_back("kdCg " + sstr.str());
			sstr.str("");
			sstr << attributes[i].kdcB;
			text.push_back("kdCb " + sstr.str());
			sstr.str("");
			sstr << attributes[i].kscR;
			text.push_back("ksCr " + sstr.str());
			sstr.str("");
			sstr << attributes[i].kscG;
			text.push_back("ksCg " + sstr.str());
			sstr.str("");
			sstr << attributes[i].kscB;
			text.push_back("ksCb " + sstr.str());
			sstr.str("");
			sstr << attributes[i].krcR;
			text.push_back("krCr " + sstr.str());
			sstr.str("");
			sstr << attributes[i].krcG;
			text.push_back("krCg " + sstr.str());
			sstr.str("");
			sstr << attributes[i].krcB;
			text.push_back("krCb " + sstr.str());
			sstr.str("");
			sstr << attributes[i].kacR;
			text.push_back("kaCr " + sstr.str());
			sstr.str("");
			sstr << attributes[i].kacG;
			text.push_back("kaCg " + sstr.str());
			sstr.str("");
			sstr << attributes[i].kacB;
			text.push_back("kaCb " + sstr.str());
			sstr.str("");
			sstr << attributes[i].g;
			text.push_back("g " + sstr.str());
			sstr.str("");
			sstr << attributes[i].eta;
			text.push_back("n " + sstr.str());
			sstr.str("");
		}

		offset = 0;
		text.push_back("");
		text.push_back("");
		text.push_back("// przypisanie materialow");
		for (int i = 0; i < attributesNuber - emptyPartsCount; i++) {
			if (find(emptyMatListIdx.begin() + offset, emptyMatListIdx.end(), i) != emptyMatListIdx.end()) {
				++offset;
				--i;
				continue;
			}
			sstr << i;
			x = sstr.str();
			sstr.str("");
			text.push_back(x + " mat" + x);
		}

		text.push_back("");
		text.push_back("");
		text.push_back("// swiatla");

		sstr << lightNumber;
		text.push_back("lights_count " + sstr.str());
		sstr.str("");
		for (int i = 0; i < lightNumber; i++) {
			text.push_back("");

			sstr << i;
			text.push_back("light_name lgt" + sstr.str());
			sstr.str("");
			text.push_back("enabled 1");
            text.push_back("light_type point");

			sstr << lights[i].r;
			x = sstr.str();
			sstr.str("");
			sstr << lights[i].g;
			y = sstr.str();
			sstr.str("");
			sstr << lights[i].b;
			z = sstr.str();
			sstr.str("");
            text.push_back("rgb " + x + " " + y + " " + z);
			sstr << lights[i].flux;
            text.push_back("power " + sstr.str());
			sstr.str("");
			sstr << lights[i].position.z;
			x = sstr.str();
			sstr.str("");
			sstr << lights[i].position.y;
			y = sstr.str();
			sstr.str("");
			sstr << lights[i].position.x;
			z = sstr.str();
			sstr.str("");
            text.push_back("pos " + x + " " + y + " " + z);
            text.push_back("dir 1 1 1");
            text.push_back("inner_angle 30");
            text.push_back("outer_angle 60");
            text.push_back("gonio_count 2");
			text.push_back("0 1");
			text.push_back("180 1");
		}

		text.push_back("");
		text.push_back("");

		text.push_back("// kamery");
		text.push_back("cams_count 1");
		text.push_back("active 0");

		text.push_back("");

        text.push_back("cam_name cam0");
		sstr << camera.xResolution << " " << camera.yResolution;
        text.push_back("resolution " + sstr.str());
		sstr.str("");
		sstr << camera.cameraCenter.z << " " << camera.cameraCenter.y << " " << camera.cameraCenter.x;
        text.push_back("pos " + sstr.str());
		sstr.str("");

		// Do wyciągnięcia lookat i fov
		
		Vector3d vec1, vec2;
		vec1 = Vector3d(camera.cameraCenter, camera.topLeft, true);
		vec2 = Vector3d(camera.cameraCenter, camera.topRight, true);

		camera.lookAt.x = (camera.topRight.x + camera.bottomLeft.x) / 2;
		camera.lookAt.y = (camera.topRight.y + camera.bottomLeft.y) / 2;
		camera.lookAt.z = (camera.topRight.z + camera.bottomLeft.z) / 2;

		sstr << camera.lookAt.z  << " " << camera.lookAt.y << " " << camera.lookAt.x;
        text.push_back("lookAt " + sstr.str());
		sstr.str("");
		float angle = acos( vec1.dotProduct(vec2)) * (180 / PI);
		sstr << angle;
        text.push_back("fov " + sstr.str());
		sstr.str("");
        text.push_back("rotation 0");

		text.push_back("");
		text.push_back("");
		text.push_back("// hierarchia");
		sstr << (lightNumber + attributesNuber - emptyPartsCount);
		text.push_back("node_count " + sstr.str());
		sstr.str("");
		for (int i = 0; i < wat.size(); i++) {
			if (wat[i]->size() < 1) {
				continue;
			}
			text.push_back("");
			sstr << i;
			text.push_back("hier_type Mesh");
			text.push_back("hier_name mesh" + sstr.str());
			sstr.str("");
			sstr << wat[i]->size();
			text.push_back("triangle_count " + sstr.str());
			sstr.str("");
			string trianglesInMesh;
			for (int j = 0; j < wat[i]->size(); j++) {
				sstr << wat[i]->at(j);
				trianglesInMesh.append(sstr.str()).append(" ");
				sstr.str("");
			}
			text.push_back(trianglesInMesh);
		}

		for (int i = 0; i < lightNumber; i++) {
			text.push_back("");
			sstr << i;
			text.push_back("hier_type Light");
			text.push_back("hier_name lgt" + sstr.str());
			text.push_back("light_index " + sstr.str());
			sstr.str("");
		}

		SaveFileLines(filePath, text);
	}

	void Scene::voxelize() {
		x_tiles = ceil((float)camera.xResolution / TILE_SIZE);
		y_tiles = ceil((float)camera.yResolution / TILE_SIZE);
		float v;
		float x, y, z;
		vector<vector<vector<list<int>>>> tmpList;
		//Wyznaczenie mnożnika dla odpowiednich kierunków, aby określić gęstość wokselizacji
		v = pow((trianSize)/(domain_width*domain_height*domain_depth), 1.f/3.f);
	
		//Obliczenie ilości wokseli w każdym kierunku
		mx = ceil(v*domain_width) * 3;
		my = ceil(v*domain_height) * 3;
		mz = ceil(v*domain_depth) * 3;
		//mx = my = mz = 1;

		//Obliczenie wysokości, szerokości oraz długości pojedynczego woksela
		dx = domain_width/mx;
		dy = domain_height/my;
		dz = domain_depth/mz;
		voxel_half_legths = Vector3d(dx/2, dy/2, dz/2, false);

		//Ustalenie wspolrzednych srodka pierwszego woksela
		x = x_min_domain + voxel_half_legths.x;

		//////////////////////////////////////////////////////////////////////////
		// Tworzenie siatki wokseli
		//////////////////////////////////////////////////////////////////////////
		voxels = new Voxel**[mx];
		for (int i=0; i<mx; i++)
		{
			voxels[i] = new Voxel*[my];
			for (int j=0; j<my; j++)
			{
				voxels[i][j] = new Voxel[mz];
			}
		}

		for (int i=0; i<mx; i++) {
			y = y_min_domain + voxel_half_legths.y;
			for (int j=0; j<my; j++) {
				z = z_min_domain + voxel_half_legths.z;
				for (int k=0; k<mz; k++) {
					//Wyznacz punk środka woksela (i, j, k)
					voxels[i][j][k].center = Point3d(x, y, z);
					z += dz;
				}
				y += dy;
			}
			x += dx;
		}

		tmpList.resize(mx);
		for(int q=0; q<mx; q++) {
			tmpList[q].resize(my);
			for (int w=0; w<my; w++) {
				tmpList[q][w].resize(mz);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		// Pakowanie trójkątów do wokseli
		//////////////////////////////////////////////////////////////////////////
	
		Triangle* trians = triangles;
		Voxel* curr_vox;
		float max_x, max_y, max_z, min_x, min_y, min_z;
		int i1, j1, k1, i_max, j_max, k_max;
		for (int ind=0; ind<trianSize; ind++, trians++) {
			//Dla każdego trójkąta znajdź jego AABB
			max_x = (trians->p1.x>trians->p2.x && trians->p1.x>trians->p3.x)?trians->p1.x:(trians->p2.x>trians->p3.x?trians->p2.x:trians->p3.x);
			max_y = (trians->p1.y>trians->p2.y && trians->p1.y>trians->p3.y)?trians->p1.y:(trians->p2.y>trians->p3.y?trians->p2.y:trians->p3.y);
			max_z = (trians->p1.z>trians->p2.z && trians->p1.z>trians->p3.z)?trians->p1.z:(trians->p2.z>trians->p3.z?trians->p2.z:trians->p3.z);

			min_x = (trians->p1.x<trians->p2.x && trians->p1.x<trians->p3.x)?trians->p1.x:(trians->p2.x<trians->p3.x?trians->p2.x:trians->p3.x);
			min_y = (trians->p1.y<trians->p2.y && trians->p1.y<trians->p3.y)?trians->p1.y:(trians->p2.y<trians->p3.y?trians->p2.y:trians->p3.y);
			min_z = (trians->p1.z<trians->p2.z && trians->p1.z<trians->p3.z)?trians->p1.z:(trians->p2.z<trians->p3.z?trians->p2.z:trians->p3.z);

			//Wyznacz woksele, do ktorych potencjalnie mozna upakowac trojkat
			i1 = (min_x - x_min_domain)/dx;
			j1 = (min_y - y_min_domain)/dy;
			k1 = (min_z - z_min_domain)/dz;

			i_max = (max_x - x_min_domain)/dx;
			j_max = (max_y - y_min_domain)/dy;
			k_max = (max_z - z_min_domain)/dz;

			//Dla współrzędnych równych krawędziom domeny algorytm chcialby zapakować
			//trójkąty do wokseli o 1 za daleko, więc należy je uciąć
			if (i1>=mx) i1 = mx-1; if (j1>=my) j1 = my-1; if (k1>=mz) k1 = mz-1;
			if (i_max>=mx) i_max = mx-1; if (j_max>=my) j_max = my-1; if (k_max>=mz) k_max = mz-1; 

			//dla danego trójkąta indeksy potencjalnych wokseli wyznaczone,
			//należy sprawdzić, czy faktycznie dany trójkąt należy do wyznaczonych
			//wokseli. (jesli dwa lub trzy indeksy sa takie same, to nie trzeba
			//nic sprawdzac, trojkat lezy w wyznaczonych wokselach
			for (;i1<=i_max; i1++) {
				for(int j2 = j1;j2<=j_max; j2++) {
					for (int k2 = k1; k2<=k_max; k2++)
					{
						curr_vox = &(voxels[i1][j2][k2]);
						trians->p1.x -= curr_vox->center.x;
						trians->p1.y -= curr_vox->center.y;
						trians->p1.z -= curr_vox->center.z;

						trians->p2.x -= curr_vox->center.x;
						trians->p2.y -= curr_vox->center.y;
						trians->p2.z -= curr_vox->center.z;

						trians->p3.x -= curr_vox->center.x;
						trians->p3.y -= curr_vox->center.y;
						trians->p3.z -= curr_vox->center.z;

						if (triangleVoxelIntersection(trians, curr_vox))
						{
							//curr_vox->triangle_indexes.push_back(&trians);
							tmpList[i1][j2][k2].push_back(ind);
						}

						trians->p1.x += curr_vox->center.x;
						trians->p1.y += curr_vox->center.y;
						trians->p1.z += curr_vox->center.z;

						trians->p2.x += curr_vox->center.x;
						trians->p2.y += curr_vox->center.y;
						trians->p2.z += curr_vox->center.z;

						trians->p3.x += curr_vox->center.x;
						trians->p3.y += curr_vox->center.y;
						trians->p3.z += curr_vox->center.z;
					}
				}
			}
		}

		list<int>::iterator it;
		for (int i=0; i<mx; i++) {
			for (int j=0; j<my; j++) {
				for (int k=0; k<mz; k++) {
					if (!tmpList[i][j][k].empty()) {
						voxels[i][j][k].empty = false;
						voxels[i][j][k].trianList = new Triangle*[tmpList[i][j][k].size()];
						voxels[i][j][k].trian_num = tmpList[i][j][k].size();
						int index = 0;
						for (it = tmpList[i][j][k].begin(); it!= tmpList[i][j][k].end(); it++)
						{
							voxels[i][j][k].trianList[index] = triangles+*it;
							index++;
						}
					} else {
						voxels[i][j][k].empty = true;
						voxels[i][j][k].trian_num = 0;
					}
				}
			}
		}
	}

	bool Scene::triangleVoxelIntersection(const Triangle* const trian, const Voxel* const vox) {
		long p1_in, p2_in, p3_in;
		float d;
		Vector3d norm, vector12, vector13;

		//Sprawdzanie, czy wierzcholki trojkata leza wewnatrz woksela
		if ((p1_in = facePlane(trian->p1))==0) 
			return 1;
		if ((p2_in = facePlane(trian->p2))==0)
			return 1;
		if ((p3_in = facePlane(trian->p3))==0) 
			return 1;

		//Jeśli wszystkie wierzcholki sa na zewnatrz co najmniej jednej
		//tej samej polplaszczyzny, to znaczy ze woksel nie przecina sie
		//z trojkatem
		if ((p1_in & p2_in & p3_in) != 0) return 0;

		p1_in |= bevel2d(trian->p1) << 8;
		p2_in |= bevel2d(trian->p2) << 8;
		p3_in |= bevel2d(trian->p3) << 8;
		if ((p1_in & p2_in & p3_in) != 0) return 0;

		p1_in |= bevel3d(trian->p1) << 24;
		p2_in |= bevel3d(trian->p2) << 24;
		p3_in |= bevel3d(trian->p3) << 24;
		if ((p1_in & p2_in & p3_in) != 0) return 0;

		if ((p1_in & p2_in) == 0)
			if (checkLine(trian->p1, trian->p2, p1_in|p2_in)) return 1;
		if ((p1_in & p3_in) == 0)
			if (checkLine(trian->p1, trian->p3, p1_in|p3_in)) return 1;
		if ((p2_in & p3_in) == 0)
			if (checkLine(trian->p2, trian->p3, p2_in|p3_in)) return 1;

		vector12.x = trian->p2.x - trian->p1.x;
		vector12.y = trian->p2.y - trian->p1.y;
		vector12.z = trian->p2.z - trian->p1.z;
		vector12.normalize();
		vector13.x = trian->p3.x - trian->p1.x;
		vector13.y = trian->p3.y - trian->p1.y;
		vector13.z = trian->p3.z - trian->p1.z;
		vector13.normalize();

		norm = vector12.crossProduct(vector13, true);

		d = norm.x * trian->p1.x + norm.y * trian->p1.y + norm.z + trian->p1.z;

		return 1;
	}

	long Scene::facePlane(const Point3d& p) {
		long mask = 0;

		if (p.x - EPS > voxel_half_legths.x)  mask |= 0x01;
		if (p.x + EPS < -voxel_half_legths.x) mask |= 0x02;
		if (p.y - EPS > voxel_half_legths.y)  mask |= 0x04;
		if (p.y + EPS < -voxel_half_legths.y) mask |= 0x08;
		if (p.z - EPS > voxel_half_legths.z)  mask |= 0x10;
		if (p.z + EPS < -voxel_half_legths.z) mask |= 0x20;
		return mask;
	}

	long Scene::bevel2d(const Point3d& p) {
		long mask = 0;
		float xy = voxel_half_legths.x + voxel_half_legths.y;
		float xz = voxel_half_legths.x + voxel_half_legths.z;
		float yz = voxel_half_legths.y + voxel_half_legths.z;

		if ((p.x + p.y) > xy + EPS) mask |= 0x001;
		if ((p.x - p.y) > xy - EPS) mask |= 0x002;
		if ((-p.x + p.y) > xy + EPS) mask |= 0x004;
		if ((-p.x - p.y) > xy - EPS) mask |= 0x008;
		if ((p.x + p.z) > xz + EPS) mask |= 0x010;
		if ((p.x - p.z) > xz - EPS) mask |= 0x020;
		if ((-p.x + p.z) > xz + EPS) mask |= 0x040;
		if ((-p.x - p.z) > xz - EPS) mask |= 0x080;
		if ((p.y + p.z) > yz + EPS) mask |= 0x100;
		if ((p.y - p.z) > yz - EPS) mask |= 0x200;
		if ((-p.y + p.z) > yz + EPS) mask |= 0x400;
		if ((-p.y - p.z) > yz - EPS) mask |= 0x800;

		return mask;
	}

	long Scene::bevel3d(const Point3d& p) {
		long mask = 0;
		float xyz = voxel_half_legths.x + voxel_half_legths.y + voxel_half_legths.z;

		if ((p.x + p.y + p.z) > xyz + EPS) mask |= 0x01;
		if ((p.x + p.y - p.z) > xyz + EPS) mask |= 0x02;
		if ((p.x - p.y + p.z) > xyz + EPS) mask |= 0x04;
		if ((p.x - p.y - p.z) > xyz + EPS) mask |= 0x08;
		if ((-p.x + p.y + p.z) > xyz + EPS) mask |= 0x10;
		if ((-p.x + p.y - p.z) > xyz + EPS) mask |= 0x20;
		if ((-p.x - p.y + p.z) > xyz + EPS) mask |= 0x40;
		if ((-p.x - p.y - p.z) > xyz + EPS) mask |= 0x80;

		return mask;
	}

	long Scene::checkPoint(const Point3d& p1, const Point3d& p2, float alph, long mask) {
		Point3d planePoint;
		planePoint.x = p1.x + alph*(p2.x - p1.x);
		planePoint.y = p1.y + alph*(p2.y - p1.y);
		planePoint.z = p1.z + alph*(p2.z - p1.z);
		return (facePlane(planePoint) & mask);
	}

	long Scene::checkLine(const Point3d& p1, const Point3d& p2, long mask_diff) {
		if ((0x01 & mask_diff) != 0)
			if (checkPoint(p1, p2, (voxel_half_legths.x-p1.x)/(p2.x - p1.x), 0x3e) == 0) return 1;
		if ((0x02 & mask_diff) != 0)
			if (checkPoint(p1, p2, (-voxel_half_legths.x-p1.x)/(p2.x - p1.x), 0x3d) == 0) return 1;
		if ((0x04 & mask_diff) != 0)
			if (checkPoint(p1, p2, (voxel_half_legths.x-p1.y)/(p2.y - p1.y), 0x3b) == 0) return 1;
		if ((0x08 & mask_diff) != 0)
			if (checkPoint(p1, p2, (-voxel_half_legths.x-p1.y)/(p2.y - p1.y), 0x37) == 0) return 1;
		if ((0x10 & mask_diff) != 0)
			if (checkPoint(p1, p2, (voxel_half_legths.x-p1.z)/(p2.z - p1.z), 0x2f) == 0) return 1;
		if ((0x20 & mask_diff) != 0)
			if (checkPoint(p1, p2, (-voxel_half_legths.x-p1.z)/(p2.z - p1.z), 0x1f) == 0) return 1;
		return 0;
	}

	bool Scene::renderTile(BYTE* buffer, int psw) {
		Vector3d direction;
		Vector3d vecV = Vector3d(camera.topLeft, camera.bottomLeft, false);
		Vector3d vecU = Vector3d(camera.topLeft, camera.topRight, false);
		Point3d planePoint = Point3d();
		float len;
		float x, y, z;
		int buf_pos = 0;
		int newpos = 0;
		Color color;
		float depth;
		bool finished = false;

		int beg_x = tile_x_cntr * TILE_SIZE;
		int beg_y = tile_y_cntr * TILE_SIZE;

		int end_x = (tile_x_cntr + 1) * TILE_SIZE;
		int end_y = (tile_y_cntr + 1) * TILE_SIZE;

		#pragma omp parallel for private(color, x, y, z, planePoint, direction, len, depth, newpos)
		for (int i=beg_x; i<end_x; i++) {
			//if (i >= camera.xResolution) break;
			for (float j=beg_y; j<end_y; j++)
			{
				if (j >= camera.yResolution) break;
				//i = (float) idx;
				newpos = (camera.yResolution - j - 1) * psw + i * 3;

				x = (static_cast<float>((float)i/(camera.xResolution-1)))*vecU.x + (static_cast<float>(j/(camera.yResolution-1)))*vecV.x;
				y = (static_cast<float>((float)i/(camera.xResolution-1)))*vecU.y + (static_cast<float>(j/(camera.yResolution-1)))*vecV.y;
				z = (static_cast<float>((float)i/(camera.xResolution-1)))*vecU.z + (static_cast<float>(j/(camera.yResolution-1)))*vecV.z;

				x += camera.topLeft.x;
				y += camera.topLeft.y;
				z += camera.topLeft.z;
			
				planePoint.x = x;
				planePoint.y = y;
				planePoint.z = z;
			
				//Wektor kierunkowy promienia pierwotnego

				direction.x = planePoint.x - camera.cameraCenter.x;
				direction.y = planePoint.y - camera.cameraCenter.y;
				direction.z = planePoint.z - camera.cameraCenter.z;
				len = direction.length();
				direction.x /= len;
				direction.y /= len;
				direction.z /= len;

				depth = 0;
			
				color = calculateColorSub(camera.cameraCenter, direction, 0, -1, depth);
				//#pragma omp critical
				//{
				buffer[newpos + 2] = color.red;
				buffer[newpos + 1] = color.green;
				buffer[newpos] = color.blue;
				//}
			}
		}

		tile_x_cntr++;
		if (tile_x_cntr >= x_tiles) {
			if (tile_y_cntr >= y_tiles) {
				finished = true;
			} else {
				tile_y_cntr++;
				tile_x_cntr = 0;
			}
		}

		return finished;
	}

	void Scene::render(BYTE* buffer, int psw) {
		Vector3d vector;
		Vector3d vecV = Vector3d(camera.topLeft, camera.bottomLeft, false);
		Vector3d vecU = Vector3d(camera.topLeft, camera.topRight, false);
		Point3d planePoint = Point3d();
		float len;
		float x, y, z;
		BYTE* rgb_buffer = new BYTE[3*camera.xResolution*camera.yResolution];
		int buf_pos = 0;
		int newpos = 0;
		Color color;
		float depth;
		//float i;

		//stworz z-bufor
		zbuffer = new float*[camera.xResolution];
		for (int i=0; i<camera.xResolution; i++)
		{
			zbuffer[i] = new float[camera.yResolution];
		}
		zbuf_flag = true;

	#ifdef PARALLEL
		Concurrency::parallel_for(0, camera.xResolution, 1, [&](float i) {
	#elif defined LOOP
	//#pragma omp parallel for
		for (float i=0; i<camera.xResolution; i++)
		{
	#endif
			//cout << (i/camera.xResolution)*100 << "%\n";
			for (float j=0; j<camera.yResolution; j++)
			{
				//i = (float) idx;
				newpos = (camera.yResolution - j - 1) * psw + i * 3;
				buf_pos =j*3*camera.xResolution + i*3;

				x = (static_cast<float>(i/(camera.xResolution-1)))*vecU.x + (static_cast<float>(j/(camera.yResolution-1)))*vecV.x;
				y = (static_cast<float>(i/(camera.xResolution-1)))*vecU.y + (static_cast<float>(j/(camera.yResolution-1)))*vecV.y;
				z = (static_cast<float>(i/(camera.xResolution-1)))*vecU.z + (static_cast<float>(j/(camera.yResolution-1)))*vecV.z;

				x += camera.topLeft.x;
				y += camera.topLeft.y;
				z += camera.topLeft.z;
			
				planePoint.x = x;
				planePoint.y = y;
				planePoint.z = z;
			
				//Wektor kierunkowy promienia pierwotnego

				vector.x = planePoint.x - camera.cameraCenter.x;
				vector.y = planePoint.y - camera.cameraCenter.y;
				vector.z = planePoint.z - camera.cameraCenter.z;
				len = vector.length();
				vector.x /= len;
				vector.y /= len;
				vector.z /= len;

				depth = 0;
			
				color = calculateColorSub(camera.cameraCenter, vector, 0, -1, depth);

				zbuffer[(int)i][(int)j] = depth;

				buffer[newpos + 2] = color.red;
				buffer[newpos + 1] = color.green;
				buffer[newpos] = color.blue;

				rgb_buffer[buf_pos] = color.red;
				rgb_buffer[buf_pos + 1] = color.green;
				rgb_buffer[buf_pos + 2] = color.blue;
			
			}
	#ifdef LOOP
		}
	#elif defined PARALLEL
		});
	#endif

		BYTE* bmpbuf;
		long newsize;

		//buffer = convertRGBToBMPBuffer(rgb_buffer, camera.xResolution, camera.yResolution, &newsize);
		//bool saving = saveBMP(bmpbuf, camera.xResolution, camera.yResolution, newsize, file_name.c_str());

	#ifdef DOF
		if (dof) {

			//konwertuj z-bufor do postaci, która umożliwi zapisanie w bmp
			BYTE* rgb_zbuf = new BYTE[3*camera.xResolution*camera.yResolution];
			float max_zbuf = 0, min_zbuf = 0, max_coc=0;
			float** coc = new float*[camera.xResolution];
			for (int i=0; i<camera.xResolution; i++)
			{
				coc[i] = new float[camera.yResolution];
			}
			BYTE* rgb_blur_buf = new BYTE[3*camera.xResolution*camera.yResolution];

			////////////////////////////////////////////////////////////
			////////////////////////////////////////////////////////////

	
			////////////////////////////////////////////////////////////
			//Dla sceny pokoj, przesuniecie punktu ostrosci o 4
			//Z_focus dla sceny "pokoj" - 4
			//f - ogniskowa (Focal length)
			//N - f-stop (wartosc przyslony obiektywu)
			//CoC - krazek rozmycia (circle of confusion)
			//CoC = abs(((f/N)*f*(z_focus-z))/(z_focus*(z-f)))
			////////////////////////////////////////////////////////////

			float N = .0001;
			//float z_f = 5.;
			float scale = 1.;

			int maximum_coc = 50;
			int coc_diameter, coc_radius;
			int num_taps = 30;
			int tapx, tapy;
			int blurredBuffer, blurredBufferTap;
			float tap_depth, tap_blur;
			float tap_contr, total_contr;
			Color blurredOutput;

			srand(time(NULL));

			for (int i=0; i<camera.xResolution;i++) {
				for (int j=0; j<camera.yResolution; j++) {

					coc[i][j] = abs(((f/N)*f*(z_f-zbuffer[i][j]))/(z_f*(zbuffer[i][j]-f)));
					max_coc = coc[i][j]>max_coc?coc[i][j]:max_coc;

					min_zbuf = zbuffer[i][j]<min_zbuf?zbuffer[i][j]:min_zbuf;
					max_zbuf = zbuffer[i][j]>max_zbuf?zbuffer[i][j]:max_zbuf;
				}
			}

			for (int i=0; i<camera.xResolution;i++) {
				for (int j=0; j<camera.yResolution; j++) {
					coc[i][j] /= max_coc;
					zbuffer[i][j] /= max_zbuf;
				}
			}

			for (int i=0; i<camera.xResolution; i++)
			{
				//cout << i << endl;
				for (int j=0; j<camera.yResolution; j++)
				{
					//Dla danego punktu oblicz jego CoC
					//Wylosuj później odpowiednią liczbę punktów, które znajdą się wewnątrz CoC
					//Kazdy wylosowany punkt dodaje czesc koloru do wynikowego
					//Wynikowy kolor dzielimy przez total distribution

					coc_diameter = coc[i][j]*maximum_coc;
					coc_radius = (coc_diameter+1)/2;
					blurredBuffer = 3*(j*camera.xResolution + i);
					blurredOutput.red = rgb_buffer[blurredBuffer];
					blurredOutput.green = rgb_buffer[blurredBuffer+1];
					blurredOutput.blue = rgb_buffer[blurredBuffer+2];

					total_contr = 1;

					for (int k=0; k<num_taps; k++)
					{
						tapx = (double)rand()/(RAND_MAX+1)*coc_diameter + i;
						tapy = (double)rand()/(RAND_MAX+1)*coc_diameter + j;

						while(sqrt((double)((tapx-i)*(tapx-i)+(tapy-j)+(tapy-j)))>coc_radius) {
							tapx = (double)rand()/(RAND_MAX+1)*coc_diameter + i;
							tapy = (double)rand()/(RAND_MAX+1)*coc_diameter + j;
						}

						if (tapx>=camera.xResolution)
						{
							tapx = camera.xResolution-1;
						}
						if (tapy>=camera.yResolution)
						{
							tapy = camera.yResolution-1;
						}

						blurredBufferTap = 3*(tapy*camera.xResolution + tapx);

						tap_depth = zbuffer[tapx][tapy];
						tap_blur = coc[tapx][tapy];

						tap_contr = (tap_blur>coc[i][j]?1.f:tap_blur);

						blurredOutput.red += rgb_buffer[blurredBufferTap]*tap_contr;
						blurredOutput.green += rgb_buffer[blurredBufferTap+1]*tap_contr;
						blurredOutput.blue += rgb_buffer[blurredBufferTap+2]*tap_contr;
						total_contr += tap_contr;
					}
					blurredOutput = blurredOutput *(1/total_contr);
					rgb_blur_buf[blurredBuffer] = blurredOutput.red;
					rgb_blur_buf[blurredBuffer+1] = blurredOutput.green;
					rgb_blur_buf[blurredBuffer+2] = blurredOutput.blue;
				}
			}

			int buf_position;
			for (int i=0; i<camera.xResolution; i++) {
				for (int j=0; j<camera.yResolution; j++) {
					buf_position = 3*(j*camera.xResolution + i);
					rgb_zbuf[buf_position] = (BYTE)(255*(zbuffer[i][j]));
					rgb_zbuf[buf_position+1] = (BYTE)(255*(zbuffer[i][j]));
					rgb_zbuf[buf_position+2] = (BYTE)(255*(zbuffer[i][j]));
				}
			}

			string dof = "dof-";

			BYTE* bmpblur;
			bmpblur = convertRGBToBMPBuffer(rgb_blur_buf, camera.xResolution, camera.yResolution, &newsize);
			saveBMP(bmpblur, camera.xResolution, camera.yResolution, newsize, dof.append(file_name).c_str());

			for (int i=0; i<camera.xResolution; i++)
			{
				delete[] coc[i];
			}
			delete[] coc;

			delete[] rgb_zbuf;
		}
	#endif

		delete[] rgb_buffer;
		//delete[] bmpbuf;
	
	}

	Color Scene::calculateColorSub(Point3d& origin, Vector3d& direction, int dpth, int tri, float& z_dpth) {
		Color outputColor;
		Triangle* tmpTrian;
		Point3d intersection_p;
		float dtx, dty, dtz;
		float tx, ty, tz;
		float t_near = -numeric_limits<float>::infinity(), t_far = numeric_limits<float>::infinity();
		int attrib = -1, trianIndex = -1, i, j, k;
		int di, dj, dk;

		outputColor.red = 0;
		outputColor.green = 0;
		outputColor.blue = 0;

		/************************************************************************/
		/* Znajdź pierwszy przecinany woksel. Najpierw nalezy sprawdzic czy     */
		/* punkt początkowy promienia jest poza domeną. Jeśli tak, promień może */
		/* wejść do domeny tylko przez jeden z bocznych wokseli. Jeśli nie,		*/
		/* należy sprawdzić w którym wokselu promień aktualnie się znajduje.	*/
		/* W obu przypadkach trzeba znaleźć w którą promień będzie się poruszał,*/
		/* aby odpowiednio zwiększać lub zmniejszać indeksy wokseli				*/
		/************************************************************************/

		if (origin.x - EPS > x_min_domain && origin.x + EPS < x_max_domain &&
			origin.y - EPS > y_min_domain && origin.y + EPS < y_max_domain && 
			origin.z - EPS > z_min_domain && origin.z + EPS < z_max_domain)
		{
			//Promien rozpoczyna sie wewnatrz domeny
			//współrzędne wokseli wyliczone
			i = (origin.x - x_min_domain)/dx;
			j = (origin.y - y_min_domain)/dy;
			k = (origin.z - z_min_domain)/dz;

			//Wartości tx, ty i tz dla znalezionego woksela
		} else {
			//Promień rozpoczyna sie na zewnątrz domeny
			if (direction.x == 0 && origin.x > x_max_domain && origin.x < x_min_domain) return outputColor;
			if (direction.y == 0 && origin.y > y_max_domain && origin.y < y_min_domain) return outputColor;
			if (direction.z == 0 && origin.z > z_max_domain && origin.z < z_min_domain) return outputColor;
			float t1, t2;
			t1 = (x_min_domain-origin.x)/direction.x;
			t2 = (x_max_domain-origin.x)/direction.x;
			if (t1>t2) {float tmp = t1; t1 = t2; t2 = tmp;}
			if (t1>t_near) t_near = t1;
			if (t2<t_far) t_far = t2;
			if (t_near>t_far) return outputColor;
			if (t_far<0) return outputColor;

			t1 = (y_min_domain-origin.y)/direction.y;
			t2 = (y_max_domain-origin.y)/direction.y;
			if (t1>t2) {float tmp = t1; t1 = t2; t2 = tmp;}
			if (t1>t_near) t_near = t1;
			if (t2<t_far) t_far = t2;
			if (t_near>t_far) return outputColor;
			if (t_far<0) return outputColor;

			t1 = (z_min_domain-origin.z)/direction.z;
			t2 = (z_max_domain-origin.z)/direction.z;
			if (t1>t2) {float tmp = t1; t1 = t2; t2 = tmp;}
			if (t1>t_near) t_near = t1;
			if (t2<t_far) t_far = t2;
			if (t_near>t_far) return outputColor;
			if (t_far<0) return outputColor;

			i = (origin.x + direction.x * t_near - x_min_domain)/dx;
			j = (origin.y + direction.y * t_near - y_min_domain)/dy;
			k = (origin.z + direction.z * t_near - z_min_domain)/dz;

			i -= i==mx?1:0;
			j -= j==my?1:0;
			k -= k==mz?1:0;
		}

		tx = min((voxels[i][j][k].center.x-voxel_half_legths.x-origin.x)/direction.x, (voxels[i][j][k].center.x+voxel_half_legths.x-origin.x)/direction.x);
		ty = min((voxels[i][j][k].center.y-voxel_half_legths.y-origin.y)/direction.y, (voxels[i][j][k].center.y+voxel_half_legths.y-origin.y)/direction.y);
		tz = min((voxels[i][j][k].center.z-voxel_half_legths.z-origin.z)/direction.z, (voxels[i][j][k].center.z+voxel_half_legths.z-origin.z)/direction.z);

		di = direction.x>0?1:-1;
		dj = direction.y>0?1:-1;
		dk = direction.z>0?1:-1;

		//jesli kierunek jest 0 to powinno byc 0?
		dtx = abs(direction.x==0?0:dx/direction.x);
		dty = abs(direction.y==0?0:dy/direction.y);
		dtz = abs(direction.z==0?0:dz/direction.z);

		Triangle** triang;
		Triangle* inter_trian;
		float tx_n, ty_n, tz_n, tmp_t_int = -1, t_out, t_int = -1;
		int ind = -1, atrib = -1;
		while (i<mx && i>-1 && j<my && j>-1 && k<mz && k>-1) {
			tx_n = tx + dtx;
			ty_n = ty + dty;
			tz_n = tz + dtz;
			t_out = (tx_n<ty_n && tx_n<tz_n)?tx_n:(ty_n<tz_n)?ty_n:tz_n;
			t_int = t_out;
			//Szukaj przecięć w aktualnym wokselu
			if (!voxels[i][j][k].empty) {
				triang = voxels[i][j][k].trianList;
				for (int index = 0; index<voxels[i][j][k].trian_num; triang++, index++) {
					tmpTrian = *triang;
					tmp_t_int = -((*tmpTrian).d + ((*tmpTrian).norm.x*origin.x) + ((*tmpTrian).norm.y*origin.y) + 
						((*tmpTrian).norm.z*origin.z));
					tmp_t_int /= ((*tmpTrian).norm).dotProduct(direction);
					if (tmp_t_int - EPS > 0 && tmp_t_int - EPS <t_int && tmpTrian->ind != tri)
					switch (tmpTrian->plane)
					{
						case 0:
							//Rzutujemy na płaszczyznę (Y, Z)
							intersection_p.y = origin.y+direction.y*tmp_t_int;
							intersection_p.z = origin.z+direction.z*tmp_t_int;
							if (intersection_p.y <= tmpTrian->max1 && intersection_p.z <= tmpTrian->max2
								&& intersection_p.y >= tmpTrian->min1 && intersection_p.z >= tmpTrian->min2
								&& (tmpTrian->a1 * intersection_p.y + tmpTrian->b1 * intersection_p.z) + tmpTrian->d1 >= 0
								&& (tmpTrian->a2 * intersection_p.y + tmpTrian->b2 * intersection_p.z) + tmpTrian->d2 >= 0
								&& (tmpTrian->a3 * intersection_p.y + tmpTrian->b3 * intersection_p.z) + tmpTrian->d3 >= 0) {
									//Punkt jest wewnątrz trójkąta
									trianIndex = tmpTrian->ind;
									t_int = tmp_t_int;
									atrib = tmpTrian->atributeIndex;
							}
							break;
						case 1:
							//Rzutujemy na płaszczyznę (X, Z)
							intersection_p.x = origin.x+direction.x*tmp_t_int;
							intersection_p.z = origin.z+direction.z*tmp_t_int;
							if (intersection_p.x <= tmpTrian->max1 && intersection_p.z <= tmpTrian->max2
								&& intersection_p.x >= tmpTrian->min1 && intersection_p.z >= tmpTrian->min2
								&& (tmpTrian->a1 * intersection_p.x + tmpTrian->b1 * intersection_p.z) + tmpTrian->d1 >= 0
								&& (tmpTrian->a2 * intersection_p.x + tmpTrian->b2 * intersection_p.z) + tmpTrian->d2 >= 0
								&& (tmpTrian->a3 * intersection_p.x + tmpTrian->b3 * intersection_p.z) + tmpTrian->d3 >= 0) {
									//Punkt jest wewnątrz trójkąta
									trianIndex = tmpTrian->ind;
									t_int = tmp_t_int;
									atrib = tmpTrian->atributeIndex;
							}
							break;
						case 2:
							//Rzutujemy na płaszczyznę (X, Y)
							intersection_p.x = origin.x+direction.x*tmp_t_int;
							intersection_p.y = origin.y+direction.y*tmp_t_int;
							if (intersection_p.x <= tmpTrian->max1 && intersection_p.y <= tmpTrian->max2
								&& intersection_p.x >= tmpTrian->min1 && intersection_p.y >= tmpTrian->min2
								&& (tmpTrian->a1 * intersection_p.x + tmpTrian->b1 * intersection_p.y) + tmpTrian->d1 >= 0  
								&& (tmpTrian->a2 * intersection_p.x + tmpTrian->b2 * intersection_p.y) + tmpTrian->d2 >= 0
								&& (tmpTrian->a3 * intersection_p.x + tmpTrian->b3 * intersection_p.y) + tmpTrian->d3 >= 0) {
									//Punkt jest wewnątrz trójkąta
									trianIndex = tmpTrian->ind;
									t_int = tmp_t_int;
									atrib = tmpTrian->atributeIndex;
							}
							break;
						default: break;
					}
				}
			}
			//Jeśli znaleziono przecięcie i znajduje się ono w wokselu, wyjdz z petli
			if (t_int<t_out-EPS)
			{
				break;
			}
			//Przejdz do nastepnego woksela
			if (tx_n<ty_n) {
				if (tx_n< tz_n)
				{
					i+=di; tx = tx_n;
				} else
				{
					k+=dk; tz = tz_n;
				}
			} else {
				if (ty_n<tz_n) 
				{
					j+=dj; ty = ty_n;
				} else
				{
					k+=dk; tz = tz_n;
				}
			}
		}

		if (atrib<0)
			return outputColor;


		intersection_p.x = origin.x+direction.x*t_int;
		intersection_p.y = origin.y+direction.y*t_int;
		intersection_p.z = origin.z+direction.z*t_int;

		Vector3d vecLight;
		Vector3d vecObser;
		Vector3d reflectedRey;
		float tmpDotPr;
		float transp;
		float r;
		float sum_dif_r, sum_dif_g, sum_dif_b, sum_ref_r, sum_ref_g, sum_ref_b;
		sum_ref_r = sum_ref_g = sum_ref_b = sum_dif_r = sum_dif_g = sum_dif_b = 0;

		//Znalezione punkt przeciecia, atrybut oraz odbity trojkat
	
		Attrib* atr = attributes+atrib;
		tmpTrian = triangles+trianIndex;

		vecObser = Vector3d(intersection_p, origin, true);

		//Refrakcja
		if ((atr->krcR > 0 || atr->krcG > 0 || atr->krcB > 0) && dpth<MAX_DEPTH) {
			Vector3d refractedRay;
			Vector3d vec1;
			Vector3d vec2;
			float coef;
			float coef2;
			float ni = 1.f/atr->eta;

			tmpDotPr = abs(vecObser.dotProduct(tmpTrian->norm));

			vec2.x = vecObser.x - tmpTrian->norm.x*tmpDotPr;
			vec2.y = vecObser.y - tmpTrian->norm.y*tmpDotPr;
			vec2.z = vecObser.z - tmpTrian->norm.z*tmpDotPr;

			vec2.x *= -ni;
			vec2.y *= -ni;
			vec2.z *= -ni;

			coef = 1 - tmpDotPr*tmpDotPr;

			coef = (ni*ni)*coef;
			coef = 1 - coef;

			coef2 = sqrt(coef);
			coef2 = 0-coef2;

			vec1.x = tmpTrian->norm.x * coef2;
			vec1.y = tmpTrian->norm.y * coef2;
			vec1.z = tmpTrian->norm.z * coef2;

			refractedRay.x = vec1.x + vec2.x;
			refractedRay.y = vec1.y + vec2.y;
			refractedRay.z = vec1.z + vec2.z;

			refractedRay.normalize();

			//z_dpth += t_int;

			//outputColor = outputColor + calculateColorSub(intersection_p, refractedRay, ++dpth, trianIndex, z_dpth)*atr->kt;
			Color tmpColor = calculateColorSub(intersection_p, refractedRay, ++dpth, trianIndex, z_dpth);
			outputColor.red += tmpColor.red * atr->krcR;
			outputColor.green += tmpColor.green * atr->krcG;
			outputColor.blue += tmpColor.blue * atr->krcB;
		}

		tmpDotPr = vecObser.dotProduct(tmpTrian->norm);
		tmpDotPr *= 2;
		reflectedRey.x = tmpTrian->norm.x * tmpDotPr;
		reflectedRey.y = tmpTrian->norm.y * tmpDotPr;
		reflectedRey.z = tmpTrian->norm.z * tmpDotPr;

		reflectedRey.x -= vecObser.x;
		reflectedRey.y -= vecObser.y;
		reflectedRey.z -= vecObser.z;

		reflectedRey.normalize();

		// Refleksja
		if ((atr->kscR > 0 || atr->kscG > 0 || atr->kscB > 0) && dpth<MAX_DEPTH) {
			//reflectedRey.normalize();

			//z_dpth += t_int;

			//outputColor = outputColor + calculateColorSub(intersection_p, reflectedRey, ++dpth, trianIndex, z_dpth)*atr->ks;
			Color tmpColor = calculateColorSub(intersection_p, reflectedRey, ++dpth, trianIndex, z_dpth);
			outputColor.red += tmpColor.red * atr->kscR;
			outputColor.green += tmpColor.green * atr->kscG;
			outputColor.blue += tmpColor.blue * atr->kscB;
		}

		for (int i = 0; i<lightNumber; i++) {
			vecLight = Vector3d(intersection_p, lights[i].position, false);
			r = vecLight.length();

			vecLight.x /= r;
			vecLight.y /= r;
			vecLight.z /= r;

			//transp = 1;
			if ((transp=isVisibleSub(intersection_p, vecLight, r, trianIndex))>0) {

				float dotPr1, dotPr2;

				reflectedRey.normalize();

				dotPr1 = abs(tmpTrian->norm.dotProduct(vecLight));
				dotPr2 = abs(reflectedRey.dotProduct(vecLight));

				outputColor.red += transp * (atr->r * atr->kdcR*(lights[i].r*lights[i].flux*dotPr1)/r
					+ atr->kscR * (lights[i].r*255*lights[i].flux*pow(dotPr2, atr->wg)));
				outputColor.green += transp * (atr->g * atr->kdcG*(lights[i].g*lights[i].flux*dotPr1)/r
					+ atr->kscG * (lights[i].g*255*lights[i].flux*pow(dotPr2, atr->wg)));
				outputColor.blue += transp * (atr->b * atr->kdcB*(lights[i].b*lights[i].flux*dotPr1)/r
					+ atr->kscB * (lights[i].b*255*lights[i].flux*pow(dotPr2, atr->wg)));
			
			}
		}

		//Aktualizuj głębokość
		z_dpth += t_int;

		if (outputColor.red>255) outputColor.red = 255;
		if (outputColor.green>255) outputColor.green = 255;
		if (outputColor.blue>255) outputColor.blue = 255;
		if (outputColor.red<0) outputColor.red = 0;
		if (outputColor.green<0) outputColor.green = 0;
		if (outputColor.blue<0) outputColor.blue = 0;

		//outputColor.red = atr->r;
		//outputColor.green = atr->g;
		//outputColor.blue = atr->b;

		return outputColor;
	}

	float Scene::isVisibleSub(Point3d& origin, Vector3d& direction, float dist, int tri) {
		float transp = 1;
		//Color outputColor;
		Triangle* tmpTrian;
		Point3d intersection_p;
		float dtx, dty, dtz;
		float tx, ty, tz;
		float t_near = -numeric_limits<float>::infinity(), t_far = numeric_limits<float>::infinity();
		int attrib = -1, trianIndex = -1, i, j, k;
		int di, dj, dk;
		// Tablica pamiętająca, czy dany trójkąt został już przecięty, stosowana
		// z uwagi na to, ze dany trojkat moze znajdowac sie w dwoch wokselach
		// jednoczesnie
		char *trian_hit = new char[trianSize];
		memset(trian_hit, 0, trianSize);

		/*outputColor.red = 0;
		outputColor.green = 0;
		outputColor.blue = 0;*/

		/************************************************************************/
		/* Znajdź pierwszy przecinany woksel. Najpierw nalezy sprawdzic czy     */
		/* punkt początkowy promienia jest poza domeną. Jeśli tak, promień może */
		/* wejść do domeny tylko przez jeden z bocznych wokseli. Jeśli nie,		*/
		/* należy sprawdzić w którym wokselu promień aktualnie się znajduje.	*/
		/* W obu przypadkach trzeba znaleźć w którą promień będzie się poruszał,*/
		/* aby odpowiednio zwiększać lub zmniejszać indeksy wokseli				*/
		/************************************************************************/

		if (origin.x + EPS > x_min_domain && origin.x - EPS < x_max_domain &&
			origin.y + EPS > y_min_domain && origin.y - EPS < y_max_domain && 
			origin.z + EPS > z_min_domain && origin.z - EPS < z_max_domain)
		{
			//Promien rozpoczyna sie wewnatrz domeny
			//współrzędne wokseli wyliczone
			i = (origin.x - x_min_domain)/dx;
			j = (origin.y - y_min_domain)/dy;
			k = (origin.z - z_min_domain)/dz;

			//Wartości tx, ty i tz dla znalezionego woksela
		} else {
			//Promień rozpoczyna sie na zewnątrz domeny
			if (direction.x == 0 && origin.x > x_max_domain && origin.x < x_min_domain) return 0;// outputColor;
			if (direction.y == 0 && origin.y > y_max_domain && origin.y < y_min_domain) return 0;//outputColor;
			if (direction.z == 0 && origin.z > z_max_domain && origin.z < z_min_domain) return 0;//outputColor;
			float t1, t2;
			t1 = (x_min_domain-origin.x)/direction.x;
			t2 = (x_max_domain-origin.x)/direction.x;
			if (t1>t2) {float tmp = t1; t1 = t2; t2 = tmp;}
			if (t1>t_near) t_near = t1;
			if (t2<t_far) t_far = t2;
			if (t_near>t_far) return 0;//outputColor;
			if (t_far<0) return 0;//outputColor;

			t1 = (y_min_domain-origin.y)/direction.y;
			t2 = (y_max_domain-origin.y)/direction.y;
			if (t1>t2) {float tmp = t1; t1 = t2; t2 = tmp;}
			if (t1>t_near) t_near = t1;
			if (t2<t_far) t_far = t2;
			if (t_near>t_far) return 0;//outputColor;
			if (t_far<0) return 0;//outputColor;

			t1 = (z_min_domain-origin.z)/direction.z;
			t2 = (z_max_domain-origin.z)/direction.z;
			if (t1>t2) {float tmp = t1; t1 = t2; t2 = tmp;}
			if (t1>t_near) t_near = t1;
			if (t2<t_far) t_far = t2;
			if (t_near>t_far) return 0;//outputColor;
			if (t_far<0) return 0;//outputColor;

			i = (origin.x + direction.x * t_near - x_min_domain)/dx;
			j = (origin.y + direction.y * t_near - y_min_domain)/dy;
			k = (origin.z + direction.z * t_near - z_min_domain)/dz;

			//wartości tx, ty, tz dla znalezionego woksela
		}
		i -= i==mx?1:0;
		j -= j==my?1:0;
		k -= k==mz?1:0;

		tx = min((voxels[i][j][k].center.x-voxel_half_legths.x-origin.x)/direction.x, (voxels[i][j][k].center.x+voxel_half_legths.x-origin.x)/direction.x);
		ty = min((voxels[i][j][k].center.y-voxel_half_legths.y-origin.y)/direction.y, (voxels[i][j][k].center.y+voxel_half_legths.y-origin.y)/direction.y);
		tz = min((voxels[i][j][k].center.z-voxel_half_legths.z-origin.z)/direction.z, (voxels[i][j][k].center.z+voxel_half_legths.z-origin.z)/direction.z);

		di = direction.x>0?1:-1;
		dj = direction.y>0?1:-1;
		dk = direction.z>0?1:-1;

		//jesli kierunek jest 0 to powinno byc 0?
		dtx = abs(direction.x==0?0:dx/direction.x);
		dty = abs(direction.y==0?0:dy/direction.y);
		dtz = abs(direction.z==0?0:dz/direction.z);

		Triangle** triang;
		Triangle* inter_trian;
		float tx_n, ty_n, tz_n, tmp_t_int = -1, t_out = -1, t_int = -1;
		int ind = -1, atrib = -1;
		while (i<mx && i>-1 && j<my && j>-1 && k<mz && k>-1 && t_out < dist) {
			tx_n = tx + dtx;
			ty_n = ty + dty;
			tz_n = tz + dtz;
			t_out = (tx_n<ty_n && tx_n<tz_n)?tx_n:(ty_n<tz_n)?ty_n:tz_n;
			t_int = t_out;
			//Szukaj przecięć w aktualnym wokselu
			if (!voxels[i][j][k].empty) {
				triang = voxels[i][j][k].trianList;
				for (int index = 0; index<voxels[i][j][k].trian_num; triang++, index++) {
					tmpTrian = *triang;
					tmp_t_int = -((*tmpTrian).d + ((*tmpTrian).norm.x*origin.x) + ((*tmpTrian).norm.y*origin.y) + 
						((*tmpTrian).norm.z*origin.z));
					tmp_t_int /= ((*tmpTrian).norm).dotProduct(direction);
					if (tmpTrian->plane == 0) {
						tmp_t_int -= EPS;
					}
					if (tmp_t_int < t_out && tmp_t_int - EPS > 0 && tmp_t_int < dist && tmpTrian->ind != tri)
						switch (tmpTrian->plane)
					{
						case 0:
							//Rzutujemy na płaszczyznę (Y, Z)
							intersection_p.y = origin.y+direction.y*tmp_t_int;
							intersection_p.z = origin.z+direction.z*tmp_t_int;
							if (intersection_p.y <= tmpTrian->max1 && intersection_p.z <= tmpTrian->max2
								&& intersection_p.y >= tmpTrian->min1 && intersection_p.z >= tmpTrian->min2
								&& (tmpTrian->a1 * intersection_p.y + tmpTrian->b1 * intersection_p.z) + tmpTrian->d1 >= 0
								&& (tmpTrian->a2 * intersection_p.y + tmpTrian->b2 * intersection_p.z) + tmpTrian->d2 >= 0
								&& (tmpTrian->a3 * intersection_p.y + tmpTrian->b3 * intersection_p.z) + tmpTrian->d3 >= 0) {
									//Punkt jest wewnątrz trójkąta
									if (trian_hit[tmpTrian->ind] == 0) {
										transp = transp - (1-attributes[tmpTrian->atributeIndex].krcR);
										trian_hit[tmpTrian->ind] = 1;
									}
									//return transp;
									if (transp<EPS1) {
										delete[] trian_hit;
										return transp;
									}
							}
							break;
						case 1:
							//Rzutujemy na płaszczyznę (X, Z)
							intersection_p.x = origin.x+direction.x*tmp_t_int;
							intersection_p.z = origin.z+direction.z*tmp_t_int;
							if (intersection_p.x <= tmpTrian->max1 && intersection_p.z <= tmpTrian->max2
								&& intersection_p.x >= tmpTrian->min1 && intersection_p.z >= tmpTrian->min2
								&& (tmpTrian->a1 * intersection_p.x + tmpTrian->b1 * intersection_p.z) + tmpTrian->d1 >= 0
								&& (tmpTrian->a2 * intersection_p.x + tmpTrian->b2 * intersection_p.z) + tmpTrian->d2 >= 0
								&& (tmpTrian->a3 * intersection_p.x + tmpTrian->b3 * intersection_p.z) + tmpTrian->d3 >= 0) {
									//Punkt jest wewnątrz trójkąta
									if (trian_hit[tmpTrian->ind] == 0) {
										transp = transp - (1-attributes[tmpTrian->atributeIndex].krcR);
										trian_hit[tmpTrian->ind] = 1;
									}
									//return transp;
									if (transp<EPS1) {
										delete[] trian_hit;
										return transp;
									}
							}
							break;
						case 2:
							//Rzutujemy na płaszczyznę (X, Y)
							intersection_p.x = origin.x+direction.x*tmp_t_int;
							intersection_p.y = origin.y+direction.y*tmp_t_int;
							if (intersection_p.x <= tmpTrian->max1 && intersection_p.y <= tmpTrian->max2
								&& intersection_p.x >= tmpTrian->min1 && intersection_p.y >= tmpTrian->min2
								&& (tmpTrian->a1 * intersection_p.x + tmpTrian->b1 * intersection_p.y) + tmpTrian->d1 >= 0  
								&& (tmpTrian->a2 * intersection_p.x + tmpTrian->b2 * intersection_p.y) + tmpTrian->d2 >= 0
								&& (tmpTrian->a3 * intersection_p.x + tmpTrian->b3 * intersection_p.y) + tmpTrian->d3 >= 0) {
									//Punkt jest wewnątrz trójkąta
									if (trian_hit[tmpTrian->ind] == 0) {
										transp = transp - (1-attributes[tmpTrian->atributeIndex].krcR);
										trian_hit[tmpTrian->ind] = 1;
									}
									//return transp;
									if (transp<EPS1) {
										delete[] trian_hit;
										return transp;
									}
							}
							break;
						default: break;
					}
				}
			}
			//Przejdz do nastepnego woksela
			if (tx_n<ty_n) {
				if (tx_n< tz_n)
				{
					i+=di; tx = tx_n;
				} else
				{
					k+=dk; tz = tz_n;
				}
			} else {
				if (ty_n<tz_n) 
				{
					j+=dj; ty = ty_n;
				} else
				{
					k+=dk; tz = tz_n;
				}
			}
		}

		delete[] trian_hit;
		return transp;
	}

	BYTE* Scene::convertRGBToBMPBuffer(BYTE* buff, int width, int height, long* newsize) {
		// first make sure the parameters are valid
		if ( ( NULL == buff ) || ( width == 0 ) || ( height == 0 ) )
			return NULL;

		// now we have to find with how many bytes
		// we have to pad for the next DWORD boundary	

		int padding = 0;
		int scanlinebytes = width * 3;
		while ( ( scanlinebytes + padding ) % 4 != 0 )     // DWORD = 4 bytes
			padding++;
		// get the padded scanline width
		int psw = scanlinebytes + padding;

		// we can already store the size of the new padded buffer
		*newsize = height * psw;

		// and create new buffer
		BYTE* newbuf = new BYTE[*newsize];

		// fill the buffer with zero bytes then we dont have to add
		// extra padding zero bytes later on
		memset ( newbuf, 0, *newsize );

		// now we loop trough all bytes of the original buffer, 
		// swap the R and B bytes and the scanlines
		long bufpos = 0;   
		long newpos = 0;
		for ( int y = 0; y < height; y++ )
			for ( int x = 0; x < 3 * width; x+=3 )
			{
				bufpos = y * 3 * width + x;						 // position in original buffer
				newpos = ( height - y - 1 ) * psw + x;           // position in padded buffer

				newbuf[newpos] = buff[bufpos+2];       // swap r and b
				newbuf[newpos + 1] = buff[bufpos + 1]; // g stays
				newbuf[newpos + 2] = buff[bufpos];     // swap b and r
			}

			return newbuf;
	}

	bool Scene::saveBMP ( BYTE* buff, int width, int height, long paddedsize, LPCTSTR bmpfile )
	{
		// declare bmp structures 
		BITMAPFILEHEADER bmfh;
		BITMAPINFOHEADER info;

		// andinitialize them to zero
		memset ( &bmfh, 0, sizeof (BITMAPFILEHEADER ) );
		memset ( &info, 0, sizeof (BITMAPINFOHEADER ) );

		// fill the fileheader with data
		bmfh.bfType = 0x4d42;       // 0x4d42 = 'BM'
		bmfh.bfReserved1 = 0;
		bmfh.bfReserved2 = 0;
		bmfh.bfSize = sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER) + paddedsize;
		bmfh.bfOffBits = 0x36;		// number of bytes to start of bitmap bits

		// fill the infoheader

		info.biSize = sizeof(BITMAPINFOHEADER);
		info.biWidth = width;
		info.biHeight = height;
		info.biPlanes = 1;			// we only have one bitplane
		info.biBitCount = 24;		// RGB mode is 24 bits
		info.biCompression = BI_RGB;	
		info.biSizeImage = 0;		// can be 0 for 24 bit images
		info.biXPelsPerMeter = 0x0ec4;     // paint and PSP use this values
		info.biYPelsPerMeter = 0x0ec4;     
		info.biClrUsed = 0;			// we are in RGB mode and have no palette
		info.biClrImportant = 0;    // all colors are important

		// now we open the file to write to
		HANDLE file = CreateFile ( bmpfile , GENERIC_WRITE, FILE_SHARE_READ,
			NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL );
		if ( file == NULL )
		{
			CloseHandle ( file );
			return false;
		}

		// write file header
		unsigned long bwritten;
		if ( WriteFile ( file, &bmfh, sizeof ( BITMAPFILEHEADER ), &bwritten, NULL ) == false )
		{	
			CloseHandle ( file );
			return false;
		}
		// write infoheader
		if ( WriteFile ( file, &info, sizeof ( BITMAPINFOHEADER ), &bwritten, NULL ) == false )
		{	
			CloseHandle ( file );
			return false;
		}
		// write image data
		if ( WriteFile ( file, buff, paddedsize, &bwritten, NULL ) == false )
		{	
			CloseHandle ( file );
			return false;
		}

		// and close file
		CloseHandle ( file );

		return true;
	}

	void Scene::calcCoordsImp(float x, float y, Point3d pos, Point3d lookAt,
						   Vector3d& outUpLeftFst, Vector3d& outUpRightFst, 
						   Vector3d& outLoLeftFst, Vector3d& outLoRightFst) {
		float dx = lookAt.x - pos.x;
		float dy = lookAt.y - pos.y;
		float dz = lookAt.z - pos.z;
		float d = sqrt(dx*dx + dy*dy + dz*dz);
		float D = sqrt(dx*dx + dz*dz);

		float dy1 = y * D / (2.0f * d);
        float dx1 = (dx * dy * y) / (2.0f * d * D);
        float dz1 = (dz * dy * y) / (2.0f * d * D);
        float dx2 = (dz * x) / (2.0f * D);
        float dz2 = (dx * x) / (2.0f * D);

		outUpLeftFst = Vector3d(lookAt.x - dx1 + dx2, lookAt.y + dy1, lookAt.z - dz1 - dz2, false);
		outUpRightFst = Vector3d(lookAt.x - dx1 - dx2, lookAt.y + dy1, lookAt.z - dz1 + dz2, false);
		outLoLeftFst = Vector3d(lookAt.x + dx1 + dx2, lookAt.y - dy1, lookAt.z + dz1 - dz2, false);
		outLoRightFst = Vector3d(lookAt.x + dx1 - dx2, lookAt.y - dy1, lookAt.z + dz1 + dz2, false);
	}

	void Scene::rotatePoint(float x, float y, float xRes, float yRes, float rectSizeX, float rectSizeY,						 
						 float angle, Vector3d& upLeftFst, Vector3d& upRightFst, 
						 Vector3d& loLeftFst, Vector3d& loRightFst, Point3d& outPoint) {
		float scaleX = (float)x / xRes;
		float scaleY = (float)y / yRes;
        float rectDX = (scaleX - 0.5f) * rectSizeX;
        float rectDY = (scaleY - 0.5f) * rectSizeY;

		float alph = angle * PI / 180;
        float rotatedDX = rectDX * (float)cos(alph) - rectDY * (float)sin(alph);
        float rotatedDY = rectDX * (float)sin(alph) + rectDY * (float)cos(alph);		

        scaleX = rotatedDX / rectSizeX + 0.5f;
        scaleY = rotatedDY / rectSizeY + 0.5f;

		outPoint.x = upLeftFst.x + (upRightFst.x - upLeftFst.x) * scaleX + (loLeftFst.x - upLeftFst.x) * scaleY;
		outPoint.y = upLeftFst.y - (upLeftFst.y - loLeftFst.y) * scaleY;
		outPoint.z = upLeftFst.z + (upRightFst.z - upLeftFst.z) * scaleX + (loLeftFst.z - upLeftFst.z) * scaleY;
	}

	/*void Scene::print() {
		for (int i=0; i<trianSize; i++) {
			cout << triangles[i];
		}
		cout << camera;
	}

	void Scene::printLight() {
		for (int i=0; i<lightNumber; i++) {
			cout << lights[i];
		}
	}

	void Scene::printAttributes() {
		for (int i=0; i<attributesNuber; i++) {
			cout << i << endl << attributes[i] << "\n";
		}
	}

	void Scene::printMinMax() {
		cout << x_min_domain << " " << y_min_domain << " " << z_min_domain << "\n"
			 << x_max_domain << " " << y_max_domain << " " << z_max_domain << "\n"
			 << domain_width << " " << domain_height << " " << domain_depth << endl;
	}*/
}