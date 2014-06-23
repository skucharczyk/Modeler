#include "SceneLoader.h"
#include "File.h"
#include <iostream>

namespace loader {

	bool LoadScene(string file, vector<Vector3D>& outPoints, vector<Triangle>& outTriangles, vector<Part>& outParts, vector<Material>& outMaterials,
		vector<string>& outMatAssigns, vector<Light>& outLights, unsigned& outActiveCamera,vector<Camera>& outCams)
	{
		vector<string> text;
		ReadFileLines(file, text);
		int pointer = 0;

		if (text.size() < 1) {
			return false;
		}

		string outPointsNumLabel = GetAttribute(text[pointer], 0);
		if(outPointsNumLabel != "points_count")
		{
			return false;
		}
		unsigned outPointsNum = StringToNum<unsigned>(GetAttribute(text[pointer++], 1));

		for(unsigned i = 0; i < outPointsNum; ++i)
		{
			vector<string> attsPoint;
			GetAttributes(text[pointer++], attsPoint);
			outPoints.push_back(Vector3D(StringToNum<float>(attsPoint[0]),
									StringToNum<float>(attsPoint[1]),
									StringToNum<float>(attsPoint[2])));
		}

		string triangleNumLabel = GetAttribute(text[pointer], 0);
		if(triangleNumLabel != "triangles_count")
		{
			return false;
		}
		unsigned triangleNum = StringToNum<unsigned>(GetAttribute(text[pointer++], 1));

		for(unsigned i = 0; i < triangleNum; ++i)
		{
			vector<string> attsTriangle;
			GetAttributes(text[pointer++], attsTriangle);
			Triangle tr = Triangle(StringToNum<unsigned>(attsTriangle[0]), StringToNum<unsigned>(attsTriangle[1]), StringToNum<unsigned>(attsTriangle[2]));
			outTriangles.push_back(tr);
		}

		string outPartsNumLabel = GetAttribute(text[pointer], 0);
		if(outPartsNumLabel != "parts_count")
		{
			return false;
		}
		unsigned outPartsNum = StringToNum<unsigned>(GetAttribute(text[pointer++], 1));

		vector<unsigned> trPart = vector<unsigned>();

		vector<string> atts;
		GetAttributes(text[pointer], atts);
		for(unsigned i = 0; i < triangleNum; ++i)
		{
			trPart.push_back(StringToNum<unsigned>(atts[i]));
		}
		++pointer;

		for(unsigned i = 0; i < outPartsNum; ++i)
		{
			vector<int> partTriangles = vector<int>();

			for(unsigned j = 0; j < trPart.size(); ++j)
			{
				if(trPart[j] == i)
				{
					partTriangles.push_back(j);
				}
			}

			outParts.push_back(Part(partTriangles));
		}

		string matNumLabel = GetAttribute(text[pointer], 0);
		if(matNumLabel != "materials_count")
		{
			return false;
		}
		unsigned matNum = StringToNum<unsigned>(GetAttribute(text[pointer++], 1));

		for(unsigned i = 0; i < matNum; ++i)
		{
			vector<string> matName;
			GetAttributes(text[pointer], matName);
			if(matName[0] != "mat_name")
			{
				return false;
			}
			string name = CutFirstString(text[pointer]);
			++pointer;

			string rgbLabel = GetAttribute(text[pointer], 0);
			if(rgbLabel != "rgb")
			{
				return false;
			}
			float colorR = StringToNum<float>(GetAttribute(text[pointer], 1));
			float colorG = StringToNum<float>(GetAttribute(text[pointer], 2));
			float colorB = StringToNum<float>(GetAttribute(text[pointer++], 3));

			string kdCrLabel = GetAttribute(text[pointer], 0);
			float kdCr = StringToNum<float>(GetAttribute(text[pointer++], 1));
			if(kdCrLabel != "kdCr")
			{
				return false;
			}
			string kdCgLabel = GetAttribute(text[pointer], 0);
			float kdCg = StringToNum<float>(GetAttribute(text[pointer++], 1));
			if(kdCgLabel != "kdCg")
			{
				return false;
			}
			string kdCbLabel = GetAttribute(text[pointer], 0);
			float kdCb = StringToNum<float>(GetAttribute(text[pointer++], 1));
			if(kdCbLabel != "kdCb")
			{
				return false;
			}

			string ksCrLabel = GetAttribute(text[pointer], 0);
			float ksCr = StringToNum<float>(GetAttribute(text[pointer++], 1));
			if(ksCrLabel != "ksCr")
			{
				return false;
			}
			string ksCgLabel = GetAttribute(text[pointer], 0);
			float ksCg = StringToNum<float>(GetAttribute(text[pointer++], 1));
			if(ksCgLabel != "ksCg")
			{
				return false;
			}
			string ksCbLabel = GetAttribute(text[pointer], 0);
			float ksCb = StringToNum<float>(GetAttribute(text[pointer++], 1));
			if(ksCbLabel != "ksCb")
			{
				return false;
			}

			string krCrLabel = GetAttribute(text[pointer], 0);
			float krCr = StringToNum<float>(GetAttribute(text[pointer++], 1));
			if(krCrLabel != "krCr")
			{
				return false;
			}
			string krCgLabel = GetAttribute(text[pointer], 0);
			float krCg = StringToNum<float>(GetAttribute(text[pointer++], 1));
			if(krCgLabel != "krCg")
			{
				return false;
			}
			string krCbLabel = GetAttribute(text[pointer], 0);
			float krCb = StringToNum<float>(GetAttribute(text[pointer++], 1));
			if(krCbLabel != "krCb")
			{
				return false;
			}

			string kaCrLabel = GetAttribute(text[pointer], 0);
			float kaCr = StringToNum<float>(GetAttribute(text[pointer++], 1));
			if(kaCrLabel != "kaCr")
			{
				return false;
			}
			string kaCgLabel = GetAttribute(text[pointer], 0);
			float kaCg = StringToNum<float>(GetAttribute(text[pointer++], 1));
			if(kaCgLabel != "kaCg")
			{
				return false;
			}
			string kaCbLabel = GetAttribute(text[pointer], 0);
			float kaCb = StringToNum<float>(GetAttribute(text[pointer++], 1));
			if(kaCbLabel != "kaCb")
			{
				return false;
			}

			string gLabel = GetAttribute(text[pointer], 0);
			float g = StringToNum<float>(GetAttribute(text[pointer++], 1));
			if(gLabel != "g")
			{
				return false;
			}
			string nLabel = GetAttribute(text[pointer], 0);
			float n = StringToNum<float>(GetAttribute(text[pointer++], 1));
			if(nLabel != "n")
			{
				return false;
			}

			outMaterials.push_back(Material(name, colorR, colorG, colorB, kdCr, kdCg, kdCb, ksCr, ksCg, ksCb, krCr, krCg, krCb, kaCr, kaCg, kaCb, g, n));
		}

		for(unsigned i = 0; i < outPartsNum; ++i)
		{
			string mat = CutFirstString(text[pointer++]);
			outMatAssigns.push_back(mat);
		}

		string outLightsLabel = GetAttribute(text[pointer], 0);
		if(outLightsLabel != "lights_count")
		{
			return false;
		}
		unsigned outLightsNum = StringToNum<unsigned>(GetAttribute(text[pointer++], 1));

		for(unsigned i = 0; i < outLightsNum; ++i)
		{
			vector<string> lightName;
			GetAttributes(text[pointer], lightName);
			if(lightName[0] != "light_name")
			{
				return false;
			}
			string name = CutFirstString(text[pointer]);
			++pointer;

			string enabledLabel = GetAttribute(text[pointer], 0);
			if(enabledLabel != "enabled")
			{
				return false;
			}
			bool enabled = StringToNum<int>(GetAttribute(text[pointer++], 1)) == 1 ? true : false;

			string typeLabel = GetAttribute(text[pointer], 0);
			if(typeLabel != "light_type")
			{
				return false;
			}
			LightType type = Point;
			if(GetAttribute(text[pointer], 1) == "point")
			{
				type = Point;
			}
			else if(GetAttribute(text[pointer], 1) == "spot")
			{
				type = Spot;
			}
			else if(GetAttribute(text[pointer], 1) == "goniometric")
			{
				type = Goniometric;
			}
			pointer++;

			string colorLabel = GetAttribute(text[pointer], 0);
			if(colorLabel != "rgb")
			{
				return false;
			}
			float colorR = StringToNum<float>(GetAttribute(text[pointer], 1));
			float colorG = StringToNum<float>(GetAttribute(text[pointer], 2));
			float colorB = StringToNum<float>(GetAttribute(text[pointer++], 3));

			string powerLabel = GetAttribute(text[pointer], 0);
			if(powerLabel != "power")
			{
				return false;
			}
			float power = StringToNum<float>(GetAttribute(text[pointer++], 1));

			string posLabel = GetAttribute(text[pointer], 0);
			if(posLabel != "pos")
			{
				return false;
			}
			float posX = StringToNum<float>(GetAttribute(text[pointer], 1));
			float posY = StringToNum<float>(GetAttribute(text[pointer], 2));			
			float posZ = StringToNum<float>(GetAttribute(text[pointer++], 3));
			Vector3D pos = Vector3D(posX, posY, posZ);

			string dirLabel = GetAttribute(text[pointer], 0);
			if(dirLabel != "dir")
			{
				return false;
			}
			float dirX = StringToNum<float>(GetAttribute(text[pointer], 1));
			float dirY = StringToNum<float>(GetAttribute(text[pointer], 2));
			float dirZ = StringToNum<float>(GetAttribute(text[pointer++], 3));
			Vector3D dir = Vector3D(dirX,
				dirY,
				dirZ);

			string innerAngleLabel = GetAttribute(text[pointer], 0);
			if(innerAngleLabel != "inner_angle")
			{
				return false;
			}
			float innerAngle = StringToNum<float>(GetAttribute(text[pointer++], 1));
			string outerAngleLabel = GetAttribute(text[pointer], 0);
			if(outerAngleLabel != "outer_angle")
			{
				return false;
			}
			float outerAngle = StringToNum<float>(GetAttribute(text[pointer++], 1));

			vector<pair<float, float> > goniometric = vector<pair<float, float> >();

			string gonioNumLabel = GetAttribute(text[pointer], 0);
			if(gonioNumLabel != "gonio_count")
			{
				return false;
			}
			unsigned gonioNum = StringToNum<unsigned>(GetAttribute(text[pointer++], 1));

			for(unsigned j = 0; j < gonioNum; ++j)
			{
				float gonioIndex = StringToNum<float>(GetAttribute(text[pointer], 0));
				float gonioValue = StringToNum<float>(GetAttribute(text[pointer++], 1));

				goniometric.push_back(make_pair(gonioIndex, gonioValue));
			}

			outLights.push_back(Light(name, type, enabled, colorR, colorG, colorB, power, pos));

			outLights[outLights.size() - 1].direction = Vector3D(dir.x, dir.y, dir.z);
			outLights[outLights.size() - 1].innerAngle = innerAngle;
			outLights[outLights.size() - 1].outerAngle = outerAngle;
			outLights[outLights.size() - 1].goniometric = goniometric;
		}

		string outCamsNumLabel = GetAttribute(text[pointer], 0);
		if(outCamsNumLabel != "cams_count")
		{
			return false;
		}
		unsigned outCamsNum = StringToNum<unsigned>(GetAttribute(text[pointer++], 1));

		string activeCamLabel = GetAttribute(text[pointer], 0);
		if(activeCamLabel != "active")
		{
			return false;
		}
		unsigned activeCam = StringToNum<unsigned>(GetAttribute(text[pointer++], 1));
		outActiveCamera = activeCam;

		for(unsigned i = 0; i < outCamsNum; ++i)
		{
			string nameLabel = GetAttribute(text[pointer], 0);
			if(nameLabel != "cam_name")
			{
				return false;
			}
			string name = CutFirstString(text[pointer]);
			++pointer;

			string resLabel = GetAttribute(text[pointer], 0);
			if(resLabel != "resolution")
			{
				return false;
			}
			//std::cout << GetAttribute(text[pointer], 1) << " " << GetAttribute(text[pointer], 2) << "\n";
			int resX = StringToNum<int>(GetAttribute(text[pointer], 1));
			int resY = StringToNum<int>(GetAttribute(text[pointer++], 2));
			pair<int, int> res = pair<int, int>(resX, resY);

			string posLabel = GetAttribute(text[pointer], 0);
			if(posLabel != "pos")
			{
				return false;
			}
			float x = StringToNum<float>(GetAttribute(text[pointer], 1));
			float y = StringToNum<float>(GetAttribute(text[pointer], 2));
			float z = StringToNum<float>(GetAttribute(text[pointer++], 3));
			Vector3D pos = Vector3D(x, y, z);

			string lookAtLabel = GetAttribute(text[pointer], 0);
			if(lookAtLabel != "lookAt")
			{
				return false;
			}
			x = StringToNum<float>(GetAttribute(text[pointer], 1));
			y = StringToNum<float>(GetAttribute(text[pointer], 2));
			z = StringToNum<float>(GetAttribute(text[pointer++], 3));
			Vector3D lookAt = Vector3D(x, y, z);

			string fovAngleLabel = GetAttribute(text[pointer], 0);
			if(fovAngleLabel != "fov")
			{
				return false;
			}
			float fovAngle = StringToNum<float>(GetAttribute(text[pointer++], 1));
			string rotateAngleLabel = GetAttribute(text[pointer], 0);
			if(rotateAngleLabel != "rotation")
			{
				return false;
			}
			float rotateAngle = StringToNum<float>(GetAttribute(text[pointer++], 1));

			outCams.push_back(Camera(name, res.first, res.second, pos, lookAt, fovAngle, rotateAngle));
		}

		return true;
	}
}