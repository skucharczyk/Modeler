#ifndef SCENELOADER_H
#define  SCENELOADER_H

#include <vector>
#include <string>
#include "Structures.h"

using namespace std;
namespace loader {

	bool LoadScene(string file, vector<Vector3D>& outPoints, vector<Triangle>& outTriangles, vector<Part>& outParts, vector<Material>& outMaterials,
		vector<string>& outMatAssigns, vector<Light>& outLights, unsigned& outActiveCamera,vector<Camera>& outCams);
}

#endif