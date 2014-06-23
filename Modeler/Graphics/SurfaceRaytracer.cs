using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Modeler.Data.Scene;
using SlimDX;

/*
 * Renderowana scena: kula, za którą jest ściana z teksturą szachownicy.
 * 
 */

namespace Modeler.Graphics
{
    class SurfaceRaytracer
    {
        // Rozdzielczość obrazka
        private static int xResolution = 200;
        private static int yResolution = 200;

        // obserwator i rzutnia
        private static Vector3D observer = new Vector3D(0, 0, -8);
        // Wektor między lewym górnym a lewym dolnym wierzchołkiem rzutni
        private static Vector3D vectorV = new Vector3D(0, -1.9999f, 0);
        // Wektor między lewym górnym a prawym górnym wierzchołkiem rzutni
        private static Vector3D vectorU = new Vector3D(1.999f, 0, 0);
        // Lewy gorny punkt plaszczyzny rzutni
        private static Vector3D topLeft = new Vector3D(-1.000f, 0.9999f, -2);

        // sfera
        private static Vector3D sphereCenter = new Vector3D(0, 0, 0);
        private static float sphereRadius = 1;

        // zrodlo swiatla
        private static Light_ light = new Light_(null, Light_Type.Point, true, 1, 1, 1, 2, new Vector3(-1, 1, -2));

        // plaszczyzny z tekstura szachownicowa za oraz pod swera
        // Płaszczyzna za kula
        private static Plane planeR = new Plane(new Vector3D(-0.7071f, 0, -0.70710f), 8);
        private static Plane planeL = new Plane(new Vector3D(0.7071f, 0, -0.70710f), 8);
        // plaszczyzna pod kula
        private static Plane planeD = new Plane(new Vector3D(0, 1, 0), 1.2f);

        // maksymalna glebokosc sledzenia promieni
        private static uint maxDepth = 10;


        private static Material_ material;
        public static Bitmap output = new Bitmap(xResolution, yResolution);

        public static Bitmap Render(Material_ mat)
        {
            //System.Drawing.Graphics canvas = System.Drawing.Graphics.FromImage(output);
            //canvas.FillRectangle(Brushes.Black, 0, 0, xResolution, yResolution);
            material = mat;
            Vector3D directionVector = new Vector3D();
            Color color;
            for (int i = 0; i < xResolution; i++)
            {
                for (int j = 0; j < yResolution; j++)
                {
                    directionVector = vectorU * (i / (float)(xResolution - 1)) + vectorV * (j / (float)(yResolution - 1));
                    directionVector += topLeft;
                    directionVector -= observer;
                    directionVector.Normalize();

                    color = CalculateColor(observer, directionVector, 0);
                    output.SetPixel(i, j, color);
                }
            }

            return output;
        }

