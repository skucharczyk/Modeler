using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modeler.Data.Scene;

namespace Modeler.Transformations
{
    class Intersection
    {
        private static BoundingBox selectedObjectBB;
        private static List<BoundingBox> otherSceneObjBB = new List<BoundingBox>();
        private static BoundingBox sceneBB;
        private static BoundingBox requiredSpaceBB;

        private static void SetBBoxes(Scene s) 
            //działa na razie tylko na meshach które nie są zagnieżdżone w hierarchii, dodaje tylko te które nie są zaznaczone, ustawia bb sceny
        {

            float minx = float.MaxValue;
            float miny = float.MaxValue;
            float minz = float.MaxValue;
            float maxx = float.MinValue;
            float maxy = float.MinValue;
            float maxz = float.MinValue;
            float tmpx, tmpy, tmpz;
            BoundingBox tmpbb;

            foreach (HierarchyMesh mesh in s.hierarchy.GetAllMeshes())
            {
                tmpbb = new BoundingBox(s, mesh);

                if (!s.IsTriangleSelected(mesh.triangles[0]))
                {
                    //sprawdzamy, czy obiekt jest zaznaczony - jest kiedy jakikolwiek jego trójkąt jest na liście trójkatów zaznaczonych
                    //jeśli nie jest dodajemy jego bb do listy
                    otherSceneObjBB.Add(tmpbb);
                }
                //wyznaczamy skrajne punkty do wyznaczenia bb sceny
                tmpx = tmpbb.minBB.x;
                tmpy = tmpbb.minBB.y;
                tmpz = tmpbb.minBB.z;

                if (tmpx < minx)
                    minx = tmpx;
                if (tmpy < miny)
                    miny = tmpy;
                if (tmpz < minz)
                    minz = tmpz;

                tmpx = tmpbb.maxBB.x;
                tmpy = tmpbb.maxBB.y;
                tmpz = tmpbb.maxBB.z;

                if (tmpx > maxx)
                    maxx = tmpx;
                if (tmpy > maxy)
                    maxy = tmpy;
                if (tmpz > maxz)
                    maxz = tmpz;
            }

            Vector3D tmpmin = new Vector3D(minx, miny, minz);
            Vector3D tmpmax = new Vector3D(maxx, maxy, maxz);
            sceneBB = new BoundingBox(tmpmin, tmpmax);
        }

        private static void SetSlidingObjectBBox(Scene s)
        {
            List<Triangle> selected = new List<Triangle>();

            foreach (HierarchyMesh selMesh in s.selTriangles)
            {
                foreach (uint triangleIdx in selMesh.triangles)
                {
                    if(s.IsTriangleSelected(triangleIdx))
                        selected.Add(s.triangles[(int)triangleIdx]);
                }
                
            }

            selectedObjectBB = new BoundingBox(s, selected);
        }

