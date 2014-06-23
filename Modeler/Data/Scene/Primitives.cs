using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace Modeler.Data.Scene
{
    class Vector3D
    {
        public float x, y, z;

        public Vector3D(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3D()
        {
            x = y = z = 0;
        }

        public Vector3D(Vector3D point)
        {
            x = point.x;
            y = point.y;
            z = point.z;
        }

        public static Vector3D operator-(Vector3D p1)
        {
            return new Vector3D(-p1.x, -p1.y, -p1.z);
        }

        public static Vector3D operator+(Vector3D p1, Vector3D p2)
        {
            return new Vector3D(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);
        }

        public static Vector3D operator-(Vector3D p1, Vector3D p2)
        {
            return new Vector3D(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
        }

        public static Vector3D operator*(Vector3D p1, Vector3D p2)
        {
            return new Vector3D(p1.x * p2.x, p1.y * p2.y, p1.z * p2.z);
        }

        public static Vector3D operator/(Vector3D p1, Vector3D p2)
        {
            return new Vector3D(p1.x / p2.x, p1.y / p2.y, p1.z / p2.z);
        }

        public static Vector3D operator/(Vector3D vec, float divisor)
        {
            return new Vector3D(vec.x/divisor, vec.y/divisor, vec.z/divisor);
        }

        public static Vector3D operator*(Vector3D vec, float multiplier)
        {
            return new Vector3D(vec.x * multiplier, vec.y * multiplier, vec.z * multiplier);
        }

        public void Normalize()
        {
            float length = Length();

            x /= length;
            y /= length;
            z /= length;
        }

        public float DotProduct(Vector3D vector)
        {
            return x * vector.x + y * vector.y + z * vector.z;
        }

        public Vector3D CrossProduct(Vector3D vector)
        {
            Vector3D output = new Vector3D();
            output.x = y * vector.z - z * vector.y;
            output.y = z * vector.x - x * vector.z;
            output.z = x * vector.y - y * vector.x;

            return output;
        }

        /// <summary>
        /// Metoda zwracająca długość między punktem a początkiem układu
        /// współrzędnych (punkt [0, 0, 0]).
        /// </summary>
        /// <returns></returns>
        public float Length()
        {
            //float length;
            //length = (float) Math.Sqrt((x * x) + (y * y) + (z * z));
            //return length;
            return (float)Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        /// <summary>
        /// Metoda zwracająca długość między dwoma punktami.
        /// </summary>
        /// <param name="refrence"></param>
        /// <returns></returns>
        public float Length(Vector3D refrence)
        {
            throw new NotImplementedException("Do implementacji.");
        }

        public static implicit operator Vector3(Vector3D p)
        {
            return new Vector3(p.x, p.y, p.z);
        }

        /// <summary>
        /// Metoda mnożąca współrzędne punktu przez liczbę.
        /// </summary>
        /// <param name="multiplier"></param>

        public override string ToString()
        {
            return x + " " + y + " " + z;
        }
    }

    class Triangle
    {
        public uint p1, p2, p3; // indeksy punktów

        public Triangle(uint p1, uint p2, uint p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
    }
}
