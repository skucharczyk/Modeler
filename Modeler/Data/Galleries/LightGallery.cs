using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Modeler.Data.Scene;
using Modeler.Data.Light;


namespace Modeler.Data.Galleries
{
    class LightGallery : ObservableCollection<LightObj>
    {
        public LightGallery()
            : base()
        {
            LoadGallery();
        }

        public void LoadGallery()
        {
            string currDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
#if FINAL
            string fullPath = currDirectory + "\\galleries\\lights\\";
#else
            string fullPath = Path.Combine(currDirectory, "..\\..\\galleries\\lights\\");
#endif

            string[] paths = Directory.GetFiles(fullPath); // TODO poprawić ścieżkę w finalnej wersji

            foreach (string path in paths)
            {
                string file = Path.GetFileNameWithoutExtension(path);

                List<Light_> lgt = Light_.LoadLights(path);
                //Material_ material = mat != null ? mat[0] : null;
                if (lgt != null)
                {
                    //bezier.Name = file;
#if FINAL
                    string ImageUri = currDirectory + "\\galleries\\lights\\" + lgt[0].name + ".png";
#else
                    string ImageUri = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\galleries\\lights\\" + lgt[0].name + ".png";
#endif
                    Add(new LightObj(lgt[0], ImageUri)); // TODO poprawić ścieżkę w finalnej wersji
                }
            }
        }

        protected override void RemoveItem(int index)
        {
            string name = Items[index].Light.name;
            base.RemoveItem(index);

            // Usuwanie plików
            string currDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
#if FINAL
            string fullPath = currDirectory + "\\galleries\\lights\\";
#else
            string fullPath = Path.Combine(currDirectory, "..\\..\\galleries\\lights\\");
#endif

            System.IO.File.Delete(fullPath + name + ".lgt");
            System.IO.File.Delete(fullPath + name + ".png");
        }

        public void SaveGallery()
        {
            List<Light_> lights = new List<Light_>();

            foreach(LightObj light in Items)
            {
                lights.Add(light.Light);

                // TODO zapisać ikonę
            }
#if FINAL
            Light_.SaveLights(lights, "/galleries/LightsGallery.txt"); // TODO poprawić ścieżkę w finalnej wersji
#else
            Light_.SaveLights(lights, "../../galleries/LightsGallery.txt"); // TODO poprawić ścieżkę w finalnej wersji
#endif
        }

        public LightObj SaveLightToGallery(Light_ light)
        {
            string currDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
#if FINAL
            LightObj lgtObj = new LightObj(light, currDirectory + "\\galleries\\lights\\" + light.name + ".png");
#else
            LightObj lgtObj = new LightObj(light, AppDomain.CurrentDomain.BaseDirectory + "..\\..\\galleries\\lights\\" + light.name + ".png");
#endif
            List<Light_> lgt = new List<Light_>() { light };

#if FINAL
            Light_.SaveLights(lgt, currDirectory + "\\galleries\\lights\\" + light.name + ".lgt");
#else
            Light_.SaveLights(lgt, "../../galleries/lights/" + light.name + ".lgt");
#endif

            Modeler.Graphics.LightRaytracer.SaveImage(light.name, lgtObj.ImageUri);

            return lgtObj;
        }

        public List<string> GetNames()
        {
            List<string> names = new List<string>();
            foreach (LightObj lgt in Items)
            {
                names.Add(lgt.Light.name);
            }
            return names;
        }

        public int GetNameIndex(string name)
        {
            int idx = 0;

            foreach (LightObj lgt in Items)
            {
                if (String.Compare(lgt.Light.name, name, true) == 0)
                    return idx;

                idx++;
            }

            return -1;
        }
    }
}
