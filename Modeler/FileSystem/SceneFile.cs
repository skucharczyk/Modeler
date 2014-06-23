using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modeler.FileSystem;
using SlimDX;
using System.Globalization;
using Modeler.Graphics;

namespace Modeler.Data.Scene
{
    partial class Scene
    {
        public static Scene ReadSceneFromFile(string file)
        {
            Scene scene = new Scene();

            try
            {
                scene.filePath = file;
                scene.modified = false;

                List<string> text = File.ReadFileLines(file);
                int pointer = 0;

                string pointsNumLabel = File.GetAttribute(text[pointer], 0);
                if(pointsNumLabel != "points_count")
                {
                    return null;
                }
                uint pointsNum = uint.Parse(File.GetAttribute(text[pointer++], 1));

                List<Vector3D> points = new List<Vector3D>();

                for(int i = 0; i < pointsNum; ++i)
                {
                    string[] attsPoint = File.GetAttributes(text[pointer++]);
                    points.Add(new Vector3D(float.Parse(attsPoint[0], CultureInfo.InvariantCulture),
                                            float.Parse(attsPoint[1], CultureInfo.InvariantCulture),
                                            float.Parse(attsPoint[2], CultureInfo.InvariantCulture)));
                }

                string triangleNumLabel = File.GetAttribute(text[pointer], 0);
                if(triangleNumLabel != "triangles_count")
                {
                    return null;
                }
                uint triangleNum = uint.Parse(File.GetAttribute(text[pointer++], 1));

                List<Triangle> triangles = new List<Triangle>();

                for(int i = 0; i < triangleNum; ++i)
                {
                    string[] attsTriangle = File.GetAttributes(text[pointer++]);
                    triangles.Add(new Triangle(uint.Parse(attsTriangle[0]), uint.Parse(attsTriangle[1]), uint.Parse(attsTriangle[2])));
                }

                string partsNumLabel = File.GetAttribute(text[pointer], 0);
                if(partsNumLabel != "parts_count")
                {
                    return null;
                }
                uint partsNum = uint.Parse(File.GetAttribute(text[pointer++], 1));

                List<uint> trPart = new List<uint>();

                string[] atts = File.GetAttributes(text[pointer]);
                for(int i = 0; i < triangleNum; ++i)
                {
                    trPart.Add(uint.Parse(atts[i]));
                }
                ++pointer;

                List<Part> parts = new List<Part>();

                for(int i = 0; i < partsNum; ++i)
                {
                    List<int> partTriangles = new List<int>();

                    for(int j = 0; j < trPart.Count; ++j)
                    {
                        if(trPart[j] == i)
                        {
                            partTriangles.Add(j);
                        }
                    }

                    parts.Add(new Part(partTriangles));
                }


                string matNumLabel = File.GetAttribute(text[pointer], 0);
                if(matNumLabel != "materials_count")
                {
                    return null;
                }
                uint matNum = uint.Parse(File.GetAttribute(text[pointer++], 1));

                List<Material_> materials = new List<Material_>();

                for(int i = 0; i < matNum; ++i)
                {
                    string[] matName = File.GetAttributes(text[pointer]);
                    if(matName[0] != "mat_name")
                    {
                        return null;
                    }
                    string name = File.CutFirstString(text[pointer]);
                    ++pointer;

                    string rgbLabel = File.GetAttribute(text[pointer], 0);
                    if(rgbLabel != "rgb")
                    {
                        return null;
                    }
                    float colorR = float.Parse(File.GetAttribute(text[pointer], 1), CultureInfo.InvariantCulture);
                    float colorG = float.Parse(File.GetAttribute(text[pointer], 2), CultureInfo.InvariantCulture);
                    float colorB = float.Parse(File.GetAttribute(text[pointer++], 3), CultureInfo.InvariantCulture);

                    string kdCrLabel = File.GetAttribute(text[pointer], 0);
                    float kdCr = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(kdCrLabel != "kdCr")
                    {
                        return null;
                    }
                    string kdCgLabel = File.GetAttribute(text[pointer], 0);
                    float kdCg = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(kdCgLabel != "kdCg")
                    {
                        return null;
                    }
                    string kdCbLabel = File.GetAttribute(text[pointer], 0);
                    float kdCb = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(kdCbLabel != "kdCb")
                    {
                        return null;
                    }

                    string ksCrLabel = File.GetAttribute(text[pointer], 0);
                    float ksCr = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(ksCrLabel != "ksCr")
                    {
                        return null;
                    }
                    string ksCgLabel = File.GetAttribute(text[pointer], 0);
                    float ksCg = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(ksCgLabel != "ksCg")
                    {
                        return null;
                    }
                    string ksCbLabel = File.GetAttribute(text[pointer], 0);
                    float ksCb = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(ksCbLabel != "ksCb")
                    {
                        return null;
                    }

                    string krCrLabel = File.GetAttribute(text[pointer], 0);
                    float krCr = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(krCrLabel != "krCr")
                    {
                        return null;
                    }
                    string krCgLabel = File.GetAttribute(text[pointer], 0);
                    float krCg = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(krCgLabel != "krCg")
                    {
                        return null;
                    }
                    string krCbLabel = File.GetAttribute(text[pointer], 0);
                    float krCb = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(krCbLabel != "krCb")
                    {
                        return null;
                    }

                    string kaCrLabel = File.GetAttribute(text[pointer], 0);
                    float kaCr = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(kaCrLabel != "kaCr")
                    {
                        return null;
                    }
                    string kaCgLabel = File.GetAttribute(text[pointer], 0);
                    float kaCg = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(kaCgLabel != "kaCg")
                    {
                        return null;
                    }
                    string kaCbLabel = File.GetAttribute(text[pointer], 0);
                    float kaCb = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(kaCbLabel != "kaCb")
                    {
                        return null;
                    }

                    string gLabel = File.GetAttribute(text[pointer], 0);
                    float g = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(gLabel != "g")
                    {
                        return null;
                    }
                    string nLabel = File.GetAttribute(text[pointer], 0);
                    float n = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(nLabel != "n")
                    {
                        return null;
                    }

                    materials.Add(new Material_(name, colorR, colorG, colorB, kdCr, kdCg, kdCb, ksCr, ksCg, ksCb, krCr, krCg, krCb, kaCr, kaCg, kaCb, g, n));
                }

                List<string> matAssign = new List<string>();

                for(int i = 0; i < partsNum; ++i)
                {
                    string mat = File.CutFirstString(text[pointer++]);
                    matAssign.Add(mat);
                }

                string lightsLabel = File.GetAttribute(text[pointer], 0);
                if(lightsLabel != "lights_count")
                {
                    return null;
                }
                uint lightsNum = uint.Parse(File.GetAttribute(text[pointer++], 1));
                
                List<Light_> lights = new List<Light_>();

                for(int i = 0; i < lightsNum; ++i)
                {
                    string[] lightName = File.GetAttributes(text[pointer]);
                    if(lightName[0] != "light_name")
                    {
                        return null;
                    }
                    string name = File.CutFirstString(text[pointer]);
                    ++pointer;

                    string enabledLabel = File.GetAttribute(text[pointer], 0);
                    if(enabledLabel != "enabled")
                    {
                        return null;
                    }
                    bool enabled = int.Parse(File.GetAttribute(text[pointer++], 1)) == 1 ? true : false;

                    string typeLabel = File.GetAttribute(text[pointer], 0);
                    if(typeLabel != "light_type")
                    {
                        return null;
                    }
                    Light_Type type = Light_Type.Point;
                    switch(File.GetAttribute(text[pointer++], 1))
                    {
                        case "point":
                            type = Light_Type.Point;
                            break;

                        case "spot":
                            type = Light_Type.Spot;
                            break;

                        case "goniometric":
                            type = Light_Type.Goniometric;
                            break;
                    }

                    string colorLabel = File.GetAttribute(text[pointer], 0);
                    if(colorLabel != "rgb")
                    {
                        return null;
                    }
                    float colorR = float.Parse(File.GetAttribute(text[pointer], 1), CultureInfo.InvariantCulture);
                    float colorG = float.Parse(File.GetAttribute(text[pointer], 2), CultureInfo.InvariantCulture);
                    float colorB = float.Parse(File.GetAttribute(text[pointer++], 3), CultureInfo.InvariantCulture);

                    string powerLabel = File.GetAttribute(text[pointer], 0);
                    if(powerLabel != "power")
                    {
                        return null;
                    }
                    float power = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);

                    string posLabel = File.GetAttribute(text[pointer], 0);
                    if(posLabel != "pos")
                    {
                        return null;
                    }
                    Vector3 pos = new Vector3(float.Parse(File.GetAttribute(text[pointer], 1), CultureInfo.InvariantCulture),
                        float.Parse(File.GetAttribute(text[pointer], 2), CultureInfo.InvariantCulture),
                        float.Parse(File.GetAttribute(text[pointer++], 3), CultureInfo.InvariantCulture));

                    string dirLabel = File.GetAttribute(text[pointer], 0);
                    if(dirLabel != "dir")
                    {
                        return null;
                    }
                    Vector3 dir = new Vector3(float.Parse(File.GetAttribute(text[pointer], 1), CultureInfo.InvariantCulture),
                        float.Parse(File.GetAttribute(text[pointer], 2), CultureInfo.InvariantCulture),
                        float.Parse(File.GetAttribute(text[pointer++], 3), CultureInfo.InvariantCulture));

                    string innerAngleLabel = File.GetAttribute(text[pointer], 0);
                    if(innerAngleLabel != "inner_angle")
                    {
                        return null;
                    }
                    float innerAngle = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    string outerAngleLabel = File.GetAttribute(text[pointer], 0);
                    if(outerAngleLabel != "outer_angle")
                    {
                        return null;
                    }
                    float outerAngle = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);

                    SortedList<float, float> goniometric = new SortedList<float, float>();

                    string gonioNumLabel = File.GetAttribute(text[pointer], 0);
                    if(gonioNumLabel != "gonio_count")
                    {
                        return null;
                    }
                    uint gonioNum = uint.Parse(File.GetAttribute(text[pointer++], 1));

                    for(int j = 0; j < gonioNum; ++j)
                    {
                        float gonioIndex = float.Parse(File.GetAttribute(text[pointer], 0), CultureInfo.InvariantCulture);
                        float gonioValue = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);

                        goniometric.Add(gonioIndex, gonioValue);
                    }

