using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modeler.Data.Scene;

namespace Modeler.Data.Shapes
{
    class Cube : Shape_
    {
        private const int maxStep = 10;

        public Cube(string _name, string uri)
            : base(_name, uri)
        { }

        public override Scene.Scene Triangulate(float density)
        {
            List<Vector3D> vertices = new List<Vector3D>();

            vertices.Add(new Vector3D(-1, -1, -1));   //0
            vertices.Add(new Vector3D(1, -1, -1));    //1
            vertices.Add(new Vector3D(1, -1, 1));     //2
            vertices.Add(new Vector3D(-1, -1, 1));    //3
            vertices.Add(new Vector3D(-1, 1, -1));    //4
            vertices.Add(new Vector3D(1, 1, -1));     //5
            vertices.Add(new Vector3D(1, 1, 1));      //6
            vertices.Add(new Vector3D(-1, 1, 1));     //7

            // Lista trójkątów tworzących sześcian
            List<Triangle> triangles = new List<Triangle>();

            // Górna ściana
            triangles.Add(new Triangle(4, 5, 7));
            triangles.Add(new Triangle(6, 7, 5));
            // Tylna ściana
            triangles.Add(new Triangle(0, 1, 4));
            triangles.Add(new Triangle(5, 4, 1));
            // Lewa ściana
            triangles.Add(new Triangle(1, 2, 5));
            triangles.Add(new Triangle(6, 5, 2));
            // Przednia ściana
            triangles.Add(new Triangle(2, 3, 6));
            triangles.Add(new Triangle(7, 6, 3));
            // Prawa ściana
            triangles.Add(new Triangle(3, 0, 7));
            triangles.Add(new Triangle(4, 7, 0));
            // Dolna ściana
            triangles.Add(new Triangle(1, 0, 2));
            triangles.Add(new Triangle(3, 2, 0));

            int step = (int)(maxStep * density);

            Vector3D midAB;
            int vertIdx = 0;
            List<Triangle> newTriangles;
            for (int i = 0; i < step; i++)
            {
                newTriangles = new List<Triangle>();
                foreach (Triangle triangle in triangles)
                {
                    midAB = new Vector3D();
                    midAB.x = (vertices[(int)triangle.p2].x + vertices[(int)triangle.p3].x) / 2.0f;
                    midAB.y = (vertices[(int)triangle.p2].y + vertices[(int)triangle.p3].y) / 2.0f;
                    midAB.z = (vertices[(int)triangle.p2].z + vertices[(int)triangle.p3].z) / 2.0f;

                    vertIdx = vertices.Count;
                    vertices.Add(midAB);

                    newTriangles.Add(new Triangle((uint)vertIdx, triangle.p1, triangle.p2));
                    newTriangles.Add(new Triangle((uint)vertIdx, triangle.p3, triangle.p1));
                }
                triangles = null;
                triangles = newTriangles;
            }

            Modeler.Data.Scene.Scene scene = new Modeler.Data.Scene.Scene();
            scene.points = vertices;
            scene.triangles = triangles;

            return scene;
        }
    }
}
