using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modeler.Data.Elements
{
    class PreparedElement
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

        private Scene.Scene scene;
        public Scene.Scene Scene
        {
            get
            {
                return scene;
            }
            set
            {
                scene = value;
            }
        }

        public PreparedElement()
        { }

        public PreparedElement(string _name, string _elementUri, Modeler.Data.Scene.Scene scene)
        {
            this.name = _name;
            this.imageUri = _elementUri;
            this.scene = scene;
        }
    }
}
