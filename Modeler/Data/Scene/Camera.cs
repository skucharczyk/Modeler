using System;
using SlimDX;

namespace Modeler.Data.Scene
{
    class Camera
    {
        public String name;
        public int resolutionX, resolutionY;
        public Vector3 position, lookAt;
        public float fovAngle;
        public float rotateAngle;

        public Camera()
        {
            name = "";
            resolutionX = 800;
            resolutionY = 600;
            position = new Vector3(0, 0, 0);
            lookAt = new Vector3(1, 0, 0);
            fovAngle = 60;
            rotateAngle = 0; 
        }

        public Camera(String name, int resolutionX, int resolutionY, Vector3 position, Vector3 lookAt, float fovAngle, float rotateAngle)
        {
            this.name = name;
            this.resolutionX = resolutionX;
            this.resolutionY = resolutionY;
            this.position = position;
            this.lookAt = lookAt;
            this.fovAngle = fovAngle;
            this.rotateAngle = rotateAngle;
        }

        public Camera(Camera copy)
        {
            this.name = copy.name;
            this.resolutionX = copy.resolutionX;
            this.resolutionY = copy.resolutionY;
            this.position = copy.position;
            this.lookAt = copy.lookAt;
            this.fovAngle = copy.fovAngle;
            this.rotateAngle = copy.rotateAngle;
        }
    }
}
