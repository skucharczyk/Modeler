using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modeler.Data.Scene;
using System.Globalization;

namespace Modeler.Data.Shapes
{
    class BezierSurface : Shape_
    {
        private const int numControlPoints = 4;
        public Vector3D[] ControlPoints
        { get; set;}
        // Docelowo parametry beda uzaleznione od wspolczynnika triangulacji
        //private const int n = 20;
        private int n;
        private const int minTria = 8;
        private const int maxTria = 40;
        public Vector3D[] OutputPoints
        { get; set;}
        public List<Triangle> triangles;
        public int selectedPointIdx;

        public BezierSurface(string name, string uri) 
            : base(name, uri)
        {
            ControlPoints = new Vector3D[numControlPoints*numControlPoints];

            ResetControlPoints();

            selectedPointIdx = -1;
            //Triangulate(triangCoef);
        }

        public BezierSurface(BezierSurface copy)
            : base(copy.Name, copy.ImageUri)
        {
            ControlPoints = new Vector3D[numControlPoints*numControlPoints];
            for (int i = 0; i < copy.ControlPoints.Length; i++)
            {
                ControlPoints[i] = new Vector3D(copy.ControlPoints[i].x,
                                                copy.ControlPoints[i].y,
                                                copy.ControlPoints[i].z);
            }

            n = copy.n;
            if (copy.OutputPoints != null)
            {
                {
                }
                OutputPoints = new Vector3D[copy.OutputPoints.Length];
                for (int i = 0; i < copy.OutputPoints.Length; i++)
                {
                    OutputPoints[i] = new Vector3D(copy.OutputPoints[i].x,
                                                   copy.OutputPoints[i].y,
                                                   copy.OutputPoints[i].z);
                }
                triangles = new List<Triangle>(copy.triangles.Count);
                for (int i = 0; i < copy.triangles.Count; i++)
                {
                    triangles.Add(new Triangle(copy.triangles[i].p1,
                                               copy.triangles[i].p2,
                                               copy.triangles[i].p3));
                }

                selectedPointIdx = copy.selectedPointIdx;
            }
        }

        public void ResetControlPoints()
        {
            float det = 2.0f / (float)(numControlPoints - 1);
            float x = -1, y = 0, z = -1;
            for (int i = 0; i < numControlPoints; i++)
            {
                x = -1;
                for (int j = 0; j < numControlPoints; j++)
                {
                    ControlPoints[i * numControlPoints + j] = new Vector3D(x, y, z);
                    x += det;
                }
                z += det;
            }
            CalculateSurfacePoints();
        }

        private void CalculateSurfacePoints()
        {
            float u = 0, v = 0;
            float det = 1.0f / (float)(n-1);

            for (int i = 0; i < n; i++)
            {
                v = 0;
                for (int j = 0; j < n; j++)
                {
                    // W kazdym przebiegu wewnetrznej petli obliczany jest 
                    // jeden punkt powierzchni.
                    OutputPoints[i * n + j] = BezierBlend(u, v);

                    v += det;
                }
                u += det;
            }
        }

        private Vector3D BezierBlend(float u, float v)
        {
            Vector3D outputPoint = new Vector3D(0, 0, 0);

            float bi, bj;
            for (int i = 0; i < numControlPoints; i++)
            {
                bi = BernsteinPolynomial(i, numControlPoints-1, u);
                for (int j = 0; j < numControlPoints; j++)
                {
                    bj = BernsteinPolynomial(j, numControlPoints-1, v);
                    outputPoint += ControlPoints[i * numControlPoints + j] * (bi * bj);
                }
            }            

            return outputPoint;
        }

        private float BernsteinPolynomial(int i, int m, float t)
        {
            float result = 0;

            result = (float)Utilities.Factorial(m) / (float)(Utilities.Factorial(i) * Utilities.Factorial(m - i));
            result *= (float)(Math.Pow(t, i) * Math.Pow(1 - t, m - i));

            return result;
        }

