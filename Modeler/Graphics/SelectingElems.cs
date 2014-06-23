using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimDX;
using Modeler.Data.Scene;
using Modeler.Data.Shapes;

namespace Modeler.Graphics
{
    enum ViewportType { Perspective, Orto }

    class Triang
    {
        public Vector3 p1, p2, p3;

        public Triang(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
    }

    static class SelectingElems
    {
        public static int pointFound = -1;

        private static void CalcCoordsImpl(Vector2 orthoSize, Vector3 orthoPos, Vector3 orthoLookAt, out Vector3 upLeft, out Vector3 upRight, out Vector3 loLeft, out Vector3 loRight)
        {
            float dx = orthoLookAt.X - orthoPos.X;
            float dy = orthoLookAt.Y - orthoPos.Y;
            float dz = orthoLookAt.Z - orthoPos.Z;
            float d = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
            float D = (float)Math.Sqrt(dx * dx + dz * dz);

            float sX = orthoSize.X;
            float sY = orthoSize.Y;

            float dy1 = sY * D / (2.0f * d);
            float dx1 = (dx * dy * sY) / (2.0f * d * D);
            float dz1 = (dz * dy * sY) / (2.0f * d * D);
            float dx2 = (dz * sX) / (2.0f * D);
            float dz2 = (dx * sX) / (2.0f * D);

            upLeft = new Vector3(orthoLookAt.X - dx1 + dx2, orthoLookAt.Y + dy1, orthoLookAt.Z - dz1 - dz2);
            upRight = new Vector3(orthoLookAt.X - dx1 - dx2, orthoLookAt.Y + dy1, orthoLookAt.Z - dz1 + dz2);
            loLeft = new Vector3(orthoLookAt.X + dx1 + dx2, orthoLookAt.Y - dy1, orthoLookAt.Z + dz1 - dz2);
            loRight = new Vector3(orthoLookAt.X + dx1 - dx2, orthoLookAt.Y - dy1, orthoLookAt.Z + dz1 + dz2);
        }

        private static void RotatePoint(Point pos, Point size, Vector2 rectSize, float angle, Vector3 upLeft, Vector3 upRight, Vector3 loLeft, Vector3 loRight, out Vector3 outPoint)
        {
            float scaleX = (float)pos.X / size.X;
            float scaleY = (float)pos.Y / size.Y;
            float rectDX = (scaleX - 0.5f) * rectSize.X;
            float rectDY = (scaleY - 0.5f) * rectSize.Y;

            float alpha = Utilities.DegToRad(angle);
            float rotatedDX = rectDX * (float)Math.Cos(alpha) - rectDY * (float)Math.Sin(alpha);
            float rotatedDY = rectDX * (float)Math.Sin(alpha) + rectDY * (float)Math.Cos(alpha);

            scaleX = rotatedDX / rectSize.X + 0.5f;
            scaleY = rotatedDY / rectSize.Y + 0.5f;

            outPoint.X = upLeft.X + (upRight.X - upLeft.X) * scaleX + (loLeft.X - upLeft.X) * scaleY;
            outPoint.Y = upLeft.Y - (upLeft.Y - loLeft.Y) * scaleY;
            outPoint.Z = upLeft.Z + (upRight.Z - upLeft.Z) * scaleX + (loLeft.Z - upLeft.Z) * scaleY;
        }

