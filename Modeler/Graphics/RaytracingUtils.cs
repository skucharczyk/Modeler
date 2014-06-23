using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modeler.Data.Scene;

namespace Modeler.Graphics
{
    class Plane
    {
        public Vector3D vectorNormal;
        public float d;

        public Plane(Vector3D normal, float d)
        {
            this.vectorNormal = normal;
            this.d = d;
        }
    }
}
