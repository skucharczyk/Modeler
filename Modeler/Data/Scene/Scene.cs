using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SlimDX;
using Modeler.Graphics;
using Modeler.Data.Elements;

namespace Modeler.Data.Scene
{
    partial class Scene
    {
        public List<Vector3D> points;
        public List<Triangle> triangles;
        public List<Part> parts;
        public List<Material_> materials;
        public List<String> materialAssign;
        public List<Light_> lights;
        public List<Camera> cams;
        public int activeCamera;
        public Hierarchy hierarchy;
        public HierarchyNode selectedHierObj;
        //public bool addWithHierarchy = false;  //

        public List<HierarchyMesh> selTriangles;
        public List<int> selLights;   // zmieniono na indeks światła w lights
        public List<int> selCams;     // zmieniono na indeks kamery w cams

        public List<Vector3D> normals;
        public bool cameraRemoved = false; //tymczasowy pomocniczy parametr do wstawiania kamery
        public bool cameraMoved = false; //pomocniczy parametr do aktualizowania pozycji aktywnej kamery w panelu kamery
        public bool hierarchyChange = false; //pomocniczy parametr aktualizacji wyświetlania hierarchii

        public bool modified = false; // okresla, czy od ostatniego zapisu zostalo cos zmienione w scenie
        public string filePath = null; // sciezka do pliku, w ktorym zapisana zostala scena

        public Scene()
        {
            points = new List<Vector3D>();
            triangles = new List<Triangle>();
            parts = new List<Part>();
            materials = new List<Material_>();
            materialAssign = new List<String>();
            lights = new List<Light_>();
            cams = new List<Camera>();
            hierarchy = new Hierarchy();

            selTriangles = new List<HierarchyMesh>();

            selLights = new List<int>();
            selCams = new List<int>();

            //Light_ defaultLight = new Light_("default", Light_Type.Point, true, 1, 1, 1, 30, new Vector3(1, 1, 1));
            //lights.Add(defaultLight);

            //HierarchyLight defaultHierLight = new HierarchyLight(defaultLight.name, lights.IndexOf(defaultLight));
            //hierarchy.objects.Add(defaultHierLight);

            cams.Add(new Camera("Cam1", 800, 600, new Vector3(4.9f, 2, 5), new Vector3(3.7f, 1.5f, 3.5f), 60, 0));
            activeCamera = 0;
            hierarchyChange = true;
            normals = new List<Vector3D>();
        }

        public Scene(Scene copy)
        {
            points = new List<Vector3D>();
            foreach (Vector3D copyElem in copy.points) points.Add(new Vector3D(copyElem));
            triangles = new List<Triangle>();
            foreach (Triangle copyElem in copy.triangles) triangles.Add(new Triangle(copyElem.p1,copyElem.p2,copyElem.p3));
            parts = new List<Part>();
            foreach (Part copyElem in copy.parts) parts.Add(new Part(copyElem.triangles));
            materials = new List<Material_>();
            foreach (Material_ copyElem in copy.materials) materials.Add(new Material_(copyElem));
            materialAssign = new List<String>(copy.materialAssign);
            lights = new List<Light_>();
            foreach (Light_ copyElem in copy.lights) lights.Add(new Light_(copyElem));
            cams = new List<Camera>();
            foreach (Camera copyElem in copy.cams)
            {
                cams.Add(new Camera(copyElem));
            }
            hierarchy = new Hierarchy();
            hierarchy.objects = new List<HierarchyObject>();
            foreach (HierarchyObject hierarchyObject in copy.hierarchy.objects)
            {
                if (hierarchyObject is HierarchyNode)
                {
                    hierarchy.objects.Add(new HierarchyNode((HierarchyNode)hierarchyObject));
                }
                else if (hierarchyObject is HierarchyMesh)
                {
                    hierarchy.objects.Add(new HierarchyMesh((HierarchyMesh)hierarchyObject));
                }
                else if (hierarchyObject is HierarchyLight)
                {
                    hierarchy.objects.Add(new HierarchyLight((HierarchyLight)hierarchyObject));
                }
            }

            selTriangles = new List<HierarchyMesh>(copy.selTriangles);
            selLights = new List<int>(copy.selLights);
            selCams = new List<int>(copy.selCams);

            normals = new List<Vector3D>(copy.normals);

            activeCamera = copy.activeCamera;
            hierarchyChange = true;
        }

        public void ClearSelectedTriangles()
        {
            selTriangles.Clear();
            Renderer.RecalculateData(this);
        }

