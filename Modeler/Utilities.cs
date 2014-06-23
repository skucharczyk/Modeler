using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using Modeler.Data.Scene;

namespace Modeler
{
    public class Pair<T, U>
    {
        public Pair()
        {
        }

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Pair<T, U>)
            {
                return First.Equals(((Pair<T, U>)obj).First);
            }
            return false;
        }
    };

    static class Utilities
    {
        public static float DegToRad(float deg)
        {
            return deg * (float)Math.PI / 180;
        }

        public static float RadToDeg(float rad)
        {
            return rad * 180 / (float)Math.PI;
        }

        public static Vector3 RotatePointAroundVector(Vector3 point, Vector3 vector, float angle)
        {
            float x = point.X;
            float y = point.Y;
            float z = point.Z;

            float u = vector.X;
            float v = vector.Y;
            float w = vector.Z;

            float ux = u * x;
            float uy = u * y;
            float uz = u * z;
            float vx = v * x;
            float vy = v * y;
            float vz = v * z;
            float wx = w * x;
            float wy = w * y;
            float wz = w * z;

            float sa = (float)Math.Sin(DegToRad(angle));
            float ca = (float)Math.Cos(DegToRad(angle));

            float resX = u * (ux + vy + wz) + (x * (v * v + w * w) - u * (vy + wz)) * ca + (-wy + vz) * sa;
            float resY = v * (ux + vy + wz) + (y * (u * u + w * w) - v * (ux + wz)) * ca + (wx - uz) * sa;
            float resZ = w * (ux + vy + wz) + (z * (u * u + v * v) - w * (ux + vy)) * ca + (-vx + uy) * sa;

            return new Vector3(resX, resY, resZ);
        }

        public static T FindFirstKeyInDictionary<T, U>(Dictionary<T, U> dictionary, U value)
        {
            foreach(KeyValuePair<T, U> elem in dictionary)
            {
                if(elem.Value.Equals(value))
                {
                    return elem.Key;
                }
            }

            return default(T);
        }

        public static Vector3D CalculateNormal(Vector3D p0, Vector3D p1, Vector3D p2)
        {
            Vector3D v0 = new Vector3D(p1.x - p0.x, p1.y - p0.y, p1.z - p0.z);
            Vector3D v1 = new Vector3D(p2.x - p1.x, p2.y - p1.y, p2.z - p1.z);

            return v0.CrossProduct(v1);
        }

        public static int Factorial(int n)
        {
            int result = 1;
            while (n > 1)
                result *= n--;
            return result;
        }
    }
}