        public static void CalcPerspCoords(Point pos, Point size, float fovAngle, float rotateAngle, Vector3 perspPos, Vector3 perspLookAt, out Vector3 outCamPos, out Vector3 outSurfPos)
        {
            Vector3 oldLookAt = new Vector3(perspLookAt.X, perspLookAt.Y, perspLookAt.Z);
            perspPos -= perspLookAt;
            perspLookAt -= perspLookAt;

            float dx = perspLookAt.X - perspPos.X;
            float dy = perspLookAt.Y - perspPos.Y;
            float dz = perspLookAt.Z - perspPos.Z;
            float d = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
            float rectSizeX = 2.0f * d * (float)Math.Tan(Utilities.DegToRad(fovAngle) / 2.0f);
            float rectSizeY = (float)(rectSizeX * size.Y) / size.X;

            Vector3 upLeft, upRight, loLeft, loRight;
            CalcCoordsImpl(new Vector2(rectSizeX, rectSizeY), perspPos, perspLookAt, out upLeft, out upRight, out loLeft, out loRight);

            float scaleX = (float)pos.X / size.X;
            float scaleY = (float)pos.Y / size.Y;
            float rectDX = (scaleX - 0.5f) * rectSizeX;
            float rectDY = (scaleY - 0.5f) * rectSizeY;

            float alpha = Utilities.DegToRad(rotateAngle);
            float rotatedDX = rectDX * (float)Math.Cos(alpha) - rectDY * (float)Math.Sin(alpha);
            float rotatedDY = rectDX * (float)Math.Sin(alpha) + rectDY * (float)Math.Cos(alpha);

            scaleX = rotatedDX / rectSizeX + 0.5f;
            scaleY = rotatedDY / rectSizeY + 0.5f;

            float resX = upLeft.X + (upRight.X - upLeft.X) * scaleX + (loLeft.X - upLeft.X) * scaleY;
            float resY = upLeft.Y - (upLeft.Y - loLeft.Y) * scaleY;
            float resZ = upLeft.Z + (upRight.Z - upLeft.Z) * scaleX + (loLeft.Z - upLeft.Z) * scaleY;

            perspLookAt += oldLookAt;
            perspPos += oldLookAt;

            outCamPos = new Vector3(perspPos.X, perspPos.Y, perspPos.Z);
            outSurfPos = new Vector3(perspLookAt.X + resX, perspLookAt.Y + resY, perspLookAt.Z + resZ);
        }

        public static void CalcOrthoCoords(Point pos, Point size, Vector2 orthoSize, Vector3 orthoPos, Vector3 orthoLookAt, out Vector3 outCamPos, out Vector3 outSurfPos)
        {
            Vector3 oldLookAt = new Vector3(orthoLookAt.X,orthoLookAt.Y,orthoLookAt.Z);
            orthoPos -= orthoLookAt;
            orthoLookAt -= orthoLookAt;

            Vector3 upLeft, upRight, loLeft, loRight;
            CalcCoordsImpl(orthoSize, orthoPos, orthoLookAt, out upLeft, out upRight, out loLeft, out loRight);

            float scaleX = (float)pos.X / size.X;
            float scaleY = (float)pos.Y / size.Y;

            float resX = upLeft.X + (upRight.X - upLeft.X) * scaleX + (loLeft.X - upLeft.X) * scaleY;
            float resY = upLeft.Y - (upLeft.Y - loLeft.Y) * scaleY;
            float resZ = upLeft.Z + (upRight.Z - upLeft.Z) * scaleX + (loLeft.Z - upLeft.Z) * scaleY;

            orthoLookAt += oldLookAt;
            orthoPos += oldLookAt;

            outCamPos = new Vector3(orthoPos.X + resX, orthoPos.Y + resY, orthoPos.Z + resZ);
            outSurfPos = new Vector3(orthoLookAt.X + resX, orthoLookAt.Y + resY, orthoLookAt.Z + resZ);
        }

        public static void GetViewCorners(Vector3 perspPos, Vector3 perspLookAt, float fovAngle, float rotateAngle, Point size,
            out Vector3 upLeft, out Vector3 upRight, out Vector3 loLeft, out Vector3 loRight)
        {
            //upLeft = upRight = loLeft = loRight = new Vector3(0, 0, 0);
            Vector3 oldLookAt = new Vector3(perspLookAt.X, perspLookAt.Y, perspLookAt.Z);
            perspPos -= perspLookAt;
            perspLookAt -= perspLookAt;

            float dx = perspLookAt.X - perspPos.X;
            float dy = perspLookAt.Y - perspPos.Y;
            float dz = perspLookAt.Z - perspPos.Z;
            float d = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
            float rectSizeX = 2.0f * d * (float)Math.Tan(Utilities.DegToRad(fovAngle) / 2.0f);
            float rectSizeY = (float)(rectSizeX * size.Y) / size.X;

            Vector3 upLeftFst, upRightFst, loLeftFst, loRightFst;
            CalcCoordsImpl(new Vector2(rectSizeX, rectSizeY), perspPos, perspLookAt, out upLeftFst, out upRightFst, out loLeftFst, out loRightFst);

            RotatePoint(new Point(0, 0), size, new Vector2(rectSizeX, rectSizeY), rotateAngle, upLeftFst, upRightFst, loLeftFst, loRightFst, out upLeft);
            RotatePoint(new Point(size.X, 0), size, new Vector2(rectSizeX, rectSizeY), rotateAngle, upLeftFst, upRightFst, loLeftFst, loRightFst, out upRight);
            RotatePoint(new Point(0, size.Y), size, new Vector2(rectSizeX, rectSizeY), rotateAngle, upLeftFst, upRightFst, loLeftFst, loRightFst, out loLeft);
            RotatePoint(new Point(size.X, size.Y), size, new Vector2(rectSizeX, rectSizeY), rotateAngle, upLeftFst, upRightFst, loLeftFst, loRightFst, out loRight);

            perspLookAt += oldLookAt;
            perspPos += oldLookAt;

            upLeft += perspLookAt;
            upRight += perspLookAt;
            loLeft += perspLookAt;
            loRight += perspLookAt;
        }

