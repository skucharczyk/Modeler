using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modeler.Data.Scene;

namespace Modeler.Data.Shapes
{
    class TriangleShape : Shape_
    {
        private const int maxStep = 10;

        public TriangleShape(string _name, string uri)
            : base(_name, uri)
        { }

        public override Scene.Scene Triangulate(float density)
        {
            Scene.Scene scene = new Scene.Scene();
            List<Vector3D> vertices = new List<Vector3D>()
            {
                new Vector3D(0, 0, 0),
                new Vector3D(0, 1, 0),
                new Vector3D(1, 0, 0)
            };
            List<Triangle> triangles = new List<Triangle>()
            {
                new Triangle(2, 0, 1)
            };

            int step = (int) (maxStep * density);

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

            scene.points = vertices;
            scene.triangles = triangles;

            return scene;
        }
    }
}