        private static void SetSpaceRequiredBox(Scene s, String direction) 
        //parametr direction jest odpowiedzialny za kierunek w którym dosuwamy zaznaczony objekt,
        //może przyjmować 6 wartości: xyl, xyr, zxf, zxb, yzu, yzd. Będzie podawany odpowiednio na podstawie
        //wybranej opcji w menu kontekstowym oraz widoku, w którym dana opcja została wybrana
        {
            if(direction.Equals("xyl")) //rzutowanie na płaszczyznę x y przesunięcie do lewej
            {
                Vector3D tmp = new Vector3D(sceneBB.minBB.x, selectedObjectBB.minBB.y, selectedObjectBB.minBB.z);
                Vector3D tmp2 = new Vector3D(selectedObjectBB.minBB.x, selectedObjectBB.maxBB.y, selectedObjectBB.maxBB.z);
                requiredSpaceBB = new BoundingBox(tmp, tmp2);
            }
            else if(direction.Equals("xyr")) //rzutowanie na płaszczyznę x y przesunięcie do prawej
            {
                Vector3D tmp = new Vector3D(sceneBB.maxBB.x, selectedObjectBB.maxBB.y, selectedObjectBB.maxBB.z);
                Vector3D tmp2 = new Vector3D(selectedObjectBB.maxBB.x, selectedObjectBB.minBB.y, selectedObjectBB.minBB.z);
                requiredSpaceBB = new BoundingBox(tmp2, tmp);
            }
            else if(direction.Equals("yzu")) //rzutowanie na płaszczyznę y z przesunięcie do góry
            {
                Vector3D tmp = new Vector3D(selectedObjectBB.maxBB.x, sceneBB.maxBB.y, selectedObjectBB.maxBB.z);
                Vector3D tmp2 = new Vector3D(selectedObjectBB.minBB.x, selectedObjectBB.maxBB.y, selectedObjectBB.minBB.z);
                requiredSpaceBB = new BoundingBox(tmp2, tmp);
            }
            else if(direction.Equals("yzd")) //rzutowanie na płaszczyznę y z przesunięcie do dołu
            {
                Vector3D tmp = new Vector3D(selectedObjectBB.minBB.x, sceneBB.minBB.y, selectedObjectBB.minBB.z);
                Vector3D tmp2 = new Vector3D(selectedObjectBB.maxBB.x, selectedObjectBB.minBB.y, selectedObjectBB.maxBB.z);
                requiredSpaceBB = new BoundingBox(tmp, tmp2);
            }
            else if(direction.Equals("zxb")) //rzutowanie na płaszczyznę z x przesunięcie do tyłu 
            {
                Vector3D tmp = new Vector3D(selectedObjectBB.minBB.x, selectedObjectBB.minBB.y, sceneBB.minBB.z);
                Vector3D tmp2 = new Vector3D(selectedObjectBB.maxBB.x, selectedObjectBB.maxBB.y, selectedObjectBB.minBB.z);
                requiredSpaceBB = new BoundingBox(tmp, tmp2);
            }
            else //if(direction.Equals("zxf")) //rzutowanie na płaszczyznę z x przesunięcie do przodu
            {
                Vector3D tmp = new Vector3D(selectedObjectBB.maxBB.x, selectedObjectBB.maxBB.y, sceneBB.maxBB.z);
                Vector3D tmp2 = new Vector3D(selectedObjectBB.minBB.x, selectedObjectBB.minBB.y, selectedObjectBB.maxBB.z);
                requiredSpaceBB = new BoundingBox(tmp2, tmp);
            }
        }

        private static bool BBoxCollisionDetection(BoundingBox b1, BoundingBox b2)//, String direction)
        {
                if ((b1.maxBB.x < b2.minBB.x || b2.maxBB.x < b1.minBB.x) || 
                    (b1.maxBB.y < b2.minBB.y || b2.maxBB.y < b1.minBB.y) ||
                    (b1.maxBB.z < b2.minBB.z || b2.maxBB.z < b1.minBB.z))
                    return false;
                return true;
        }

        private static float CalculateDistanceEZ(BoundingBox otherObj, BoundingBox selObj, String direction)
        {
            if (direction.Equals("xyl")) //rzutowanie na płaszczyznę x y przesunięcie do lewej
            {
                return Math.Abs(selObj.minBB.x - otherObj.maxBB.x);
            }
            else if (direction.Equals("xyr")) //rzutowanie na płaszczyznę x y przesunięcie do prawej
            {
                return Math.Abs(selObj.maxBB.x - otherObj.minBB.x);
            }
            else if (direction.Equals("yzu")) //rzutowanie na płaszczyznę y z przesunięcie do góry
            {
                return Math.Abs(selObj.maxBB.y - otherObj.minBB.y);
            }
            else if (direction.Equals("yzd")) //rzutowanie na płaszczyznę y z przesunięcie do dołu
            {
                return Math.Abs(selObj.minBB.y - otherObj.maxBB.y);
            }
            else if (direction.Equals("zxb")) //rzutowanie na płaszczyznę z x przesunięcie do tyłu 
            {
                return Math.Abs(selObj.minBB.z - otherObj.maxBB.z);
            }
            else //if(direction.Equals("zxf")) //rzutowanie na płaszczyznę z x przesunięcie do przodu
            {
                return Math.Abs(selObj.maxBB.z - otherObj.minBB.z);
            }
        }

