using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modeler.Data.Scene;
using Modeler.Data.Elements;
using SlimDX;

namespace Modeler.CopyPaste
{
    class CopyPaste
    {
        private PreparedElement copyBuffer;
        private Vector3D copiedElemCenter;

        public CopyPaste()
        {
            copyBuffer = new PreparedElement("copy_buffer", "", new Scene());
        }

        public void CopySelection(Scene scene)
        {
            Scene tmpScene = scene.SceneFromSelection(out copiedElemCenter);
            copyBuffer = new PreparedElement("copy_buffer", "", tmpScene);
        }

        public void Paste(Scene scene, Vector3 translation, ViewportOrientation viewport)
        {
            if (viewport == ViewportOrientation.Top)
            {
                translation.Y = copiedElemCenter.y;
            }
            else if (viewport == ViewportOrientation.Front)
            {
                translation.Z = copiedElemCenter.z;
            }
            else if (viewport == ViewportOrientation.Side)
            {
                translation.X = copiedElemCenter.x;
            }
            scene.AddPreparedElement(copyBuffer, translation);
        }
    }
}