        public void MoveLightCamPoints(int point, Vector3 newPos, int viewport, float ortoWidth)
        {
            int light = -1;
            int camPos = -1;
            int camLookAt = -1;
            bool found = false;

            int index = 0;
            for(int i = 0; i < lights.Count && found == false; ++i)
            {
                if(lights[i].type == Light_Type.Spot || lights[i].type == Light_Type.Goniometric)
                {
                    if(index == point)
                    {
                        light = i;
                        found = true;
                    }
                    ++index;
                }
            }

            for(int i = 0; i < cams.Count && found == false; ++i)
            {
                if(index == point)
                {
                    camPos = i;
                    found = true;
                }
                else
                {
                    ++index;
                    if(index == point)
                    {
                        camLookAt = i;
                        found = true;
                    }
                    ++index;
                }
            }

            if(light >= 0)
            {
                modified = true;

                Vector3 oldPoint = lights[light].position + lights[light].direction * Renderer.spotLightDist * ortoWidth / 10;
                
                if(viewport == 1)
                {
                    newPos.X = oldPoint.X;
                }
                else if(viewport == 2)
                {
                    newPos.Y = oldPoint.Y;
                }
                else if(viewport == 0)
                {
                    newPos.Z = oldPoint.Z;
                }

                lights[light].direction = Vector3.Normalize(newPos - lights[light].position);
            }
            else if(camPos >= 0)
            {
                modified = true;

                if(viewport == 1)
                {
                    newPos.X = cams[camPos].position.X;
                }
                else if(viewport == 2)
                {
                    newPos.Y = cams[camPos].position.Y;
                }
                else if(viewport == 0)
                {
                    newPos.Z = cams[camPos].position.Z;
                }

                cams[camPos].position = newPos;
            }
            else if(camLookAt >= 0)
            {
                modified = true;

                if(viewport == 1)
                {
                    newPos.X = cams[camLookAt].lookAt.X;
                }
                else if(viewport == 2)
                {
                    newPos.Y = cams[camLookAt].lookAt.Y;
                }
                else if(viewport == 0)
                {
                    newPos.Z = cams[camLookAt].lookAt.Z;
                }

                cams[camLookAt].lookAt = newPos;
            }
        }

        public void NormalizeCamera(Camera camera) //chwilowo używane do obrotu kamery, po wprowadzniu nowej metody interaktywnej manipulacji kamery - usunąć
        {
            float tmpLength =
                (float)
                Math.Sqrt(Math.Pow(camera.lookAt.X - camera.position.X, 2) +
                          Math.Pow(camera.lookAt.Y - camera.position.Y, 2)
                          + Math.Pow(camera.lookAt.Z - camera.position.Z, 2));
            camera.lookAt.X = camera.position.X + (camera.lookAt.X - camera.position.X) / tmpLength;
            camera.lookAt.Y = camera.position.Y + (camera.lookAt.Y - camera.position.Y) / tmpLength;
            camera.lookAt.Z = camera.position.Z + (camera.lookAt.Z - camera.position.Z) / tmpLength;
        }

        public void ChangeCameraAngle(float value)
        {
            float radValue = Utilities.DegToRad(value);
            if (cams[activeCamera].fovAngle < 180 && cams[activeCamera].fovAngle > 0)
            {
                modified = true;
                cams[activeCamera].fovAngle += radValue;
                if (cams[activeCamera].fovAngle > 180)
                {
                    cams[activeCamera].fovAngle = 179;
                }
                if (cams[activeCamera].fovAngle < 0)
                {
                    cams[activeCamera].fovAngle = 1;
                }
            }
        }

        public int EstimatedMemory()
        {
            int m_parts = 0;
            foreach(Part part in parts) 
            {
                m_parts += part.triangles.Count * 4;
            }

            return points.Count * 12 + triangles.Count * 12 + m_parts + materials.Count * 60
                + materialAssign.Count * 8 + lights.Count * 72+cams.Count * 48 + selLights.Count * 4 + selCams.Count * 4 +
                normals.Count * 12 + GetHierarchyMemorySize(hierarchy.objects);
        }

        private int GetHierarchyMemorySize(List<HierarchyObject> objects)
        {
            int memory = 0;

            foreach(HierarchyObject obj in objects)
            {
                if(obj is HierarchyNode)
                {
                    memory += GetHierarchyMemorySize(((HierarchyNode)obj).hObjects);
                }
                else if(obj is HierarchyMesh)
                {
                    memory += ((HierarchyMesh)obj).triangles.Count * 4;
                }
            }

            return memory;
        }

        public bool ContainsMaterialName(string name)
        {
            foreach (Material_ material in materials)
            {
                if (String.Compare(material.name, name, true) == 0)
                    return true;
            }
            return false;
        }

