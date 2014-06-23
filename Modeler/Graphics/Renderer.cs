//#define MEASURE_TIMES

using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;
using Modeler.Data.Scene;
using Modeler.Data.Shapes;
using System.Windows.Interop;
using System.IO;
using System.Threading.Tasks;
using BoundingBox = Modeler.Transformations.BoundingBox;

namespace Modeler.Graphics
{

    public enum ClipPlaneType { XMIN = 2, XPLUS = 3, YMIN = 0, YPLUS = 1, ZMIN = 5, ZPLUS = 4, NONE = -1 };

    struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public int Color;
        public float tex0, tex1;
    }

    struct ViewportInfo
    {
        public int resX, resY;

        public int[] posX, posY;
        public int[] sizeX, sizeY;

        public ViewportInfo(int resX, int resY, int[] posX, int[] posY, int[] sizeX, int[] sizeY)
        {
            this.resX = resX;
            this.resY = resY;
            this.posX = posX;
            this.posY = posY;
            this.sizeX = sizeX;
            this.sizeY = sizeY;
        }
    }

    class RenderCamera
    {
        public bool set;

        public Vector3 position, lookAt;
        public float fovAngle;
        public float rotateAngle;

        public Vector3[] points;

        public static Triangle[] triangles;

        static RenderCamera()
        {
            triangles = new Triangle[5];

            triangles[0] = new Triangle(0, 1, 2);
            triangles[1] = new Triangle(0, 2, 3);
            triangles[2] = new Triangle(0, 3, 4);
            triangles[3] = new Triangle(0, 4, 1);
            triangles[4] = new Triangle(5, 7, 6);
        }

        public RenderCamera()
        {
            points = new Vector3[8];
            set = false;
        }
    }

    class RenderLight
    {
        public const int pointsPointNum = 40;
        public const int pointsSpotNum = 122;
        public const int trianglesPointNum = 46;
        public const int trianglesSpotNum = 90;

        public bool set;

        public Light_Type type;
        public Vector3 position;
        public Vector3 direction;
        public float innerAngle;
        public float outerAngle;

        public Vector3[] points;

        public static Vector3[] pointsDef;
        public static Triangle[] triangles;

        public RenderLight()
        {
            points = new Vector3[pointsSpotNum];
            set = false;
        }

        static RenderLight()
        {
            pointsDef = new Vector3[pointsPointNum];
            pointsDef[0] = new Vector3(-0.063667f, - 0.087632f, - 0.000000f);
            pointsDef[1] = new Vector3(-0.019673f, -0.087632f, 0.060553f);
            pointsDef[2] = new Vector3(0.051511f, -0.087632f, 0.037424f);
            pointsDef[3] = new Vector3(0.051511f, -0.087632f, -0.037424f);
            pointsDef[4] = new Vector3(-0.019673f, -0.087632f, -0.060553f);
            pointsDef[5] = new Vector3(-0.103016f, -0.033472f, -0.000000f);
            pointsDef[6] = new Vector3(-0.031833f, -0.033472f, 0.097976f);
            pointsDef[7] = new Vector3(0.083345f, -0.033472f, 0.060553f);
            pointsDef[8] = new Vector3(0.083345f, -0.033472f, -0.060553f);
            pointsDef[9] = new Vector3(-0.031833f, -0.033472f, -0.097976f);
            pointsDef[10] = new Vector3(-0.103016f, 0.033473f, -0.000000f);
            pointsDef[11] = new Vector3(-0.031833f, 0.033473f, 0.097976f);
            pointsDef[12] = new Vector3(0.083345f, 0.033473f, 0.060553f);
            pointsDef[13] = new Vector3(0.083345f, 0.033473f, -0.060553f);
            pointsDef[14] = new Vector3(-0.031833f, 0.033473f, -0.097976f);
            pointsDef[15] = new Vector3(-0.063667f, 0.087633f, -0.000000f);
            pointsDef[16] = new Vector3(-0.019673f, 0.087633f, 0.060553f);
            pointsDef[17] = new Vector3(0.051511f, 0.087633f, 0.037424f);
            pointsDef[18] = new Vector3(0.051511f, 0.087633f, -0.037424f);
            pointsDef[19] = new Vector3(-0.019673f, 0.087633f, -0.060553f);
            pointsDef[20] = new Vector3(0.000002f, 0.108320f, -0.000000f);
            pointsDef[21] = new Vector3(0.000002f, -0.108320f, -0.000000f);
            pointsDef[22] = new Vector3(0, 0.12f, 0);
            pointsDef[23] = new Vector3(0.000001f, 0.12f, 0);
            pointsDef[24] = new Vector3(0, 0.2f, 0);
            pointsDef[25] = new Vector3(0, -0.12f, 0);
            pointsDef[26] = new Vector3(0.000001f, -0.12f, 0);
            pointsDef[27] = new Vector3(0, -0.2f, 0);
            pointsDef[28] = new Vector3(0.12f, 0, 0);
            pointsDef[29] = new Vector3(0.12f, 0, 0.000001f);
            pointsDef[30] = new Vector3(0.2f, 0, 0);
            pointsDef[31] = new Vector3(-0.12f, 0, 0);
            pointsDef[32] = new Vector3(-0.12f, 0, 0.000001f);
            pointsDef[33] = new Vector3(-0.2f, 0, 0);
            pointsDef[34] = new Vector3(0, 0, 0.12f);
            pointsDef[35] = new Vector3(0, 0.000001f, 0.12f);
            pointsDef[36] = new Vector3(0, 0, 0.2f);
            pointsDef[37] = new Vector3(0, 0, -0.12f);
            pointsDef[38] = new Vector3(0, 0.000001f, -0.12f);
            pointsDef[39] = new Vector3(0, 0, -0.2f);

            triangles = new Triangle[trianglesSpotNum];
            triangles[0] = new Triangle(0, 6, 1);
            triangles[1] = new Triangle(0, 5, 6);
            triangles[2] = new Triangle(1, 7, 2);
            triangles[3] = new Triangle(1, 6, 7);
            triangles[4] = new Triangle(2, 8, 3);
            triangles[5] = new Triangle(2, 7, 8);
            triangles[6] = new Triangle(3, 9, 4);
            triangles[7] = new Triangle(3, 8, 9);
            triangles[8] = new Triangle(4, 5, 0);
            triangles[9] = new Triangle(4, 9, 5);
            triangles[10] = new Triangle(5, 11, 6);
            triangles[11] = new Triangle(5, 10, 11);
            triangles[12] = new Triangle(6, 12, 7);
            triangles[13] = new Triangle(6, 11, 12);
            triangles[14] = new Triangle(7, 13, 8);
            triangles[15] = new Triangle(7, 12, 13);
            triangles[16] = new Triangle(8, 14, 9);
            triangles[17] = new Triangle(8, 13, 14);
            triangles[18] = new Triangle(9, 10, 5);
            triangles[19] = new Triangle(9, 14, 10);
            triangles[20] = new Triangle(10, 16, 11);
            triangles[21] = new Triangle(10, 15, 16);
            triangles[22] = new Triangle(11, 17, 12);
            triangles[23] = new Triangle(11, 16, 17);
            triangles[24] = new Triangle(12, 18, 13);
            triangles[25] = new Triangle(12, 17, 18);
            triangles[26] = new Triangle(13, 19, 14);
            triangles[27] = new Triangle(13, 18, 19);
            triangles[28] = new Triangle(14, 15, 10);
            triangles[29] = new Triangle(14, 19, 15);
            triangles[30] = new Triangle(21, 0, 1);
            triangles[31] = new Triangle(16, 15, 20);
            triangles[32] = new Triangle(21, 1, 2);
            triangles[33] = new Triangle(17, 16, 20);
            triangles[34] = new Triangle(21, 2, 3);
            triangles[35] = new Triangle(18, 17, 20);
            triangles[36] = new Triangle(21, 3, 4);
            triangles[37] = new Triangle(19, 18, 20);
            triangles[38] = new Triangle(21, 4, 0);
            triangles[39] = new Triangle(15, 19, 20);
            triangles[40] = new Triangle(22, 23, 24);
            triangles[41] = new Triangle(25, 26, 27);
            triangles[42] = new Triangle(28, 29, 30);
            triangles[43] = new Triangle(31, 32, 33);
            triangles[44] = new Triangle(34, 35, 36);
            triangles[45] = new Triangle(37, 38, 39);

            triangles[46] = new Triangle(120, 121, 40);
            triangles[47] = new Triangle(120, 121, 60);
            triangles[48] = new Triangle(120, 121, 80);
            triangles[49] = new Triangle(120, 121, 100);

            triangles[50] = new Triangle(40, 41, 42);
            triangles[51] = new Triangle(42, 43, 44);
            triangles[52] = new Triangle(44, 45, 46);
            triangles[53] = new Triangle(46, 47, 48);
            triangles[54] = new Triangle(48, 49, 50);
            triangles[55] = new Triangle(50, 51, 52);
            triangles[56] = new Triangle(52, 53, 54);
            triangles[57] = new Triangle(54, 55, 56);
            triangles[58] = new Triangle(56, 57, 58);
            triangles[59] = new Triangle(58, 59, 60);
            triangles[60] = new Triangle(60, 61, 62);
            triangles[61] = new Triangle(62, 63, 64);
            triangles[62] = new Triangle(64, 65, 66);
            triangles[63] = new Triangle(66, 67, 68);
            triangles[64] = new Triangle(68, 69, 70);
            triangles[65] = new Triangle(70, 71, 72);
            triangles[66] = new Triangle(72, 73, 74);
            triangles[67] = new Triangle(74, 75, 76);
            triangles[68] = new Triangle(76, 77, 78);
            triangles[69] = new Triangle(78, 79, 40);

            triangles[70] = new Triangle(80, 81, 82);
            triangles[71] = new Triangle(82, 83, 84);
            triangles[72] = new Triangle(84, 85, 86);
            triangles[73] = new Triangle(86, 87, 88);
            triangles[74] = new Triangle(88, 89, 90);
            triangles[75] = new Triangle(90, 91, 92);
            triangles[76] = new Triangle(92, 93, 94);
            triangles[77] = new Triangle(94, 95, 96);
            triangles[78] = new Triangle(96, 97, 98);
            triangles[79] = new Triangle(98, 99, 100);
            triangles[80] = new Triangle(100, 101, 102);
            triangles[81] = new Triangle(102, 103, 104);
            triangles[82] = new Triangle(104, 105, 106);
            triangles[83] = new Triangle(106, 107, 108);
            triangles[84] = new Triangle(108, 109, 110);
            triangles[85] = new Triangle(110, 111, 112);
            triangles[86] = new Triangle(112, 113, 114);
            triangles[87] = new Triangle(114, 115, 116);
            triangles[88] = new Triangle(116, 117, 118);
            triangles[89] = new Triangle(118, 119, 80);
        }
    }

    class Renderer
    {
#if MEASURE_TIMES
        StreamWriter w = File.AppendText("times4.txt");
#endif
        IntPtr handle;
        IntPtr handleBezier;

        PresentParameters pp;
        Device device;
        Direct3D d3d;
        PresentParameters bezierPp;
        Device deviceBezier;
        Direct3D d3dBezier;

        Viewport perspective;
        Viewport top;
        Viewport front;
        Viewport side;

        SlimDX.Direct3D9.Font font;
        SlimDX.Direct3D9.Font font2;

        System.Drawing.Point persPos;

        VertexElement[] vertexElems;
        Scene currScene;

        static bool[] selected;
        static Vertex[] vertices;
        static List<int>[] vertexTriangle;
        static int[] indices;
        static int[] selIndices;
        static uint numIndices;
        static uint numSelIndices;
        static bool[] selPoints;
        static Material_[] materials;

        public static List<Vector3> camsLookAtPoints;
        public const float spotLightDist = 1;
        public const float pointSize = 0.12f;

        Light defLight;
        int maxLights;

        List<RenderCamera> cameras;
        List<RenderLight> lights;

        static bool clipping;
        public static Vertex[] clipVertices;
        public static int[] clipIndices;
        const float clipPlaneWidth = 0.004f;
        static public bool Clipping
        {
            get { return clipping; }
            set { clipping = value; }
        }

        public float[] orthoWidth;
        private bool ortoWidhtChange;
        Vector3[] orthoPos;
        Vector3[] orthoLookAt;

        float[] bezierOrthoWidth;
        Vector3[] bezierOrthoPos;
        Vector3[] bezierOrthoLookAt;
        public Camera bezierCam;
        Material selectedControlPointMaterial;
        Material controlPointMaterial;
        bool bezierImageCreated = false;
        //Texture selectionTex;

        public float[] BezierOrthoWidth
        { get { return bezierOrthoWidth; } }
        public Vector3[] BezierOrthoPos
        { get {return bezierOrthoPos; } } 
        public Vector3[] BezierOrthoLookAt
        { get { return bezierOrthoLookAt; } }

        public float[] OrthoWidth
        {
            get
            {
                return orthoWidth;
            }
        }

        public Vector3[] OrthoPos
        {
            get
            {
                return orthoPos;
            }
        }

        public Vector3[] OrthoLookAt
        {
            get
            {
                return orthoLookAt;
            }
        }

        static Renderer()
        {
            selected = new bool[0];
            vertices = new Vertex[0];
            vertexTriangle = new List<int>[0];
            indices = new int[0];
            selIndices = new int[0];
            numIndices = 0;
            numSelIndices = 0;
            selPoints = new bool[0];
            materials = new Material_[0];
        }

        public Renderer(IntPtr handle, IntPtr handleBezier)
        {
            this.handle = handle;
            this.handleBezier = handleBezier;

            currScene = null;

            font = null;
            font2 = null;

            cameras = new List<RenderCamera>();
            lights = new List<RenderLight>();

            orthoWidth = new float[] { 10, 10, 10 };
            orthoPos = new Vector3[] { new Vector3(0, 0, 50001), new Vector3(50001, 0, 0), new Vector3(0, 50001, 0) };
            orthoLookAt = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, -0.01f) };

            ortoWidhtChange = false;

            bezierOrthoWidth = new float[] { 5, 5, 5 };
            bezierOrthoPos = new Vector3[] { new Vector3(0, 0, 50000), new Vector3(50000, 0, 0), new Vector3(0, 50000, 0) };
            bezierOrthoLookAt = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, -0.01f) };
            bezierCam = new Camera(null, 0, 0, new Vector3(-10, 10, -10), new Vector3(0, 0, 0), Utilities.DegToRad(15), 0);
            selectedControlPointMaterial = new Material() { Diffuse = new Color4(Color.Red) };
            controlPointMaterial = new Material() { Diffuse = new Color4(Color.Black) };

            camsLookAtPoints = new List<Vector3>();

            clipping = false;
            clipVertices = new Vertex[48];
            for(int i = 0; i < clipVertices.Length; ++i)
            {
                clipVertices[i] = new Vertex();
                clipVertices[i].Color = Color.DarkViolet.ToArgb();
            }
            for(int i = 0; i < 8; ++i)
            {
                clipVertices[i + 0].Position = new Vector3(i == 0 || i == 3 || i == 4 || i == 7 ? -50000 : 50000,
                                                           i == 0 || i == 1 || i == 2 || i == 3 ? 2 : 2 + clipPlaneWidth * orthoWidth[0],
                                                           i == 0 || i == 1 || i == 4 || i == 5 ? 50000 : -50000);
            }
            for(int i = 0; i < 8; ++i)
            {
                clipVertices[i + 8].Position = new Vector3(i == 0 || i == 3 || i == 4 || i == 7 ? 50000 : -50000,
                                                           i == 0 || i == 1 || i == 2 || i == 3 ? -2 : -2 - clipPlaneWidth * orthoWidth[0],
                                                           i == 0 || i == 1 || i == 4 || i == 5 ? 50000 : -50000);
            }
            for(int i = 0; i < 8; ++i)
            {
                clipVertices[i + 16].Position = new Vector3(i == 0 || i == 1 || i == 2 || i == 3 ? 2 : 2 + clipPlaneWidth * orthoWidth[0],
                                                            i == 0 || i == 3 || i == 4 || i == 7 ? 50000 : -50000,
                                                            i == 0 || i == 1 || i == 4 || i == 5 ? 50000 : -50000);
            }
            for(int i = 0; i < 8; ++i)
            {
                clipVertices[i + 24].Position = new Vector3(i == 0 || i == 1 || i == 2 || i == 3 ? -2 : -2 - clipPlaneWidth * orthoWidth[0],
                                                            i == 0 || i == 3 || i == 4 || i == 7 ? -50000 : 50000,
                                                            i == 0 || i == 1 || i == 4 || i == 5 ? 50000 : -50000);
            }
            for(int i = 0; i < 8; ++i)
            {
                clipVertices[i + 32].Position = new Vector3(i == 0 || i == 3 || i == 4 || i == 7 ? -50000 : 50000,
                                                            i == 0 || i == 1 || i == 4 || i == 5 ? -50000 : 50000,
                                                            i == 0 || i == 1 || i == 2 || i == 3 ? -2 : -2 - clipPlaneWidth * orthoWidth[0]);
            }
            for(int i = 0; i < 8; ++i)
            {
                clipVertices[i + 40].Position = new Vector3(i == 0 || i == 3 || i == 4 || i == 7 ? 50000 : -50000,
                                                            i == 0 || i == 1 || i == 4 || i == 5 ? -50000 : 50000,
                                                            i == 0 || i == 1 || i == 2 || i == 3 ? 2 : 2 + clipPlaneWidth * orthoWidth[0]);
            }

            clipIndices = new int[216];
            for(int i = 0; i < 6; ++i)
            {
                clipIndices[36 * i + 0] = 8 * i + 0; clipIndices[36 * i + 1] = 8 * i + 1; clipIndices[36 * i + 2] = 8 * i + 5;
                clipIndices[36 * i + 3] = 8 * i + 0; clipIndices[36 * i + 4] = 8 * i + 5; clipIndices[36 * i + 5] = 8 * i + 4;
                clipIndices[36 * i + 6] = 8 * i + 1; clipIndices[36 * i + 7] = 8 * i + 2; clipIndices[36 * i + 8] = 8 * i + 6;
                clipIndices[36 * i + 9] = 8 * i + 1; clipIndices[36 * i + 10] = 8 * i + 6; clipIndices[36 * i + 11] = 8 * i + 5;
                clipIndices[36 * i + 12] = 8 * i + 2; clipIndices[36 * i + 13] = 8 * i + 3; clipIndices[36 * i + 14] = 8 * i + 7;
                clipIndices[36 * i + 15] = 8 * i + 2; clipIndices[36 * i + 16] = 8 * i + 7; clipIndices[36 * i + 17] = 8 * i + 6;
                clipIndices[36 * i + 18] = 8 * i + 3; clipIndices[36 * i + 19] = 8 * i + 0; clipIndices[36 * i + 20] = 8 * i + 5;
                clipIndices[36 * i + 21] = 8 * i + 3; clipIndices[36 * i + 22] = 8 * i + 5; clipIndices[36 * i + 23] = 8 * i + 7;
                clipIndices[36 * i + 24] = 8 * i + 0; clipIndices[36 * i + 25] = 8 * i + 1; clipIndices[36 * i + 26] = 8 * i + 2;
                clipIndices[36 * i + 27] = 8 * i + 0; clipIndices[36 * i + 28] = 8 * i + 2; clipIndices[36 * i + 29] = 8 * i + 3;
                clipIndices[36 * i + 30] = 8 * i + 4; clipIndices[36 * i + 31] = 8 * i + 5; clipIndices[36 * i + 32] = 8 * i + 6;
                clipIndices[36 * i + 33] = 8 * i + 4; clipIndices[36 * i + 34] = 8 * i + 6; clipIndices[36 * i + 35] = 8 * i + 7;
            }

            perspective = new Viewport();
            perspective.X = 0;
            perspective.Y = 0;
            perspective.Width = 1;
            perspective.Height = 1;
            perspective.MinZ = -1000;
            perspective.MaxZ = 1000;

            top = new Viewport();
            top.X = 0;
            top.Y = 0;
            top.Width = 1;
            top.Height = 1;
            top.MinZ = -1000;
            top.MaxZ = 1000;

            front = new Viewport();
            front.X = 0;
            front.Y = 0;
            front.Width = 1;
            front.Height = 1;
            front.MinZ = -1000;
            front.MaxZ = 1000;

            side = new Viewport();
            side.X = 0;
            side.Y = 0;
            side.Width = 1;
            side.Height = 1;
            side.MinZ = -1000;
            side.MaxZ = 1000;

            defLight = new Light();
            defLight.Type = LightType.Directional;
            defLight.Diffuse = Color.White;
            defLight.Direction = new Vector3(1, -1, -2);

            maxLights = 0;

            vertexElems = new[] {
        		new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
        		new VertexElement(0, 12, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0),
                new VertexElement(0, 24, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
                new VertexElement(0, 28, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
				VertexElement.VertexDeclarationEnd
        	};

            wireframe = true;
            pp = new PresentParameters();
            pp.SwapEffect = SwapEffect.Discard;
            pp.Windowed = true;
            pp.BackBufferFormat = Format.A8R8G8B8;

            bezierPp = new PresentParameters();
            bezierPp.SwapEffect = SwapEffect.Discard;
            bezierPp.Windowed = true;
            bezierPp.BackBufferFormat = Format.A8R8G8B8;
        }

        public static float GetClipPlanePosition(ClipPlaneType plane)
        {
            float pos = 0;

            switch(plane)
            {
                case ClipPlaneType.XMIN:
                    pos = clipVertices[24].Position.X;
                    break;

                case ClipPlaneType.XPLUS:
                    pos = clipVertices[16].Position.X;
                    break;

                case ClipPlaneType.YMIN:
                    pos = clipVertices[8].Position.Y;
                    break;

                case ClipPlaneType.YPLUS:
                    pos = clipVertices[0].Position.Y;
                    break;

                case ClipPlaneType.ZMIN:
                    pos = clipVertices[32].Position.Z;
                    break;

                case ClipPlaneType.ZPLUS:
                    pos = clipVertices[40].Position.Z;
                    break;
            }

            return pos;
        }

        private bool wireframe;

        public void ChangeWireframe()
        {
            wireframe = !wireframe;
        }

        public void MoveOrtho(int orthoViewport, float x, float y, int sizeX, int sizeY, bool correct = true)
        {
            float xx = x / sizeX * orthoWidth[orthoViewport];
            float yy = y / sizeX * orthoWidth[orthoViewport];

            float X = 0, Y = 0, Z = 0;
            switch(orthoViewport)
            {
                case 2:
                    X = xx;
                    Y = 0;
                    Z = yy;

                    if(correct == true)
                    {
                        MoveOrtho(0, x, 0, sizeX, sizeY, false);
                        MoveOrtho(1, -y, 0, sizeX, sizeY, false);
                    }
                    break;

                case 0:
                    X = xx;
                    Y = -yy;
                    Z = 0;

                    if(correct == true)
                    {
                        MoveOrtho(1, 0, y, sizeX, sizeY, false);
                        MoveOrtho(2, x, 0, sizeX, sizeY, false);
                    }
                    break;

                case 1:
                    X = 0;
                    Y = -yy;
                    Z = -xx;

                    if(correct == true)
                    {
                        MoveOrtho(0, 0, y, sizeX, sizeY, false);
                        MoveOrtho(2, 0, -x, sizeX, sizeY, false);
                    }
                    break;
            }

            orthoPos[orthoViewport].X += X;
            orthoPos[orthoViewport].Y += Y;
            orthoPos[orthoViewport].Z += Z;

            orthoLookAt[orthoViewport].X += X;
            orthoLookAt[orthoViewport].Y += Y;
            orthoLookAt[orthoViewport].Z += Z;
        }

        public void MoveBezierOrtho(int orthoViewport, float x, float y, int sizeX, int sizeY, bool correct = true)
        {
            float xx = x / sizeX * bezierOrthoWidth[orthoViewport];
            float yy = y / sizeX * bezierOrthoWidth[orthoViewport];

            float X = 0, Y = 0, Z = 0;
            switch (orthoViewport)
            {
                case 2:
                    X = xx;
                    Y = 0;
                    Z = yy;
                    if (correct == true)
                    {
                        MoveBezierOrtho(0, x, 0, sizeX, sizeY, false);
                        MoveBezierOrtho(1, -y, 0, sizeX, sizeY, false);
                    }
                    break;

                case 0:
                    X = xx;
                    Y = -yy;
                    Z = 0;
                    if (correct == true)
                    {
                        MoveBezierOrtho(1, 0, y, sizeX, sizeY, false);
                        MoveBezierOrtho(2, x, 0, sizeX, sizeY, false);
                    }
                    break;

                case 1:
                    X = 0;
                    Y = -yy;
                    Z = -xx;
                    if (correct == true)
                    {
                        MoveBezierOrtho(0, 0, y, sizeX, sizeY, false);
                        MoveBezierOrtho(2, 0, -x, sizeX, sizeY, false);
                    }
                    break;
            }

            bezierOrthoPos[orthoViewport].X += X;
            bezierOrthoPos[orthoViewport].Y += Y;
            bezierOrthoPos[orthoViewport].Z += Z;

            bezierOrthoLookAt[orthoViewport].X += X;
            bezierOrthoLookAt[orthoViewport].Y += Y;
            bezierOrthoLookAt[orthoViewport].Z += Z;
        }

        public void ScaleOrtho(int orthoViewport, float value, ViewportInfo info, int x, int y)
        {
            Vector2 oldSize = new Vector2(orthoWidth[orthoViewport], orthoWidth[orthoViewport] * (info.sizeY[orthoViewport] / (float)info.sizeX[orthoViewport]));

            orthoWidth[0] *= 1 + 0.05f * -value;
            orthoWidth[1] *= 1 + 0.05f * -value;
            orthoWidth[2] *= 1 + 0.05f * -value;
            if(orthoWidth[orthoViewport] < 0.01f)
            {
                orthoWidth[0] = 0.01f;
                orthoWidth[1] = 0.01f;
                orthoWidth[2] = 0.01f;
            }

            Vector2 newSize = new Vector2(orthoWidth[orthoViewport], orthoWidth[orthoViewport] * (info.sizeY[orthoViewport] / (float)info.sizeX[orthoViewport]));
            Vector2 mousePos = new Vector2((x - info.posX[orthoViewport]) / (float)info.sizeX[orthoViewport],
                (y - info.posY[orthoViewport]) / (float)info.sizeY[orthoViewport]);

            Vector2 shift = new Vector2();
            shift.X = (oldSize.X - newSize.X) * (mousePos.X - 0.5f) * info.sizeX[orthoViewport] / orthoWidth[orthoViewport];
            shift.Y = (oldSize.Y - newSize.Y) * (mousePos.Y - 0.5f) * info.sizeX[orthoViewport] / orthoWidth[orthoViewport];

            MoveOrtho(orthoViewport, shift.X, shift.Y, info.sizeX[orthoViewport], info.sizeY[orthoViewport]);

            UpdateClipPlanes();

            ortoWidhtChange = true;
        }

        public void ScaleBezierOrtho(int orthoViewport, float value, ViewportInfo info, int x, int y)
        {
            Vector2 oldSize = new Vector2(BezierOrthoWidth[orthoViewport], BezierOrthoWidth[orthoViewport] * (info.sizeY[orthoViewport] / (float)info.sizeX[orthoViewport]));

            BezierOrthoWidth[0] *= 1 + 0.05f * -value;
            BezierOrthoWidth[1] *= 1 + 0.05f * -value;
            BezierOrthoWidth[2] *= 1 + 0.05f * -value;
            if (BezierOrthoWidth[orthoViewport] < 0.01f)
            {
                BezierOrthoWidth[0] = 0.01f;
                BezierOrthoWidth[1] = 0.01f;
                BezierOrthoWidth[2] = 0.01f;
            }

            Vector2 newSize = new Vector2(bezierOrthoWidth[orthoViewport], bezierOrthoWidth[orthoViewport] * (info.sizeY[orthoViewport] / (float)info.sizeX[orthoViewport]));
            Vector2 mousePos = new Vector2((x - info.posX[orthoViewport]) / (float)info.sizeX[orthoViewport],
                (y - info.posY[orthoViewport]) / (float)info.sizeY[orthoViewport]);

            Vector2 shift = new Vector2();
            shift.X = (oldSize.X - newSize.X) * (mousePos.X - 0.5f) * info.sizeX[orthoViewport] / bezierOrthoWidth[orthoViewport];
            shift.Y = (oldSize.Y - newSize.Y) * (mousePos.Y - 0.5f) * info.sizeX[orthoViewport] / bezierOrthoWidth[orthoViewport];

            MoveBezierOrtho(orthoViewport, shift.X, shift.Y, info.sizeX[orthoViewport], info.sizeY[orthoViewport]);
        }

        public void ScaleBezierPersp(float value)
        {
            bezierCam.fovAngle += Utilities.DegToRad(value);
        }

        public static void RecalculateData(Scene scene)
        {
            CalculateSelections(scene);
            CalculateBuffers(scene);
            CalculateMaterials(scene);
        }

        public static void RecalculateNormals(Scene scene)
        {
            scene.normals.Clear();
            for(int i = 0; i < scene.points.Count; ++i)
            {
                Vector3D normal = new Vector3D();

                foreach(int face in vertexTriangle[i])
                {
                    normal += Utilities.CalculateNormal(scene.points[(int)scene.triangles[face].p3], scene.points[(int)scene.triangles[face].p2],
                        scene.points[(int)scene.triangles[face].p1]);
                }

                normal.Normalize();
                scene.normals.Add(normal);
            }
        }

        public static void CalculateSelections(Scene scene)
        {
            selected = scene.GetSelectedTriangles();
        }

        public static void CalculateBuffers(Scene scene)
        {
            vertices = new Vertex[scene.points.Count];
            vertexTriangle = new List<int>[scene.points.Count];
            Parallel.For(0, vertexTriangle.Length, index => vertexTriangle[index] = new List<int>());
            /*for(int i = 0; i < vertexTriangle.Length; ++i)
            {
                vertexTriangle[i] = new List<int>();
            }*/

            indices = new int[3 * scene.triangles.Count];
            selIndices = new int[3 * scene.triangles.Count];
            numIndices = 0;
            numSelIndices = 0;

            selPoints = new bool[scene.points.Count];
            Parallel.For(0, selPoints.Length, index => selPoints[index] = false);
            /*for(int i = 0; i < selPoints.Length; ++i)
            {
                selPoints[i] = false;
            }*/

            for(int i = 0; i < scene.triangles.Count; ++i)
            {
                Triangle triangle = scene.triangles[i];

                if(selected[i] == false)
                {
                    indices[numIndices] = (int)triangle.p1;
                    indices[numIndices + 1] = (int)triangle.p3;
                    indices[numIndices + 2] = (int)triangle.p2;

                    numIndices += 3;
                }
                else
                {
                    selIndices[numSelIndices] = (int)triangle.p1;
                    selIndices[numSelIndices + 1] = (int)triangle.p3;
                    selIndices[numSelIndices + 2] = (int)triangle.p2;

                    selPoints[triangle.p1] = true;
                    selPoints[triangle.p2] = true;
                    selPoints[triangle.p3] = true;

                    numSelIndices += 3;
                }

                vertexTriangle[triangle.p1].Add(i);
                vertexTriangle[triangle.p2].Add(i);
                vertexTriangle[triangle.p3].Add(i);
            }
        }

        public static void CalculateMaterials(Scene scene)
        {
            for(int i = 0; i < scene.points.Count; ++i)
            {
                if(selPoints[i] == true || scene.normals.Count <= i)
                {
                    Vector3D normal = new Vector3D();

                    foreach(int face in vertexTriangle[i])
                    {
                        normal += Utilities.CalculateNormal(scene.points[(int)scene.triangles[face].p3], scene.points[(int)scene.triangles[face].p2],
                            scene.points[(int)scene.triangles[face].p1]);
                    }

                    normal.Normalize();
                    if(scene.normals.Count <= i)
                    {
                        scene.normals.Add(normal);
                    }
                    else
                    {
                        scene.normals[i] = normal;
                    }
                }
            }

            // Szemrane poprawki
            int count = scene.materials.Count;
            // Koniec
            Dictionary<string, int> matNames = new Dictionary<string, int>();
            for(int i = 0; i < count; ++i)
            {
                if (!matNames.ContainsKey(scene.materials[i].name))
                {
                    matNames.Add(scene.materials[i].name, i);
                }
                // Szemrane poprawki, tymczasowy patch
                else
                {
                    scene.materials.RemoveAt(i);
                    count--;
                    i--;
                }
                // Koniec
            }

            int[] trPart = new int[scene.triangles.Count];
            for(int i = 0; i < scene.parts.Count; ++i)
            {
                for(int j = 0; j < scene.parts[i].triangles.Count; ++j)
                {
                    trPart[scene.parts[i].triangles[j]] = i;
                }
            }

            for(int i = 0; i < vertices.Length; ++i)
            {
                //szemrane poprawki
                if (vertexTriangle[i].Count == 0)
                    continue;
                int partIndex = trPart[vertexTriangle[i][0]];

                String matName = scene.materialAssign[partIndex];
                Material_ material = scene.materials[matNames[matName]];

                Vector3D point = scene.points[i];

                vertices[i].Position = new Vector3(point.x, point.y, point.z);
                vertices[i].Normal = new Vector3(scene.normals[i].x, scene.normals[i].y, scene.normals[i].z);
                vertices[i].Color = Color.FromArgb((int)(255 * material.colorR * material.kdcR),
                                                   (int)(255 * material.colorG * material.kdcG),
                                                   (int)(255 * material.colorB * material.kdcB)).ToArgb();
            }
        }

        public List<Vector3> GetCamsPoints()
        {
            List<Vector3> camsPoints = new List<Vector3>();

            foreach(RenderCamera camera in cameras)
            {
                camsPoints.AddRange(camera.points);
            }

            return camsPoints;
        }

        public Pair<List<Vector3>, List<int> > GetLightsPoints()
        {
            List<Vector3> lightsPoints = new List<Vector3>();
            List<int> lightsPointsNum = new List<int>();

            foreach(RenderLight light in lights)
            {
                int numPoints = light.type == Light_Type.Point ? RenderLight.pointsPointNum : RenderLight.pointsSpotNum;
                for(int i = 0; i < numPoints; ++i)
                {
                    lightsPoints.Add(light.points[i]);
                }
                int numTriangles = light.type == Light_Type.Point ? RenderLight.trianglesPointNum : RenderLight.trianglesSpotNum;
                lightsPointsNum.Add(numTriangles);
            }

            return new Pair<List<Vector3>,List<int>>(lightsPoints, lightsPointsNum);
        }

        private void UpdateClipPlanes()
        {
            float oldValue = clipVertices[0].Position.Y;
            for(int i = 0; i < 8; ++i)
            {
                clipVertices[i + 0].Position.Y = i == 0 || i == 1 || i == 2 || i == 3 ? oldValue : oldValue + clipPlaneWidth * orthoWidth[0];
            }
            oldValue = clipVertices[8].Position.Y;
            for(int i = 0; i < 8; ++i)
            {
                clipVertices[i + 8].Position.Y = i == 0 || i == 1 || i == 2 || i == 3 ? oldValue : oldValue - clipPlaneWidth * orthoWidth[0];
            }
            oldValue = clipVertices[16].Position.X;
            for(int i = 0; i < 8; ++i)
            {
                clipVertices[i + 16].Position.X = i == 0 || i == 1 || i == 2 || i == 3 ? oldValue : oldValue + clipPlaneWidth * orthoWidth[0];
            }
            oldValue = clipVertices[24].Position.X;
            for(int i = 0; i < 8; ++i)
            {
                clipVertices[i + 24].Position.X = i == 0 || i == 1 || i == 2 || i == 3 ? oldValue : oldValue - clipPlaneWidth * orthoWidth[0];
            }
            oldValue = clipVertices[32].Position.Z;
            for(int i = 0; i < 8; ++i)
            {
                clipVertices[i + 32].Position.Z = i == 0 || i == 1 || i == 2 || i == 3 ? oldValue : oldValue - clipPlaneWidth * orthoWidth[0];
            }
            oldValue = clipVertices[40].Position.Z;
            for(int i = 0; i < 8; ++i)
            {
                clipVertices[i + 40].Position.Z = i == 0 || i == 1 || i == 2 || i == 3 ? oldValue : oldValue + clipPlaneWidth * orthoWidth[0];
            }
        }

        public void SetClipping(bool enabled)
        {
            clipping = enabled;
        }

        /// <summary>
        /// Przesuwa płaszczyzny obcinające
        /// </summary>
        /// <param name="plane">Która płaszczyzna jest przesuwana</param>
        /// <param name="value">Wartość przesunięcia w jednostkach sceny</param>
        public void ShiftClipPlane(ClipPlaneType plane, float value)
        {
            int i = -1;

            switch(plane)
            {
                case ClipPlaneType.XMIN:
                    i = 16;
                    break;

                case ClipPlaneType.XPLUS:
                    i = 24;
                    break;

                case ClipPlaneType.YMIN:
                    i = 0;
                    break;

                case ClipPlaneType.YPLUS:
                    i = 8;
                    break;

                case ClipPlaneType.ZMIN:
                    i = 40;
                    break;

                case ClipPlaneType.ZPLUS:
                    i = 32;
                    break;
            }

            int j = i + 8;
            if(i == 0)
            {
                float newValue = clipVertices[i].Position.Y + value;
                if(newValue < clipVertices[i + 8].Position.Y)
                {
                    value = clipVertices[i + 8].Position.Y - clipVertices[i].Position.Y;
                }
                for(; i < j; ++i)
                {
                    clipVertices[i].Position.Y += value;
                }
            }
            else if(i == 8)
            {
                float newValue = clipVertices[i].Position.Y + value;
                if(newValue > clipVertices[i - 8].Position.Y)
                {
                    value = clipVertices[i - 8].Position.Y - clipVertices[i].Position.Y;
                }
                for(; i < j; ++i)
                {
                    clipVertices[i].Position.Y += value;
                }
            }
            else if(i == 16)
            {
                float newValue = clipVertices[i].Position.X + value;
                if(newValue < clipVertices[i + 8].Position.X)
                {
                    value = clipVertices[i + 8].Position.X - clipVertices[i].Position.X;
                }
                for(; i < j; ++i)
                {
                    clipVertices[i].Position.X += value;
                }
            }
            else if(i == 24)
            {
                float newValue = clipVertices[i].Position.X + value;
                if(newValue > clipVertices[i - 8].Position.X)
                {
                    value = clipVertices[i - 8].Position.X - clipVertices[i].Position.X;
                }
                for(; i < j; ++i)
                {
                    clipVertices[i].Position.X += value;
                }
            }
            else if(i == 32)
            {
                float newValue = clipVertices[i].Position.Z + value;
                if(newValue > clipVertices[i + 8].Position.Z)
                {
                    value = clipVertices[i + 8].Position.Z - clipVertices[i].Position.Z;
                }
                for(; i < j; ++i)
                {
                    clipVertices[i].Position.Z += value;
                }
            }
            else if(i == 40)
            {
                float newValue = clipVertices[i].Position.Z + value;
                if(newValue < clipVertices[i - 8].Position.Z)
                {
                    value = clipVertices[i - 8].Position.Z - clipVertices[i].Position.Z;
                }
                for(; i < j; ++i)
                {
                    clipVertices[i].Position.Z += value;
                }
            }
        }

        public Image GetSceneImage(Scene scene)
        {
            RecalculateData(scene);

            pp = new PresentParameters();
            pp.SwapEffect = SwapEffect.Discard;
            pp.Windowed = true;
            pp.BackBufferWidth = 512;
            pp.BackBufferHeight = 512;
            pp.BackBufferFormat = Format.A8R8G8B8;

            if(d3d != null)
            {
                d3d.Dispose();
            }
            if(device != null)
            {
                device.Dispose();
            }

            d3d = new Direct3D();
            device = new Device(d3d, 0, DeviceType.Hardware, handle, CreateFlags.HardwareVertexProcessing, pp);

            device.SetRenderState(RenderState.Lighting, true);

            for(int i = scene.lights.Count; i < maxLights; ++i)
            {
                device.EnableLight(i, false);
            }
            maxLights = scene.lights.Count;

            Modeler.Transformations.BoundingBox bb = new Modeler.Transformations.BoundingBox(scene);
            Camera cam = new Camera();
            Vector3 dir = Vector3.Normalize(bb.minBB - bb.maxBB);
            cam.position = bb.maxBB - 1 * (float)Math.Sqrt((bb.minBB.x - bb.maxBB.x) * (bb.minBB.x - bb.maxBB.x) +
                (bb.minBB.y - bb.maxBB.y) * (bb.minBB.y - bb.maxBB.y) + (bb.minBB.z - bb.maxBB.z) * (bb.minBB.z - bb.maxBB.z)) * dir;
            cam.lookAt = bb.maxBB;
            cam.fovAngle = 40;
            cam.rotateAngle = 0;

            Vector3 oldDir = defLight.Direction;

            Vector3 dirLight = new Vector3();
            dirLight.X = dir.X * (float)Math.Cos(-Math.PI / 4) + dir.Z * (float)Math.Sin(-Math.PI / 4);
            dirLight.Y = dir.Y;
            dirLight.Z = dir.Z * (float)Math.Cos(-Math.PI / 4) - dir.X * (float)Math.Sin(-Math.PI / 4);
            dir.Normalize();
            defLight.Direction = dirLight;
            device.SetLight(0, defLight);
            device.EnableLight(0, true);

            device.SetRenderState(RenderState.FillMode, FillMode.Solid);
            device.SetRenderState(RenderState.CullMode, Cull.None);
            device.SetRenderState(RenderState.ShadeMode, ShadeMode.Gouraud);

            Mesh mesh = numIndices >= 3 ? new Mesh(device, (int)numIndices / 3, scene.points.Count, MeshFlags.Managed | MeshFlags.Use32Bit, vertexElems) : null;
            VertexBuffer vb = mesh != null ? mesh.VertexBuffer : null;
            IndexBuffer ib = mesh != null ? mesh.IndexBuffer : null;

            if(mesh != null)
            {
                vb.Lock(0, 0, LockFlags.None).WriteRange(vertices);
                vb.Unlock();

                ib.Lock(0, 0, LockFlags.None).WriteRange(indices, 0, (int)numIndices);
                ib.Unlock();
            }

            Mesh selMesh = numSelIndices >= 3 ? new Mesh(device, (int)numSelIndices / 3, scene.points.Count, MeshFlags.Managed | MeshFlags.Use32Bit, vertexElems) : null;
            VertexBuffer selvb = selMesh != null ? selMesh.VertexBuffer : null;
            IndexBuffer selib = selMesh != null ? selMesh.IndexBuffer : null;

            if(selMesh != null)
            {
                selvb.Lock(0, 0, LockFlags.None).WriteRange(vertices);
                selvb.Unlock();

                selib.Lock(0, 0, LockFlags.None).WriteRange(selIndices, 0, (int)numSelIndices);
                selib.Unlock();
            }

            Viewport viewport = new Viewport(0, 0, 512, 512, 0, 1);

            Texture texture = new Texture(device, 64, 64, 0, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            device.SetRenderTarget(0, texture.GetSurfaceLevel(0));

            float camRotAngle = cam.rotateAngle;
            float aspect = 1;
            float camAngle = 2.0f * (float)Math.Atan(Math.Tan(Utilities.DegToRad(cam.fovAngle) / 2.0f) / aspect);

            device.SetTransform(TransformState.View, Matrix.LookAtRH(
                cam.position,
                cam.lookAt,
                Utilities.RotatePointAroundVector(new Vector3(0, 1, 0),
                Vector3.Normalize(cam.lookAt - cam.position), camRotAngle)));

            device.SetTransform(TransformState.Projection, Matrix.PerspectiveFovRH(
                camAngle,
                aspect,
                0.01f,
                110000));


            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.White, 1.0f, 0);
            device.BeginScene();

            if(mesh != null)
            {
                mesh.DrawSubset(0);
            }

            if(selMesh != null)
            {
                selMesh.DrawSubset(0);
            }

            device.EndScene();

            Image image = Bitmap.FromStream(Texture.ToStream(texture, ImageFileFormat.Png));

            texture.Dispose();

            RecalculateData(currScene);
            defLight.Direction = oldDir;

            return image;
        }

        public void RenderViews(ViewportInfo viewportInfo, Scene scene)
        {
            currScene = scene;
#if MEASURE_TIMES
            DateTime ts = DateTime.Now;

            DateTime t1 = DateTime.Now;
#endif
            if (pp.BackBufferWidth != viewportInfo.resX || pp.BackBufferHeight != viewportInfo.resY)
            {
                pp = new PresentParameters();
                pp.SwapEffect = SwapEffect.Discard;
                pp.Windowed = true;
                pp.BackBufferWidth = viewportInfo.resX;
                pp.BackBufferHeight = viewportInfo.resY;
                pp.BackBufferFormat = Format.A8R8G8B8;

                if (d3d != null)
                {
                    d3d.Dispose();
                }
                if (device != null)
                {
                    device.Dispose();
                }

                d3d = new Direct3D();
                device = new Device(d3d, 0, DeviceType.Hardware, handle, CreateFlags.HardwareVertexProcessing, pp);

                if(font != null)
                {
                    font.Dispose();
                }
                font = new SlimDX.Direct3D9.Font(device, new System.Drawing.Font(FontFamily.GenericSansSerif, 7));
            }

            bool viewportChange = false;

            if(top.X != viewportInfo.posX[2] || top.Y != viewportInfo.posY[2] || top.Width != viewportInfo.sizeX[2] ||
                top.Height != viewportInfo.sizeY[2])
            {
                top = new Viewport();
                top.X = viewportInfo.posX[2];
                top.Y = viewportInfo.posY[2];
                top.Width = viewportInfo.sizeX[2];
                top.Height = viewportInfo.sizeY[2];
                top.MinZ = 0;
                top.MaxZ = 1;

                viewportChange = true;
            }

            if(front.X != viewportInfo.posX[0] || front.Y != viewportInfo.posY[0] || front.Width != viewportInfo.sizeX[0] ||
                front.Height != viewportInfo.sizeY[0])
            {
                front = new Viewport();
                front.X = viewportInfo.posX[0];
                front.Y = viewportInfo.posY[0];
                front.Width = viewportInfo.sizeX[0];
                front.Height = viewportInfo.sizeY[0];
                front.MinZ = 0;
                front.MaxZ = 1;

                viewportChange = true;
            }

            if(side.X != viewportInfo.posX[1] || side.Y != viewportInfo.posY[1] || side.Width != viewportInfo.sizeX[1] ||
                side.Height != viewportInfo.sizeY[1])
            {
                side = new Viewport();
                side.X = viewportInfo.posX[1];
                side.Y = viewportInfo.posY[1];
                side.Width = viewportInfo.sizeX[1];
                side.Height = viewportInfo.sizeY[1];
                side.MinZ = 0;
                side.MaxZ = 1;

                viewportChange = true;
            }

            if(perspective.X != viewportInfo.posX[3] || perspective.Y != viewportInfo.posY[3] || perspective.Width != viewportInfo.sizeX[3] ||
                perspective.Height != viewportInfo.sizeY[3])
            {
                perspective = new Viewport();
                perspective.X = viewportInfo.posX[3];
                perspective.Y = viewportInfo.posY[3];
                perspective.Width = viewportInfo.sizeX[3];
                perspective.Height = viewportInfo.sizeY[3];
                perspective.MinZ = 0;
                perspective.MaxZ = 1;

                viewportChange = true;
            }

            if(perspective.Width > 0 && perspective.Height > 0)
            {
                persPos = new System.Drawing.Point(perspective.Width, perspective.Height);
            }

#if MEASURE_TIMES
            DateTime t2 = DateTime.Now;
            TimeSpan t = t2 - t1;
            w.WriteLine("Tworzenie device'a                          " + t.Milliseconds);
#endif

            //selectionTex = Texture.FromFile(device, "..\\..\\selectionTex.png");

#if MEASURE_TIMES
            t1 = DateTime.Now;
#endif
            device.SetRenderState(RenderState.Lighting, true);

            for(int i = scene.lights.Count; i < maxLights; ++i)
            {
                device.EnableLight(i, false);
            }
            maxLights = scene.lights.Count;

            int l = 0;
            foreach (Light_ light_ in scene.lights)
            {
                Light light = new Light();
                light.Diffuse = new Color4(light_.colorR, light_.colorG, light_.colorB);
                light.Position = light_.position;
                light.Range = 100000;
                light.Attenuation1 = 2.0f / light_.power;
                if(light_.type == Light_Type.Point || light_.type == Light_Type.Goniometric)
                {
                    light.Type = LightType.Point;
                }
                else
                {
                    light.Type = LightType.Spot;
                }
                if(light_.type == Light_Type.Spot)
                {
                    light.Direction = light_.direction;
                    light.Theta = Utilities.DegToRad(light_.innerAngle);
                    light.Phi = Utilities.DegToRad(light_.outerAngle);
                    light.Falloff = 1;
                }
                device.SetLight(l, light);
                device.EnableLight(l, true);
                ++l;
            }
#if MEASURE_TIMES
            t2 = DateTime.Now;
            t = t2 - t1;
            w.WriteLine("Tworzenie świateł                           " + t.Milliseconds);

            t1 = DateTime.Now;
#endif
            device.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);
            device.SetRenderState(RenderState.CullMode, Cull.None);
            device.SetRenderState(RenderState.ShadeMode, ShadeMode.Gouraud);
#if MEASURE_TIMES
            t2 = DateTime.Now;
            t = t2 - t1;
            w.WriteLine("SetRenderState                              " + t.Milliseconds);

            t1 = DateTime.Now;
#endif
            
            for(int i = 0; i < scene.points.Count; ++i)
            {
                if(selPoints[i] == true || scene.normals.Count <= i)
                {
                    Vector3D normal = new Vector3D();

                    foreach(int face in vertexTriangle[i])
                    {
                        normal += Utilities.CalculateNormal(scene.points[(int)scene.triangles[face].p3], scene.points[(int)scene.triangles[face].p2],
                            scene.points[(int)scene.triangles[face].p1]);
                    }

                    normal.Normalize();
                    if(scene.normals.Count <= i)
                    {
                        scene.normals.Add(normal);
                    }
                    else
                    {
                        scene.normals[i] = normal;
                    }

                    Vector3D point = scene.points[i];
                    vertices[i].Position = new Vector3(point.x, point.y, point.z);
                    vertices[i].Normal = new Vector3(scene.normals[i].x, scene.normals[i].y, scene.normals[i].z);
                }
            }
#if MEASURE_TIMES
            t2 = DateTime.Now;
            t = t2 - t1;
            w.WriteLine("Liczenie normalnych                         " + t.Milliseconds);

            t1 = DateTime.Now;
#endif
            Mesh mesh = numIndices >= 3 ? new Mesh(device, (int)numIndices / 3, scene.points.Count, MeshFlags.Managed | MeshFlags.Use32Bit, vertexElems) : null;
            VertexBuffer vb = mesh != null ? mesh.VertexBuffer : null;
            IndexBuffer ib = mesh != null ? mesh.IndexBuffer : null;
#if MEASURE_TIMES
            t2 = DateTime.Now;
            t = t2 - t1;
            w.WriteLine("Tworzenie mesh1                             " + t.Milliseconds);

            t1 = DateTime.Now;
#endif
            if (mesh != null)
            {
                vb.Lock(0, 0, LockFlags.None).WriteRange(vertices);
                vb.Unlock();

                ib.Lock(0, 0, LockFlags.None).WriteRange(indices, 0, (int)numIndices);
                ib.Unlock(); 
#if MEASURE_TIMES
                t2 = DateTime.Now;
                t = t2 - t1;
                w.WriteLine("Kopiowanie buforów mesh1                    " + t.Milliseconds);
#endif
            }

#if MEASURE_TIMES
            t1 = DateTime.Now;
#endif
            Mesh selMesh = numSelIndices >= 3 ? new Mesh(device, (int)numSelIndices / 3, scene.points.Count, MeshFlags.Managed | MeshFlags.Use32Bit, vertexElems) : null;
            VertexBuffer selvb = selMesh != null ? selMesh.VertexBuffer : null;
            IndexBuffer selib = selMesh != null ? selMesh.IndexBuffer : null;
#if MEASURE_TIMES
            t2 = DateTime.Now;
            t = t2 - t1;
            w.WriteLine("Tworzenie mesh2                              " + t.Milliseconds);

            t1 = DateTime.Now;
#endif
            if (selMesh != null)
            {
                selvb.Lock(0, 0, LockFlags.None).WriteRange(vertices);
                selvb.Unlock();

                selib.Lock(0, 0, LockFlags.None).WriteRange(selIndices, 0, (int)numSelIndices);
                selib.Unlock();
#if MEASURE_TIMES
                t2 = DateTime.Now;
                t = t2 - t1;
                w.WriteLine("Kopiowanie buforów mesh2                    " + t.Milliseconds);
#endif
            }

            while(cameras.Count < scene.cams.Count)
            {
                cameras.Add(new RenderCamera());
            }
            while(cameras.Count > scene.cams.Count)
            {
                cameras.RemoveAt(cameras.Count - 1);
            }

            while(camsLookAtPoints.Count < scene.cams.Count)
            {
                camsLookAtPoints.Add(new Vector3());
            }
            while(camsLookAtPoints.Count > scene.cams.Count)
            {
                camsLookAtPoints.RemoveAt(camsLookAtPoints.Count - 1);
            }

            for(int i = 0; i < cameras.Count; ++i)
            {
                if(cameras[i].set == false || viewportChange || cameras[i].position != scene.cams[i].position || cameras[i].lookAt != scene.cams[i].lookAt ||
                    cameras[i].fovAngle != scene.cams[i].fovAngle || cameras[i].rotateAngle != scene.cams[i].rotateAngle)
                {
                    if(cameras[i].set == false)
                    {
                        cameras[i].set = true;
                    }

                    cameras[i].position = scene.cams[i].position;
                    cameras[i].lookAt = scene.cams[i].lookAt;
                    cameras[i].fovAngle = scene.cams[i].fovAngle;
                    cameras[i].rotateAngle = scene.cams[i].rotateAngle;

                    Vector3 upLeft, upRight, loLeft, loRight;
                    SelectingElems.GetViewCorners(cameras[i].position, cameras[i].lookAt, cameras[i].fovAngle, cameras[i].rotateAngle,
                        persPos, out upLeft, out upRight, out loLeft, out loRight);

                    const float camViewLength = 2.0f;

                    Vector3 ld = loLeft - cameras[i].position;
                    ld.Normalize();
                    Vector3 lu = upLeft - cameras[i].position;
                    lu.Normalize();
                    Vector3 rd = loRight - cameras[i].position;
                    rd.Normalize();
                    Vector3 ru = upRight - cameras[i].position;
                    ru.Normalize();

                    cameras[i].points[0] = cameras[i].position;
                    /*cameras[i].points[1] = cameras[i].position + ((0.5f + (cameras[i].fovAngle / 180)) * camViewLength) * ld;
                    cameras[i].points[2] = cameras[i].position + ((0.5f + (cameras[i].fovAngle / 180)) * camViewLength) * lu;
                    cameras[i].points[3] = cameras[i].position + ((0.5f + (cameras[i].fovAngle / 180)) * camViewLength) * ru;
                    cameras[i].points[4] = cameras[i].position + ((0.5f + (cameras[i].fovAngle / 180)) * camViewLength) * rd;*/
                    cameras[i].points[1] = loLeft;
                    cameras[i].points[2] = upLeft;
                    cameras[i].points[3] = upRight;
                    cameras[i].points[4] = loRight;

                    Vector3 up = cameras[i].points[2] - cameras[i].points[1];
                    up.Normalize();
                    Vector3 left = cameras[i].points[2] - cameras[i].points[3];
                    left.Normalize();
                    Vector3 right = -left;

                    float dist = (float)Math.Sqrt((cameras[i].points[2].X - cameras[i].points[3].X) * (cameras[i].points[2].X - cameras[i].points[3].X) +
                                                  (cameras[i].points[2].Y - cameras[i].points[3].Y) * (cameras[i].points[2].Y - cameras[i].points[3].Y) +
                                                  (cameras[i].points[2].Z - cameras[i].points[3].Z) * (cameras[i].points[2].Z - cameras[i].points[3].Z));
                    dist *= 0.8f;

                    float upTriangleFactor = cameras[i].fovAngle / 60;

                    cameras[i].points[5] = (cameras[i].points[2] + cameras[i].points[3]) / 2 + left * upTriangleFactor * 0.3f * dist;
                    cameras[i].points[6] = (cameras[i].points[2] + cameras[i].points[3]) / 2 + up * upTriangleFactor * 0.25f * dist;
                    cameras[i].points[7] = (cameras[i].points[2] + cameras[i].points[3]) / 2 + right * upTriangleFactor * 0.3f * dist;

                    camsLookAtPoints[i] = (cameras[i].points[1] + cameras[i].points[2] + cameras[i].points[3] + cameras[i].points[4]) / 4;
                }
            }

            Vertex[] camVertices = new Vertex[cameras.Count * 8];
            int[] camIndices = new int[3 * cameras.Count * RenderCamera.triangles.Length];

            for(int i = 0; i < cameras.Count; ++i)
            {
                for(int j = 0; j < RenderCamera.triangles.Length; ++j)
                {
                    camIndices[i * 3 * RenderCamera.triangles.Length + 3 * j + 0] = i * cameras[i].points.Length + (int)RenderCamera.triangles[j].p1;
                    camIndices[i * 3 * RenderCamera.triangles.Length + 3 * j + 1] = i * cameras[i].points.Length + (int)RenderCamera.triangles[j].p2;
                    camIndices[i * 3 * RenderCamera.triangles.Length + 3 * j + 2] = i * cameras[i].points.Length + (int)RenderCamera.triangles[j].p3;
                }
            }

            bool[] selCameras = new bool[cameras.Count];
            for(int i = 0; i < selCameras.Length; ++i)
            {
                selCameras[i] = false;
            }
            for(int i = 0; i < scene.selCams.Count; ++i)
            {
                selCameras[scene.selCams[i]] = true;
            }

            for(int i = 0; i < cameras.Count; ++i)
            {
                for(int j = 0; j < cameras[i].points.Length; ++j)
                {
                    camVertices[i * 8 + j].Position = cameras[i].points[j];
                    camVertices[i * 8 + j].Normal = new Vector3();
                    if(selCameras[i] == false)
                    {
                        if(scene.activeCamera == i)
                        {
                            camVertices[i * 8 + j].Color = Color.FromArgb(249, 203, 44).ToArgb();
                        }
                        else
                        {
                            camVertices[i * 8 + j].Color = Color.FromArgb(193, 227, 195).ToArgb();
                        }
                    }
                    else
                    {
                        if(scene.activeCamera == i)
                        {
                            camVertices[i * 8 + j].Color = Color.FromArgb(243, 106, 24).ToArgb();
                        }
                        else
                        {
                            camVertices[i * 8 + j].Color = Color.FromArgb(255, 255, 150).ToArgb();
                        }
                    }
                    camVertices[i * 8 + j].tex0 = 0;
                    camVertices[i * 8 + j].tex1 = 0;
                }
            }

            Mesh camMesh = cameras.Count > 0 ? new Mesh(device, (int)camIndices.Length / 3, camVertices.Length, MeshFlags.Managed | MeshFlags.Use32Bit, vertexElems) : null;
            VertexBuffer camVB = camMesh != null ? camMesh.VertexBuffer : null;
            IndexBuffer camIB = camMesh != null ? camMesh.IndexBuffer : null;

            if(camMesh != null)
            {
                camVB.Lock(0, 0, LockFlags.None).WriteRange(camVertices);
                camVB.Unlock();

                camIB.Lock(0, 0, LockFlags.None).WriteRange(camIndices);
                camIB.Unlock();
            }

            while(lights.Count < scene.lights.Count)
            {
                lights.Add(new RenderLight());
            }
            while(lights.Count > scene.lights.Count)
            {
                lights.RemoveAt(lights.Count - 1);
            }

            for(int i = 0; i < lights.Count; ++i)
            {
                if(ortoWidhtChange == true || lights[i].set == false || lights[i].position != scene.lights[i].position || lights[i].direction != scene.lights[i].direction ||
                    lights[i].type != scene.lights[i].type || lights[i].innerAngle != scene.lights[i].innerAngle || lights[i].outerAngle != scene.lights[i].outerAngle)
                {
                    if(lights[i].set == false)
                    {
                        lights[i].set = true;
                    }

                    lights[i].position = scene.lights[i].position;
                    lights[i].direction = scene.lights[i].direction;
                    lights[i].type = scene.lights[i].type;
                    lights[i].innerAngle = scene.lights[i].innerAngle;
                    lights[i].outerAngle = scene.lights[i].outerAngle;

                    for(int j = 0; j < RenderLight.pointsDef.Length; ++j)
                    {
                        lights[i].points[j] = RenderLight.pointsDef[j];
                    }
                    for(int j = RenderLight.pointsDef.Length; j < RenderLight.pointsSpotNum; ++j)
                    {
                        lights[i].points[j] = new Vector3();
                    }

                    if(lights[i].type == Light_Type.Spot || lights[i].type == Light_Type.Goniometric)
                    {                      
                        Vector3 dir = lights[i].direction;
                        dir.Normalize();

                        if(dir.Z == 0)
                        {
                            dir.Z = 0.0001f;
                        }
                        Vector3 Vs = new Vector3(1, 1, -(dir.X + dir.Y) / dir.Z);

                        Vector3 V1 = Vector3.Normalize(Vs);
                        Vector3 V2 = Vector3.Cross(dir, V1);

                        Vector2[,] circle = new Vector2[2, 20];

                        float r1 = spotLightDist * (float)Math.Tan(Utilities.DegToRad(lights[i].innerAngle / 2));
                        float r2 = spotLightDist * (float)Math.Tan(Utilities.DegToRad(lights[i].outerAngle / 2));

                        for(int j = 0; j < 2; ++j)
                        {                           
                            for(int k = 0; k < 20; ++k)
                            {
                                circle[j, k] = new Vector2((j == 0 ? r1 : r2) * (float)Math.Sin(Utilities.DegToRad(18 * k)), 
                                                           (j == 0 ? r1 : r2) * (float)Math.Cos(Utilities.DegToRad(18 * k)));
                            }
                        }
                      
                        Vector3[,] circle3D = new Vector3[2, 20];
                        
                        for(int j = 0; j < 2; ++j)
                        {
                            for(int k = 0; k < 20; ++k)
                            {
                                float X = V1.X * circle[j, k].X + V2.X * circle[j, k].Y;
                                float Y = V1.Y * circle[j, k].X + V2.Y * circle[j, k].Y;
                                float Z = V1.Z * circle[j, k].X + V2.Z * circle[j, k].Y;

                                Vector3 radius = spotLightDist * dir;
                                circle3D[j, k] = new Vector3(X, Y, Z) + radius;
                            }
                        }

                        for(int j = 0; j < 20; ++j)
                        {
                            lights[i].points[RenderLight.pointsPointNum + 2 * j] = new Vector3(circle3D[0, j].X, circle3D[0, j].Y, circle3D[0, j].Z);
                            lights[i].points[RenderLight.pointsPointNum + 2 * j + 1] = new Vector3(circle3D[0, j].X + 0.000001f, circle3D[0, j].Y, circle3D[0, j].Z);

                            lights[i].points[RenderLight.pointsPointNum + 40 + 2 * j] = new Vector3(circle3D[1, j].X, circle3D[1, j].Y, circle3D[1, j].Z);
                            lights[i].points[RenderLight.pointsPointNum + 40 + 2 * j + 1] = new Vector3(circle3D[1, j].X + 0.000001f, circle3D[1, j].Y, circle3D[1, j].Z);
                        }

                        lights[i].points[120] = new Vector3(0, 0, 0);
                        lights[i].points[121] = new Vector3(0.000001f, 0, 0);
                    }

                    for(int j = 0; j < lights[i].points.Length; ++j)
                    {
                        lights[i].points[j] *= OrthoWidth[0] / 10;
                        lights[i].points[j] += lights[i].position;
                    } 
                }
            }

            ortoWidhtChange = false;

            Vertex[] lightsVertices = new Vertex[lights.Count * RenderLight.pointsSpotNum];
            int[] lightsIndices = new int[3 * lights.Count * RenderLight.triangles.Length];

            int trIndex = 0;
            for(int i = 0; i < lights.Count; ++i)
            {
                int trNum = lights[i].type == Light_Type.Point ? RenderLight.trianglesPointNum : RenderLight.trianglesSpotNum;
                for(int j = 0; j < trNum; ++j)
                {
                    lightsIndices[trIndex++] = i * lights[i].points.Length + (int)RenderLight.triangles[j].p1;
                    lightsIndices[trIndex++] = i * lights[i].points.Length + (int)RenderLight.triangles[j].p2;
                    lightsIndices[trIndex++] = i * lights[i].points.Length + (int)RenderLight.triangles[j].p3;
                }
            }

            bool[] selLights = new bool[lights.Count];
            for(int i = 0; i < selLights.Length; ++i)
            {
                selLights[i] = false;
            }
            for(int i = 0; i < scene.selLights.Count; ++i)
            {
                selLights[scene.selLights[i]] = true;
            }

            for(int i = 0; i < lights.Count; ++i)
            {
                for(int j = 0; j < lights[i].points.Length; ++j)
                {
                    lightsVertices[i * RenderLight.pointsSpotNum + j].Position = lights[i].points[j];
                    lightsVertices[i * RenderLight.pointsSpotNum + j].Normal = new Vector3();
                    if(selLights[i] == false)
                    {
                        if(scene.lights[i].enabled == true)
                        {
                            if(scene.lights[i].type != Light_Type.Goniometric)
                            {
                                lightsVertices[i * RenderLight.pointsSpotNum + j].Color = Color.FromArgb(234, 203, 28).ToArgb();
                            }
                            else
                            {
                                lightsVertices[i * RenderLight.pointsSpotNum + j].Color = Color.FromArgb(123, 120, 46).ToArgb();
                            }
                        }
                        else
                        {
                            if(scene.lights[i].type != Light_Type.Goniometric)
                            {
                                lightsVertices[i * RenderLight.pointsSpotNum + j].Color = Color.FromArgb(48, 46, 54).ToArgb();
                            }
                            else
                            {
                                lightsVertices[i * RenderLight.pointsSpotNum + j].Color = Color.FromArgb(67, 78, 63).ToArgb();
                            }
                        }
                    }
                    else
                    {
                        if(scene.lights[i].enabled == true)
                        {
                            if(scene.lights[i].type != Light_Type.Goniometric)
                            {
                                lightsVertices[i * RenderLight.pointsSpotNum + j].Color = Color.FromArgb(243, 106, 24).ToArgb();
                            }
                            else
                            {
                                lightsVertices[i * RenderLight.pointsSpotNum + j].Color = Color.FromArgb(159, 210, 4).ToArgb();
                            }
                        }
                        else
                        {
                            if(scene.lights[i].type != Light_Type.Goniometric)
                            {
                                lightsVertices[i * RenderLight.pointsSpotNum + j].Color = Color.FromArgb(19, 24, 224).ToArgb();
                            }
                            else
                            {
                                lightsVertices[i * RenderLight.pointsSpotNum + j].Color = Color.FromArgb(19, 224, 96).ToArgb();
                            }
                        }
                    }
                    lightsVertices[i * RenderLight.pointsSpotNum + j].tex0 = 0;
                    lightsVertices[i * RenderLight.pointsSpotNum + j].tex1 = 0;
                }
            }

            Mesh lightsMesh = lights.Count > 0 ? new Mesh(device, trIndex / 3, lightsVertices.Length, MeshFlags.Managed | MeshFlags.Use32Bit, vertexElems) : null;
            VertexBuffer lightsVB = lightsMesh != null ? lightsMesh.VertexBuffer : null;
            IndexBuffer lightsIB = lightsMesh != null ? lightsMesh.IndexBuffer : null;

            if(lightsMesh != null)
            {
                lightsVB.Lock(0, 0, LockFlags.None).WriteRange(lightsVertices);
                lightsVB.Unlock();

                lightsIB.Lock(0, 0, LockFlags.None).WriteRange(lightsIndices, 0, trIndex);
                lightsIB.Unlock();
            }

            int coneLights = 0;
            foreach(Light_ light in scene.lights)
            {
                if(light.type == Light_Type.Spot || light.type == Light_Type.Goniometric)
                {
                    ++coneLights;
                }
            }
            Mesh pointsMesh = Mesh.CreateBox(device, pointSize, pointSize, pointSize);

            Mesh clipMeshX = null;
            Mesh clipMeshY = null;
            Mesh clipMeshZ = null;
            VertexBuffer clipVBX = null;
            IndexBuffer clipIBX = null;
            VertexBuffer clipVBY = null;
            IndexBuffer clipIBY = null;
            VertexBuffer clipVBZ = null;
            IndexBuffer clipIBZ = null;

            if(clipping == true)
            {
                clipMeshX = new Mesh(device, clipIndices.Length / 9, clipVertices.Length, MeshFlags.Managed | MeshFlags.Use32Bit, vertexElems);
                clipMeshY = new Mesh(device, clipIndices.Length / 9, clipVertices.Length, MeshFlags.Managed | MeshFlags.Use32Bit, vertexElems);
                clipMeshZ = new Mesh(device, clipIndices.Length / 9, clipVertices.Length, MeshFlags.Managed | MeshFlags.Use32Bit, vertexElems);

                clipVBX = clipMeshX.VertexBuffer;
                clipIBX = clipMeshX.IndexBuffer;

                clipVBX.Lock(0, 0, LockFlags.None).WriteRange(clipVertices);
                clipVBX.Unlock();

                clipIBX.Lock(0, 0, LockFlags.None).WriteRange(clipIndices, (clipIndices.Length / 6) * 2, (clipIndices.Length / 6) * 2);
                clipIBX.Unlock();

                clipVBY = clipMeshY.VertexBuffer;
                clipIBY = clipMeshY.IndexBuffer;

                clipVBY.Lock(0, 0, LockFlags.None).WriteRange(clipVertices);
                clipVBY.Unlock();

                clipIBY.Lock(0, 0, LockFlags.None).WriteRange(clipIndices, 0, (clipIndices.Length / 6) * 2);
                clipIBY.Unlock();

                clipVBZ = clipMeshZ.VertexBuffer;
                clipIBZ = clipMeshZ.IndexBuffer;

                clipVBZ.Lock(0, 0, LockFlags.None).WriteRange(clipVertices);
                clipVBZ.Unlock();

                clipIBZ.Lock(0, 0, LockFlags.None).WriteRange(clipIndices, (clipIndices.Length / 6) * 4, (clipIndices.Length / 6) * 2);
                clipIBZ.Unlock();
            }

#if MEASURE_TIMES
            t1 = DateTime.Now;
#endif
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

#if MEASURE_TIMES
            t2 = DateTime.Now;
            t = t2 - t1;
            w.WriteLine("Modyfikacja viewport'ów                     " + t.Milliseconds);

            t1 = DateTime.Now;
#endif
            if (perspective.Width > 0 && perspective.Height > 0)
            {
                device.Viewport = perspective;

                float camRotAngle = scene.cams.ElementAt(scene.activeCamera).rotateAngle;
                float aspect = (float)perspective.Width / perspective.Height;
                float angle = 2.0f * (float)Math.Atan(Math.Tan(Utilities.DegToRad(scene.cams.ElementAt(scene.activeCamera).fovAngle) / 2.0f) / aspect);

                device.SetTransform(TransformState.View, Matrix.LookAtRH(
                    scene.cams[scene.activeCamera].position,
                    scene.cams[scene.activeCamera].lookAt,
                    Utilities.RotatePointAroundVector(new Vector3(0, 1, 0),
                    Vector3.Normalize(scene.cams[scene.activeCamera].lookAt - scene.cams[scene.activeCamera].position), camRotAngle)));

                device.SetTransform(TransformState.Projection, Matrix.PerspectiveFovRH(
                    angle,
                    aspect,
                    0.01f,
                    110000));

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(230, 230, 230), 1.0f, 0);
                device.BeginScene();

                if (mesh != null)
                {
                    mesh.DrawSubset(0);
                }

                //device.SetTexture(0, selectionTex);
                device.SetRenderState(RenderState.Lighting, false);
                if (selMesh != null)
                {
                    selMesh.DrawSubset(0);
                }
                device.SetRenderState(RenderState.Lighting, true);
                //device.SetTexture(0, null);

                device.EndScene();
            }

            SlimDX.Plane clipPlaneX = new SlimDX.Plane();
            SlimDX.Plane clipPlaneX2 = new SlimDX.Plane();
            SlimDX.Plane clipPlaneY = new SlimDX.Plane();
            SlimDX.Plane clipPlaneY2 = new SlimDX.Plane();
            SlimDX.Plane clipPlaneZ = new SlimDX.Plane();
            SlimDX.Plane clipPlaneZ2 = new SlimDX.Plane();

            SlimDX.Plane clipPlaneXPlus = new SlimDX.Plane();
            SlimDX.Plane clipPlaneX2Minus = new SlimDX.Plane();
            SlimDX.Plane clipPlaneYPlus = new SlimDX.Plane();
            SlimDX.Plane clipPlaneY2Minus = new SlimDX.Plane();
            SlimDX.Plane clipPlaneZPlus = new SlimDX.Plane();
            SlimDX.Plane clipPlaneZ2Minus = new SlimDX.Plane();

            clipPlaneYPlus = new SlimDX.Plane(new Vector3(0, 50000, 0), new Vector3(0, -1, 0));
            clipPlaneY2Minus = new SlimDX.Plane(new Vector3(0, -50000, 0), new Vector3(0, 1, 0));
            clipPlaneXPlus = new SlimDX.Plane(new Vector3(50000, 0, 0), new Vector3(-1, 0, 0));
            clipPlaneX2Minus = new SlimDX.Plane(new Vector3(-50000, 0, 0), new Vector3(1, 0, 0));
            clipPlaneZPlus = new SlimDX.Plane(new Vector3(0, 0, -50000), new Vector3(0, 0, 1));
            clipPlaneZ2Minus = new SlimDX.Plane(new Vector3(0, 0, 50000), new Vector3(0, 0, -1));

            if(clipping == true)
            {
                clipPlaneY =  new SlimDX.Plane(new Vector3(0, clipVertices[0].Position.Y, 0), new Vector3(0, -1, 0));
                clipPlaneY2 = new SlimDX.Plane(new Vector3(0, clipVertices[8].Position.Y, 0), new Vector3(0, 1, 0));
                clipPlaneX =  new SlimDX.Plane(new Vector3(clipVertices[16].Position.X, 0, 0), new Vector3(-1, 0, 0));
                clipPlaneX2 = new SlimDX.Plane(new Vector3(clipVertices[24].Position.X, 0, 0), new Vector3(1, 0, 0));
                clipPlaneZ =  new SlimDX.Plane(new Vector3(0, 0, clipVertices[32].Position.Z), new Vector3(0, 0, 1));
                clipPlaneZ2 = new SlimDX.Plane(new Vector3(0, 0, clipVertices[40].Position.Z), new Vector3(0, 0, -1));

                device.SetRenderState(RenderState.ClipPlaneEnable, ClipFlags.Bottom | ClipFlags.Front | ClipFlags.Left | ClipFlags.Right |
                    ClipFlags.Back | ClipFlags.Top);
            }

            if (top.Width > 0 && top.Height > 0)
            {
                device.Viewport = top;

                device.SetTransform(TransformState.View, Matrix.LookAtRH(
                   orthoPos[2],
                   orthoLookAt[2],
                   new Vector3(0, 1, 0)));

                device.SetTransform(TransformState.Projection, Matrix.OrthoRH(
                    orthoWidth[2],
                    (float)(top.Height) / top.Width * orthoWidth[2],
                    0.01f,
                    110000));

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(230, 230, 230), 1.0f, 0);
                device.BeginScene();

                string text1 = " -z\n/|\\\n |\n";
                font.DrawString(null, text1, viewportInfo.posX[2] + 15, viewportInfo.posY[2], Color.Blue);
                string text2 = "\n\n\n  -----> x";
                font.DrawString(null, text2, viewportInfo.posX[2] + 15, viewportInfo.posY[2], Color.Red);

                if(clipping == true)
                {
                    device.SetRenderState(RenderState.Lighting, false);
                    device.SetRenderState(RenderState.FillMode, FillMode.Solid);
                    clipMeshX.DrawSubset(0);
                    clipMeshZ.DrawSubset(0);
                    device.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);
                    device.SetRenderState(RenderState.Lighting, true);

                    device.SetClipPlane(0, clipPlaneX);
                    device.SetClipPlane(1, clipPlaneX2);
                    device.SetClipPlane(2, clipPlaneY);
                    device.SetClipPlane(3, clipPlaneY2);
                    device.SetClipPlane(4, clipPlaneZ);
                    device.SetClipPlane(5, clipPlaneZ2);
                }

                if (mesh != null)
                {
                    mesh.DrawSubset(0);
                }

                //device.SetTexture(0, selectionTex);
                device.SetRenderState(RenderState.Lighting, false);
                if (selMesh != null)
                {
                    selMesh.DrawSubset(0);
                }
                device.SetRenderState(RenderState.Lighting, true);
                //device.SetTexture(0, null);
                                              
                device.SetClipPlane(0, clipPlaneXPlus);
                device.SetClipPlane(1, clipPlaneX2Minus);
                device.SetClipPlane(2, clipPlaneYPlus);
                device.SetClipPlane(3, clipPlaneY2Minus);
                device.SetClipPlane(4, clipPlaneZPlus);
                device.SetClipPlane(5, clipPlaneZ2Minus);

                device.SetRenderState(RenderState.FillMode, FillMode.Solid);
                for(int i = 0; i < scene.lights.Count; ++i)
                {
                    if(scene.lights[i].type == Light_Type.Spot || scene.lights[i].type == Light_Type.Goniometric)
                    {
                        device.SetTransform(TransformState.World, Matrix.Scaling(orthoWidth[2] / 10, orthoWidth[0] / 10, orthoWidth[2] / 10) *
                                            Matrix.Translation(scene.lights[i].position + scene.lights[i].direction * spotLightDist * orthoWidth[1] / 10));
                        pointsMesh.DrawSubset(0);
                    }
                }
                for(int i = 0; i < scene.cams.Count; ++i)
                {
                    device.SetTransform(TransformState.World, Matrix.Scaling(orthoWidth[2] / 10, orthoWidth[2] / 10, orthoWidth[2] / 10) *
                        Matrix.Translation(scene.cams[i].position));
                    pointsMesh.DrawSubset(0);

                    device.SetTransform(TransformState.World, Matrix.Scaling(orthoWidth[2] / 10, orthoWidth[2] / 10, orthoWidth[2] / 10) *
                        Matrix.Translation(camsLookAtPoints[i]));
                    pointsMesh.DrawSubset(0);
                }
                device.SetTransform(TransformState.World, Matrix.Identity);
                device.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);

                if(camMesh != null)
                {
                    device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);
                    device.SetRenderState(RenderState.Lighting, false);
                    camMesh.DrawSubset(0);
                    device.SetRenderState(RenderState.Lighting, true);
                    device.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);
                }
                if(lightsMesh != null)
                {
                    device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);
                    device.SetRenderState(RenderState.Lighting, false);
                    lightsMesh.DrawSubset(0);
                    device.SetRenderState(RenderState.Lighting, true);
                    device.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);
                }

                device.EndScene();
            }

            if (front.Width > 0 && front.Height > 0)
            {
                device.Viewport = front;

                device.SetTransform(TransformState.View, Matrix.LookAtRH(
                   orthoPos[0],
                   orthoLookAt[0],
                   new Vector3(0, 1, 0)));

                device.SetTransform(TransformState.Projection, Matrix.OrthoRH(
                    orthoWidth[0],
                    (float)(front.Height) / front.Width * orthoWidth[0],
                    0.01f,
                    110000));

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(230, 230, 230), 1.0f, 0);
                device.BeginScene();

                string text1 = " y\n/|\\\n |\n";
                font.DrawString(null, text1, viewportInfo.posX[0] + 15, viewportInfo.posY[0], Color.Green);
                string text2 = "\n\n\n  -----> x";
                font.DrawString(null, text2, viewportInfo.posX[0] + 15, viewportInfo.posY[0], Color.Red);

                if(clipping == true)
                {
                    device.SetRenderState(RenderState.Lighting, false);
                    device.SetRenderState(RenderState.FillMode, FillMode.Solid);
                    clipMeshX.DrawSubset(0);
                    clipMeshY.DrawSubset(0);
                    device.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);
                    device.SetRenderState(RenderState.Lighting, true);

                    device.SetClipPlane(0, clipPlaneX);
                    device.SetClipPlane(1, clipPlaneX2);
                    device.SetClipPlane(2, clipPlaneY);
                    device.SetClipPlane(3, clipPlaneY2);
                    device.SetClipPlane(4, clipPlaneZ);
                    device.SetClipPlane(5, clipPlaneZ2);
                }

                if (mesh != null)
                {
                    mesh.DrawSubset(0);
                }

                //device.SetTexture(0, selectionTex);
                device.SetRenderState(RenderState.Lighting, false);
                if (selMesh != null)
                {
                    selMesh.DrawSubset(0);
                }
                device.SetRenderState(RenderState.Lighting, true);
                //device.SetTexture(0, null);
                                            
                device.SetClipPlane(0, clipPlaneXPlus);
                device.SetClipPlane(1, clipPlaneX2Minus);
                device.SetClipPlane(2, clipPlaneYPlus);
                device.SetClipPlane(3, clipPlaneY2Minus);
                device.SetClipPlane(4, clipPlaneZPlus);
                device.SetClipPlane(5, clipPlaneZ2Minus);

                device.SetRenderState(RenderState.FillMode, FillMode.Solid);
                for(int i = 0; i < scene.lights.Count; ++i)
                {
                    if(scene.lights[i].type == Light_Type.Spot || scene.lights[i].type == Light_Type.Goniometric)
                    {
                        device.SetTransform(TransformState.World, Matrix.Scaling(orthoWidth[0] / 10, orthoWidth[0] / 10, orthoWidth[0] / 10) *
                                            Matrix.Translation(scene.lights[i].position + scene.lights[i].direction * spotLightDist * orthoWidth[0] / 10));
                        pointsMesh.DrawSubset(0);
                    }
                }
                for(int i = 0; i < scene.cams.Count; ++i)
                {
                    device.SetTransform(TransformState.World, Matrix.Scaling(orthoWidth[0] / 10, orthoWidth[0] / 10, orthoWidth[0] / 10) *
                        Matrix.Translation(scene.cams[i].position));
                    pointsMesh.DrawSubset(0);

                    device.SetTransform(TransformState.World, Matrix.Scaling(orthoWidth[0] / 10, orthoWidth[0] / 10, orthoWidth[0] / 10) *
                        Matrix.Translation(camsLookAtPoints[i]));
                    pointsMesh.DrawSubset(0);
                }
                device.SetTransform(TransformState.World, Matrix.Identity);
                device.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);

                if(camMesh != null)
                {
                    device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);
                    device.SetRenderState(RenderState.Lighting, false);
                    camMesh.DrawSubset(0);
                    device.SetRenderState(RenderState.Lighting, true);
                    device.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);
                }
                if(lightsMesh != null)
                {
                    device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);
                    device.SetRenderState(RenderState.Lighting, false);
                    lightsMesh.DrawSubset(0);
                    device.SetRenderState(RenderState.Lighting, true);
                    device.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);
                }

                device.EndScene();
            }

            if (side.Width > 0 && side.Height > 0)
            {
                device.Viewport = side;   

                device.SetTransform(TransformState.View, Matrix.LookAtRH(
                   orthoPos[1],
                   orthoLookAt[1],
                   new Vector3(0, 1, 0)));

                device.SetTransform(TransformState.Projection, Matrix.OrthoRH(
                    orthoWidth[1],
                    (float)(side.Height) / side.Width * orthoWidth[1],
                    0.01f,
                    110000));

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(230, 230, 230), 1.0f, 0);
                device.BeginScene();

                string text1 = " y\n/|\\\n |\n";
                font.DrawString(null, text1, viewportInfo.posX[1] + 15, viewportInfo.posY[1], Color.Green);
                string text2 = "\n\n\n  -----> -z";
                font.DrawString(null, text2, viewportInfo.posX[1] + 15, viewportInfo.posY[1], Color.Blue);

                if(clipping == true)
                {
                    device.SetRenderState(RenderState.Lighting, false);
                    device.SetRenderState(RenderState.FillMode, FillMode.Solid);
                    clipMeshY.DrawSubset(0);
                    clipMeshZ.DrawSubset(0);
                    device.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);
                    device.SetRenderState(RenderState.Lighting, true);

                    device.SetClipPlane(0, clipPlaneX);
                    device.SetClipPlane(1, clipPlaneX2);
                    device.SetClipPlane(2, clipPlaneY);
                    device.SetClipPlane(3, clipPlaneY2);
                    device.SetClipPlane(4, clipPlaneZ);
                    device.SetClipPlane(5, clipPlaneZ2);
                }

                if (mesh != null)
                {
                    mesh.DrawSubset(0);
                }

                //device.SetTexture(0, selectionTex);
                device.SetRenderState(RenderState.Lighting, false);
                if (selMesh != null)
                {
                    selMesh.DrawSubset(0);
                }
                device.SetRenderState(RenderState.Lighting, true);
                //device.SetTexture(0, null);

                device.SetClipPlane(0, clipPlaneXPlus);
                device.SetClipPlane(1, clipPlaneX2Minus);
                device.SetClipPlane(2, clipPlaneYPlus);
                device.SetClipPlane(3, clipPlaneY2Minus);
                device.SetClipPlane(4, clipPlaneZPlus);
                device.SetClipPlane(5, clipPlaneZ2Minus);

                device.SetRenderState(RenderState.FillMode, FillMode.Solid);
                for(int i = 0; i < scene.lights.Count; ++i)
                {
                    if(scene.lights[i].type == Light_Type.Spot || scene.lights[i].type == Light_Type.Goniometric)
                    {
                        device.SetTransform(TransformState.World, Matrix.Scaling(orthoWidth[1] / 10, orthoWidth[0] / 10, orthoWidth[1] / 10) *
                                            Matrix.Translation(scene.lights[i].position + scene.lights[i].direction * spotLightDist * orthoWidth[1] / 10));
                        pointsMesh.DrawSubset(0);
                    }
                }
                for(int i = 0; i < scene.cams.Count; ++i)
                {
                    device.SetTransform(TransformState.World, Matrix.Scaling(orthoWidth[1] / 10, orthoWidth[1] / 10, orthoWidth[1] / 10) *
                        Matrix.Translation(scene.cams[i].position));
                    pointsMesh.DrawSubset(0);

                    device.SetTransform(TransformState.World, Matrix.Scaling(orthoWidth[1] / 10, orthoWidth[1] / 10, orthoWidth[1] / 10) *
                        Matrix.Translation(camsLookAtPoints[i]));
                    pointsMesh.DrawSubset(0);
                }
                device.SetTransform(TransformState.World, Matrix.Identity);
                device.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);

                if(camMesh != null)
                {
                    device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);
                    device.SetRenderState(RenderState.Lighting, false);
                    camMesh.DrawSubset(0);
                    device.SetRenderState(RenderState.Lighting, true);
                    device.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);
                }
                if(lightsMesh != null)
                {
                    device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);
                    device.SetRenderState(RenderState.Lighting, false);
                    lightsMesh.DrawSubset(0);
                    device.SetRenderState(RenderState.Lighting, true);
                    device.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);
                }

                device.EndScene();
            }