        public static void SlideEZver(Scene scene, String slideDir)
        {
            float minDistance = float.MaxValue;
            float tmpDistance;
            bool colisionFound = false;
            bool canTranslate = true;

            SetBBoxes(scene);                       //tworzy strukture bboxów nie zaznaczonych obiektów na dole hierarchii, oraz bboxx sceny
            SetSlidingObjectBBox(scene);
            SetSpaceRequiredBox(scene, slideDir);   //wyznacza przestrzeń, w której będzie przesuwany zaznaczony obiekt
            
            Console.WriteLine("bb sceny:                " + sceneBB.minBB.x + " " + sceneBB.minBB.y + " " + sceneBB.minBB.z +" : " + sceneBB.maxBB.x + " " + sceneBB.maxBB.y + " " + sceneBB.maxBB.z);
            Console.WriteLine("space bb:                " + requiredSpaceBB.minBB.x + " " + requiredSpaceBB.minBB.y + " " + requiredSpaceBB.minBB.z + " : "+ requiredSpaceBB.maxBB.x + " " + requiredSpaceBB.maxBB.y + " " + requiredSpaceBB.maxBB.z + " ");
            Console.WriteLine("bb przesuwanego obiektu: " + selectedObjectBB.minBB.x + " " + selectedObjectBB.minBB.y + " " + selectedObjectBB.minBB.z + " : " + selectedObjectBB.maxBB.x + " " + selectedObjectBB.maxBB.y + " " + selectedObjectBB.maxBB.z);

            if (otherSceneObjBB.Count > 0)
            {
                foreach (BoundingBox otherBB in otherSceneObjBB)
                {
                    Console.WriteLine("Kolizja sprawdzana z obiektem " + otherBB.minBB.x + " " + otherBB.minBB.y + " " + otherBB.minBB.z + " : "+ otherBB.maxBB.x + " " + otherBB.maxBB.y + " " + otherBB.maxBB.z);
                    if (BBoxCollisionDetection(otherBB, requiredSpaceBB))//, slideDir))
                    {
                        colisionFound = true;
                        tmpDistance = CalculateDistanceEZ(otherBB, selectedObjectBB, slideDir);
                        if (tmpDistance < minDistance)
                            minDistance = tmpDistance;
                        Console.WriteLine("Kolizja wykryta z obiektem " + otherBB.minBB.x + " " + otherBB.minBB.y +" " + otherBB.minBB.z + " : "+ otherBB.maxBB.x + " " + otherBB.maxBB.y + " " + otherBB.maxBB.z);
                        if (BBoxCollisionDetection(otherBB, selectedObjectBB)) //jeśli jakiś obiekt przecina się z dosuwanym na początku operacji- nie można dosuwać
                            canTranslate = false;
                    }
                }

                if (colisionFound && canTranslate)
                {
                    Console.WriteLine("Min distance: " + minDistance);

                    if (slideDir.Equals("xyr")) //rzutowanie na płaszczyznę x y - przesunięcie w osi x
                    {
                        Transformations.Translate(scene, minDistance, 0, 0);
                    }

                    else if (slideDir.Equals("xyl"))
                    {
                        Transformations.Translate(scene, -minDistance, 0, 0);
                    }

                    else if (slideDir.Equals("yzu")) //rzutowanie na płaszczyznę y z - przesunięcie w osi y
                    {
                        Transformations.Translate(scene, 0, minDistance, 0);
                    }

                    else if (slideDir.Equals("yzd")) //rzutowanie na płaszczyznę y z - przesunięcie w osi y
                    {
                        Transformations.Translate(scene, 0, -minDistance, 0);
                    }

                    else if (slideDir.Equals("zxf")) //rzutowanie na płaszczyznę y z - przesunięcie w osi y
                    {
                        Transformations.Translate(scene, 0, 0, minDistance);
                    }

                    else //rzutowanie na płaszczyznę z x - przesunięcie w osi z zxb
                    {
                        Transformations.Translate(scene, 0, 0, -minDistance);
                    }
                }
                else if (colisionFound && !canTranslate)
                {
                    Console.WriteLine("Płaszczyzna BBoxa którą chcemy dosuwać jest wewnątrz BBoxa innego obiektu. Nie można wykonać operacji.");
                }
                else
                {
                    Console.WriteLine("Brak kolizji");
                }
            }
            else
                Console.WriteLine("Brak innych elementów ");
            Clear();
        }