        public static void SelectElems(Scene scene, List<Vector3> camsPoints, Pair<List<Vector3>, List<int> > lightsPoints,
            ViewportType viewportType, Point pos, Point size, Vector2 orthoSize, Vector3 orthoPos, Vector3 orthoLookAt, bool ctrl)
        {
            Vector3 outCamPos = new Vector3(), outSurfPos = new Vector3();
            
            switch(viewportType)
            {
                case ViewportType.Perspective:
                    CalcPerspCoords(pos, size, scene.cams[scene.activeCamera].fovAngle, scene.cams[scene.activeCamera].rotateAngle,
                scene.cams[scene.activeCamera].position, scene.cams[scene.activeCamera].lookAt, out outCamPos, out outSurfPos);
                    break;

                case ViewportType.Orto:
                    CalcOrthoCoords(pos, size, orthoSize, orthoPos, orthoLookAt, out outCamPos, out outSurfPos);
                    break;
            }

            List<Triang> triangsCam = new List<Triang>();
            for(int i = 0; i < camsPoints.Count / 8; ++i)
            {
                for(int j = RenderCamera.triangles.Length - 1; j < RenderCamera.triangles.Length; ++j)
                {
                    triangsCam.Add(new Triang(camsPoints[3 * i + (int)RenderCamera.triangles[j].p1],
                                              camsPoints[3 * i + (int)RenderCamera.triangles[j].p2],
                                              camsPoints[3 * i + (int)RenderCamera.triangles[j].p3]));
                }
            }

            List<Triang> triangsLight = new List<Triang>();
            int sumPoints = 0;
            for(int i = 0; i < lightsPoints.Second.Count; ++i)
            {
                for(int j = 0; j < lightsPoints.Second[i]; ++j)
                {
                    triangsLight.Add(new Triang(lightsPoints.First[sumPoints + (int)RenderLight.triangles[j].p1],
                                                lightsPoints.First[sumPoints + (int)RenderLight.triangles[j].p2],
                                                lightsPoints.First[sumPoints + (int)RenderLight.triangles[j].p3]));
                }

                sumPoints += lightsPoints.Second[i] == RenderLight.trianglesSpotNum ? RenderLight.pointsSpotNum : RenderLight.pointsPointNum;
            }

            bool clipped = false;
            float clipMin = 0, clipMax = 0;
            Vector3 rayShift = new Vector3(0);

            if(viewportType == ViewportType.Orto && Renderer.Clipping == true)
            {
                float xmin, xplus, ymin, yplus, zmin, zplus;

                if(outCamPos.Z > 40000)
                {
                    xmin = Renderer.GetClipPlanePosition(ClipPlaneType.XMIN);
                    xplus = Renderer.GetClipPlanePosition(ClipPlaneType.XPLUS);
                    ymin = Renderer.GetClipPlanePosition(ClipPlaneType.YMIN);
                    yplus = Renderer.GetClipPlanePosition(ClipPlaneType.YPLUS);
                    zmin = Renderer.GetClipPlanePosition(ClipPlaneType.ZMIN);
                    zplus = Renderer.GetClipPlanePosition(ClipPlaneType.ZPLUS);

                    if(outCamPos.X < xmin || outCamPos.X > xplus || outCamPos.Y < ymin || outCamPos.Y > yplus)
                    {
                        clipped = true;
                    }
                    else
                    {
                        clipMin = 50001 - zplus;
                        clipMax = zplus - zmin;

                        rayShift = new Vector3(0, 0, clipMin);
                    }
                }
                else if(outCamPos.X > 40000)
                {
                    xmin = Renderer.GetClipPlanePosition(ClipPlaneType.ZPLUS);
                    xplus = Renderer.GetClipPlanePosition(ClipPlaneType.ZMIN);
                    ymin = Renderer.GetClipPlanePosition(ClipPlaneType.YMIN);
                    yplus = Renderer.GetClipPlanePosition(ClipPlaneType.YPLUS);
                    zmin = Renderer.GetClipPlanePosition(ClipPlaneType.XMIN);
                    zplus = Renderer.GetClipPlanePosition(ClipPlaneType.XPLUS);

                    if(outCamPos.Z > xmin || outCamPos.Z < xplus || outCamPos.Y < ymin || outCamPos.Y > yplus)
                    {
                        clipped = true;
                    }
                    else
                    {
                        clipMin = 50001 - zplus;
                        clipMax = zplus - zmin;

                        rayShift = new Vector3(clipMin, 0, 0);
                    }
                }
                else if(outCamPos.Y > 40000)
                {
                    xmin = Renderer.GetClipPlanePosition(ClipPlaneType.XMIN);
                    xplus = Renderer.GetClipPlanePosition(ClipPlaneType.XPLUS);
                    ymin = Renderer.GetClipPlanePosition(ClipPlaneType.ZPLUS);
                    yplus = Renderer.GetClipPlanePosition(ClipPlaneType.ZMIN);
                    zmin = Renderer.GetClipPlanePosition(ClipPlaneType.YMIN);
                    zplus = Renderer.GetClipPlanePosition(ClipPlaneType.YPLUS);

                    if(outCamPos.X < xmin || outCamPos.X > xplus || outCamPos.Z > ymin || outCamPos.Z < yplus)
                    {
                        clipped = true;
                    }
                    else
                    {
                        clipMin = 50001 - zplus;
                        clipMax = zplus - zmin;

                        rayShift = new Vector3(0, clipMin, 0);
                    }
                }
            }

            Vector3 rayDir = Vector3.Normalize(outSurfPos - outCamPos);

            SlimDX.Ray ray = new SlimDX.Ray(outCamPos + 0.01f * rayDir - rayShift, rayDir);

            float[] triangleDist = new float[scene.triangles.Count];
            float minDist = float.PositiveInfinity;
            int minIndex = -1;

            if(clipped == false)
            {
                if(viewportType == ViewportType.Orto && Renderer.Clipping == true)
                {
                    for(int i = 0; i < scene.triangles.Count; ++i)
                    {
                        float dist;

                        if(SlimDX.Ray.Intersects(ray, scene.points[(int)scene.triangles[i].p1], scene.points[(int)scene.triangles[i].p2],
                            scene.points[(int)scene.triangles[i].p3], out dist))
                        {
                            if(dist >= 0 && dist < minDist && dist < clipMax)
                            {
                                minIndex = i;
                                minDist = dist;
                            }
                        }
                    }
                }
                else
                {
                    for(int i = 0; i < scene.triangles.Count; ++i)
                    {
                        float dist;

                        if(SlimDX.Ray.Intersects(ray, scene.points[(int)scene.triangles[i].p1], scene.points[(int)scene.triangles[i].p2],
                            scene.points[(int)scene.triangles[i].p3], out dist))
                        {
                            if(dist >= 0 && dist < minDist)
                            {
                                minIndex = i;
                                minDist = dist;
                            }
                        }
                    }
                }
            }

            minDist += clipMin;

            ray = new SlimDX.Ray(outCamPos + 0.01f * rayDir, rayDir);

            float[] camsTriangleDist = new float[triangsCam.Count];
            float camsMinDist = float.PositiveInfinity;
            int camsMinIndex = -1;

            if(viewportType != ViewportType.Perspective)
            {
                for(int i = 0; i < triangsCam.Count; ++i)
                {
                    float dist;

                    if(SlimDX.Ray.Intersects(ray, triangsCam[i].p1, triangsCam[i].p2, triangsCam[i].p3, out dist))
                    {
                        if(dist >= 0 && dist < camsMinDist)
                        {
                            camsMinIndex = i;
                            camsMinDist = dist;
                        }
                    }
                }
            }

            float[] lightsTriangleDist = new float[triangsLight.Count];
            float lightsMinDist = float.PositiveInfinity;
            int lightsMinIndex = -1;

            if(viewportType != ViewportType.Perspective)
            {
                for(int i = 0; i < triangsLight.Count; ++i)
                {
                    float dist;

                    if(SlimDX.Ray.Intersects(ray, triangsLight[i].p1, triangsLight[i].p2, triangsLight[i].p3, out dist))
                    {
                        if(dist >= 0 && dist < lightsMinDist)
                        {
                            lightsMinIndex = i;
                            lightsMinDist = lightsTriangleDist[i];
                        }
                    }
                }
            }

            List<float> pointsDist = new List<float>();
            float pointsMinDist = float.PositiveInfinity;
            int pointsMinIndex = -1;

            if(viewportType != ViewportType.Perspective)
            {
                for(int i = 0; i < scene.lights.Count; ++i)
                {
                    if(scene.lights[i].type == Light_Type.Spot || scene.lights[i].type == Light_Type.Goniometric)
                    {
                        float dist;

                        Vector3 lightPos = scene.lights[i].position + scene.lights[i].direction * Renderer.spotLightDist * orthoSize.X / 10;
                        if(SlimDX.Ray.Intersects(ray, new BoundingBox(new Vector3(lightPos.X - Renderer.pointSize * orthoSize.X / 20,
                                                                                  lightPos.Y - Renderer.pointSize * orthoSize.X / 20,
                                                                                  lightPos.Z - Renderer.pointSize * orthoSize.X / 20),
                                                                      new Vector3(lightPos.X + Renderer.pointSize * orthoSize.X / 20,
                                                                                  lightPos.Y + Renderer.pointSize * orthoSize.X / 20,
                                                                                  lightPos.Z + Renderer.pointSize * orthoSize.X / 20)), out dist))
                        {
                            pointsDist.Add(dist);
                        }
                        else
                        {
                            pointsDist.Add(-1);
                        }
                    }
                }

                for(int i = 0; i < scene.cams.Count; ++i)
                {
                    float dist;

                    Vector3 camPos = scene.cams[i].position;
                    if(SlimDX.Ray.Intersects(ray, new BoundingBox(new Vector3(camPos.X - Renderer.pointSize * orthoSize.X / 20,
                                                                                camPos.Y - Renderer.pointSize * orthoSize.X / 20,
                                                                                camPos.Z - Renderer.pointSize * orthoSize.X / 20),
                                                                    new Vector3(camPos.X + Renderer.pointSize * orthoSize.X / 20,
                                                                                camPos.Y + Renderer.pointSize * orthoSize.X / 20,
                                                                                camPos.Z + Renderer.pointSize * orthoSize.X / 20)), out dist))
                    {
                        pointsDist.Add(dist);
                    }
                    else
                    {
                        pointsDist.Add(-1);
                    }

                    Vector3 camLookAt = Renderer.camsLookAtPoints[i];
                    if(SlimDX.Ray.Intersects(ray, new BoundingBox(new Vector3(camLookAt.X - Renderer.pointSize * orthoSize.X / 20,
                                                                                camLookAt.Y - Renderer.pointSize * orthoSize.X / 20,
                                                                                camLookAt.Z - Renderer.pointSize * orthoSize.X / 20),
                                                                    new Vector3(camLookAt.X + Renderer.pointSize * orthoSize.X / 20,
                                                                                camLookAt.Y + Renderer.pointSize * orthoSize.X / 20,
                                                                                camLookAt.Z + Renderer.pointSize * orthoSize.X / 20)), out dist))
                    {
                        pointsDist.Add(dist);
                    }
                    else
                    {
                        pointsDist.Add(-1);
                    }
                }

                for(int i = 0; i < pointsDist.Count; ++i)
                {
                    if(pointsDist[i] >= 0 && pointsDist[i] < pointsMinDist)
                    {
                        pointsMinIndex = i;
                        pointsMinDist = pointsDist[i];
                    }
                }
            }

            int foundTriangle = minIndex;
            int foundLight = lightsMinIndex;
            int foundCamera = camsMinIndex;
            int foundPoint = pointsMinIndex;

            if(foundPoint >= 0 && pointsMinDist < minDist && pointsMinDist < lightsMinDist)
            {
                pointFound = pointsMinIndex;

                scene.selTriangles.Clear();
                scene.selLights.Clear();
                scene.selCams.Clear();
            }
            else
            {
                pointFound = -1;

                if(foundLight >= 0 && lightsMinDist < minDist && lightsMinDist < camsMinDist && lightsMinDist < pointsMinDist)
                {
                    foundTriangle = -1;
                    foundCamera = -1;
                }
                if(foundCamera >= 0 && camsMinDist < minDist && camsMinDist < lightsMinDist && camsMinDist < pointsMinDist)
                {
                    foundTriangle = -1;
                    foundLight = -1;
                }

                if(ctrl == false)
                {
                    scene.selTriangles.Clear();
                    scene.selLights.Clear();
                    scene.selCams.Clear();
                }

                if(foundTriangle >= 0)
                {
                    HierarchyMesh mesh = Hierarchy.GetSelectedMesh(scene.hierarchy.objects, foundTriangle);
                    if(scene.selTriangles.Contains(mesh) == false)
                    {
                        scene.selTriangles.Add(mesh);
                    }
                    else
                    {
                        if(ctrl == true)
                        {
                            scene.selTriangles.Remove(mesh);
                        }
                    }
                }
                else if(foundLight >= 0)
                {
                    int i = 0;
                    int index = lightsPoints.Second[i];
                    while(foundLight > index)
                    {
                        index += lightsPoints.Second[++i];
                    }
                    int lightIndex = i;

                    if(scene.selLights.Contains(lightIndex) == false)
                    {
                        scene.selLights.Add(lightIndex);
                    }
                    else
                    {
                        if(ctrl == true)
                        {
                            scene.selLights.Remove(lightIndex);
                        }
                    }
                }
                else if(foundCamera >= 0)
                {
                    int cameraIndex = foundCamera / RenderCamera.triangles.Length;
                    if(scene.selCams.Contains(cameraIndex) == false)
                    {
                        scene.selCams.Add(cameraIndex);
                    }
                    else
                    {
                        if(ctrl == true)
                        {
                            scene.selCams.Remove(cameraIndex);
                        }
                    }
                }
                else
                {
                    if(ctrl == false)
                    {
                        scene.selTriangles.Clear();
                        scene.selLights.Clear();
                        scene.selCams.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Działa tylko dla widoków ortogonalnych.
        /// </summary>
        /// <param name="bezierSurface"></param>
        /// <param name="bezierCam"></param>
        /// <param name="viewportType"></param>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="orthoSize"></param>
        /// <param name="orthoPos"></param>
        /// <param name="orthoLookAt"></param>
        public static void SelectBezierControlPoint(BezierSurface bezierSurface, Camera bezierCam, ViewportType viewportType, 
                                                    Point pos, Point size, Vector2 orthoSize, Vector3 orthoPos, Vector3 orthoLookAt)
        {
            Vector3 outCamPos = new Vector3(), outSurfPos = new Vector3();

            switch (viewportType)
            {
                case ViewportType.Perspective:
                    CalcPerspCoords(pos, size, bezierCam.fovAngle, bezierCam.rotateAngle,
                                    bezierCam.position, bezierCam.lookAt, out outCamPos, out outSurfPos);
                    break;

                case ViewportType.Orto:
                    CalcOrthoCoords(pos, size, orthoSize, orthoPos, orthoLookAt, out outCamPos, out outSurfPos);
                    break;
            }

            Vector3 rayDir = Vector3.Normalize(outSurfPos - outCamPos);

            SlimDX.Ray ray = new SlimDX.Ray(outCamPos + 0.01f * rayDir, rayDir);
            float dist = float.PositiveInfinity;
            float tmpDist = -1;
            BoundingBox bb;

            for (int i = 0; i < bezierSurface.ControlPoints.Length; i++)
            {
                bb = new BoundingBox(new Vector3(bezierSurface.ControlPoints[i].x - 0.05f,
                                                 bezierSurface.ControlPoints[i].y - 0.05f,
                                                 bezierSurface.ControlPoints[i].z - 0.05f),
                                     new Vector3(bezierSurface.ControlPoints[i].x + 0.05f,
                                                 bezierSurface.ControlPoints[i].y + 0.05f,
                                                 bezierSurface.ControlPoints[i].z + 0.05f));

                if (SlimDX.Ray.Intersects(ray, bb, out tmpDist))
                {
                    if (tmpDist < dist)
                    {
                        dist = tmpDist;
                        bezierSurface.selectedPointIdx = i;
                    }
                }
            }

            if (dist == float.PositiveInfinity)
            {
                bezierSurface.selectedPointIdx = -1;
            }
        }

        public static ClipPlaneType SelectClippingPlane(Vertex[] clipVertices, int[] clipIndices,
                                               ViewportType viewportType, Point pos, Point size, Vector2 orthoSize, Vector3 orthoPos, 
                                               Vector3 orthoLookAt, ViewportOrientation viewport)
        {
            if (viewportType == ViewportType.Perspective)
                return ClipPlaneType.NONE;

            Vector3 outCamPos = new Vector3(), outSurfPos = new Vector3();
            CalcOrthoCoords(pos, size, orthoSize, orthoPos, orthoLookAt, out outCamPos, out outSurfPos);

            // Indeksy trójkątów które trzeba odrzucić, aby można było wybrać
            // właściwą płaszczyznę obcinającą.
            int minRejectIdx, maxRejectIdx;

            switch (viewport)
            {
                case ViewportOrientation.Front:
                    minRejectIdx = 144/3;
                    maxRejectIdx = 216/3-1;
                    break;
                case ViewportOrientation.Side:
                    minRejectIdx = 72/3;
                    maxRejectIdx = 144/3-1;
                    break;
                case ViewportOrientation.Top:
                    minRejectIdx = 0/3;
                    maxRejectIdx = 72/3-1;
                    break;
                default:
                    minRejectIdx = 0;
                    maxRejectIdx = 0;
                    break;
            }

            Vector3[] shiftDist = new Vector3[6];
            for(int i = 0; i < 6; ++i)
            {
                shiftDist[i] = (clipVertices[8 * i].Position - clipVertices[8 * i + 4].Position) * 2;
            }

            for(int i = 0; i < 6; ++i)
            {
                for(int j = 0; j < 8; ++j)
                {
                    if(j < 4)
                    {
                        clipVertices[i * 8 + j].Position += shiftDist[i];
                    }
                    else
                    {
                        clipVertices[i * 8 + j].Position -= shiftDist[i];
                    }
                }
            }

            List<Triang> clipPlaneTriangs = new List<Triang>();
            for (int i = 0; i < clipIndices.Length; i += 3)
            {
                clipPlaneTriangs.Add(new Triang(clipVertices[clipIndices[i]].Position,
                                                clipVertices[clipIndices[i + 1]].Position,
                                                clipVertices[clipIndices[i + 2]].Position));
                
            }

            for(int i = 0; i < 6; ++i)
            {
                for(int j = 0; j < 8; ++j)
                {
                    if(j < 4)
                    {
                        clipVertices[i * 8 + j].Position -= shiftDist[i];
                    }
                    else
                    {
                        clipVertices[i * 8 + j].Position += shiftDist[i];
                    }
                }
            }

            Vector3 rayDir = Vector3.Normalize(outSurfPos - outCamPos);
            Ray ray = new Ray(outCamPos + 0.01f*rayDir, rayDir);
            float dist = float.PositiveInfinity;
            float tmpDist;
            int minIdx = -1;

            for (int i = 0; i < clipPlaneTriangs.Count; ++i)
            {
                if (i < minRejectIdx || i > maxRejectIdx)
                {
                    if (Ray.Intersects(ray, clipPlaneTriangs[i].p1, clipPlaneTriangs[i].p2, clipPlaneTriangs[i].p3,
                                       out tmpDist))
                    {
                        if (tmpDist < dist)
                        {
                            dist = tmpDist;
                            minIdx = i;
                        }
                    }
                }
            }

            return (ClipPlaneType) (minIdx == -1 ? -1 : minIdx/12);
        }
    }
}
