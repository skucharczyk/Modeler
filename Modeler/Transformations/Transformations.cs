using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modeler.Data.Scene;

namespace Modeler.Transformations
{
    class Transformations
    {
        public static void Translate(Scene scene, float x, float y, float z)
        {
            // Transformacja odbywa się tylko jeśli zaznaczony jest jakiś obiekt
            // tymczasowo tylko siatka trojkatow
            //List<uint> uniqueVertices = new List<uint>();
            HashSet<uint> uniqueVertices = new HashSet<uint>();
            if (scene.selTriangles.Count > 0)
            {
                scene.modified = true;
                //DateTime startSearchVert = DateTime.Now;
                foreach (HierarchyMesh obj in scene.selTriangles)
                {
                    foreach (uint triagleIdx in obj.triangles)
                    {
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p1);
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p2);
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p3);
                    }
                }
                //TimeSpan Elapsed = DateTime.Now - startSearchVert;

                //DateTime startTranslateVert = DateTime.Now;
                foreach (int vertIdx in uniqueVertices)
                {
                    scene.points[vertIdx].x += x;
                    scene.points[vertIdx].y += y;
                    scene.points[vertIdx].z += z;
                }
                //TimeSpan ElapsedTranslate = DateTime.Now - startTranslateVert;

                //Console.WriteLine("Czas wyszukiwania: {0}, czas przesuwania {1}", Elapsed, ElapsedTranslate);
            }
            foreach (int light in scene.selLights)
            {
                scene.modified = true;
                scene.lights[light].position.X += x;
                scene.lights[light].position.Y += y;
                scene.lights[light].position.Z += z;
            }
            foreach (int cam in scene.selCams)
            {
                scene.modified = true;
                scene.cams[cam].position.X += x;
                scene.cams[cam].position.Y += y;
                scene.cams[cam].position.Z += z;
                scene.cams[cam].lookAt.X += x;
                scene.cams[cam].lookAt.Y += y;
                scene.cams[cam].lookAt.Z += z;

                if (cam == scene.activeCamera)
                {
                    //Console.WriteLine("zmiana pozycji aktywnej kamery");
                    scene.cameraMoved = true;
                }
            }
        }

        public static void Scale(Scene scene, float x, float y, float z)
        {
            List<uint> selectedTriangles = new List<uint>();

            foreach (HierarchyMesh mesh in scene.selTriangles)
            {
                selectedTriangles.AddRange(mesh.triangles);
                scene.modified = true;
            }

            Vector3D center = new Vector3D(0, 0, 0);

            HashSet<uint> uniquePoints = new HashSet<uint>();
            foreach (uint triangleIndex in selectedTriangles)
            {
                scene.modified = true;
                uint p1 = scene.triangles[(int)triangleIndex].p1;
                uint p2 = scene.triangles[(int)triangleIndex].p2;
                uint p3 = scene.triangles[(int)triangleIndex].p3;

                uniquePoints.Add(p1);
                uniquePoints.Add(p2);
                uniquePoints.Add(p3);
            }

            foreach (uint uniquePoint in uniquePoints)
            {
                scene.modified = true;
                center.x += scene.points[(int)uniquePoint].x;
                center.y += scene.points[(int)uniquePoint].y;
                center.z += scene.points[(int)uniquePoint].z;
            }

            center.x /= uniquePoints.Count;
            center.y /= uniquePoints.Count;
            center.z /= uniquePoints.Count;

            float factorX = 0.003f * x;
            float factorY = 0.003f * y;
            float factorZ = 0.003f * z;

            foreach (uint pointIndex in uniquePoints)
            {
                scene.modified = true;
                scene.points[(int)pointIndex] -= center;
                scene.points[(int)pointIndex] *= new Vector3D(1 + factorX, 1 + factorY, 1 + factorZ);
                scene.points[(int)pointIndex] += center;
            }
        }

        public static void ScalePar(Scene scene, float x, float y, float z)
        {
            List<uint> selectedTriangles = new List<uint>();

            foreach (HierarchyMesh mesh in scene.selTriangles)
            {
                scene.modified = true;
                selectedTriangles.AddRange(mesh.triangles);
            }

            Vector3D center = new Vector3D(0, 0, 0);

            HashSet<uint> uniquePoints = new HashSet<uint>();
            foreach (uint triangleIndex in selectedTriangles)
            {
                scene.modified = true;
                uint p1 = scene.triangles[(int)triangleIndex].p1;
                uint p2 = scene.triangles[(int)triangleIndex].p2;
                uint p3 = scene.triangles[(int)triangleIndex].p3;

                uniquePoints.Add(p1);
                uniquePoints.Add(p2);
                uniquePoints.Add(p3);
            }

            foreach (uint uniquePoint in uniquePoints)
            {
                scene.modified = true;
                center.x += scene.points[(int)uniquePoint].x;
                center.y += scene.points[(int)uniquePoint].y;
                center.z += scene.points[(int)uniquePoint].z;
            }

            center.x /= uniquePoints.Count;
            center.y /= uniquePoints.Count;
            center.z /= uniquePoints.Count;

            foreach (uint pointIndex in uniquePoints)
            {
                scene.modified = true;
                scene.points[(int)pointIndex] -= center;
                scene.points[(int)pointIndex] *= new Vector3D(x, y, z);
                scene.points[(int)pointIndex] += center;
            }
        }

        public static void RotateOX(Scene scene, float phi)
        {
            //List<uint> uniqueVertices = new List<uint>();
            HashSet<uint> uniqueVertices = new HashSet<uint>();
            if (scene.selTriangles.Count > 0||scene.selLights.Count>0)
            {
                scene.modified = true;
                foreach (HierarchyMesh obj in scene.selTriangles)
                {
                    //uint tmp;
                    foreach (uint triagleIdx in obj.triangles)
                    {
                        //tmp = scene.triangles[(int)triagleIdx].p1;
                        //if (!uniqueVertices.Contains(tmp))
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p1);
                        //tmp = scene.triangles[(int)triagleIdx].p2;
                        //if (!uniqueVertices.Contains(tmp))
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p2);
                        //tmp = scene.triangles[(int)triagleIdx].p3;
                        //if (!uniqueVertices.Contains(tmp))
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p3);
                    }
                }

                //Wyznaczenie środka
                Vector3D center = new Vector3D(0, 0, 0);
                int count = uniqueVertices.Count + scene.selLights.Count;
                foreach (int vertIdx in uniqueVertices)
                {
                    center.x = center.x + scene.points[vertIdx].x / count;
                    center.y = center.y + scene.points[vertIdx].y / count;
                    center.z = center.z + scene.points[vertIdx].z / count;
                }
                foreach (int lightIdx in scene.selLights)
                {
                    center.x = center.x + scene.lights[lightIdx].position.X / count;
                    center.y = center.y + scene.lights[lightIdx].position.Y / count;
                    center.z = center.z + scene.lights[lightIdx].position.Z / count;
                }

                //Obrót względem środka
                foreach (int vertIdx in uniqueVertices)
                {
                    scene.points[vertIdx].z = scene.points[vertIdx].z - center.z;
                    scene.points[vertIdx].y = scene.points[vertIdx].y - center.y;
                    float temp = scene.points[vertIdx].z * (float)Math.Cos(-phi) - scene.points[vertIdx].y * (float)Math.Sin(-phi);
                    scene.points[vertIdx].y = scene.points[vertIdx].z * (float)Math.Sin(-phi) + scene.points[vertIdx].y * (float)Math.Cos(-phi);
                    scene.points[vertIdx].z = temp;
                    scene.points[vertIdx].z = scene.points[vertIdx].z + center.z;
                    scene.points[vertIdx].y = scene.points[vertIdx].y + center.y;
                }
                foreach (int lightIdx in scene.selLights)
                {
                    scene.lights[lightIdx].position.Z = scene.lights[lightIdx].position.Z - center.z;
                    scene.lights[lightIdx].position.Y = scene.lights[lightIdx].position.Y - center.y;
                    float temp = scene.lights[lightIdx].position.Z * (float)Math.Cos(-phi) - scene.lights[lightIdx].position.Y * (float)Math.Sin(-phi);
                    scene.lights[lightIdx].position.Y = scene.lights[lightIdx].position.Z * (float)Math.Sin(-phi) + scene.lights[lightIdx].position.Y * (float)Math.Cos(-phi);
                    scene.lights[lightIdx].position.Z = temp;
                    scene.lights[lightIdx].position.Z = scene.lights[lightIdx].position.Z + center.z;
                    scene.lights[lightIdx].position.Y = scene.lights[lightIdx].position.Y + center.y;

                    temp = scene.lights[lightIdx].direction.Z * (float)Math.Cos(-phi) - scene.lights[lightIdx].direction.Y * (float)Math.Sin(-phi);
                    scene.lights[lightIdx].direction.Y = scene.lights[lightIdx].direction.Z * (float)Math.Sin(-phi) + scene.lights[lightIdx].direction.Y * (float)Math.Cos(-phi);
                    scene.lights[lightIdx].direction.Z = temp;
                }                
            }
        }

        public static void RotateOY(Scene scene, float phi)
        {
            //List<uint> uniqueVertices = new List<uint>();
            HashSet<uint> uniqueVertices = new HashSet<uint>();
            if (scene.selTriangles.Count > 0 || scene.selCams.Count > 0)
            {
                scene.modified = true;
                foreach (HierarchyMesh obj in scene.selTriangles)
                {
                    //uint tmp;
                    foreach (uint triagleIdx in obj.triangles)
                    {
                        //tmp = scene.triangles[(int)triagleIdx].p1;
                        //if (!uniqueVertices.Contains(tmp))
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p1);
                        //tmp = scene.triangles[(int)triagleIdx].p2;
                        //if (!uniqueVertices.Contains(tmp))
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p2);
                        //tmp = scene.triangles[(int)triagleIdx].p3;
                        //if (!uniqueVertices.Contains(tmp))
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p3);
                    }
                }

                //Wyznaczenie środka
                Vector3D center = new Vector3D(0, 0, 0);
                int count = uniqueVertices.Count + scene.selLights.Count;
                foreach (int vertIdx in uniqueVertices)
                {
                    center.x = center.x + scene.points[vertIdx].x / count;
                    center.y = center.y + scene.points[vertIdx].y / count;
                    center.z = center.z + scene.points[vertIdx].z / count;
                }
                foreach (int lightIdx in scene.selLights)
                {
                    center.x = center.x + scene.lights[lightIdx].position.X / count;
                    center.y = center.y + scene.lights[lightIdx].position.Y / count;
                    center.z = center.z + scene.lights[lightIdx].position.Z / count;
                }

                //Obrót względem środka
                foreach (int vertIdx in uniqueVertices)
                {
                    scene.points[vertIdx].x = scene.points[vertIdx].x - center.x;
                    scene.points[vertIdx].z = scene.points[vertIdx].z - center.z;
                    float temp = scene.points[vertIdx].x * (float)Math.Cos(-phi) - scene.points[vertIdx].z * (float)Math.Sin(-phi);
                    scene.points[vertIdx].z = scene.points[vertIdx].x * (float)Math.Sin(-phi) + scene.points[vertIdx].z * (float)Math.Cos(-phi);
                    scene.points[vertIdx].x = temp;
                    scene.points[vertIdx].x = scene.points[vertIdx].x + center.x;
                    scene.points[vertIdx].z = scene.points[vertIdx].z + center.z;
                }
                foreach (int lightIdx in scene.selLights)
                {
                    scene.lights[lightIdx].position.X = scene.lights[lightIdx].position.X - center.x;
                    scene.lights[lightIdx].position.Z = scene.lights[lightIdx].position.Z - center.z;
                    float temp = scene.lights[lightIdx].position.X * (float)Math.Cos(-phi) - scene.lights[lightIdx].position.Z * (float)Math.Sin(-phi);
                    scene.lights[lightIdx].position.Z = scene.lights[lightIdx].position.X * (float)Math.Sin(-phi) + scene.lights[lightIdx].position.Z * (float)Math.Cos(-phi);
                    scene.lights[lightIdx].position.X = temp;
                    scene.lights[lightIdx].position.X = scene.lights[lightIdx].position.X + center.x;
                    scene.lights[lightIdx].position.Z = scene.lights[lightIdx].position.Z + center.z;

                    temp = scene.lights[lightIdx].direction.X * (float)Math.Cos(-phi) - scene.lights[lightIdx].direction.Z * (float)Math.Sin(-phi);
                    scene.lights[lightIdx].direction.Z = scene.lights[lightIdx].direction.X * (float)Math.Sin(-phi) + scene.lights[lightIdx].direction.Z * (float)Math.Cos(-phi);
                    scene.lights[lightIdx].direction.X = temp;
                }
            }
        }

        public static void RotateOZ(Scene scene, float phi)
        {
            //List<uint> uniqueVertices = new List<uint>();
            HashSet<uint> uniqueVertices = new HashSet<uint>();
            if (scene.selTriangles.Count > 0 || scene.selLights.Count > 0)
            {
                scene.modified = true;
                foreach (HierarchyMesh obj in scene.selTriangles)
                {
                    //uint tmp;
                    foreach (uint triagleIdx in obj.triangles)
                    {
                        //tmp = scene.triangles[(int)triagleIdx].p1;
                        //if (!uniqueVertices.Contains(tmp))
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p1);
                        //tmp = scene.triangles[(int)triagleIdx].p2;
                        //if (!uniqueVertices.Contains(tmp))
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p2);
                        //tmp = scene.triangles[(int)triagleIdx].p3;
                        //if (!uniqueVertices.Contains(tmp))
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p3);
                    }
                }
                
                //Wyznaczenie środka
                Vector3D center = new Vector3D(0, 0, 0);
                int count = uniqueVertices.Count + scene.selLights.Count;
                foreach (int vertIdx in uniqueVertices)
                {
                    center.x = center.x + scene.points[vertIdx].x / count;
                    center.y = center.y + scene.points[vertIdx].y / count;
                    center.z = center.z + scene.points[vertIdx].z / count;
                }
                foreach (int lightIdx in scene.selLights)
                {
                    center.x = center.x + scene.lights[lightIdx].position.X / count;
                    center.y = center.y + scene.lights[lightIdx].position.Y / count;
                    center.z = center.z + scene.lights[lightIdx].position.Z / count;
                }

                //Obrót wyględem środka
                foreach (int vertIdx in uniqueVertices)
                {
                    scene.points[vertIdx].x = scene.points[vertIdx].x - center.x;
                    scene.points[vertIdx].y = scene.points[vertIdx].y - center.y;
                    float temp = scene.points[vertIdx].x * (float)Math.Cos(-phi) - scene.points[vertIdx].y * (float)Math.Sin(-phi);
                    scene.points[vertIdx].y = scene.points[vertIdx].x * (float)Math.Sin(-phi) + scene.points[vertIdx].y * (float)Math.Cos(-phi);
                    scene.points[vertIdx].x = temp;
                    scene.points[vertIdx].x = scene.points[vertIdx].x + center.x;
                    scene.points[vertIdx].y = scene.points[vertIdx].y + center.y;
                }
                foreach (int lightIdx in scene.selLights)
                {
                    scene.lights[lightIdx].position.X = scene.lights[lightIdx].position.X - center.x;
                    scene.lights[lightIdx].position.Y = scene.lights[lightIdx].position.Y - center.y;
                    float temp = scene.lights[lightIdx].position.X * (float)Math.Cos(-phi) - scene.lights[lightIdx].position.Y * (float)Math.Sin(-phi);
                    scene.lights[lightIdx].position.Y = scene.lights[lightIdx].position.X * (float)Math.Sin(-phi) + scene.lights[lightIdx].position.Y * (float)Math.Cos(-phi);
                    scene.lights[lightIdx].position.X = temp;
                    scene.lights[lightIdx].position.X = scene.lights[lightIdx].position.X + center.x;
                    scene.lights[lightIdx].position.Y = scene.lights[lightIdx].position.Y + center.y;

                    temp = scene.lights[lightIdx].direction.X * (float)Math.Cos(-phi) - scene.lights[lightIdx].direction.Y * (float)Math.Sin(-phi);
                    scene.lights[lightIdx].direction.Y = scene.lights[lightIdx].direction.X * (float)Math.Sin(-phi) + scene.lights[lightIdx].direction.Y * (float)Math.Cos(-phi);
                    scene.lights[lightIdx].direction.X = temp;
                }
            }
        }

        public static void Rotate(Scene scene, float phi_X, float phi_Y, float phi_Z)
        {
            //przeliczamy stopnie na radiany
            float phiX = phi_X * (float)Math.PI / 180;
            float phiY = phi_Y * (float)Math.PI / 180;
            float phiZ = phi_Z * (float)Math.PI / 180;
            //List<uint> uniqueVertices = new List<uint>();
            HashSet<uint> uniqueVertices = new HashSet<uint>();
            if (scene.selTriangles.Count > 0 || scene.selLights.Count > 0)
            {
                scene.modified = true;
                foreach (HierarchyMesh obj in scene.selTriangles)
                {
                    //uint tmp;
                    foreach (uint triagleIdx in obj.triangles)
                    {
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p1);
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p2);
                        uniqueVertices.Add(scene.triangles[(int)triagleIdx].p3);
                    }
                }

                //Wyznaczenie środka
                Vector3D center = new Vector3D(0, 0, 0);
                int count = uniqueVertices.Count + scene.selLights.Count;
                foreach (int vertIdx in uniqueVertices)
                {
                    center.x = center.x + scene.points[vertIdx].x / count;
                    center.y = center.y + scene.points[vertIdx].y / count;
                    center.z = center.z + scene.points[vertIdx].z / count;
                }
                foreach (int lightIdx in scene.selLights)
                {
                    center.x = center.x + scene.lights[lightIdx].position.X / count;
                    center.y = center.y + scene.lights[lightIdx].position.Y / count;
                    center.z = center.z + scene.lights[lightIdx].position.Z / count;
                }

                //Obrót wyględem środka
                foreach (int vertIdx in uniqueVertices)
                {
                    scene.points[vertIdx].x = scene.points[vertIdx].x - center.x;
                    scene.points[vertIdx].y = scene.points[vertIdx].y - center.y;
                    scene.points[vertIdx].z = scene.points[vertIdx].z - center.z;

                    float tempX = scene.points[vertIdx].z * (float)Math.Cos(-phiX) - scene.points[vertIdx].y * (float)Math.Sin(-phiX);
                    scene.points[vertIdx].y = scene.points[vertIdx].z * (float)Math.Sin(-phiX) + scene.points[vertIdx].y * (float)Math.Cos(-phiX);
                    scene.points[vertIdx].z = tempX;

                    float tempY = scene.points[vertIdx].x * (float)Math.Cos(-phiY) - scene.points[vertIdx].z * (float)Math.Sin(-phiY);
                    scene.points[vertIdx].z = scene.points[vertIdx].x * (float)Math.Sin(-phiY) + scene.points[vertIdx].z * (float)Math.Cos(-phiY);
                    scene.points[vertIdx].x = tempY;

                    float tempZ = scene.points[vertIdx].x * (float)Math.Cos(-phiZ) - scene.points[vertIdx].y * (float)Math.Sin(-phiZ);
                    scene.points[vertIdx].y = scene.points[vertIdx].x * (float)Math.Sin(-phiZ) + scene.points[vertIdx].y * (float)Math.Cos(-phiZ);
                    scene.points[vertIdx].x = tempZ;
                    
                    scene.points[vertIdx].x = scene.points[vertIdx].x + center.x;
                    scene.points[vertIdx].y = scene.points[vertIdx].y + center.y;
                    scene.points[vertIdx].z = scene.points[vertIdx].z + center.z;
                }
                foreach (int lightIdx in scene.selLights)
                {
                    scene.lights[lightIdx].position.X = scene.lights[lightIdx].position.X - center.x;
                    scene.lights[lightIdx].position.Y = scene.lights[lightIdx].position.Y - center.y;
                    scene.lights[lightIdx].position.Z = scene.lights[lightIdx].position.Z - center.z;

                    float tempX = scene.lights[lightIdx].position.Z * (float)Math.Cos(-phiX) - scene.lights[lightIdx].position.Y * (float)Math.Sin(-phiX);
                    scene.lights[lightIdx].position.Y = scene.lights[lightIdx].position.Z * (float)Math.Sin(-phiX) + scene.lights[lightIdx].position.Y * (float)Math.Cos(-phiX);
                    scene.lights[lightIdx].position.Z = tempX;

                    float tempY = scene.lights[lightIdx].position.X * (float)Math.Cos(-phiY) - scene.lights[lightIdx].position.Z * (float)Math.Sin(-phiY);
                    scene.lights[lightIdx].position.Z = scene.lights[lightIdx].position.X * (float)Math.Sin(-phiY) + scene.lights[lightIdx].position.Z * (float)Math.Cos(-phiY);
                    scene.lights[lightIdx].position.X = tempY;

                    float tempZ = scene.lights[lightIdx].position.X * (float)Math.Cos(-phiZ) - scene.lights[lightIdx].position.Y * (float)Math.Sin(-phiZ);
                    scene.lights[lightIdx].position.Y = scene.lights[lightIdx].position.X * (float)Math.Sin(-phiZ) + scene.lights[lightIdx].position.Y * (float)Math.Cos(-phiZ);
                    scene.lights[lightIdx].position.X = tempZ;

                    scene.lights[lightIdx].position.X = scene.lights[lightIdx].position.X + center.x;
                    scene.lights[lightIdx].position.Y = scene.lights[lightIdx].position.Y + center.y;
                    scene.lights[lightIdx].position.Z = scene.lights[lightIdx].position.Z + center.z;

                    tempX = scene.lights[lightIdx].direction.Z * (float)Math.Cos(-phiX) - scene.lights[lightIdx].direction.Y * (float)Math.Sin(-phiX);
                    scene.lights[lightIdx].direction.Y = scene.lights[lightIdx].direction.Z * (float)Math.Sin(-phiX) + scene.lights[lightIdx].direction.Y * (float)Math.Cos(-phiX);
                    scene.lights[lightIdx].direction.Z = tempX;

                    tempY = scene.lights[lightIdx].direction.X * (float)Math.Cos(-phiY) - scene.lights[lightIdx].direction.Z * (float)Math.Sin(-phiY);
                    scene.lights[lightIdx].direction.Z = scene.lights[lightIdx].direction.X * (float)Math.Sin(-phiY) + scene.lights[lightIdx].direction.Z * (float)Math.Cos(-phiY);
                    scene.lights[lightIdx].direction.X = tempY;

                    tempZ = scene.lights[lightIdx].direction.X * (float)Math.Cos(-phiZ) - scene.lights[lightIdx].direction.Y * (float)Math.Sin(-phiZ);
                    scene.lights[lightIdx].direction.Y = scene.lights[lightIdx].direction.X * (float)Math.Sin(-phiZ) + scene.lights[lightIdx].direction.Y * (float)Math.Cos(-phiZ);
                    scene.lights[lightIdx].direction.X = tempZ;
                }
            }
        }
    }
}