        public bool ContainsLightName(string name)
        {
            foreach (Light_ light in lights)
            {
                if (String.Compare(light.name, name, true) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsTriangleSelected(uint triangle)
        {
            bool selected = false;

            foreach(HierarchyMesh mesh in selTriangles)
            {
                foreach(uint triang in mesh.triangles)
                {
                    if(triang == triangle)
                    {
                        selected = true;
                        break;
                    }
                }

                if(selected == true)
                {
                    break;
                }
            }

            return selected;
        }

        public bool[] GetSelectedTriangles()
        {
            bool[] selected = new bool[triangles.Count];
            for(int i = 0; i < selected.Length; ++i)
            {
                selected[i] = false;
            }

            foreach(HierarchyMesh mesh in selTriangles)
            {
                for(int i = 0; i < mesh.triangles.Count; ++i)
                {
                    selected[mesh.triangles[i]] = true;
                }
            }

            return selected;
        }

        public void AddObject(Scene sceneObject, string hierarchyName, Vector3 translation)
        {
            if (sceneObject != null)
            {
                modified = true;
                uint point_idx = (uint)points.Count();
                foreach (Vector3D point in sceneObject.points)
                {
                    point.x += translation.X;
                    point.y += translation.Y;
                    point.z += translation.Z;
                }
                points.AddRange(sceneObject.points);

                int triangles_idx = triangles.Count();
                foreach (Triangle triangle in sceneObject.triangles)
                {
                    triangle.p1 += point_idx;
                    triangle.p2 += point_idx;
                    triangle.p3 += point_idx;
                }

                triangles.AddRange(sceneObject.triangles);

                if (!ContainsMaterialName("mat1"))
                {
                    materials.Add(new Material_("mat1", 0.6f, 0.6f, 0.6f, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 1));
                }
                parts.Add(new Part(new List<int>()));
                materialAssign.Add("mat1");

                HierarchyMesh obj = new HierarchyMesh(hierarchyName);

                for (int i = 0; i < sceneObject.triangles.Count(); i++)
                {
                    parts[parts.Count-1].triangles.Add(triangles_idx);
                    obj.triangles.Add((uint)triangles_idx++);
                }

                hierarchy.objects.Add(obj);
                hierarchyChange = true;
                //Renderer.RecalculateData(this);
            }
        }

        public void AddCamera(Camera camera, Vector3 translation)
        {
            modified = true;
            camera.name = "Kamera " + cams.Count.ToString();
            camera.position += translation;
            cams.Add(camera);
            activeCamera = cams.Count - 1;
        }

        public void AddLight(Light_ light, Vector3 translation)
        {
            modified = true;
            lights.Add(light);
            light.position += translation;

            HierarchyLight lgt = new HierarchyLight(light.name, lights.Count - 1);
            hierarchy.objects.Add(lgt);
            hierarchyChange = true;
            Console.WriteLine("dadajemy swiatlo "+lgt.name);

        }

        public void AddMaterial(Material_ material)
        {
            modified = true;
            bool zastap = false;
            for (int i = 0; i < materials.Count; i++) if (materials[i].name == material.Name)
                {
                    zastap = true;
                    materials[i] = new Material_(material);
                }
            if (!zastap) materials.Add(new Material_(material));

            for (int i = 0; i < parts.Count; i++)
            {
                foreach (HierarchyMesh obj in selTriangles)
                    if (parts[i].triangles.Contains((int)obj.triangles[0]))
                        materialAssign[i] = material.Name;
            }
        }

        public void AddPreparedElement(PreparedElement element, Vector3 translation)
        {
            modified = true;
            uint pCount = (uint)points.Count;
            for (int i = 0; i < element.Scene.points.Count; i++)
                points.Add(new Vector3D(
                    element.Scene.points[i].x + translation.X, element.Scene.points[i].y + translation.Y, element.Scene.points[i].z + translation.Z));

            uint tCount = (uint)triangles.Count;
            for (int i = 0; i < element.Scene.triangles.Count; i++)
            {
                triangles.Add(new Modeler.Data.Scene.Triangle(
                                  element.Scene.triangles[i].p1 + pCount, element.Scene.triangles[i].p2 + pCount,
                                  element.Scene.triangles[i].p3 + pCount));
            }
            int lCount = lights.Count;
            foreach (Light_ light in element.Scene.lights)
            {
                lights.Add(new Light_(light));
                lights[lights.Count - 1].position += translation;
            }
            for (int i = 0; i < element.Scene.parts.Count; i++)
            {
                Part part = new Part(new List<int>());
                for (int j = 0; j < element.Scene.parts[i].triangles.Count; j++)
                {
                    part.triangles.Add(element.Scene.parts[i].triangles[j] + (int) tCount);
                }
                parts.Add(part);
                materialAssign.Add(element.Scene.materialAssign[i]);
            }

            Hierarchy newHierarchy = new Hierarchy();
            newHierarchy.objects = new List<HierarchyObject>();
            foreach (HierarchyObject ho in element.Scene.hierarchy.objects)
            {
                if (ho is HierarchyLight)
                {
                    newHierarchy.objects.Add(new HierarchyLight((HierarchyLight)ho));
                }
                else if (ho is HierarchyMesh)
                {
                    newHierarchy.objects.Add(new HierarchyMesh((HierarchyMesh)ho));
                }
                else if (ho is HierarchyNode)
                {
                    newHierarchy.objects.Add(new HierarchyNode((HierarchyNode)ho));
                }
            }

            RebuildAddedHierarchy(newHierarchy.objects, (int)tCount, lCount);

            hierarchy.objects.AddRange(newHierarchy.objects);

            foreach (Material_ material in element.Scene.materials)
            {
                if (!ContainsMaterialName(material.name))
                {
                    materials.Add(new Material_(material));
                }
            }
            hierarchyChange = true;

        }

        public void DeleteSelected()
        {
            modified = true;
            // Najpierw stworz listy indeksow zaznaczonych elementow: trojkatow
            // punktow, swiatel, kamer
            // Obiekty nalezy usunac rowniez z hierarchii
            #region usuwanie_geometrii
            HashSet<uint> selectedTriangIdx = new HashSet<uint>();
            HashSet<uint> selectedPointsIdx = new HashSet<uint>();
            List<HierarchyMesh> selectedMeshes = new List<HierarchyMesh>();
            bool[] deletedPoints = new bool[points.Count];
            bool[] deletedTriangs = new bool[triangles.Count];
            for (int i = 0; i < deletedPoints.Length; i++)
            {
                deletedPoints[i] = false;
            }
            for (int i = 0; i < deletedTriangs.Length; i++ )
            {
                deletedTriangs[i] = false;
            }

            foreach (HierarchyMesh mesh in selTriangles)
            //for (int i = 0; i < selTriangles.Count; i++)
            {
                selectedMeshes.Add(Hierarchy.GetSelectedMesh(hierarchy.objects, (int) mesh.triangles[0]));
                for (int j = 0; j < mesh.triangles.Count; j++)
                {
                    selectedTriangIdx.Add(mesh.triangles[j]);
                    deletedTriangs[mesh.triangles[j]] = true;

                    selectedPointsIdx.Add(triangles[(int) mesh.triangles[j]].p1);
                    selectedPointsIdx.Add(triangles[(int) mesh.triangles[j]].p2);
                    selectedPointsIdx.Add(triangles[(int) mesh.triangles[j]].p3);

                    deletedPoints[triangles[(int) mesh.triangles[j]].p1] = true;
                    deletedPoints[triangles[(int) mesh.triangles[j]].p2] = true;
                    deletedPoints[triangles[(int) mesh.triangles[j]].p3] = true;
                }
            }

            // Usuwanie z hierarchii
            foreach (HierarchyMesh mesh in selectedMeshes)
            {
                deleteMesh((int)mesh.triangles.ElementAt(0), hierarchy.objects);
            }
            

            // Lista zawiera indeksy poczatka i konca usuwanych punktow (beda
            // one wystepowaly sekcjami)
            List<int> pointStartEnd = new List<int>();
            bool flag = true;
            for (int i = 0; i < deletedPoints.Length; i++)
            {
                if (deletedPoints[i] && flag)
                {
                    pointStartEnd.Add(i);
                    flag = false;
                }
                if (!deletedPoints[i] && !flag)
                {
                    pointStartEnd.Add(i-1);
                    flag = true;
                }
            }
            if (!flag)
                pointStartEnd.Add(deletedPoints.Length - 1);

            // Lista zawiera indeksy poczatka i konca usuwanych trojkatow
            List<int> triangStartEnd = new List<int>();
            flag = true;
            for (int i = 0; i < deletedTriangs.Length; i++)
            {
                if (deletedTriangs[i] && flag)
                {
                    triangStartEnd.Add(i);
                    flag = false;
                }
                if (!deletedTriangs[i] && !flag)
                {
                    triangStartEnd.Add(i-1);
                    flag = true;
                }
            }
            if (!flag)
                triangStartEnd.Add(deletedTriangs.Length - 1);

            // Usuwanie punktow oraz trojkatow i czesci
            int offset = 0;
            for (int i = 0; i < pointStartEnd.Count; i+=2)
            {
                int diff = pointStartEnd[i + 1] - pointStartEnd[i] + 1;
                points.RemoveRange(pointStartEnd[i] - offset, diff);
                offset += diff;
            }
            offset = 0;
            List<int> idxToDelete = new List<int>();
            for (int i = 0; i < triangStartEnd.Count; i += 2)
            {
                int diff = triangStartEnd[i + 1] - triangStartEnd[i] + 1;
                triangles.RemoveRange(triangStartEnd[i] - offset, diff);
                offset += diff;
                for (int j = 0; j < parts.Count; j++)
                {
                    //if (parts[j].triangles.Contains(triangStartEnd[i]))
                    // Szemrana poprawka z tym ifem
                    //if (parts[j].triangles.Count > 0)
                    //{
                        if (parts[j].triangles.Min() >= triangStartEnd[i] &&
                            parts[j].triangles.Max() <= triangStartEnd[i + 1])
                            idxToDelete.Add(j);
                    //}
                }
                //if (idxToDelete > -1)
                //{
                //    parts.RemoveAt(idxToDelete);
                //    materialAssign.RemoveAt(idxToDelete);
                //}
            }
            offset = 0;
            foreach (int i in idxToDelete)
            {
                parts.RemoveAt(i-offset);
                materialAssign.RemoveAt(i-offset);
                ++offset;
            }
            if (parts.Count == 0)
            {
                materials.RemoveRange(0, materials.Count);
                materialAssign.RemoveRange(0, materialAssign.Count);
            }

            // Przebudowa sceny - nalezy pozmieniac indeksy punktow do ktorych 
            // odwoluja sie pozostale trojkaty, oraz naprawic indeksy trojkatow
            // w hierarchii

            List<HierarchyMesh> meshes = hierarchy.GetAllMeshes();
            offset = 0;
            for (int i = 0; i < pointStartEnd.Count; i += 2)
            {
                int diffP = pointStartEnd[i + 1] - pointStartEnd[i] + 1;
                foreach (Triangle triangle in triangles)
                {
                    if (triangle.p1 > pointStartEnd[i + 1] - offset)
                        triangle.p1 -= (uint) diffP;
                    if (triangle.p2 > pointStartEnd[i + 1] - offset)
                        triangle.p2 -= (uint) diffP;
                    if (triangle.p3 > pointStartEnd[i + 1] - offset)
                        triangle.p3 -= (uint) diffP;
                }
                offset += diffP;
            }
            offset = 0;
            for (int i = 0; i < triangStartEnd.Count; i+=2)
            {
                int diffT = triangStartEnd[i + 1] - triangStartEnd[i] + 1;
                foreach (Part part in parts)
                {
                    for (int j = 0; j < part.triangles.Count; j++)
                    {
                        if (part.triangles[j] > triangStartEnd[i + 1] - offset)
                            part.triangles[j] -= diffT;
                    }
                }
                foreach (HierarchyMesh mesh in meshes)
                {
                    for (int j = 0; j < mesh.triangles.Count; j++)
                    {
                        if (mesh.triangles[j] > triangStartEnd[i + 1] - offset)
                            mesh.triangles[j] -= (uint)diffT;
                    }
                }
                offset += diffT;
            }

            ClearSelectedTriangles();
            #endregion

            #region usuwanie_kamer
            if (cams.Count>1 && selCams.Count > 0)
            {
                if (activeCamera == cams.Count()-1)
                    activeCamera--;
                
                cams.RemoveAt(selCams[0]);
                //if (selCams[0] == activeCamera)
                //if (activeCamera < 1)
                //{
                //    activeCamera++;
                //}
                //else
                //{          
                //    activeCamera--;
                //}
                selCams.RemoveRange(0, selCams.Count);
               
                cameraRemoved = true;
            }
            #endregion

            #region usuwanie_swiatel
            offset = 0;
            selLights.Sort();
            Dictionary<int , int> newLightIdxOffsset = new Dictionary<int, int>();
            foreach (int i in selLights)
            {
                deleteLight(i, hierarchy.objects);
            }
            for (int i = 0; i < lights.Count; i++)
            {
                newLightIdxOffsset[i] = offset;
                if (selLights.Contains(i))
                {
                    offset++;
                }
            }

            for (int i = 0; i < selLights.Count; i++)
            {
                lights.RemoveAt(selLights[i] - i);
            }
            RebuildLightHierarchy(hierarchy.objects, newLightIdxOffsset);
            selLights.RemoveRange(0, selLights.Count);

            //// Usuwanie z hierarchii świateł
            //foreach (int hLight in selLights)
            //{
            //    deleteLight(lights.ElementAt(hLight).name, hierarchy.objects);
            //}

            hierarchyChange = true;
            #endregion
        }

        private void deleteMesh(int firstTriangleIndex, List<HierarchyObject> listHObj) //usuwa z hierarchii
        {
            List<HierarchyObject> treeList = new List<HierarchyObject>();
            foreach (HierarchyObject ho in listHObj)
            {
                if (ho is HierarchyMesh)  //jeśli jest to mesh
                {
                    HierarchyMesh hm = (HierarchyMesh)ho;
                    if ((int)hm.triangles.ElementAt(0) == firstTriangleIndex)  //jeśli jest to mesh którego szukamy
                    {
                        treeList.Add(ho);
                    }
                }
                else if (ho is HierarchyNode) //jeśli jest to node
                {
                    HierarchyNode hn = (HierarchyNode)ho;
                    deleteMesh(firstTriangleIndex, hn.hObjects);
                }
            }
            foreach (HierarchyObject ho in treeList)
            {
                listHObj.Remove(ho);
            }
        }

        private void deleteLight(int lightIndex, List<HierarchyObject> listHObj)//usuwa z hierarchii
        {
            List<HierarchyObject> treeList = new List<HierarchyObject>();
            foreach (HierarchyObject ho in listHObj)
            {
                if (ho is HierarchyLight)//jeśli jest to light
                {
                    HierarchyLight hl = (HierarchyLight)ho;
                    if (lightIndex == hl.lightIndex)  //jeśli jest to swiatlo, które szukamy
                    {
                        //listHObj.Remove(ho);
                        treeList.Add(ho);
                    }
                }
                else if (ho is HierarchyNode)  //jeśli jest to node
                {
                    HierarchyNode hn = (HierarchyNode)ho;
                    deleteLight(lightIndex, hn.hObjects);
                }
            }
            foreach (HierarchyObject ho in treeList)
            {
                listHObj.Remove(ho);
            }
        }

        public Scene SceneFromSelection(out Vector3D center)
        {
            Scene retScene = new Scene();

            HashSet<uint> selectedTriangIdx = new HashSet<uint>();
            HashSet<uint> selectedPointsIdx = new HashSet<uint>();
            List<Triangle> newTriangles = new List<Triangle>();
            List<Vector3D> newPoints = new List<Vector3D>();
            List<uint> newTriangleIdx = new List<uint>();
            List<int> newLightIdx = new List<int>();
            uint idx = 0;
            for (int i = 0; i < triangles.Count; ++i)
                newTriangleIdx.Add(0);
            //List<HierarchyMesh> selectedMeshes = new List<HierarchyMesh>();
            bool[] selectedPoints = new bool[points.Count];
            bool[] selectedTriangs = new bool[triangles.Count];
            for (int i = 0; i < selectedPoints.Length; i++)
            {
                selectedPoints[i] = false;
            }
            for (int i = 0; i < selectedTriangs.Length; i++)
            {
                selectedTriangs[i] = false;
            }

            foreach (HierarchyMesh mesh in selTriangles)
            //for (int i = 0; i < selTriangles.Count; i++)
            {
                //selectedMeshes.Add(Hierarchy.GetSelectedMesh(hierarchy.objects, (int)mesh.triangles[0]));
                for (int j = 0; j < mesh.triangles.Count; j++)
                {
                    int oldIdx = (int)mesh.triangles[j];
                    newTriangleIdx[oldIdx] = idx++;
                    selectedTriangIdx.Add(mesh.triangles[j]);
                    int triang = (int)mesh.triangles[j];
                    selectedTriangs[mesh.triangles[j]] = true;

                    newTriangles.Add(new Triangle(triangles[triang].p1, triangles[triang].p2, triangles[triang].p3));

                    selectedPointsIdx.Add(triangles[(int)mesh.triangles[j]].p1);
                    selectedPointsIdx.Add(triangles[(int)mesh.triangles[j]].p2);
                    selectedPointsIdx.Add(triangles[(int)mesh.triangles[j]].p3);

                    selectedPoints[triangles[(int)mesh.triangles[j]].p1] = true;
                    selectedPoints[triangles[(int)mesh.triangles[j]].p2] = true;
                    selectedPoints[triangles[(int)mesh.triangles[j]].p3] = true;
                }
            }

            idx = 0;
            for (int i = 0; i < lights.Count; i++)
            {
                newLightIdx.Add(-1);
            }
            for (int i = 0; i < selLights.Count; i++)
            {
                newLightIdx[selLights[i]] = (int)idx++;
            }

            for (int i=0; i<points.Count; i++)
            {
                if (selectedPoints[i])
                {
                    newPoints.Add(new Vector3D(points[i]));
                }
            }


            // Lista zawiera indeksy poczatka i konca usuwanych punktow (beda
            // one wystepowaly sekcjami)
            List<int> pointStartEnd = new List<int>();
            bool flag = true;
            for (int i = 0; i < selectedPoints.Length; i++)
            {
                if (!selectedPoints[i] && flag)
                {
                    pointStartEnd.Add(i);
                    flag = false;
                }
                if (selectedPoints[i] && !flag)
                {
                    pointStartEnd.Add(i - 1);
                    flag = true;
                }
            }
            if (!flag)
                pointStartEnd.Add(selectedPoints.Length - 1);

            // Lista zawiera indeksy poczatka i konca usuwanych trojkatow
            List<int> triangStartEnd = new List<int>();
            flag = true;
            for (int i = 0; i < selectedTriangs.Length; i++)
            {
                if (!selectedTriangs[i] && flag)
                {
                    triangStartEnd.Add(i);
                    flag = false;
                }
                if (selectedTriangs[i] && !flag)
                {
                    triangStartEnd.Add(i - 1);
                    flag = true;
                }
            }
            if (!flag)
                triangStartEnd.Add(selectedTriangs.Length - 1);

            int offset = 0;
            for (int i = 0; i < pointStartEnd.Count; i += 2)
            {
                int diffP = pointStartEnd[i + 1] - pointStartEnd[i] + 1;
                foreach (Triangle triangle in newTriangles)
                {
                    if (triangle.p1 > pointStartEnd[i + 1] - offset)
                        triangle.p1 -= (uint)diffP;
                    if (triangle.p2 > pointStartEnd[i + 1] - offset)
                        triangle.p2 -= (uint)diffP;
                    if (triangle.p3 > pointStartEnd[i + 1] - offset)
                        triangle.p3 -= (uint)diffP;
                }
                offset += diffP;
            }
            List<HierarchyObject> newHierarchy = new List<HierarchyObject>();
            if (selectedHierObj != null)
            {
                HierarchyNode newNode = new HierarchyNode(selectedHierObj);
                newHierarchy.Add(newNode);
                RebuildHierarchy(newHierarchy, newTriangleIdx, newLightIdx);
            }
            else
            {
                foreach (HierarchyMesh mesh in selTriangles)
                {
                    HierarchyMesh newMesh = (new HierarchyMesh(mesh.name));
                    for (int i = 0; i < mesh.triangles.Count; i++)
                    {
                        newMesh.triangles.Add(newTriangleIdx[(int) mesh.triangles[i]]);
                    }
                    newHierarchy.Add(newMesh);
                }
            }

            List<Part> newParts = new List<Part>();
            List<String> newMaterialAssign = new List<string>();
            for (int i = 0; i < parts.Count; i++)
            {
                Part newPart = (new Part(new List<int>()));
                bool used = false;
                for (int j = 0; j < parts[i].triangles.Count; j++)
                {
                    newPart.triangles.Add((int)newTriangleIdx[parts[i].triangles[j]]);
                    if ((int)newTriangleIdx[parts[i].triangles[j]] > 0) used = true;
                }
                if (used)
                {
                    newParts.Add(newPart);
                    newMaterialAssign.Add(materialAssign[i]);
                }
            }

            foreach (Material_ material in materials) retScene.materials.Add(new Material_(material));
            center = new Data.Scene.Vector3D(0, 0, 0);
            int count = newPoints.Count + selCams.Count + selLights.Count;


            foreach (Data.Scene.Vector3D v in newPoints)
            {
                center.x = center.x + v.x / count;
                center.y = center.y + v.y / count;
                center.z = center.z + v.z / count;
            }
            //foreach (int c in selCams)
            //{
            //    Camera cam = cams[c];
            //    center.x = center.x + cam.position.X / count;
            //    center.y = center.y + cam.position.Y / count;
            //    center.z = center.z + cam.position.Z / count;
            //}
            foreach (int l in selLights)
            {
                Light_ light = lights[l];
                center.x = center.x + light.position.X / count;
                center.y = center.y + light.position.Y / count;
                center.z = center.z + light.position.Z / count;
            }
            foreach (Data.Scene.Vector3D v in newPoints)
            {
                v.x -= center.x;
                v.y -= center.y;
                v.z -= center.z;
            }

            List<Light_> newwLights1 = new List<Light_>();

            foreach (int lightIndex in selLights)
            {
                Light_ light = new Light_(lights[lightIndex]); 
                newwLights1.Add(light);
                newwLights1[newwLights1.Count - 1].position -= center;
                int tmp = newwLights1.Count - 1;

                //dodawanie światel do hierarchii
                if (selectedHierObj == null)
                {
                    HierarchyLight newLight = new HierarchyLight(light.name.ToString(), tmp);
                    newHierarchy.Add(newLight);
                }
                //newLight.name = light.name;
                Console.WriteLine( "dodawanie do hierarchiii");
            }
            
            //List<Camera> newCams = new List<Camera>();
            //foreach (int camIndex in selCams)
            //{
            //    newCams.Add(new Camera(cams[camIndex]));
            //    newCams[newCams.Count - 1].position -= center;
            //}

            retScene.points = newPoints;
            retScene.triangles = newTriangles;
            retScene.hierarchy.objects = newHierarchy;
            retScene.parts = newParts;
            retScene.materialAssign = newMaterialAssign;
            retScene.lights = newwLights1;
            //retScene.cams = newCams;

            hierarchyChange = true;
            return retScene;
        }

        public void RebuildLightHierarchy(List<HierarchyObject> hierarchy, Dictionary<int, int> newLightIdxOffset)
        {
            modified = true;
            foreach (HierarchyObject ho in hierarchy) 
            {
                if (ho is HierarchyLight)
                {
                    if (newLightIdxOffset.ContainsKey(((HierarchyLight)ho).lightIndex))
                    {
                        ((HierarchyLight) ho).lightIndex -= newLightIdxOffset[((HierarchyLight) ho).lightIndex];
                    }
                }
                else if (ho is HierarchyNode)
                {
                    RebuildLightHierarchy(((HierarchyNode)ho).hObjects, newLightIdxOffset);
                }
            }
        }

        public void RebuildHierarchy(List<HierarchyObject> newHierarchy, List<uint> newTriangleIdx, List<int> newLightIdx)
        {
            modified = true;
            foreach (HierarchyObject ho in newHierarchy)
            {
                if (ho is HierarchyMesh)
                {
                    for (int i = 0; i < ((HierarchyMesh)ho).triangles.Count; i++)
                    {
                        ((HierarchyMesh) ho).triangles[i] = newTriangleIdx[(int)((HierarchyMesh) ho).triangles[i]];
                    }
                }
                else if (ho is HierarchyLight)
                {
                    ((HierarchyLight)ho).lightIndex = newLightIdx[((HierarchyLight)ho).lightIndex];
                }
                else if (ho is HierarchyNode)
                {
                    RebuildHierarchy(((HierarchyNode)ho).hObjects, newTriangleIdx, newLightIdx);
                }
            }
        }

        public void RebuildAddedHierarchy(List<HierarchyObject> addedHierarchy, int triangleCount, int lightCount)
        {
            modified = true;
            foreach (HierarchyObject ho in addedHierarchy)
            {
                if (ho is HierarchyMesh)
                {
                    for (int i = 0; i < ((HierarchyMesh)ho).triangles.Count; i++)
                    {
                        ((HierarchyMesh) ho).triangles[i] += (uint)triangleCount;
                    }
                }
                else if (ho is HierarchyLight)
                {
                    ((HierarchyLight) ho).lightIndex += lightCount;
                }
                else if (ho is HierarchyNode)
                {
                    RebuildAddedHierarchy(((HierarchyNode)ho).hObjects, triangleCount, lightCount);
                }
            }
        }

        public static Scene GetExampleScene()
        {
            Scene scene = new Scene();

            scene.points.Add(new Vector3D(-1, -1, 1));
            scene.points.Add(new Vector3D(1, -1, 1));
            scene.points.Add(new Vector3D(1, -1, -1));
            scene.points.Add(new Vector3D(-1, -1, -1));
            scene.points.Add(new Vector3D(-1, 1, 1));
            scene.points.Add(new Vector3D(1, 1, 1));
            scene.points.Add(new Vector3D(1, 1, -1));
            scene.points.Add(new Vector3D(-1, 1, -1));

            scene.points.Add(new Vector3D(1.5f, -0.5f, 0.5f));
            scene.points.Add(new Vector3D(3, -0.3f, 0));
            scene.points.Add(new Vector3D(2.5f, 0, -0.8f));
            scene.points.Add(new Vector3D(2, 2, -0.2f));

            scene.triangles.Add(new Triangle(0, 1, 5));
            scene.triangles.Add(new Triangle(0, 5, 4));
            scene.triangles.Add(new Triangle(1, 2, 5));
            scene.triangles.Add(new Triangle(2, 6, 5));
            scene.triangles.Add(new Triangle(2, 3, 6));
            scene.triangles.Add(new Triangle(3, 6, 7));
            scene.triangles.Add(new Triangle(3, 0, 7));
            scene.triangles.Add(new Triangle(0, 7, 4));
            scene.triangles.Add(new Triangle(0, 1, 2));
            scene.triangles.Add(new Triangle(0, 2, 3));
            scene.triangles.Add(new Triangle(4, 5, 6));
            scene.triangles.Add(new Triangle(6, 7, 4));

            scene.triangles.Add(new Triangle(8, 9, 10));
            scene.triangles.Add(new Triangle(8, 9, 11));
            scene.triangles.Add(new Triangle(9, 10, 11));
            scene.triangles.Add(new Triangle(8, 10, 11));

            List<int> part1 = new List<int>();
            part1.Add(0);
            part1.Add(1);
            part1.Add(2);
            part1.Add(3);
            part1.Add(4);
            part1.Add(5);
            part1.Add(6);
            part1.Add(7);
            part1.Add(8);
            part1.Add(9);
            part1.Add(10);
            part1.Add(11);
            scene.parts.Add(new Part(part1));

            List<int> part2 = new List<int>();
            part2.Add(12);
            part2.Add(13);
            part2.Add(14);
            part2.Add(15);
            scene.parts.Add(new Part(part2));

            scene.materials.Add(new Material_("mat1", 0.6f, 0.95f, 0.5f, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1));
            scene.materials.Add(new Material_("mat2", 0.4f, 0.3f, 0.9f, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1));

            scene.materialAssign.Add("mat1");
            scene.materialAssign.Add("mat2");

            scene.cams[0] = new Camera("Cam1", 800, 600, new Vector3(-8, -4, -10), new Vector3(3, 2, 2), 60, 0);

            scene.activeCamera = 0;

            HierarchyMesh cube = new HierarchyMesh("Cube");
            cube.triangles.AddRange(new uint[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });

            HierarchyMesh pyramid = new HierarchyMesh("Pyramid");
            pyramid.triangles.AddRange(new uint[] { 12, 13, 14, 15 });

            scene.hierarchy.objects.AddRange( new HierarchyMesh[] { cube, pyramid } );

            return scene;
        }
    }
}