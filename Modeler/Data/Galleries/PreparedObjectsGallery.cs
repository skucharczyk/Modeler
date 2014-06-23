using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Modeler.Data.Elements;

namespace Modeler.Data
{
    class PreparedObjectsGallery : ObservableCollection<PreparedElement>
    {
        public PreparedObjectsGallery()
            : base()
        {
            LoadGallery();
            /*Add(new PreparedElement("Krzeslo", "/Ikony/GotoweElementy/Krzeslo.png"));
            Add(new PreparedElement("Stol", "/Ikony/GotoweElementy/Stol.png"));
            Add(new PreparedElement("Lampa", "/Ikony/GotoweElementy/Lampa.png"));*/
        }

        public List<string> GetNames()
        {
            List<string> names = new List<string>();
            foreach (PreparedElement elem in Items)
            {
                names.Add(elem.Name);
            }

            return names;
        }

        public int GetNameIdx(string name)
        {
            int idx = 0;
            foreach (PreparedElement element in Items)
            {
                if (String.Compare(element.Name, name, true) == 0)
                    return idx;
                idx++;
            }
            return -1;
        }

        protected override void RemoveItem(int index)
        {
            string name = Items[index].Name;
            base.RemoveItem(index);

            string currDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
#if FINAL
            string fullPath = currDirectory + "\\galleries\\objects\\";
#else
            string fullPath = Path.Combine(currDirectory, "..\\..\\galleries\\objects\\");
#endif

            System.IO.File.Delete(fullPath + name + ".txt");
            System.IO.File.Delete(fullPath + name + ".png");
        }

        public void LoadGallery()
        {
            string currDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
#if FINAL
            string fullPath = currDirectory + "/galleries/objects/";
#else
            string fullPath = Path.Combine(currDirectory, "../../galleries/objects");
#endif

            string[] paths = Directory.GetFiles(fullPath); // TODO poprawić ścieżkę w finalnej wersji

            foreach(string path in paths)
            {
                string file = Path.GetFileNameWithoutExtension(path);

                Scene.Scene scene = Scene.Scene.ReadSceneFromFile(path);
                if(scene != null)
                {
#if FINAL
                    string ImageUri = currDirectory + "\\galleries\\objects\\" + file + ".png";
#else
                    string ImageUri = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\galleries\\objects\\" + file + ".png";
#endif
                    Add(new PreparedElement(file, ImageUri, scene)); // TODO poprawić ścieżkę w finalnej wersji
                }
            }
        }

        // obiekty do galerii gotowych obiektów zapisywane są oddzielnie
        // element w scene zawiera wydzielony obiekt ze sceny
        public void SaveObjectToGallery(PreparedElement element, Image elementImage)
        {
            string currDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
#if FINAL
            element.Scene.SaveSceneFile(currDirectory + "\\galleries\\objects\\" + element.Name + ".txt"); // TODO poprawić ścieżkę w finalnej wersji
            element.ImageUri = currDirectory +"\\galleries\\objects\\" + element.Name + ".png";
#else
             element.Scene.SaveSceneFile("../../galleries/objects/" + element.Name + ".txt"); // TODO poprawić ścieżkę w finalnej wersji
            element.ImageUri = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\galleries\\objects\\" + element.Name + ".png";
#endif

            elementImage.Save(element.ImageUri, ImageFormat.Png);

            elementImage.Dispose();

            // TODO zapisać ikonę
        }
    }
}