        public static void Slide(Scene scene, String direction)
        {
            BoundingBox tmpbb;
            float tmpDistance = float.MaxValue, minDistance = float.MaxValue, minDistanceGlobal = float.MaxValue;
            bool colisionFound = false;
            bool canTranslate = true;
            SetBBoxes(scene);                       //tworzy strukture bboxów nie zaznaczonych obiektów na dole hierarchii, oraz bboxx sceny

            if (otherSceneObjBB.Count > 0)
            {
                if (direction.Equals("xyl")) //rzutowanie na płaszczyznę x y przesunięcie do lewej
                {
                    foreach (HierarchyMesh mesh in scene.hierarchy.GetAllMeshes())
                    {
                        if (scene.IsTriangleSelected(mesh.triangles[0]))
                        {
                            tmpbb = new BoundingBox(scene, mesh);
                            Vector3D tmp = new Vector3D(sceneBB.minBB.x, tmpbb.minBB.y, tmpbb.minBB.z);
                            Vector3D tmp2 = new Vector3D(tmpbb.minBB.x, tmpbb.maxBB.y, tmpbb.maxBB.z);
                            requiredSpaceBB = new BoundingBox(tmp, tmp2);

                            foreach (BoundingBox otherbb in otherSceneObjBB)
                            {
                                if (BBoxCollisionDetection(otherbb, requiredSpaceBB))//, slideDir))
                                {
                                    colisionFound = true;
                                    tmpDistance = CalculateDistanceEZ(otherbb, tmpbb, direction);
                                    if (tmpDistance < minDistance)
                                        minDistance = tmpDistance;
                                    if (minDistance < minDistanceGlobal)
                                        minDistanceGlobal = minDistance;
                                    if (BBoxCollisionDetection(otherbb, tmpbb)) //jeśli jakiś obiekt przecina się z dosuwanym na początku operacji- nie można dosuwać
                                        canTranslate = false;
                                    Console.WriteLine("Kolizja wykryta z obiektem " + otherbb.minBB.x + " " + otherbb.minBB.y + " " + otherbb.minBB.z + " : "
                                        + otherbb.maxBB.x + " " + otherbb.maxBB.y + " " + otherbb.maxBB.z);
                                }
                            }
                        }
                    }
                    if (colisionFound && canTranslate)
                        Transformations.Translate(scene, -minDistanceGlobal, 0, 0);
                    else if(colisionFound && !canTranslate)
                        Console.WriteLine("Płaszczyzna BBoxa którą chcemy dosuwać jest wewnątrz BBoxa innego obiektu. Nie można wykonać operacji.");
                    else
                        Console.WriteLine("Brak kolizji");
                
                }
                else if (direction.Equals("xyr")) //rzutowanie na płaszczyznę x y przesunięcie do prawej
                {
                    foreach (HierarchyMesh mesh in scene.hierarchy.GetAllMeshes())
                    {
                        if (scene.IsTriangleSelected(mesh.triangles[0]))
                        {
                            tmpbb = new BoundingBox(scene, mesh);
                            Vector3D tmp = new Vector3D(sceneBB.maxBB.x, tmpbb.maxBB.y, tmpbb.maxBB.z);
                            Vector3D tmp2 = new Vector3D(tmpbb.minBB.x, tmpbb.minBB.y, tmpbb.minBB.z);
                            requiredSpaceBB = new BoundingBox(tmp2, tmp);
                            foreach (BoundingBox otherbb in otherSceneObjBB)
                            {
                                if (BBoxCollisionDetection(otherbb, requiredSpaceBB))//, slideDir))
                                {
                                    colisionFound = true;
                                    Console.WriteLine("Zaznaczony obiekt " + tmpbb.minBB.x + " " + tmpbb.minBB.y + " " + tmpbb.minBB.z + " : "
                                        + tmpbb.maxBB.x + " " + tmpbb.maxBB.y + " " + tmpbb.maxBB.z);
                                    tmpDistance = CalculateDistanceEZ(otherbb, tmpbb, direction);
                                    if (tmpDistance < minDistance)
                                        minDistance = tmpDistance;
                                    if (minDistance < minDistanceGlobal)
                                        minDistanceGlobal = minDistance;
                                    if (BBoxCollisionDetection(otherbb, tmpbb)) //jeśli jakiś obiekt przecina się z dosuwanym na początku operacji- nie można dosuwać
                                        canTranslate = false;
                                    Console.WriteLine("Kolizja wykryta z obiektem " + otherbb.minBB.x + " " + otherbb.minBB.y + " " + otherbb.minBB.z + " : "
                                        + otherbb.maxBB.x + " " + otherbb.maxBB.y + " " + otherbb.maxBB.z);
                                }
                            }
                        } 
                    }
                    if (colisionFound)
                        Transformations.Translate(scene, minDistanceGlobal, 0, 0);
                    else if (colisionFound && !canTranslate)
                        Console.WriteLine("Płaszczyzna BBoxa którą chcemy dosuwać jest wewnątrz BBoxa innego obiektu. Nie można wykonać operacji.");
                    else
                        Console.WriteLine("Brak kolizji");
                }
                else if (direction.Equals("yzu")) //rzutowanie na płaszczyznę y z przesunięcie do góry
                {
                    foreach (HierarchyMesh mesh in scene.hierarchy.GetAllMeshes())
                    {
                        if (scene.IsTriangleSelected(mesh.triangles[0]))
                        {
                            tmpbb = new BoundingBox(scene, mesh);
                            Vector3D tmp = new Vector3D(tmpbb.maxBB.x, sceneBB.maxBB.y, tmpbb.maxBB.z);
                            Vector3D tmp2 = new Vector3D(tmpbb.minBB.x, tmpbb.maxBB.y, tmpbb.minBB.z);
                            requiredSpaceBB = new BoundingBox(tmp2, tmp);

                            foreach (BoundingBox otherbb in otherSceneObjBB)
                            {
                                if (BBoxCollisionDetection(otherbb, requiredSpaceBB))//, slideDir))
                                {
                                    colisionFound = true;
                                    tmpDistance = CalculateDistanceEZ(otherbb, tmpbb, direction);
                                    if (tmpDistance < minDistance)
                                        minDistance = tmpDistance;
                                    if (minDistance < minDistanceGlobal)
                                        minDistanceGlobal = minDistance;
                                    if (BBoxCollisionDetection(otherbb, tmpbb)) //jeśli jakiś obiekt przecina się z dosuwanym na początku operacji- nie można dosuwać
                                        canTranslate = false;
                                    Console.WriteLine("Kolizja wykryta z obiektem " + otherbb.minBB.x + " " + otherbb.minBB.y + " " + otherbb.minBB.z + " : "
                                        + otherbb.maxBB.x + " " + otherbb.maxBB.y + " " + otherbb.maxBB.z);
                                }
                            }
                        }
                    }
                    if (colisionFound)
                        Transformations.Translate(scene, 0, minDistanceGlobal, 0);
                    else if (colisionFound && !canTranslate)
                        Console.WriteLine("Płaszczyzna BBoxa którą chcemy dosuwać jest wewnątrz BBoxa innego obiektu. Nie można wykonać operacji.");
                    else
                        Console.WriteLine("Brak kolizji");
                }
                else if (direction.Equals("yzd")) //rzutowanie na płaszczyznę y z przesunięcie do dołu
                {
                    foreach (HierarchyMesh mesh in scene.hierarchy.GetAllMeshes())
                    {
                        if (scene.IsTriangleSelected(mesh.triangles[0]))
                        {
                            tmpbb = new BoundingBox(scene, mesh);
                            Vector3D tmp = new Vector3D(tmpbb.minBB.x, sceneBB.minBB.y, tmpbb.minBB.z);
                            Vector3D tmp2 = new Vector3D(tmpbb.maxBB.x, tmpbb.minBB.y, tmpbb.maxBB.z);
                            requiredSpaceBB = new BoundingBox(tmp, tmp2);

                            foreach (BoundingBox otherbb in otherSceneObjBB)
                            {
                                if (BBoxCollisionDetection(otherbb, requiredSpaceBB))//, slideDir))
                                {
                                    colisionFound = true;
                                    tmpDistance = CalculateDistanceEZ(otherbb, tmpbb, direction);
                                    if (tmpDistance < minDistance)
                                        minDistance = tmpDistance;
                                    if (minDistance < minDistanceGlobal)
                                        minDistanceGlobal = minDistance;
                                    if (BBoxCollisionDetection(otherbb, tmpbb)) //jeśli jakiś obiekt przecina się z dosuwanym na początku operacji- nie można dosuwać
                                        canTranslate = false;
                                    Console.WriteLine("Kolizja wykryta z obiektem " + otherbb.minBB.x + " " + otherbb.minBB.y + " " + otherbb.minBB.z + " : "
                                        + otherbb.maxBB.x + " " + otherbb.maxBB.y + " " + otherbb.maxBB.z);
                                }
                            }
                        } 
                    }
                    if (colisionFound)
                        Transformations.Translate(scene, 0, -minDistanceGlobal, 0);
                    else if (colisionFound && !canTranslate)
                        Console.WriteLine("Płaszczyzna BBoxa którą chcemy dosuwać jest wewnątrz BBoxa innego obiektu. Nie można wykonać operacji.");
                    else
                        Console.WriteLine("Brak kolizji");
                }
                else if (direction.Equals("zxb")) //rzutowanie na płaszczyznę z x przesunięcie do tyłu 
                {
                    foreach (HierarchyMesh mesh in scene.hierarchy.GetAllMeshes())
                    {
                        if (scene.IsTriangleSelected(mesh.triangles[0]))
                        {
                            tmpbb = new BoundingBox(scene, mesh);
                            Vector3D tmp = new Vector3D(tmpbb.minBB.x, tmpbb.minBB.y, sceneBB.minBB.z);
                            Vector3D tmp2 = new Vector3D(tmpbb.maxBB.x, tmpbb.maxBB.y, tmpbb.minBB.z);
                            requiredSpaceBB = new BoundingBox(tmp, tmp2);

                            foreach (BoundingBox otherbb in otherSceneObjBB)
                            {
                                if (BBoxCollisionDetection(otherbb, requiredSpaceBB))//, slideDir))
                                {
                                    colisionFound = true;
                                    tmpDistance = CalculateDistanceEZ(otherbb, tmpbb, direction);
                                    if (tmpDistance < minDistance)
                                        minDistance = tmpDistance;
                                    if (minDistance < minDistanceGlobal)
                                        minDistanceGlobal = minDistance;
                                    if (BBoxCollisionDetection(otherbb, tmpbb)) //jeśli jakiś obiekt przecina się z dosuwanym na początku operacji- nie można dosuwać
                                        canTranslate = false;
                                    Console.WriteLine("Kolizja wykryta z obiektem " + otherbb.minBB.x + " " + otherbb.minBB.y + " " + otherbb.minBB.z + " : "
                                        + otherbb.maxBB.x + " " + otherbb.maxBB.y + " " + otherbb.maxBB.z);
                                }
                            }
                        }
                    }
                    if (colisionFound)
                        Transformations.Translate(scene, 0, 0, -minDistanceGlobal);
                    else if (colisionFound && !canTranslate)
                        Console.WriteLine("Płaszczyzna BBoxa którą chcemy dosuwać jest wewnątrz BBoxa innego obiektu. Nie można wykonać operacji.");
                    else
                        Console.WriteLine("Brak kolizji");
                }
                else //if(direction.Equals("zxf")) //rzutowanie na płaszczyznę z x przesunięcie do przodu
                {
                    foreach (HierarchyMesh mesh in scene.hierarchy.GetAllMeshes())
                    {
                        if (scene.IsTriangleSelected(mesh.triangles[0]))
                        {
                            tmpbb = new BoundingBox(scene, mesh);
                            Vector3D tmp = new Vector3D(tmpbb.maxBB.x, tmpbb.maxBB.y, sceneBB.maxBB.z);
                            Vector3D tmp2 = new Vector3D(tmpbb.minBB.x, tmpbb.minBB.y, tmpbb.maxBB.z);
                            requiredSpaceBB = new BoundingBox(tmp2, tmp);

                            foreach (BoundingBox otherbb in otherSceneObjBB)
                            {
                                if (BBoxCollisionDetection(otherbb, requiredSpaceBB))//, slideDir))
                                {
                                    colisionFound = true;
                                    tmpDistance = CalculateDistanceEZ(otherbb, tmpbb, direction);
                                    if (tmpDistance < minDistance)
                                        minDistance = tmpDistance;
                                    if (minDistance < minDistanceGlobal)
                                        minDistanceGlobal = minDistance;
                                    if (BBoxCollisionDetection(otherbb, tmpbb)) //jeśli jakiś obiekt przecina się z dosuwanym na początku operacji- nie można dosuwać
                                        canTranslate = false;
                                    Console.WriteLine("Kolizja wykryta z obiektem " + otherbb.minBB.x + " " + otherbb.minBB.y + " " + otherbb.minBB.z + " : "
                                        + otherbb.maxBB.x + " " + otherbb.maxBB.y + " " + otherbb.maxBB.z);
                                }
                            }
                        }
                    }
                    if (colisionFound)
                        Transformations.Translate(scene, 0, 0, minDistanceGlobal);
                    else if (colisionFound && !canTranslate)
                        Console.WriteLine("Płaszczyzna BBoxa którą chcemy dosuwać jest wewnątrz BBoxa innego obiektu. Nie można wykonać operacji.");
                    else
                        Console.WriteLine("Brak kolizji");
                }
            }
            else
                Console.WriteLine("Brak innych objektów sceny");
            Clear();
        }