                    lights.Add(new Light_(name, type, enabled, colorR, colorG, colorB, power, pos));

                    lights[lights.Count - 1].direction = new Vector3(dir.X, dir.Y, dir.Z);
                    lights[lights.Count - 1].innerAngle = innerAngle;
                    lights[lights.Count - 1].outerAngle = outerAngle;
                    lights[lights.Count - 1].goniometric = goniometric;
                }

                string camsNumLabel = File.GetAttribute(text[pointer], 0);
                if(camsNumLabel != "cams_count")
                {
                    return null;
                }
                uint camsNum = uint.Parse(File.GetAttribute(text[pointer++], 1));

                List<Camera> cams = new List<Camera>();

                string activeCamLabel = File.GetAttribute(text[pointer], 0);
                if(activeCamLabel != "active")
                {
                    return null;
                }
                uint activeCam = uint.Parse(File.GetAttribute(text[pointer++], 1));

                for(int i = 0; i < camsNum; ++i)
                {
                    string nameLabel = File.GetAttribute(text[pointer], 0);
                    if(nameLabel != "cam_name")
                    {
                        return null;
                    }
                    string name = File.CutFirstString(text[pointer]);
                    ++pointer;

                    string resLabel = File.GetAttribute(text[pointer], 0);
                    if(resLabel != "resolution")
                    {
                        return null;
                    }
                    Pair<int, int> res = new Pair<int, int>(int.Parse(File.GetAttribute(text[pointer], 1)), int.Parse(File.GetAttribute(text[pointer++], 2)));

                    string posLabel = File.GetAttribute(text[pointer], 0);
                    if(posLabel != "pos")
                    {
                        return null;
                    }
                    Vector3 pos = new Vector3(float.Parse(File.GetAttribute(text[pointer], 1), CultureInfo.InvariantCulture),
                        float.Parse(File.GetAttribute(text[pointer], 2), CultureInfo.InvariantCulture),
                        float.Parse(File.GetAttribute(text[pointer++], 3), CultureInfo.InvariantCulture));

                    string lookAtLabel = File.GetAttribute(text[pointer], 0);
                    if(lookAtLabel != "lookAt")
                    {
                        return null;
                    }
                    Vector3 lookAt = new Vector3(float.Parse(File.GetAttribute(text[pointer], 1), CultureInfo.InvariantCulture),
                        float.Parse(File.GetAttribute(text[pointer], 2), CultureInfo.InvariantCulture),
                        float.Parse(File.GetAttribute(text[pointer++], 3), CultureInfo.InvariantCulture));

                    string fovAngleLabel = File.GetAttribute(text[pointer], 0);
                    if(fovAngleLabel != "fov")
                    {
                        return null;
                    }
                    float fovAngle = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    string rotateAngleLabel = File.GetAttribute(text[pointer], 0);
                    if(rotateAngleLabel != "rotation")
                    {
                        return null;
                    }
                    float rotateAngle = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);