#if MEASURE_TIMES
            t2 = DateTime.Now;
            t = t2 - t1;
            w.WriteLine("Renderowanie                               " + t.Milliseconds);

            t1 = DateTime.Now;
#endif
            device.Present();

#if MEASURE_TIMES
            t2 = DateTime.Now;
            t = t2 - t1;
            w.WriteLine("Present                                    " + t.Milliseconds);

            t1 = DateTime.Now;
#endif
            //selectionTex.Dispose();
            if (selMesh != null)
            {
                selvb.Dispose();
                selib.Dispose();
                selMesh.Dispose();
            }
            if (mesh != null)
            {
                vb.Dispose();
                ib.Dispose();
                mesh.Dispose();
            }
            if(camMesh != null)
            {
                camVB.Dispose();
                camIB.Dispose();
                camMesh.Dispose();
            }
            if(lightsMesh != null)
            {
                lightsVB.Dispose();
                lightsIB.Dispose();
                lightsMesh.Dispose();
            }
            if(clipping == true)
            {
                clipVBX.Dispose();
                clipIBX.Dispose();
                clipVBY.Dispose();
                clipIBY.Dispose();
                clipVBZ.Dispose();
                clipIBZ.Dispose();
                clipMeshX.Dispose();
                clipMeshY.Dispose();
                clipMeshZ.Dispose();
            }
            pointsMesh.Dispose();