        private static void Clear()
        {
            selectedObjectBB=null;
            otherSceneObjBB.Clear();
            sceneBB=null;
            requiredSpaceBB=null;
        }

    }

    class BoundingBox
    {
        public Vector3D minBB, maxBB;

        public BoundingBox(Scene sc, HierarchyMesh obj)
        {
            float minx = float.MaxValue;
            float miny = float.MaxValue;
            float minz = float.MaxValue;
            float maxx = float.MinValue;
            float maxy = float.MinValue;
            float maxz = float.MinValue;
            float tmpx, tmpy, tmpz;

            foreach (uint triangleIdx in obj.triangles)
            {
                tmpx = sc.points[(int)(sc.triangles[(int)triangleIdx].p1)].x;
                if (minx > tmpx)
                    minx = tmpx;
                if (maxx < tmpx)
                    maxx = tmpx;

                tmpy = sc.points[(int)(sc.triangles[(int)triangleIdx].p1)].y;
                if (miny > tmpy)
                    miny = tmpy;
                if (maxy < tmpy)
                    maxy = tmpy;

                tmpz = sc.points[(int)(sc.triangles[(int)triangleIdx].p1)].z;
                if (minz > tmpz)
                    minz = tmpz;
                if (maxz < tmpz)
                    maxz = tmpz;

                tmpx = sc.points[(int)(sc.triangles[(int)triangleIdx].p2)].x;
                if (minx > tmpx)
                    minx = tmpx;
                if (maxx < tmpx)
                    maxx = tmpx;

                tmpy = sc.points[(int)(sc.triangles[(int)triangleIdx].p2)].y;
                if (miny > tmpy)
                    miny = tmpy;
                if (maxy < tmpy)
                    maxy = tmpy;

                tmpz = sc.points[(int)(sc.triangles[(int)triangleIdx].p2)].z;
                if (minz > tmpz)
                    minz = tmpz;
                if (maxz < tmpz)
                    maxz = tmpz;

                tmpx = sc.points[(int)(sc.triangles[(int)triangleIdx].p3)].x;
                if (minx > tmpx)
                    minx = tmpx;
                if (maxx < tmpx)
                    maxx = tmpx;

                tmpy = sc.points[(int)(sc.triangles[(int)triangleIdx].p3)].y;
                if (miny > tmpy)
                    miny = tmpy;
                if (maxy < tmpy)
                    maxy = tmpy;

                tmpz = sc.points[(int)(sc.triangles[(int)triangleIdx].p3)].z;
                if (minz > tmpz)
                    minz = tmpz;
                if (maxz < tmpz)
                    maxz = tmpz;
            }

            minBB = new Vector3D(minx, miny, minz);
            maxBB = new Vector3D(maxx, maxy, maxz);
        }

