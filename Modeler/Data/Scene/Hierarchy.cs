using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modeler.Data.Scene
{
    class Hierarchy
    {
        public List<HierarchyObject> objects;

        public Hierarchy()
        {
            objects = new List<HierarchyObject>();
        }

        public override string ToString()
        {
            string result = "";
            foreach (HierarchyObject obj in objects)
            {
                result += obj.ToString();
            }
            return result;
        }

        public HierarchyMesh GetMesh(string name)
        {
            return GetMeshRec(objects, name);
        }

        public List<HierarchyMesh> GetAllMeshes()
        {
            List<HierarchyMesh> meshes = new List<HierarchyMesh>();

            GetAllMeshesRec(objects, meshes);

            return meshes;
        }

        private void GetAllMeshesRec(List<HierarchyObject> objs, List<HierarchyMesh> outMeshes)
        {
            foreach(HierarchyObject obj in objs)
            {
                if(obj is HierarchyMesh)
                {
                    outMeshes.Add((HierarchyMesh)obj);
                }
                else if(obj is HierarchyNode)
                {
                    GetAllMeshesRec(((HierarchyNode)obj).hObjects, outMeshes);
                }
            }
        }

        private HierarchyMesh GetMeshRec(List<HierarchyObject> objs, string name)
        {
            foreach(HierarchyObject obj in objs)
            {
                if(obj is HierarchyMesh)
                {
                    if(((HierarchyMesh)obj).name == name)
                    {
                        return (HierarchyMesh)obj;
                    }
                }
                else if(obj is HierarchyNode)
                {
                    return GetMeshRec(((HierarchyNode)obj).hObjects, name);
                }
            }

            return null;
        }

        public static HierarchyMesh GetSelectedMesh(List<HierarchyObject> objects, int triangle)
        {
            foreach(HierarchyObject hObject in objects)
            {
                if(hObject is HierarchyMesh)
                {
                    foreach(uint tr in ((HierarchyMesh)hObject).triangles)
                    {
                        if(tr == triangle)
                        {
                            return (HierarchyMesh)hObject;
                        }
                    }
                }
                else if(hObject is HierarchyNode)
                {
                    {
                        HierarchyMesh mesh = GetSelectedMesh(((HierarchyNode)hObject).hObjects, triangle);

                        if(mesh != null)
                        {
                            return mesh;
                        }
                    }
                }
            }

            return null;
        }

        public List<HierarchyLight> GetAllLights()
        {
            List<HierarchyLight> lights = new List<HierarchyLight>();

            GetAllLightsRec(objects, lights);

            return lights;
        }

        private void GetAllLightsRec(List<HierarchyObject> objs, List<HierarchyLight> outLights)
        {
            foreach (HierarchyObject obj in objs)
            {
                if (obj is HierarchyLight)
                {
                    outLights.Add((HierarchyLight)obj);
                }
                else if (obj is HierarchyNode)
                {
                    GetAllLightsRec(((HierarchyNode)obj).hObjects, outLights);
                }
            }
        }
    }

    abstract class HierarchyObject
    {
        public String name;

        public HierarchyObject(String name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }
    }

    class HierarchyNode : HierarchyObject
    {
        public List<HierarchyObject> hObjects;

        public HierarchyNode(String name) : base(name)
        {
            hObjects = new List<HierarchyObject>();
        }

        public HierarchyNode(HierarchyNode copy) : base(copy.name)
        {
            hObjects = new List<HierarchyObject>(copy.hObjects.Count);
            CopyHierarchy(copy.hObjects, out hObjects);
        }

        private void CopyHierarchy(List<HierarchyObject> objs, out List<HierarchyObject> outHierarchy)
        {
            outHierarchy = new List<HierarchyObject>(objs.Count);
            foreach (HierarchyObject hierarchyObject in objs)
            {
                if (hierarchyObject is HierarchyNode)
                {
                    outHierarchy.Add(new HierarchyNode((HierarchyNode) hierarchyObject));
                } 
                else if (hierarchyObject is HierarchyLight)
                {
                    //outHierarchy.Add(new HierarchyLight((HierarchyLight) hierarchyObject));
                    outHierarchy.Add(new HierarchyLight(hierarchyObject.name.ToString(), ((HierarchyLight)hierarchyObject).lightIndex));
                } 
                else if (hierarchyObject is HierarchyMesh)
                {
                    outHierarchy.Add(new HierarchyMesh((HierarchyMesh) hierarchyObject));
                }
            }
        }
    }

    class HierarchyMesh : HierarchyObject
    {     
        public List<uint> triangles;

        public HierarchyMesh(String name) : base(name)
        {
            triangles = new List<uint>();
        }

        public HierarchyMesh(HierarchyMesh copy) : base(copy.name)
        {
            triangles = new List<uint>(copy.triangles);
        }
    }

    class HierarchyLight : HierarchyObject
    {
        public int lightIndex;

        public HierarchyLight(String name, int lightIndex) : base(name)
        {
            this.lightIndex = lightIndex;
        }

        public HierarchyLight(HierarchyLight copy)
            : base(copy.name)
        {
            //this.lightIndex = new Light_(copy.lightIndex);
            lightIndex = copy.lightIndex;
        }

        //public HierarchyLight(HierarchyLight copy) : base(copy.name)
        //{
        //    this.light = new Light_(copy.light);
        //}
    }
}
