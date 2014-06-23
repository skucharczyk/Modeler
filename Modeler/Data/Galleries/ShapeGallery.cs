using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using Modeler.Data.Elements;
using Modeler.Data.Shapes;
using Modeler.Data.Surfaces;

namespace Modeler.Data
{
    class ShapeGallery : ObservableCollection<Shape_>
    {
        //private const string currDirectory = Directory.GetCurrentDirectory();
        public static readonly List<string> ReservedNames = new List<string> { "Kula", "Stożek", "Sześcian", "Walec", "Kwadrat", "Trójkąt" };

        public int MinRemovableIndex
        { get; set; }

        public ShapeGallery()
            : base()
        {
#if FINAL
            Add(new Sphere("Kula", System.AppDomain.CurrentDomain.BaseDirectory + "\\Ikony\\Ksztalty\\Kula.png"));
            Add(new Cone("Stozek", System.AppDomain.CurrentDomain.BaseDirectory + "\\Ikony\\Ksztalty\\Stozek.png"));
            Add(new Cube("Szescian", System.AppDomain.CurrentDomain.BaseDirectory + "\\Ikony\\Ksztalty\\Prostopadl.png"));
            Add(new Cylinder("Walec", System.AppDomain.CurrentDomain.BaseDirectory + "\\Ikony\\Ksztalty\\Walec.png"));
            Add(new Modeler.Data.Shapes.Rectangle("Kwadrat", System.AppDomain.CurrentDomain.BaseDirectory + "\\Ikony\\Ksztalty\\Kwadrat.png"));
            Add(new TriangleShape("Trojkat", System.AppDomain.CurrentDomain.BaseDirectory + "\\Ikony\\Ksztalty\\Trojkat.png"));
#else
            Add(new Sphere("Kula", "Ikony/Ksztalty/Kula.png"));
            Add(new Cone("Stożek", "Ikony/Ksztalty/Stozek.png"));
            Add(new Cube("Sześcian", "Ikony/Ksztalty/Prostopadl.png"));
            Add(new Cylinder("Walec", "Ikony/Ksztalty/Walec.png"));
            Add(new Modeler.Data.Shapes.Rectangle("Kwadrat", "Ikony/Ksztalty/Kwadrat.png"));
            Add(new TriangleShape("Trójkąt", "Ikony/Ksztalty/Trojkat.png"));
#endif
            MinRemovableIndex = Count - 1;
            LoadBezierToGallery();
        }

        public List<string> GetNames()
        {
            List<string> names = new List<string>();

            foreach (Shape_ shape in this)
            {
                names.Add(shape.Name);
            }

            return names;
        }

        public int GetNameIndex(string name)
        {
            int idx = 0;

            foreach (Shape_ shape in this)
            {
                if (String.Compare(shape.Name, name, true) == 0)
                    return idx;
                idx++;
            }

            return -1;
        }

        public void DeleteBezierAt(int index)
        {
            if (index > MinRemovableIndex)
            {
                string name = Items[index].Name;
                RemoveAt(index);
                string currDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
                //string currDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
#if FINAL
                string fullPath = currDirectory + "\\galleries\\bezier_surfaces\\";
#else
                string fullPath = Path.Combine(currDirectory, "..\\..\\galleries\\bezier_surfaces\\");
#endif

                System.IO.File.Delete(fullPath + name + ".bzr");
                System.IO.File.Delete(fullPath + name + ".png");
            }
        }

        public void LoadBezierToGallery()
        {
            //string currDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //string currDirectory = Directory.GetCurrentDirectory();
            string currDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
#if FINAL
            //string fullPath = Path.Combine(currDirectory, "\\galleries\\bezier_surfaces\\");
            string fullPath = currDirectory + "\\galleries\\bezier_surfaces\\";
#else
            string fullPath = Path.Combine(currDirectory, "..\\..\\galleries\\bezier_surfaces\\");
#endif

            string[] paths = Directory.GetFiles(fullPath); // TODO poprawić ścieżkę w finalnej wersji

            foreach (string path in paths)
            {
                string file = Path.GetFileNameWithoutExtension(path);

                BezierSurface bezier = BezierSurface.ReadFromFile(path);
                if (bezier != null)
                {
                    //bezier.Name = file;
#if FINAL
                    bezier.ImageUri = currDirectory + "\\galleries\\bezier_surfaces\\" + bezier.Name + ".png";
#else
                    bezier.ImageUri = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\galleries\\bezier_surfaces\\" + bezier.Name + ".png";
#endif
                    Add(bezier); // TODO poprawić ścieżkę w finalnej wersji
                }
            }
        }

        public void SaveBezierToGallery(BezierSurface bezier, Image bezierImage)
        {
            string currDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
#if FINAL
            bezier.SaveBezierFile(currDirectory + "/galleries/bezier_surfaces/"+bezier.Name+".bzr");
            bezier.ImageUri = currDirectory+"\\galleries\\bezier_surfaces\\" + bezier.Name + ".png";
#else
            bezier.SaveBezierFile("../../galleries/bezier_surfaces/"+bezier.Name+".bzr");
            bezier.ImageUri = AppDomain.CurrentDomain.BaseDirectory+"..\\..\\galleries\\bezier_surfaces\\" + bezier.Name + ".png";
#endif

            bezierImage.Save(bezier.ImageUri, ImageFormat.Png);

            bezierImage.Dispose();
        }
    }
}
