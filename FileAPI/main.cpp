#include "SceneLoader.h"

using namespace loader;

int main()
{
	vector<Vector3D> points;
	vector<Triangle> triangles;
	vector<Part> parts;
	vector<Material> materials;
	vector<string> matAssigns;
	vector<Light> lights;
	unsigned activeCamera;
	vector<Camera> cams;

	LoadScene("C:\\Users\\Lycha\\Documents\\Visual Studio 2010\\Projects\\Modeler\\Release\\scena.scn", 
		points, triangles, parts, materials, matAssigns, lights, activeCamera, cams);

	return 0;
}