        public BoundingBox(Scene sc, List<Triangle> obj)
        {
            float minx = float.MaxValue;
            float miny = float.MaxValue;
            float minz = float.MaxValue;
            float maxx = float.MinValue;
            float maxy = float.MinValue;
            float maxz = float.MinValue;
            float tmpx, tmpy, tmpz;

            foreach (Triangle triangleIdx in obj)
            {
                tmpx = sc.points[(int)triangleIdx.p1].x;
                if (minx > tmpx)
                    minx = tmpx;
                if (maxx < tmpx)
                    maxx = tmpx;

                tmpy = sc.points[(int)triangleIdx.p1].y;
                if (miny > tmpy)
                    miny = tmpy;
                if (maxy < tmpy)
                    maxy = tmpy;

                tmpz = sc.points[(int)triangleIdx.p1].z;
                if (minz > tmpz)
                    minz = tmpz;
                if (maxz < tmpz)
                    maxz = tmpz;

                tmpx = sc.points[(int)triangleIdx.p2].x;
                if (minx > tmpx)
                    minx = tmpx;
                if (maxx < tmpx)
                    maxx = tmpx;

                tmpy = sc.points[(int)triangleIdx.p2].y;
                if (miny > tmpy)
                    miny = tmpy;
                if (maxy < tmpy)
                    maxy = tmpy;

                tmpz = sc.points[(int)triangleIdx.p2].z;
                if (minz > tmpz)
                    minz = tmpz;
                if (maxz < tmpz)
                    maxz = tmpz;

                tmpx = sc.points[(int)triangleIdx.p3].x;
                if (minx > tmpx)
                    minx = tmpx;
                if (maxx < tmpx)
                    maxx = tmpx;

                tmpy = sc.points[(int)triangleIdx.p3].y;
                if (miny > tmpy)
                    miny = tmpy;
                if (maxy < tmpy)
                    maxy = tmpy;

                tmpz = sc.points[(int)triangleIdx.p3].z;
                if (minz > tmpz)
                    minz = tmpz;
                if (maxz < tmpz)
                    maxz = tmpz;
            }

            minBB = new Vector3D(minx, miny, minz);
            maxBB = new Vector3D(maxx, maxy, maxz);
        }