#if MEASURE_TIMES
            t2 = DateTime.Now;
            t = t2 - t1;
            w.WriteLine("Czyszczenie                              " + t.Milliseconds);

            DateTime te = DateTime.Now;
            t = te - ts;
            w.WriteLine("Całość                             " + t.Milliseconds);
            w.WriteLine("-----------------------------------------------------------------");
            w.WriteLine();
            w.WriteLine();
            w.WriteLine();
#endif
        }

        public Image GetBezierImage(BezierSurface bezier)
        {
            pp = new PresentParameters();
            pp.SwapEffect = SwapEffect.Discard;
            pp.Windowed = true;
            pp.BackBufferWidth = 512;
            pp.BackBufferHeight = 512;
            pp.BackBufferFormat = Format.A8R8G8B8;

            if (d3dBezier != null)
            {
                d3dBezier.Dispose();
            }
            if (deviceBezier != null)
            {
                deviceBezier.Dispose();
            }

            d3dBezier = new Direct3D();
            deviceBezier = new Device(d3d, 0, DeviceType.Hardware, handle, CreateFlags.HardwareVertexProcessing, pp);

            deviceBezier.SetRenderState(RenderState.Lighting, true);

            deviceBezier.SetLight(0, defLight);
            deviceBezier.EnableLight(0, true);

            deviceBezier.SetRenderState(RenderState.FillMode, FillMode.Solid);
            deviceBezier.SetRenderState(RenderState.CullMode, Cull.None);
            deviceBezier.SetRenderState(RenderState.ShadeMode, ShadeMode.Gouraud);

            Vertex[] gridVertices = new Vertex[bezier.OutputPoints.Length];
            Vertex[] controlVertices = new Vertex[bezier.ControlPoints.Length];

            List<int>[] vertexTriangle = new List<int>[bezier.OutputPoints.Length];
            Parallel.For(0, vertexTriangle.Length, index => vertexTriangle[index] = new List<int>());
            /*for(int i = 0; i < vertexTriangle.Length; ++i)
            {
                vertexTriangle[i] = new List<int>();
            }*/

            int[] indices = new int[3 * bezier.triangles.Count];
            //int[] selIndices = new int[3 * scene.triangles.Count];
            uint numIndices = 0;
            //uint numSelIndices = 0;

            //bool[] selPoints = new bool[scene.points.Count];
            //Parallel.For(0, selPoints.Length, index => selPoints[index] = false); 

            for (int i = 0; i < bezier.triangles.Count; i++)
            {
                indices[numIndices++] = (int)bezier.triangles[i].p1;
                indices[numIndices++] = (int)bezier.triangles[i].p2;
                indices[numIndices++] = (int)bezier.triangles[i].p3;

                vertexTriangle[bezier.triangles[i].p1].Add(i);
                vertexTriangle[bezier.triangles[i].p2].Add(i);
                vertexTriangle[bezier.triangles[i].p3].Add(i);
            }

            // Liczenie normalnych siatki trojkątów
            for (int i = 0; i < bezier.OutputPoints.Length; i++)
            {
                Vector3 normal = new Vector3();
                foreach (int face in vertexTriangle[i])
                {
                    normal += Utilities.CalculateNormal(bezier.OutputPoints[(int)bezier.triangles[face].p3], bezier.OutputPoints[(int)bezier.triangles[face].p2],
                            bezier.OutputPoints[(int)bezier.triangles[face].p1]);
                }
                normal.Normalize();

                gridVertices[i].Position = new Vector3(bezier.OutputPoints[i].x, bezier.OutputPoints[i].y, bezier.OutputPoints[i].z);
                gridVertices[i].Normal = normal;
                gridVertices[i].Color = Color.Beige.ToArgb();

            }

            Mesh gridMesh = numIndices > 2 ? new Mesh(deviceBezier, (int)numIndices / 3, bezier.OutputPoints.Length, MeshFlags.Managed | MeshFlags.Use32Bit,
                                                        vertexElems) : null;
            VertexBuffer vb = gridMesh != null ? gridMesh.VertexBuffer : null;
            IndexBuffer ib = gridMesh != null ? gridMesh.IndexBuffer : null;


            if (gridMesh != null)
            {
                vb.Lock(0, 0, LockFlags.None).WriteRange(gridVertices);
                vb.Unlock();

                ib.Lock(0, 0, LockFlags.None).WriteRange(indices, 0, (int)numIndices);
                ib.Unlock();
            }

            Viewport viewport = new Viewport(0, 0, 512, 512, 0, 1);

            Texture texture = new Texture(deviceBezier, 64, 64, 0, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            deviceBezier.SetRenderTarget(0, texture.GetSurfaceLevel(0));

            float aspect = (float)perspective.Width / perspective.Height;

            deviceBezier.SetTransform(TransformState.View, Matrix.LookAtRH(
                bezierCam.position,
                bezierCam.lookAt,
                Utilities.RotatePointAroundVector(new Vector3(0, 1, 0),
                Vector3.Normalize(bezierCam.lookAt - bezierCam.position), bezierCam.rotateAngle)));

            deviceBezier.SetTransform(TransformState.Projection, Matrix.PerspectiveFovRH(
                bezierCam.fovAngle,
                aspect,
                0.01f,
                110000));


            deviceBezier.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.White, 1.0f, 0);
            deviceBezier.BeginScene();

            if (gridMesh != null)
            {
                gridMesh.DrawSubset(0);
            }

            deviceBezier.EndScene();

            Image image = Bitmap.FromStream(Texture.ToStream(texture, ImageFileFormat.Png));

            texture.Dispose();

            bezierImageCreated = true;

            return image;
        }

        public void RenderBezier(ViewportInfo viewportInfo, BezierSurface bezier)
        {
            if (bezierPp.BackBufferWidth != viewportInfo.resX || bezierPp.BackBufferHeight != viewportInfo.resY || bezierImageCreated)
            {
                bezierPp = new PresentParameters();
                bezierPp.SwapEffect = SwapEffect.Discard;
                bezierPp.Windowed = true;
                bezierPp.BackBufferWidth = viewportInfo.resX;
                bezierPp.BackBufferHeight = viewportInfo.resY;
                bezierPp.BackBufferFormat = Format.A8R8G8B8;

                if (d3dBezier != null)
                {
                    d3dBezier.Dispose();
                }
                if (deviceBezier != null)
                {
                    deviceBezier.Dispose();
                }

                bezierImageCreated = false;

                d3dBezier = new Direct3D();
                deviceBezier = new Device(d3dBezier, 0, DeviceType.Hardware, handleBezier, CreateFlags.HardwareVertexProcessing, bezierPp);

                if(font2 != null)
                {
                    font2.Dispose();
                }
                font2 = new SlimDX.Direct3D9.Font(deviceBezier, new System.Drawing.Font(FontFamily.GenericSansSerif, 7));
            }

            if (top.X != viewportInfo.posX[2] || top.Y != viewportInfo.posY[2] || top.Width != viewportInfo.sizeX[2] ||
                top.Height != viewportInfo.sizeY[2])
            {
                top = new Viewport();
                top.X = viewportInfo.posX[2];
                top.Y = viewportInfo.posY[2];
                top.Width = viewportInfo.sizeX[2];
                top.Height = viewportInfo.sizeY[2];
                top.MinZ = 0;
                top.MaxZ = 1;
            }

            if (front.X != viewportInfo.posX[0] || front.Y != viewportInfo.posY[0] || front.Width != viewportInfo.sizeX[0] ||
                front.Height != viewportInfo.sizeY[0])
            {
                front = new Viewport();
                front.X = viewportInfo.posX[0];
                front.Y = viewportInfo.posY[0];
                front.Width = viewportInfo.sizeX[0];
                front.Height = viewportInfo.sizeY[0];
                front.MinZ = 0;
                front.MaxZ = 1;
            }

            if (side.X != viewportInfo.posX[1] || side.Y != viewportInfo.posY[1] || side.Width != viewportInfo.sizeX[1] ||
                side.Height != viewportInfo.sizeY[1])
            {
                side = new Viewport();
                side.X = viewportInfo.posX[1];
                side.Y = viewportInfo.posY[1];
                side.Width = viewportInfo.sizeX[1];
                side.Height = viewportInfo.sizeY[1];
                side.MinZ = 0;
                side.MaxZ = 1;
            }

            if(perspective.X != viewportInfo.posX[3] || perspective.Y != viewportInfo.posY[3] || perspective.Width != viewportInfo.sizeX[3] ||
                perspective.Height != viewportInfo.sizeY[3])
            {
                perspective = new Viewport();
                perspective.X = viewportInfo.posX[3];
                perspective.Y = viewportInfo.posY[3];
                perspective.Width = viewportInfo.sizeX[3];
                perspective.Height = viewportInfo.sizeY[3];
                perspective.MinZ = 0;
                perspective.MaxZ = 1;
            }

            deviceBezier.SetRenderState(RenderState.Lighting, true);

            deviceBezier.SetLight(0, defLight);
            deviceBezier.EnableLight(0, true);

            deviceBezier.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);
            deviceBezier.SetRenderState(RenderState.CullMode, Cull.None);

            deviceBezier.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);


            Vertex[] gridVertices = new Vertex[bezier.OutputPoints.Length];
            Vertex[] controlVertices = new Vertex[bezier.ControlPoints.Length];

            List<int>[] vertexTriangle = new List<int>[bezier.OutputPoints.Length];
            Parallel.For(0, vertexTriangle.Length, index => vertexTriangle[index] = new List<int>());
            /*for(int i = 0; i < vertexTriangle.Length; ++i)
            {
                vertexTriangle[i] = new List<int>();
            }*/

            int[] indices = new int[3 * bezier.triangles.Count];
            //int[] selIndices = new int[3 * scene.triangles.Count];
            uint numIndices = 0;
            //uint numSelIndices = 0;

            //bool[] selPoints = new bool[scene.points.Count];
            //Parallel.For(0, selPoints.Length, index => selPoints[index] = false); 

            for (int i = 0; i < bezier.triangles.Count; i++)
            {
                indices[numIndices++] = (int)bezier.triangles[i].p1;
                indices[numIndices++] = (int)bezier.triangles[i].p2;
                indices[numIndices++] = (int)bezier.triangles[i].p3;

                vertexTriangle[bezier.triangles[i].p1].Add(i);
                vertexTriangle[bezier.triangles[i].p2].Add(i);
                vertexTriangle[bezier.triangles[i].p3].Add(i);
            }

            // Liczenie normalnych siatki trojkątów
            for (int i = 0; i < bezier.OutputPoints.Length; i++)
            {
                Vector3 normal = new Vector3();
                foreach (int face in vertexTriangle[i])
                {
                    normal += Utilities.CalculateNormal(bezier.OutputPoints[(int)bezier.triangles[face].p3], bezier.OutputPoints[(int)bezier.triangles[face].p2],
                            bezier.OutputPoints[(int)bezier.triangles[face].p1]);
                }
                normal.Normalize();

                gridVertices[i].Position = new Vector3(bezier.OutputPoints[i].x, bezier.OutputPoints[i].y, bezier.OutputPoints[i].z);
                gridVertices[i].Normal = normal;
                gridVertices[i].Color = Color.Beige.ToArgb();

            }

            Mesh gridMesh = numIndices > 2 ? new Mesh(deviceBezier, (int)numIndices / 3, bezier.OutputPoints.Length, MeshFlags.Managed | MeshFlags.Use32Bit, 
                                                        vertexElems): null;
            VertexBuffer vb = gridMesh != null ? gridMesh.VertexBuffer : null;
            IndexBuffer ib = gridMesh != null ? gridMesh.IndexBuffer : null;


            if (gridMesh != null)
            {
                vb.Lock(0, 0, LockFlags.None).WriteRange(gridVertices);
                vb.Unlock();

                ib.Lock(0, 0, LockFlags.None).WriteRange(indices, 0, (int)numIndices);
                ib.Unlock();
            }


            Mesh controlPointsMesh = Mesh.CreateSphere(deviceBezier, 0.05f, 12, 12);

            if (perspective.Width > 0 && perspective.Height > 0)
            {
                deviceBezier.Viewport = perspective;
                float aspect = (float)perspective.Width / perspective.Height;

                deviceBezier.SetTransform(TransformState.View, Matrix.LookAtRH(
                    bezierCam.position,
                    bezierCam.lookAt,
                    Utilities.RotatePointAroundVector(new Vector3(0, 1, 0),
                    Vector3.Normalize(bezierCam.lookAt - bezierCam.position), bezierCam.rotateAngle)));

                deviceBezier.SetTransform(TransformState.Projection, Matrix.PerspectiveFovRH(
                    bezierCam.fovAngle,
                    aspect,
                    0.01f,
                    110000));

                deviceBezier.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(230, 230, 230), 1.0f, 0);
                deviceBezier.BeginScene();

                if (gridMesh != null)
                {
                    gridMesh.DrawSubset(0);
                    deviceBezier.SetRenderState(RenderState.FillMode, FillMode.Solid);

                    for (int i = 0; i < bezier.ControlPoints.Length; i++)
                    {
                        deviceBezier.SetTransform(TransformState.World, Matrix.Translation(
                                                                        bezier.ControlPoints[i].x,
                                                                        bezier.ControlPoints[i].y,
                                                                        bezier.ControlPoints[i].z));
                        if (i == bezier.selectedPointIdx)
                        {
                            deviceBezier.Material = selectedControlPointMaterial;
                            deviceBezier.SetRenderState(RenderState.Lighting, false);
                            controlPointsMesh.DrawSubset(0);
                            deviceBezier.Material = controlPointMaterial;
                            deviceBezier.SetRenderState(RenderState.Lighting, true);
                        }
                        else
                        {
                            controlPointsMesh.DrawSubset(0);
                        }
                    }
                    deviceBezier.SetTransform(TransformState.World, Matrix.Identity);
                    deviceBezier.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);

                }

                deviceBezier.EndScene();
            }
            if (top.Width > 0 && top.Height > 0)
            {
                deviceBezier.Viewport = top;

                deviceBezier.SetTransform(TransformState.View, Matrix.LookAtRH(
                   bezierOrthoPos[2],
                   bezierOrthoLookAt[2],
                   new Vector3(0, 1, 0)));

                deviceBezier.SetTransform(TransformState.Projection, Matrix.OrthoRH(
                    bezierOrthoWidth[2],
                    (float)(top.Height) / top.Width * bezierOrthoWidth[2],
                    0.01f,
                    110000));

                deviceBezier.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(230, 230, 230), 1.0f, 0);
                deviceBezier.BeginScene();

                string text1 = " -z\n/|\\\n |\n";
                font2.DrawString(null, text1, viewportInfo.posX[2] + 15, viewportInfo.posY[2], Color.Blue);
                string text2 = "\n\n\n  -----> x";
                font2.DrawString(null, text2, viewportInfo.posX[2] + 15, viewportInfo.posY[2], Color.Red);

                if (gridMesh != null)
                {
                    gridMesh.DrawSubset(0);
                    deviceBezier.SetRenderState(RenderState.FillMode, FillMode.Solid);

                    for (int i = 0; i < bezier.ControlPoints.Length; i++)
                    {
                        deviceBezier.SetTransform(TransformState.World, Matrix.Translation(
                                                                        bezier.ControlPoints[i].x,
                                                                        bezier.ControlPoints[i].y,
                                                                        bezier.ControlPoints[i].z));
                        if (i == bezier.selectedPointIdx)
                        {
                            deviceBezier.Material = selectedControlPointMaterial;
                            deviceBezier.SetRenderState(RenderState.Lighting, false);
                            controlPointsMesh.DrawSubset(0);
                            deviceBezier.Material = controlPointMaterial;
                            deviceBezier.SetRenderState(RenderState.Lighting, true);
                        }
                        else
                        {
                            controlPointsMesh.DrawSubset(0);
                        }
                    }
                    deviceBezier.SetTransform(TransformState.World, Matrix.Identity);
                    deviceBezier.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);
                }

                deviceBezier.EndScene();
            }

            if (front.Width > 0 && front.Height > 0)
            {
                deviceBezier.Viewport = front;

                deviceBezier.SetTransform(TransformState.View, Matrix.LookAtRH(
                   bezierOrthoPos[0],
                   bezierOrthoLookAt[0],
                   new Vector3(0, 1, 0)));

                deviceBezier.SetTransform(TransformState.Projection, Matrix.OrthoRH(
                    bezierOrthoWidth[0],
                    (float)(front.Height) / front.Width * bezierOrthoWidth[0],
                    0.01f,
                    110000));

                deviceBezier.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(230, 230, 230), 1.0f, 0);
                deviceBezier.BeginScene();

                string text1 = " y\n/|\\\n |\n";
                font2.DrawString(null, text1, viewportInfo.posX[0] + 15, viewportInfo.posY[0], Color.Green);
                string text2 = "\n\n\n  -----> x";
                font2.DrawString(null, text2, viewportInfo.posX[0] + 15, viewportInfo.posY[0], Color.Red);

                if (gridMesh != null)
                {
                    gridMesh.DrawSubset(0);
                    deviceBezier.SetRenderState(RenderState.FillMode, FillMode.Solid);

                    for (int i = 0; i < bezier.ControlPoints.Length; i++)
                    {
                        deviceBezier.SetTransform(TransformState.World, Matrix.Translation(
                                                                        bezier.ControlPoints[i].x,
                                                                        bezier.ControlPoints[i].y,
                                                                        bezier.ControlPoints[i].z));
                        if (i == bezier.selectedPointIdx)
                        {
                            deviceBezier.Material = selectedControlPointMaterial;
                            deviceBezier.SetRenderState(RenderState.Lighting, false);
                            controlPointsMesh.DrawSubset(0);
                            deviceBezier.Material = controlPointMaterial;
                            deviceBezier.SetRenderState(RenderState.Lighting, true);
                        }
                        else
                        {
                            controlPointsMesh.DrawSubset(0);
                        }
                    }
                    deviceBezier.SetTransform(TransformState.World, Matrix.Identity);
                    deviceBezier.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);

                }

                deviceBezier.EndScene();
            }

            if (side.Width > 0 && side.Height > 0)
            {
                deviceBezier.Viewport = side;

                deviceBezier.SetTransform(TransformState.View, Matrix.LookAtRH(
                   bezierOrthoPos[1],
                   bezierOrthoLookAt[1],
                   new Vector3(0, 1, 0)));

                deviceBezier.SetTransform(TransformState.Projection, Matrix.OrthoRH(
                    bezierOrthoWidth[1],
                    (float)(side.Height) / side.Width * bezierOrthoWidth[1],
                    0.01f,
                    110000));

                deviceBezier.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(230, 230, 230), 1.0f, 0);
                deviceBezier.BeginScene();

                string text1 = " y\n/|\\\n |\n";
                font2.DrawString(null, text1, viewportInfo.posX[1] + 15, viewportInfo.posY[1], Color.Green);
                string text2 = "\n\n\n  -----> -z";
                font2.DrawString(null, text2, viewportInfo.posX[1] + 15, viewportInfo.posY[1], Color.Blue);

                if (gridMesh != null)
                {
                    gridMesh.DrawSubset(0);
                    deviceBezier.SetRenderState(RenderState.FillMode, FillMode.Solid);

                    for (int i = 0; i < bezier.ControlPoints.Length; i++)
                    {
                        deviceBezier.SetTransform(TransformState.World, Matrix.Translation(
                                                                        bezier.ControlPoints[i].x,
                                                                        bezier.ControlPoints[i].y,
                                                                        bezier.ControlPoints[i].z));
                        if (i == bezier.selectedPointIdx)
                        {
                            deviceBezier.Material = selectedControlPointMaterial;
                            deviceBezier.SetRenderState(RenderState.Lighting, false);
                            controlPointsMesh.DrawSubset(0);
                            deviceBezier.Material = controlPointMaterial;
                            deviceBezier.SetRenderState(RenderState.Lighting, true);
                        }
                        else
                        {
                            controlPointsMesh.DrawSubset(0);
                        }
                    }
                    deviceBezier.SetTransform(TransformState.World, Matrix.Identity);
                    deviceBezier.SetRenderState(RenderState.FillMode, wireframe ? FillMode.Wireframe : FillMode.Solid);

                }

                deviceBezier.EndScene();
            }

            deviceBezier.Present();

            gridMesh.Dispose();
            controlPointsMesh.Dispose();

            //d3dBezier.Dispose();
            //deviceBezier.Dispose();
        }
    }
}