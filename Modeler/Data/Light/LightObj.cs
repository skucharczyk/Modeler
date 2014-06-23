using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modeler.Data.Scene;

namespace Modeler.Data.Light
{
    class LightObj
    {
        public Light_ Light { get; set; }
        public String ImageUri { get; set; }

        public LightObj(Light_ lgt, string imgUri)
        {
            Light = lgt;
            ImageUri = imgUri;
        }
    }
}
