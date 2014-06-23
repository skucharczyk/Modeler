using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modeler.Data.Scene;

namespace Modeler.Data.Shapes
{
    abstract class Shape_
    {
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        private string imageUri;
        public string ImageUri
        {
            get
            {
                return imageUri;
            }
            set
            {
                imageUri = value;
            }
        }

        public Shape_(string _name, string uri)
        {
            this.name = _name;
            this.imageUri = uri;
        }

        public abstract Modeler.Data.Scene.Scene Triangulate(float density);
    }
}
