using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modeler.Data;
//using Modeler.Data.Geometry;
using Modeler.Data.Scene;

namespace Modeler.Data.Shapes
{
    class Sphere : Shape_
    {
        private const int maxStep = 30;
        private const int minStep = 5;

        public Sphere(string _name, string uri)
            : base(_name, uri)
        { }

        /// <summary>
        /// Triangulacja sfery poprzez podzial szescianu
        /// </summary>
        /// <param name="density"></param>
        /// <returns></returns>
        public Modeler.Data.Scene.Scene TriangulateAlt(float density)
        {
            //Tymczasowo na sztywno ustalona liczba kroków
            // gestosc 1 - 10 krokow
            // gestosc 0 - 0 krok
            uint step = (uint) (10f * density);

            List<Vector3D> vertices = new List<Vector3D>();

            vertices.Add(new Vector3D(-1, -1, 1));    //0
            vertices.Add(new Vector3D(1, -1, 1));     //1
            vertices.Add(new Vector3D(1, -1, -1));    //2
            vertices.Add(new Vector3D(-1, -1, -1));   //3
            vertices.Add(new Vector3D(-1, 1, 1));     //4
            vertices.Add(new Vector3D(1, 1, 1));      //5
            vertices.Add(new Vector3D(1, 1, -1));     //6
            vertices.Add(new Vector3D(-1, 1, -1));    //7

            // Lista trójkątów tworzących sześcian
            List<Triangle> triangles = new List<Triangle>();

            // Górna ściana
            triangles.Add(new Triangle(5, 7, 6));
            triangles.Add(new Triangle(7, 5, 4));
            // Przednia ściana
            triangles.Add(new Triangle(5, 0, 1));
            triangles.Add(new Triangle(0, 5, 4));
            // Prawa ściana
            triangles.Add(new Triangle(5, 2, 6));
            triangles.Add(new Triangle(2, 5, 1));
            // Tylna ściana
            triangles.Add(new Triangle(6, 3, 2));
            triangles.Add(new Triangle(3, 6, 7));
            // Lewa ściana
            triangles.Add(new Triangle(4, 3, 7));
            triangles.Add(new Triangle(3, 4, 0));
            // Dolna ściana
            triangles.Add(new Triangle(0, 1, 2));
            triangles.Add(new Triangle(2, 3, 0));

            /*
             * Poczatek wlasciwego algorytmu. Korzystajac z listy trojkatow i 
             * wierzcholkow szescianu stworzyc kule.
             */

            for (int i = 0; i < step; i++)
            {
                List<Triangle> newTriangles = new List<Triangle>(triangles.Count() * 2);
                Vector3D midAB;
                int idx = -1;
                int tmp;
                foreach (Triangle triangle in triangles)
                {
                    // Oblicz wspolrzedne nowego wierzcholka
                    midAB = vertices[(int)(triangle.p1)] + vertices[(int)(triangle.p2)];
                    midAB /= 2f;
                    //midAB.Multiply(vertices[(int)triangle.p1].Length()/midAB.Length());
                    midAB *= (vertices[(int)triangle.p1].Length() / midAB.Length());

                    // Dodaj wierzcholek do listy i sprawdz indeks
                    if ((tmp = vertices.IndexOf(midAB)) != -1)
                    {
                        idx = tmp;
                    }
                    else
                    {
                        idx = vertices.Count();
                        vertices.Add(midAB);
                    }

                    // Dodaj nowe trojkaty do listy nowych trojkatow
                    newTriangles.Add(new Triangle(triangle.p3, triangle.p1, (uint)idx));
                    newTriangles.Add(new Triangle(triangle.p2, triangle.p3, (uint)idx));
                }

                triangles = null;
                triangles = newTriangles;
            }

            Modeler.Data.Scene.Scene scene = new Modeler.Data.Scene.Scene();
            scene.points = vertices;
            scene.triangles = triangles;

            //TraingulateAlt(density);

            return scene;
        }

        /// <summary>
        /// Triangulacja sfery metodą południkowo równoleżnikową
        /// </summary>
        /// <param name="density"></param>
        /// <returns></returns>
        public override Modeler.Data.Scene.Scene Triangulate(float density)
        {
            Scene.Scene scene = new Scene.Scene();
            List<Vector3D> vertices = new List<Vector3D>();
            List<Triangle> triangles = new List<Triangle>();

            int width = minStep + (int)((maxStep - minStep) * density);
            int height = minStep + (int)((maxStep - minStep) * density);
            float theta, phi;
            float x, y, z;

            //Tworzenie wierzchołków
            for (int j = 1; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    theta = (float)((float)j / height - 1) * (float)Math.PI;
                    phi = (float)((float)i / width - 1) * (float)Math.PI * 2;

                    x = (float)(Math.Sin(theta) * Math.Cos(phi));
                    y = (float)(Math.Cos(theta));
                    z = (float)(-Math.Sin(theta) * Math.Sin(phi));

                    vertices.Add(new Vector3D(x, y, z));
                }
            }
            // Dodanie biegunow
            vertices.Add(new Vector3D(0, 1, 0));
            vertices.Add(new Vector3D(0, -1, 0));

            uint p1, p2, p3;

            // Łączenie punktow w trojkaty, za wyjatkiem trojkatow przy biegunach
            for (uint j = 0; j < height - 2 ; j++)
            {
                for (uint i = 0; i < width ; i++)
                {
                    p1 = (uint)((j * width + i) % width + j * width);
                    p2 = (uint)(((j + 1) * width + i + 1) % width + (j + 1) * width);
                    p3 = (uint)((j * width + i + 1) % width + j * width);
                    triangles.Add(new Triangle(p1, p2, p3));
                    p1 = (uint)((j * width + i) % width + j * width);
                    p2 = (uint)(((j + 1) * width + i) % width + (j + 1) * width);
                    p3 = (uint)(((j + 1) * width + i + 1) % width + (j + 1) * width);
                    triangles.Add(new Triangle(p1, p2, p3));
                }
            }

            // Laczenie trojkatow przy biegunach
            uint north = (uint)vertices.Count - 2;
            uint south = (uint)vertices.Count - 1;
            for (uint i = 0; i < width; i++)
            {
                p1 = (uint)(i % width);
                p2 = (uint)((i + 1) % width);
                triangles.Add(new Triangle(south, p1, p2));
                p1 = (uint)(((height - 2) * width + i) % width + (height - 2) * width);
                p2 = (uint)(((height - 2) * width + i + 1) % width + (height - 2) * width);
                triangles.Add(new Triangle(p2, p1, north));
            }

            scene.points = vertices;
            scene.triangles = triangles;

            return scene;   
        }
    }
}