                    cams.Add(new Camera(name, res.First, res.Second, pos, lookAt, fovAngle, rotateAngle));
                }

                HierarchyNode root = new HierarchyNode("Hierarchy");

                ReadHierarchy(root, lights, text, ref pointer);
                if(root == null)
                {
                    return null;
                }

                Hierarchy hierarchy = new Hierarchy();
                hierarchy.objects = root.hObjects;

                scene.points = points;
                scene.triangles = triangles;
                scene.parts = parts;
                scene.materials = materials;
                scene.materialAssign = matAssign;
                scene.lights = lights;
                scene.cams = cams;
                scene.activeCamera = (int)activeCam;
                scene.hierarchy = hierarchy;
            }
            catch(Exception)
            {
                return null;
            }

            return scene;
        }

        private static void ReadHierarchy(HierarchyNode node, List<Light_> lights, List<string> text, ref int pointer)
        {
            string objNumLabel = File.GetAttribute(text[pointer], 0);
            if(objNumLabel != "node_count")
            {
                throw new Exception();
            }
            uint objNum = uint.Parse(File.GetAttribute(text[pointer++], 1));

            for(int i = 0; i < objNum; ++i)
            {
                HierarchyObject newObject = null;
                string objStrLabel = File.GetAttribute(text[pointer], 0);
                if(objStrLabel != "hier_type")
                {
                    throw new Exception();
                }
                string objStr = File.GetAttribute(text[pointer++], 1);

                string[] objNameAtts = File.GetAttributes(text[pointer]);
                if(objNameAtts[0] != "hier_name")
                {
                    throw new Exception();
                }
                string objName = objName = File.CutFirstString(text[pointer]);
                ++pointer;

                switch(objStr)
                {
                    case "Mesh":
                        newObject = new HierarchyMesh(objName);
                        node.hObjects.Add(newObject);

                        string triangleNumLabel = File.GetAttribute(text[pointer], 0);
                        if(triangleNumLabel != "triangle_count")
                        {
                            new Exception();
                        }
                        uint triangleNum = uint.Parse(File.GetAttribute(text[pointer++], 1));
                        List<uint> triangles = new List<uint>();

                        string[] triangs = File.GetAttributes(text[pointer]);
                        for(int j = 0; j < triangleNum; ++j)
                        {
                            triangles.Add(uint.Parse(triangs[j]));
                        }
                        ++pointer;

                        ((HierarchyMesh)newObject).triangles = triangles;
                        break;

                    case "Light":
                        string lightIdxLabel = File.GetAttribute(text[pointer], 0);
                        if (lightIdxLabel != "light_index")
                        {
                            new Exception();
                        }
                        int lightIdx = int.Parse(File.GetAttribute(text[pointer++], 1));
                        newObject = new HierarchyLight(objName, lightIdx);
                        node.hObjects.Add(newObject);
                        break;

                    case "Node":
                        newObject = new HierarchyNode(objName);
                        node.hObjects.Add(newObject);

                        ReadHierarchy((HierarchyNode)newObject, lights, text, ref pointer);
                        break;
                }
            }
        }

        private static Light_ GetLightByName(List<Light_> lights, string name)
        {
            foreach(Light_ light in lights)
            {
                if(light.name == name)
                {
                    return light;
                }
            }

            return null;
        }

        public void SaveSceneFile(string file)
        {
            filePath = file;
            modified = false;

            List<string> text = new List<string>();

            text.Add("");
            text.Add("// punkty");
            text.Add("points_count " + points.Count.ToString());
            foreach(Vector3D point in points)
            {
                text.Add(point.x.ToString(CultureInfo.InvariantCulture) + " " + point.y.ToString(CultureInfo.InvariantCulture) + " " + point.z.ToString(CultureInfo.InvariantCulture));
            }
            text.Add("");
            text.Add("");

            text.Add("// trójkąty");
            text.Add("triangles_count " + triangles.Count.ToString());
            foreach(Triangle triangle in triangles)
            {
                text.Add(triangle.p1.ToString() + " " + triangle.p2.ToString() + " " + triangle.p3.ToString());
            }
            text.Add("");
            text.Add("");

            text.Add("// części");
            text.Add("parts_count " + parts.Count.ToString());

            int[] trParts = new int[triangles.Count];
            for(int i = 0; i < parts.Count; ++i)
            {
                foreach(int triangle in parts[i].triangles)
                {
                    trParts[triangle] = i;
                }
            }

            StringBuilder partsStr = new StringBuilder();
            foreach(int trPart in trParts)
            {
                partsStr.Append(trPart.ToString() + " ");
            }
            text.Add(partsStr.ToString());
            text.Add("");
            text.Add("");

            text.Add("// materiały");
            text.Add("materials_count " + materials.Count.ToString());

            foreach(Material_ mat in materials)
            {
                text.Add("");

                text.Add("mat_name " + mat.name);
                text.Add("rgb " + mat.colorR.ToString(CultureInfo.InvariantCulture) + " " + mat.colorG.ToString(CultureInfo.InvariantCulture) + " " + mat.colorB.ToString(CultureInfo.InvariantCulture));
                text.Add("kdCr " + mat.kdcR.ToString(CultureInfo.InvariantCulture));
                text.Add("kdCg " + mat.kdcG.ToString(CultureInfo.InvariantCulture));
                text.Add("kdCb " + mat.kdcB.ToString(CultureInfo.InvariantCulture));
                text.Add("ksCr " + mat.kscR.ToString(CultureInfo.InvariantCulture));
                text.Add("ksCg " + mat.kscG.ToString(CultureInfo.InvariantCulture));
                text.Add("ksCb " + mat.kscB.ToString(CultureInfo.InvariantCulture));
                text.Add("krCr " + mat.krcR.ToString(CultureInfo.InvariantCulture));
                text.Add("krCg " + mat.krcG.ToString(CultureInfo.InvariantCulture));
                text.Add("krCb " + mat.krcB.ToString(CultureInfo.InvariantCulture));
                text.Add("kaCr " + mat.kacR.ToString(CultureInfo.InvariantCulture));
                text.Add("kaCg " + mat.kacG.ToString(CultureInfo.InvariantCulture));
                text.Add("kaCb " + mat.kacB.ToString(CultureInfo.InvariantCulture));
                text.Add("g " + mat.g.ToString(CultureInfo.InvariantCulture));
                text.Add("n " + mat.n.ToString(CultureInfo.InvariantCulture));
            }
            text.Add("");
            text.Add("");

            text.Add("// przypisanie materiałów");
            for(int i = 0; i < materialAssign.Count; ++i)
            {
                text.Add(i.ToString() + " " + materialAssign[i]);
            }
            text.Add("");
            text.Add("");

            text.Add("// światła");
            text.Add("lights_count " + lights.Count.ToString());

            foreach(Light_ light in lights)
            {
                text.Add("");

                text.Add("light_name " + light.name);
                text.Add("enabled " + (light.enabled == true ? "1" : "0"));
                text.Add("light_type " + light.type.ToString().ToLowerInvariant());
                text.Add("rgb " + light.colorR.ToString(CultureInfo.InvariantCulture) + " " + light.colorG.ToString(CultureInfo.InvariantCulture) + " " + light.colorB.ToString(CultureInfo.InvariantCulture));
                text.Add("power " + light.power);
                text.Add("pos " + light.position.X.ToString(CultureInfo.InvariantCulture) + " " + light.position.Y.ToString(CultureInfo.InvariantCulture) + " " + light.position.Z.ToString(CultureInfo.InvariantCulture));
                text.Add("dir " + light.direction.X.ToString(CultureInfo.InvariantCulture) + " " + light.direction.Y.ToString(CultureInfo.InvariantCulture) + " " + light.direction.Z.ToString(CultureInfo.InvariantCulture));
                text.Add("inner_angle " + light.innerAngle);
                text.Add("outer_angle " + light.outerAngle);
                text.Add("gonio_count " + light.goniometric.Count.ToString());

                for(int i = 0; i < light.goniometric.Count; ++i)
                {
                    text.Add(light.goniometric.Keys[i].ToString(CultureInfo.InvariantCulture) + " " + light.goniometric.Values[i].ToString(CultureInfo.InvariantCulture));
                }
            }
            text.Add("");
            text.Add("");

            text.Add("// kamery");
            text.Add("cams_count " + cams.Count.ToString());
            text.Add("active " + activeCamera.ToString());

            foreach(Camera cam in cams)
            {
                text.Add("");

                text.Add("cam_name " + cam.name);
                text.Add("resolution " + cam.resolutionX.ToString(CultureInfo.InvariantCulture) + " " + cam.resolutionY.ToString(CultureInfo.InvariantCulture));
                text.Add("pos " + cam.position.X.ToString(CultureInfo.InvariantCulture) + " " + cam.position.Y.ToString(CultureInfo.InvariantCulture) + " " + cam.position.Z.ToString(CultureInfo.InvariantCulture));
                text.Add("lookAt " + cam.lookAt.X.ToString(CultureInfo.InvariantCulture) + " " + cam.lookAt.Y.ToString(CultureInfo.InvariantCulture) + " " + cam.lookAt.Z.ToString(CultureInfo.InvariantCulture));
                text.Add("fov " + cam.fovAngle.ToString(CultureInfo.InvariantCulture));
                text.Add("rotation " + cam.rotateAngle.ToString(CultureInfo.InvariantCulture));
            }
            text.Add("");
            text.Add("");

            text.Add("// hierarchia");
            HierarchyNode node = new HierarchyNode("Hierarchy");
            node.hObjects = hierarchy.objects;

            SaveHierarchy(node, text, 0);

            File.SaveFileLines(file, text);
        }

        private void SaveHierarchy(HierarchyNode node, List<string> text, uint level)
        {
            text.Add(InsertTabs(level) + "node_count " + node.hObjects.Count.ToString());            

            foreach(HierarchyObject obj in node.hObjects)
            {
                text.Add(InsertTabs(level) + "");

                string type = "";
                if(obj is HierarchyNode)
                {
                    type = "Node";
                }
                else if(obj is HierarchyMesh)
                {
                    type = "Mesh";
                }
                else if(obj is HierarchyLight)
                {
                    type = "Light";
                }

                text.Add(InsertTabs(level) + "hier_type " + type);
                text.Add(InsertTabs(level) + "hier_name " + obj.name);

                if(obj is HierarchyMesh)
                {
                    text.Add(InsertTabs(level) + "triangle_count " + ((HierarchyMesh)obj).triangles.Count.ToString());

                    StringBuilder line = new StringBuilder();
                    foreach(uint triangle in ((HierarchyMesh)obj).triangles)
                    {
                        line.Append(triangle.ToString() + " ");
                    }
                    text.Add(InsertTabs(level) + line.ToString());
                }
                else if(obj is HierarchyLight)
                {
                    text.Add(InsertTabs(level) + "light_index " + ((HierarchyLight)obj).lightIndex);
                }
                else if(obj is HierarchyNode)
                {
                    SaveHierarchy((HierarchyNode)obj, text, level + 1);
                }
            }
        }

        private string InsertTabs(uint level)
        {
            string line = "";
            for(int i = 0; i < level; ++i)
            {
                line += '\t';
            }

            return line;
        }
    }
}
