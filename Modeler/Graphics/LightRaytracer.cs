using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Modeler.Data.Scene;

namespace Modeler.Graphics
{
    /// <summary>
    /// Komponent służący do wizualizacji ustawień parametrów światła. Światło
    /// Umieszczone jest pomiędzy trzema płaszczyznami, na których będzie 
    /// widać efekty świetlne. Wizualizator obsługuje światła jednorodne, 
    /// stożkowe oraz światła z prostą charakterystyką goniometryczną.
    /// </summary>
    class LightRaytracer
    {
        // 3 płaszczyzny
        private static Plane[] planes = new Plane[3]
        {
            new Plane(new Vector3D(0, 0, 1), -1),
            new Plane(new Vector3D(0, 1, 0), -1),
            new Plane(new Vector3D(1, 0, 0), -1)
        };
        private const int planeNumber = 3;
        // Wizualizowane światło
        private static Light_ light;
        // Pozycja swiatla musi byc wpisana na sztywno, w rendererze zawsze bedzie w tej samej pozycji
        private static Vector3D lightPosition = new Vector3D(-1f, 2f, 2.5f);
        // Kierunek swiatla musi byc wpisany na sztowno, w rendererze zawsze bedzie swiecic w ta sama strone
        private static Vector3D lightDirection = new Vector3D(0f, -1f, 0f);
        private const int xResolution = 200;
        private const int yResolution = 200;
        private const int maxDepth = 10;
        // Lewy górny punkt płaszczyzny rzutni
        private static Vector3D topLeft = new Vector3D(-1, 2.5f, -1);
        // Wektor od lewego górnego punktu płaszczyzny rzutni w dół
        private static Vector3D vectorV = new Vector3D(0, -2.5f, 0);
        // Wektor od lewego górnego punktu płaszczyzny rzutni w prawo
        private static Vector3D vectorU = new Vector3D(4, 0, 4);
        // Punkt, w ktorym znajduje sie obserwator
        private static Vector3D observer = new Vector3D(-10, 3.5f, 10);
        private static Color backgroundColor = Color.FromArgb(255, 0, 0);
        private static Material_ planeMaterial = new Material_("Sciana", 120, 120, 120,
                                                                0.1f, 0.1f, 0.1f,
                                                                0.1f, 0.1f, 0.1f,
                                                                0, 0, 0,
                                                                0, 0, 0,
                                                                1000, 0);
        public static Bitmap image = new Bitmap(xResolution, yResolution);

        public static Bitmap Render(Light_ lgt)
        {
            light = lgt;
            Vector3D direction = new Vector3D();
            Color color;

            for (int i = 0; i < xResolution; i++)
            {
                for (int j = 0; j < yResolution; j++)
                {
                    direction = vectorU * (i / (float)(xResolution - 1)) + vectorV * (j / (float)(yResolution - 1));
                    direction += topLeft;
                    direction -= observer;
                    direction.Normalize();

                    color = CalculateColor(observer, direction, 0);

                    image.SetPixel(i, j, color);
                }
            }

            return image;
        }

