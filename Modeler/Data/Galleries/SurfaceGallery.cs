using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using Modeler.Data.Scene;
using Modeler.Data.Surfaces;
using System.IO;

namespace Modeler.Data.Galleries
{
    class SurfaceGallery : ObservableCollection<Surface>
    {
        public SurfaceGallery()
            : base()
        {
            LoadGallery();
        }

        public List<string> GetNames()
        {
            List<string> names = new List<string>();
            foreach (Surface element in Items)
            {
                names.Add(element.Material.Name);
            }
            return names;
        }

        public int GetNameIndex(string name)
        {
            int idx = 0;

            foreach (Surface element in Items)
            {
                if (String.Compare(element.Material.Name, name, true) == 0)
                    return idx;

                idx++;
            }

            return -1;
        }

        protected override void RemoveItem(int index)
        {
            string name = Items[index].Material.Name;
            base.RemoveItem(index);

            // Usuwanie plików
            string currDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
#if FINAL
            string fullPath = currDirectory + "\\galleries\\materials\\";
#else
            string fullPath = Path.Combine(currDirectory, "..\\..\\galleries\\materials\\");
#endif


            System.IO.File.Delete(fullPath + name + ".mat");
            System.IO.File.Delete(fullPath + name + ".png");
        }

        public void LoadGallery()
        {
            string currDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
#if FINAL
            string fullPath = currDirectory + "\\galleries\\materials\\";
#else
            string fullPath = Path.Combine(currDirectory, "..\\..\\galleries\\materials\\");
#endif

            string[] paths = Directory.GetFiles(fullPath); // TODO poprawić ścieżkę w finalnej wersji

            foreach (string path in paths)
            {
                string file = Path.GetFileNameWithoutExtension(path);

                List<Material_> mat = Material_.LoadMaterials(path);
                Material_ material = mat != null ? mat[0] : null;
                if (material != null)
                {
                    //bezier.Name = file;

#if FINAL
                    string ImageUri = currDirectory + "\\galleries\\materials\\" + material.Name + ".png";
#else
                    string ImageUri = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\galleries\\materials\\" + material.Name + ".png";
#endif
                    Add(new Surface(material, ImageUri)); // TODO poprawić ścieżkę w finalnej wersji
                }
            }
        }

        public void SaveGallery()
        {
            List<Material_> materials = new List<Material_>();

            foreach(Surface material in Items)
            {
                materials.Add(material.Material);

                // TODO zapisać ikonę
            }

#if FINAL
            Material_.SaveMaterials(materials, "/galleries/MaterialsGallery.txt"); // TODO poprawić ścieżkę w finalnej wersji
#else
            Material_.SaveMaterials(materials, "../../galleries/MaterialsGallery.txt"); // TODO poprawić ścieżkę w finalnej wersji
#endif
        }

        public Surface SaveToGallery(Material_ material)
        {
#if FINAL
            Surface surface = new Surface(material, AppDomain.CurrentDomain.BaseDirectory + "\\galleries\\materials\\" + material.Name + ".png");
            List<Material_> mat = new List<Material_>(){material};
            Material_.SaveMaterials(mat, AppDomain.CurrentDomain.BaseDirectory + "/galleries/materials/" + material.Name + ".mat");
#else
            Surface surface = new Surface(material, AppDomain.CurrentDomain.BaseDirectory + "..\\..\\galleries\\materials\\" + material.Name + ".png");
            List<Material_> mat = new List<Material_>(){material};
            Material_.SaveMaterials(mat, "../../galleries/materials/" + material.Name + ".mat");    
#endif

            Modeler.Graphics.SurfaceRaytracer.SaveImage(material.Name, surface.ImageUri);

            return surface;
        }
    }
}