        public static Color CalculateColor(Vector3D origin, Vector3D direction, uint depth)
        {
            /*
             * Najpierw sprawdzamy czy promien uderza w kule, jesli nie to musi
             * uderzac w plaszczyzne za kula (ze wzgledu na przyjeta specyfike
             * konstrukcji sceny. Przeciecie z plaszczyzna rowniez nalezy
             * sprawdzic, aby obliczyc kolor i stworzyc "szachownice"
             */
            Color returnColor = Color.FromArgb(0, 0, 0);
            Vector3D intersectionPoint = new Vector3D();
            float distance = float.MaxValue;
            bool inside = false;

            bool sphereHit = raySphereIntersection(origin, direction, out distance, out inside); 

            // Jeśli kula nie zostala trafiona
            if (!sphereHit)
            {
                // Sprawdz przeciecie z plaszczyznami
                //float tmpDistance = -1;
                float distRight = -(planeR.d + planeR.vectorNormal.DotProduct(origin)) / (direction.DotProduct(planeR.vectorNormal));
                float distLeft = -(planeL.d + planeL.vectorNormal.DotProduct(origin)) / (direction.DotProduct(planeL.vectorNormal));
                float distDown  = -(planeD.d + planeD.vectorNormal.DotProduct(origin)) / (direction.DotProduct(planeD.vectorNormal));

                float dist = Math.Min(distLeft, distRight);
                dist = distDown > 0 ? Math.Min(distDown, dist) : dist;

                intersectionPoint = origin + direction * dist;

                bool hit = distRight > 0 || distLeft > 0 || distDown > 0;
                if (hit)
                {
                    if (distDown == dist)
                    {
                        returnColor = checkeredTexture(intersectionPoint.x, intersectionPoint.z);
                    }
                    else
                    {
                        returnColor = checkeredTexture(intersectionPoint.x, intersectionPoint.y);
                    }
                }

                return returnColor;
            }

            //if (inside) Console.WriteLine("INSIDE");    

            Vector3D vectorObser = new Vector3D();
            Vector3D vectorLight = new Vector3D();
            Vector3D reflectedRay = new Vector3D();
            intersectionPoint = origin + direction * distance;

            // Ponieważ mamy tylko jedno światło nie trzeba robić pętli po wszystkich
            // Wektor od punktu przeciecia do swiatla
            vectorLight.x = (float)light.position.X - intersectionPoint.x;
            vectorLight.y = (float)light.position.Y - intersectionPoint.y;
            vectorLight.z = (float)light.position.Z - intersectionPoint.z;

            float lengthLight = vectorLight.Length();
            vectorLight /= lengthLight;

            // Wektor od punktu przeciecia do obserwatora

            Vector3D vecNorm = intersectionPoint - sphereCenter;
            vecNorm.Normalize();
            vectorObser = origin - intersectionPoint;
            vectorObser.Normalize();

            // Wektor odbicia
            reflectedRay = vecNorm * 2;
            reflectedRay *= vectorObser.DotProduct(vecNorm);
            reflectedRay -= vectorObser;
            reflectedRay.Normalize();

            int red = 0, green = 0, blue = 0;

            // Zalamanie
            if ((material.krcR > 0 || material.krcG > 0 || material.krcB > 0) && depth < maxDepth)
            {
                Vector3D newNorm = new Vector3D(vecNorm);
                Vector3D refractedRey = new Vector3D();
                float n = 0;
                float mult = 1;
                if (!inside)
                {
                    n = 1.0f / material.n;
                }
                else
                {
                    newNorm *= -1;
                    n = material.n / 1.0f;
                }

                float cosI = (newNorm.DotProduct(-direction));
                float cosT2 = 1.0f - n * n * (1.0f - cosI * cosI);

                if (cosT2 > 0)
                {
                    if (cosI > 0)
                        refractedRey = direction * n + newNorm * (n * cosI - (float)Math.Sqrt(cosT2));
                    else
                        refractedRey = direction * n + newNorm * (n * cosI + (float)Math.Sqrt(cosT2));

                    refractedRey.Normalize();
                    refractedRey *= mult;

                    // Punkt należy troszkę odsunąć od ściany, aby nie udeżył 
                    // w ten sam element

                    intersectionPoint += refractedRey * 0.001f;

                    Color result = CalculateColor(intersectionPoint, refractedRey, ++depth);
                    red += (int)(result.R * material.krcR);
                    green += (int)(result.G * material.krcG);
                    blue += (int)(result.B * material.krcB);
                }
            }

            // Obliczanie swiatla rozproszonego i odbitego (diffuse, specular)
            // W przypadku kuli punkt trafienia jest jednoczesnie wektorem normalnym
            float lightNormDotProduct = vecNorm.DotProduct(vectorLight);
            float reflectedLightDotProduct = reflectedRay.DotProduct(vectorLight);

            // Swiatlo rozproszone (diffuse)
            red += (int) (material.colorR * material.kdcR * (light.colorR * light.power * lightNormDotProduct) / lengthLight);
            green += (int) (material.colorG * material.kdcG * (light.colorG * light.power * lightNormDotProduct) / lengthLight );
            blue += (int)(material.colorB * material.kdcB * (light.colorB * light.power * lightNormDotProduct) / lengthLight);
            // Swiatlo odbite (specular)
            if (reflectedLightDotProduct > 0)
            {
                red += (int) (material.kscR * (light.colorR * 255*light.power*Math.Pow(reflectedLightDotProduct, material.g)));;
                green += (int) (material.kscG * (light.colorG * 255*light.power*Math.Pow(reflectedLightDotProduct, material.g)));;
                blue += (int) (material.kscB * (light.colorB * 255 * light.power * Math.Pow(reflectedLightDotProduct, material.g))); ;
            }

            red = red > 255 ? 255 : red;
            green = green > 255 ? 255 : green;
            blue = blue > 255 ? 255 : blue;

            red = red < 0 ? 0 : red;
            green = green < 0 ? 0 : green;
            blue = blue < 0 ? 0 : blue;

            returnColor = Color.FromArgb(red, green, blue);
            return returnColor;
        }

        // the sphere is implicitly defind at the begining of file
        private static bool raySphereIntersection(Vector3D origin, Vector3D direction, out float distance, out bool inside)
        {
            distance = -float.MaxValue;
            inside = false;
            Vector3D L = sphereCenter - origin;
            float tca = L.DotProduct(direction);
            if (tca < 0) return false;
            float d2 = L.DotProduct(L) - tca * tca;
            if (d2 > sphereRadius * sphereRadius) return false;
            float thc = (float)Math.Sqrt(sphereRadius * sphereRadius - d2);
            float t0 = tca - thc;
            float t1 = tca + thc;

            inside = (t0 * t1) < 0;
            distance = t0 > 0 ? t0 : t1;
            return true;
        }

        private static Color checkeredTexture(float u, float v)
        {
            int x = (int)Math.Floor(u);
            int y = (int)Math.Floor(v);

            if ((x % 2 == 0 && y % 2 == 0) || (x % 2 != 0 && y % 2 != 0))
            {
                return Color.Gray;
            }
            else
            {
                return Color.Black;
            }
        }

        public static void SaveImage(string name, string path)
        {
            Image imgToSave = (Image)output.Clone();
            imgToSave = imgToSave.GetThumbnailImage(56, 56, null, IntPtr.Zero);
            imgToSave.Save(path, ImageFormat.Png);

            imgToSave.Dispose();
        }
    }
}