        private static Color CalculateColor(Vector3D origin, Vector3D direction, int depth)
        {
            Color output = backgroundColor;
            Vector3D intersectionPoint = new Vector3D();
            float distance = float.MaxValue;
            float tmpDistance;
            int planeId = -1;

            for (int i=0; i<planeNumber; i++)
            {
                tmpDistance = -1;

                tmpDistance = -(planes[i].d + planes[i].vectorNormal.DotProduct(origin));
                tmpDistance /= planes[i].vectorNormal.DotProduct(direction);

                if (tmpDistance > -1 && tmpDistance < distance)
                {
                    distance = tmpDistance;
                    planeId = i;
                }
            }

            if (distance == float.MaxValue)
                return output;

            intersectionPoint = origin + direction * distance;

            int red = 0, green = 0, blue = 0;
            Vector3D vectorObserver;
            Vector3D reflectedRay;
            Vector3D vectorLight = new Vector3D();

            // Wektor od punktu przeciecia do obserwatora
            vectorObserver = origin - intersectionPoint;
            vectorObserver.Normalize();

            // Wektor swiatla odbitego
            reflectedRay = intersectionPoint * 2;
            reflectedRay *= vectorObserver.DotProduct(intersectionPoint);
            reflectedRay -= vectorObserver;
            reflectedRay.Normalize();

            // Wektor w kierunku swiatla
            vectorLight = lightPosition - intersectionPoint;

            float lengthLight = vectorLight.Length();
            vectorLight /= lengthLight;

            // kąt między wektorem normalnym płaszczyzny a wektorem w kierunku świarła
            float lightNormDotProduct = (float)Math.Abs(planes[planeId].vectorNormal.DotProduct(vectorLight));
            // kąt miedzy wektrem odbitym a wektorem w kierunku światła
            float reflectedLightDotProduct = reflectedRay.DotProduct(vectorLight);

            // Oliczenia koloru dla swiatla jednorodnego
            if (light.type == Light_Type.Point)
            {

                red += (int)(planeMaterial.colorR * planeMaterial.kdcR * (light.colorR * light.power * lightNormDotProduct) / lengthLight);
                green += (int)(planeMaterial.colorG * planeMaterial.kdcG * (light.colorG * light.power * lightNormDotProduct) / lengthLight);
                blue += (int)(planeMaterial.colorB * planeMaterial.kdcB * (light.colorB * light.power * lightNormDotProduct) / lengthLight);

                if (reflectedLightDotProduct > 0)
                {
                    red += (int)(planeMaterial.kscR * (light.colorR * 255 * light.power * Math.Pow(reflectedLightDotProduct, planeMaterial.g)));
                    green += (int)(planeMaterial.kscG * (light.colorG * 255 * light.power * Math.Pow(reflectedLightDotProduct, planeMaterial.g)));
                    blue += (int)(planeMaterial.kscB * (light.colorB * 255 * light.power * Math.Pow(reflectedLightDotProduct, planeMaterial.g)));
                }
            }
            // Obliczenia koloru dla swiatla stozkowego
            else if (light.type == Light_Type.Spot)
            {
                // Nalezy znalezc wektor miedzy zrodlem swiatla a punktem trafienia
                Vector3D vectorLightIntersection = intersectionPoint - lightPosition;
                vectorLightIntersection.Normalize();

                lightDirection.Normalize();
                // Jesli kat miedzy wektorem kierunkowym swiatla a wektorem w kierunku
                // punktu trafienia jest wiekszy niz zadany w swietle, punkt nie
                // zostanie oswietlony

                if (vectorLightIntersection.DotProduct(lightDirection) > 0)
                {
                    float angle = (float)Math.Acos(Math.Abs(vectorLightIntersection.DotProduct(lightDirection)));

                    float radOuterAngle = Utilities.DegToRad(light.outerAngle) / 2f;

                    if (angle < radOuterAngle)
                    {
                        red += (int)(planeMaterial.colorR * planeMaterial.kdcR * (light.colorR * light.power * lightNormDotProduct) / lengthLight);
                        green += (int)(planeMaterial.colorG * planeMaterial.kdcG * (light.colorG * light.power * lightNormDotProduct) / lengthLight);
                        blue += (int)(planeMaterial.colorB * planeMaterial.kdcB * (light.colorB * light.power * lightNormDotProduct) / lengthLight);

                        if (reflectedLightDotProduct > 0)
                        {
                            red += (int)(planeMaterial.kscR * (light.colorR * 255 * light.power * Math.Pow(reflectedLightDotProduct, planeMaterial.g)));
                            green += (int)(planeMaterial.kscG * (light.colorG * 255 * light.power * Math.Pow(reflectedLightDotProduct, planeMaterial.g)));
                            blue += (int)(planeMaterial.kscB * (light.colorB * 255 * light.power * Math.Pow(reflectedLightDotProduct, planeMaterial.g)));
                        }

                        float radInnerAngle = Utilities.DegToRad(light.innerAngle) / 2;
                        if (angle > radInnerAngle)
                        {
                            float coef = (angle - radInnerAngle) / (radOuterAngle / 2);
                            coef = 1 - coef;

                            red = (int)(red * coef);
                            green = (int)(green * coef);
                            blue = (int)(blue * coef);
                        }
                    }
                }
            }
            // Obliczenia koloru dla swiatla z prosta charakterystyka goniometryczna
            else if (light.type == Light_Type.Goniometric)
            {
                if (light.goniometric.Count > 0)
                {
                    // Nalezy znalezc wektor miedzy zrodlem swiatla a punktem trafienia
                    Vector3D vectorLightIntersection = intersectionPoint - lightPosition;
                    vectorLightIntersection.Normalize();

                    lightDirection.Normalize();
                    // Jesli kat miedzy wektorem kierunkowym swiatla a wektorem w kierunku
                    // punktu trafienia jest wiekszy niz zadany w swietle, punkt nie
                    // zostanie oswietlony
                    float neg = 0;
                    float angle = (float)Math.Acos(Math.Abs(neg = vectorLightIntersection.DotProduct(lightDirection)));
                    if (neg < 0)
                        angle = (float)(Math.PI - angle);
                    //Vector3D cross = vectorLightIntersection.CrossProduct(lightDirection);
                    //if (cross.z > 0)
                    //    angle *= -1;

                    // Wyznaczanie kata miedzy osia X a osia Z
                    //Vector3D vec1 = new Vector3D(vectorLightIntersection.x, 0, vectorLightIntersection.z);
                    //Vector3D vec2 = new Vector3D(lightDirection.x, 0, lightDirection.z);
                    //vec1.Normalize();
                    //vec2.Normalize();
                    //float angleXZ = (float)Math.Acos(Math.Abs(vec1.DotProduct(vec2)));
                    //cross = vec1.CrossProduct(vec2);
                    //if (cross.y > 0)
                        //angleXZ *= -1;

                    // Wyznaczanie kata miedzy osia X a osia Y
                    //vec1 = new Vector3D(vectorLightIntersection.x, vectorLightIntersection.y, 0);
                    //vec2 = new Vector3D(lightDirection.x, lightDirection.y, 0);
                    //vec1.Normalize();
                    //vec2.Normalize();
                    //float angleXY = (float)Math.Acos(Math.Abs(neg = vec1.DotProduct(vec2)));
                    //cross = vec2.CrossProduct(vec1);
                    //if (neg < 0)
                    //    angleXY = (float)(Math.PI - angleXY);
                    //if (cross.z > 0)
                    //    angleXY *= -1;

                    float radMinimumAngle = Utilities.DegToRad(light.goniometric.Keys[0]);
                    float radMaximumAngle = Utilities.DegToRad(light.goniometric.Keys[light.goniometric.Count - 1]);

                    // Jesli oba katy znajduja sie w dopuszczalnym zakresie to liczymy kolor
                    if (/*angleXZ < radMaximumAngle && angleXZ > radMinimumAngle &&*/ angle < radMaximumAngle && angle > radMinimumAngle)
                    {

                        red += (int)(planeMaterial.colorR * planeMaterial.kdcR * (light.colorR * light.power * lightNormDotProduct) / lengthLight);
                        green += (int)(planeMaterial.colorG * planeMaterial.kdcG * (light.colorG * light.power * lightNormDotProduct) / lengthLight);
                        blue += (int)(planeMaterial.colorB * planeMaterial.kdcB * (light.colorB * light.power * lightNormDotProduct) / lengthLight);

                        if (reflectedLightDotProduct > 0)
                        {
                            red += (int)(planeMaterial.kscR * (light.colorR * 255 * light.power * Math.Pow(reflectedLightDotProduct, planeMaterial.g)));
                            green += (int)(planeMaterial.kscG * (light.colorG * 255 * light.power * Math.Pow(reflectedLightDotProduct, planeMaterial.g)));
                            blue += (int)(planeMaterial.kscB * (light.colorB * 255 * light.power * Math.Pow(reflectedLightDotProduct, planeMaterial.g)));
                        }

                        // Nalezy znalezc wspolczynnik, o jaki trzeba przemnozyc wartosci RGB
                        // W tym celu trzeba sprawdzic, miedzy ktorymi wartosciami w 
                        // goniometrii swiatla znajduje sie kat. Jesli znajdziemy miedzy
                        // bedziemy mogli wyznaczyc "odleglosc" miedzy tymi katami
                        // i na tej podstawie wziac srednia wazona z wartosci mnoznika 
                        // obu katow
                        int idxHigher = -1, idxLower;
                        for (int i = 1; i < light.goniometric.Count; i++)
                        {
                            if (idxHigher < 0 && Utilities.DegToRad(light.goniometric.Keys[i]) > angle)
                            {
                                idxHigher = i;
                                break;
                            }
                        }

                        if (idxHigher > -1)
                        {
                            idxLower = idxHigher - 1;

                            float angleHigher = Utilities.DegToRad(light.goniometric.Keys[idxHigher]);
                            float angleLower = Utilities.DegToRad(light.goniometric.Keys[idxLower]);

                            float coefAngle = angle - angleLower;
                            float detAngle = angleHigher - angleLower;

                            float coefficient = coefAngle / detAngle;

                            coefficient = light.goniometric.Values[idxLower] * (1 - coefficient) + light.goniometric.Values[idxHigher] * (coefficient);

                            red = (int)(red * coefficient);
                            green = (int)(green * coefficient);
                            blue = (int)(blue * coefficient);
                        }
                        else { red = 0; green = 0; blue = 0; }
                    }
                }
            }

            red = red > 255 ? 255 : red;
            green = green > 255 ? 255 : green;
            blue = blue > 255 ? 255 : blue;

            red = red < 0 ? 0 : red;
            green = green < 0 ? 0 : green;
            blue = blue < 0 ? 0 : blue;

            output = Color.FromArgb(red, green, blue);

            return output;
        }

        public static void SaveImage(string name, string path)
        {
            Image imgToSave = (Image)image.Clone();
            imgToSave = imgToSave.GetThumbnailImage(xResolution, yResolution, null, IntPtr.Zero);
            imgToSave.Save(path, ImageFormat.Png);

            imgToSave.Dispose();
        }
    }
}