        private void CreateTriangleMesh()
        {
            // Normalne trójkątów są źle obrócone
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - 1; j++)
                {
                    triangles.Add(new Triangle((uint)(i * n + j),
                                               (uint)(i * n + (j + 1)),
                                               (uint)((i + 1) * n + j)));

                    triangles.Add(new Triangle((uint)((i + 1) * n + j),
                                               (uint)(i * n + (j + 1)),
                                               (uint)((i + 1) * n + (j + 1))));
                }
            }
        }

        public void translateSelectedPoint(float x, float y, float z)
        {
            if (selectedPointIdx != -1)
            {
                ControlPoints[selectedPointIdx].x += x;
                ControlPoints[selectedPointIdx].y += y;
                ControlPoints[selectedPointIdx].z += z;

                CalculateSurfacePoints();
            }
        }

        public override Scene.Scene Triangulate(float density)
        {
            Scene.Scene scene = new Scene.Scene();

            n = minTria + (int)((maxTria - minTria) * density);
            selectedPointIdx = -1;
            OutputPoints = new Vector3D[n * n];
            triangles = new List<Triangle>((n - 1) * (n - 1) * 2);
            //SampleSurface();
            CreateTriangleMesh();
            CalculateSurfacePoints();

            scene.points = OutputPoints.ToList();
            scene.triangles = triangles;

            return scene;
        }

        public void SaveBezierFile(string file)
        {
            List<string> text = new List<string>();
            text.Add("");
            text.Add("// nazwa");
            text.Add("name " + Name.ToString());
            text.Add("");
            text.Add("// liczba punktow kontrolnych (nxn)");
            text.Add("// wspolrzedne punktow zapisywane sa wierszowo - n pierwszych punktow to");
            text.Add("// pierwszy wiersz macierzy punktow kontrolnych, n kolejnych to drugi itd.");
            text.Add("num_control_points " + numControlPoints.ToString());

            foreach (Vector3D controlPoint in ControlPoints)
            {
                text.Add(controlPoint.x.ToString(CultureInfo.InvariantCulture) + " " +
                         controlPoint.y.ToString(CultureInfo.InvariantCulture) + " " +
                         controlPoint.z.ToString(CultureInfo.InvariantCulture));
            }

            FileSystem.File.SaveFileLines(file, text);
        }

        public static BezierSurface ReadFromFile(string file)
        {
            BezierSurface bezier = new BezierSurface("", "");

            try
            {
                List<string> text = FileSystem.File.ReadFileLines(file);
                int pointer = 0;

                string[] bezierName = FileSystem.File.GetAttributes(text[pointer]);
                if (bezierName[0] != "name")
                {
                    return null;
                } 
                string name = FileSystem.File.CutFirstString(text[pointer]);
                ++pointer;
                bezier.Name = name;

                string numCtrlPointsLabel = FileSystem.File.GetAttribute(text[pointer], 0);
                if (numCtrlPointsLabel != "num_control_points")
                {
                    return null;
                }

                if (numControlPoints != int.Parse(FileSystem.File.GetAttribute(text[pointer++], 1)))
                {
                    return null;
                }

                for (int i = 0; i<numControlPoints; i++)
                {
                    for (int j = 0; j < numControlPoints; j++)
                    {
                        string[] attsPoint = FileSystem.File.GetAttributes(text[pointer++]);
                        bezier.ControlPoints[i*numControlPoints + j] = 
                            new Vector3D(float.Parse(attsPoint[0], CultureInfo.InvariantCulture),
                                         float.Parse(attsPoint[1], CultureInfo.InvariantCulture),
                                         float.Parse(attsPoint[2], CultureInfo.InvariantCulture));
                    }
                }
            }
            catch (Exception)
            {                
                return null;
            }

            return bezier;
        }

        private void SampleSurface()
        {
            ControlPoints[0] = new Vector3D(-4.5f, -2.0f, 8f);
            ControlPoints[1] = new Vector3D(-2f, 1, 8);
            ControlPoints[2] = new Vector3D(2, -3, 6);
            ControlPoints[3] = new Vector3D(5, -1, 8);

            ControlPoints[4] = new Vector3D(-3, 3, 4);
            ControlPoints[5] = new Vector3D(0, -1, 4);
            ControlPoints[6] = new Vector3D(1, -1, 4);
            ControlPoints[7] = new Vector3D(3, 2, 4);

            ControlPoints[8] = new Vector3D(-5, -2, -2);
            ControlPoints[9] = new Vector3D(-2, -4, -2);
            ControlPoints[10] = new Vector3D(2, -1, -2);
            ControlPoints[11] = new Vector3D(5, -0, -2);

            ControlPoints[12] = new Vector3D(-4.5f, 2, -6);
            ControlPoints[13] = new Vector3D(-2, -4, -5);
            ControlPoints[14] = new Vector3D(2, 3, -5);
            ControlPoints[15] = new Vector3D(4.5f, -2, -6);
        }

        public int EstimatedMemory()
        {
            return ControlPoints.Length*12 + OutputPoints.Length*12 + triangles.Count*12;
        }
    }
}