        public BoundingBox(Vector3D bb1v1, Vector3D bb1v2, Vector3D bb2v1, Vector3D bb2v2)
        {
            float tmpx, tmpy, tmpz;
            
            if (bb1v1.x < bb2v1.x)
                tmpx = bb1v1.x;
            else
                tmpx = bb2v1.x;

            if (bb1v1.y < bb2v1.y)
                tmpy = bb1v1.y;
            else
                tmpy = bb2v1.y;

            if (bb1v1.z < bb2v1.z)
                tmpz = bb1v1.z;
            else
                tmpz = bb2v1.z;

            minBB = new Vector3D(tmpx, tmpy, tmpz);

            if (bb1v1.x > bb2v1.x)
                tmpx = bb1v1.x;
            else
                tmpx = bb2v1.x;

            if (bb1v1.y > bb2v1.y)
                tmpy = bb1v1.y;
            else
                tmpy = bb2v1.y;

            if (bb1v1.z > bb2v1.z)
                tmpz = bb1v1.z;
            else
                tmpz = bb2v1.z;

            maxBB = new Vector3D(tmpx, tmpy, tmpz);
        }

        public BoundingBox(Vector3D min, Vector3D max)
        {
            minBB = min;
            maxBB = max;
        }

        public BoundingBox(Scene scene)
        {
            float minx = float.MaxValue;
            float miny = float.MaxValue;
            float minz = float.MaxValue;
            float maxx = float.MinValue;
            float maxy = float.MinValue;
            float maxz = float.MinValue;
            float tmpx, tmpy, tmpz;

            foreach(Triangle triangleIdx in scene.triangles)
            {
                tmpx = scene.points[(int)triangleIdx.p1].x;
                if(minx > tmpx)
                    minx = tmpx;
                if(maxx < tmpx)
                    maxx = tmpx;

                tmpy = scene.points[(int)triangleIdx.p1].y;
                if(miny > tmpy)
                    miny = tmpy;
                if(maxy < tmpy)
                    maxy = tmpy;

                tmpz = scene.points[(int)triangleIdx.p1].z;
                if(minz > tmpz)
                    minz = tmpz;
                if(maxz < tmpz)
                    maxz = tmpz;

                tmpx = scene.points[(int)triangleIdx.p2].x;
                if(minx > tmpx)
                    minx = tmpx;
                if(maxx < tmpx)
                    maxx = tmpx;

                tmpy = scene.points[(int)triangleIdx.p2].y;
                if(miny > tmpy)
                    miny = tmpy;
                if(maxy < tmpy)
                    maxy = tmpy;

                tmpz = scene.points[(int)triangleIdx.p2].z;
                if(minz > tmpz)
                    minz = tmpz;
                if(maxz < tmpz)
                    maxz = tmpz;

                tmpx = scene.points[(int)triangleIdx.p3].x;
                if(minx > tmpx)
                    minx = tmpx;
                if(maxx < tmpx)
                    maxx = tmpx;

                tmpy = scene.points[(int)triangleIdx.p3].y;
                if(miny > tmpy)
                    miny = tmpy;
                if(maxy < tmpy)
                    maxy = tmpy;

                tmpz = scene.points[(int)triangleIdx.p3].z;
                if(minz > tmpz)
                    minz = tmpz;
                if(maxz < tmpz)
                    maxz = tmpz;
            }

            minBB = new Vector3D(minx, miny, minz);
            maxBB = new Vector3D(maxx, maxy, maxz);
        }
    }
}
