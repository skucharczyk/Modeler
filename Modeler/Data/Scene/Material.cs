using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modeler.FileSystem;
using System.Globalization;

namespace Modeler.Data.Scene
{
    public class Material_
    {
        public String name;
        public float colorR, colorG, colorB;
        public float kdcR, kdcG, kdcB;
        public float kscR, kscG, kscB;
        public float krcR, krcG, krcB;
        public float kacR, kacG, kacB;
        public float g;
        public float n;

        public String Name
        {
            get { return name; }
        }

        public Material_(String name, float colorR, float colorG, float colorB, float kdcR, float kdcG, float kdcB, float kscR, float kscG,
            float kscB, float krcR, float krcG, float krcB, float kacR, float kacG, float kacB, float g, float n)
        {
            this.name = name;
            this.colorR = colorR;
            this.colorG = colorG;
            this.colorB = colorB;
            this.kdcR = kdcR;
            this.kdcG = kdcG;
            this.kdcB = kdcB;
            this.kscR = kscR;
            this.kscG = kscG;
            this.kscB = kscB;
            this.krcR = krcR;
            this.krcG = krcG;
            this.krcB = krcB;
            this.kacR = kacR;
            this.kacG = kacG;
            this.kacB = kacB;
            this.g = g;
            this.n = n;
        }

        public Material_ (Material_ copy)
        {
            this.name = copy.name;
            this.colorR = copy.colorR;
            this.colorG = copy.colorG;
            this.colorB = copy.colorB;
            this.kdcR = copy.kdcR;
            this.kdcG = copy.kdcG;
            this.kdcB = copy.kdcB;
            this.kscR = copy.kscR;
            this.kscG = copy.kscG;
            this.kscB = copy.kscB;
            this.krcR = copy.krcR;
            this.krcG = copy.krcG;
            this.krcB = copy.krcB;
            this.kacR = copy.kacR;
            this.kacG = copy.kacG;
            this.kacB = copy.kacB;
            this.g = copy.g;
            this.n = copy.n;
        }

        public static List<Material_> LoadMaterials(string file)
        {
            List<Material_> materials = new List<Material_>();

            try
            {
                List<string> text = File.ReadFileLines(file);
                int pointer = 0;

                string matNumLabel = File.GetAttribute(text[pointer], 0);
                if(matNumLabel != "materials_count")
                {
                    return null;
                }
                uint matNum = uint.Parse(File.GetAttribute(text[pointer++], 1));

                for(int i = 0; i < matNum; ++i)
                {
                    string[] matName = File.GetAttributes(text[pointer]);
                    if(matName[0] != "mat_name")
                    {
                        return null;
                    }
                    string name = File.CutFirstString(text[pointer]);
                    ++pointer;

                    string rgbLabel = File.GetAttribute(text[pointer], 0);
                    if(rgbLabel != "rgb")
                    {
                        return null;
                    }
                    float colorR = float.Parse(File.GetAttribute(text[pointer], 1), CultureInfo.InvariantCulture);
                    float colorG = float.Parse(File.GetAttribute(text[pointer], 2), CultureInfo.InvariantCulture);
                    float colorB = float.Parse(File.GetAttribute(text[pointer++], 3), CultureInfo.InvariantCulture);

                    string kdCrLabel = File.GetAttribute(text[pointer], 0);
                    float kdCr = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(kdCrLabel != "kdCr")
                    {
                        return null;
                    }
                    string kdCgLabel = File.GetAttribute(text[pointer], 0);
                    float kdCg = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(kdCgLabel != "kdCg")
                    {
                        return null;
                    }
                    string kdCbLabel = File.GetAttribute(text[pointer], 0);
                    float kdCb = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(kdCbLabel != "kdCb")
                    {
                        return null;
                    }

                    string ksCrLabel = File.GetAttribute(text[pointer], 0);
                    float ksCr = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(ksCrLabel != "ksCr")
                    {
                        return null;
                    }
                    string ksCgLabel = File.GetAttribute(text[pointer], 0);
                    float ksCg = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(ksCgLabel != "ksCg")
                    {
                        return null;
                    }
                    string ksCbLabel = File.GetAttribute(text[pointer], 0);
                    float ksCb = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(ksCbLabel != "ksCb")
                    {
                        return null;
                    }

                    string krCrLabel = File.GetAttribute(text[pointer], 0);
                    float krCr = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(krCrLabel != "krCr")
                    {
                        return null;
                    }
                    string krCgLabel = File.GetAttribute(text[pointer], 0);
                    float krCg = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(krCgLabel != "krCg")
                    {
                        return null;
                    }
                    string krCbLabel = File.GetAttribute(text[pointer], 0);
                    float krCb = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(krCbLabel != "krCb")
                    {
                        return null;
                    }

                    string kaCrLabel = File.GetAttribute(text[pointer], 0);
                    float kaCr = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(kaCrLabel != "kaCr")
                    {
                        return null;
                    }
                    string kaCgLabel = File.GetAttribute(text[pointer], 0);
                    float kaCg = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(kaCgLabel != "kaCg")
                    {
                        return null;
                    }
                    string kaCbLabel = File.GetAttribute(text[pointer], 0);
                    float kaCb = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(kaCbLabel != "kaCb")
                    {
                        return null;
                    }

                    string gLabel = File.GetAttribute(text[pointer], 0);
                    float g = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(gLabel != "g")
                    {
                        return null;
                    }
                    string nLabel = File.GetAttribute(text[pointer], 0);
                    float n = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    if(nLabel != "n")
                    {
                        return null;
                    }

                    materials.Add(new Material_(name, colorR, colorG, colorB, kdCr, kdCg, kdCb, ksCr, ksCg, ksCb, krCr, krCg, krCb, kaCr, kaCg, kaCb, g, n));
                }
            }
            catch(Exception)
            {
                return null;
            }

            return materials;
        }

        public static void SaveMaterials(List<Material_> materials, string file)
        {
            List<string> text = new List<string>();

            text.Add("");
            text.Add("// materiały");

            text.Add("materials_count " + materials.Count.ToString());

            foreach(Material_ mat in materials)
            {
                text.Add("");

                text.Add("mat_name " + mat.name);
                text.Add("rgb " + mat.colorR.ToString(CultureInfo.InvariantCulture) + " " + mat.colorG.ToString(CultureInfo.InvariantCulture) + " " + mat.colorB.ToString(CultureInfo.InvariantCulture));
                text.Add("kdCr " + mat.kdcR.ToString(CultureInfo.InvariantCulture));
                text.Add("kdCg " + mat.kdcG.ToString(CultureInfo.InvariantCulture));
                text.Add("kdCb " + mat.kdcB.ToString(CultureInfo.InvariantCulture));
                text.Add("ksCr " + mat.kscR.ToString(CultureInfo.InvariantCulture));
                text.Add("ksCg " + mat.kscG.ToString(CultureInfo.InvariantCulture));
                text.Add("ksCb " + mat.kscB.ToString(CultureInfo.InvariantCulture));
                text.Add("krCr " + mat.krcR.ToString(CultureInfo.InvariantCulture));
                text.Add("krCg " + mat.krcG.ToString(CultureInfo.InvariantCulture));
                text.Add("krCb " + mat.krcB.ToString(CultureInfo.InvariantCulture));
                text.Add("kaCr " + mat.kacR.ToString(CultureInfo.InvariantCulture));
                text.Add("kaCg " + mat.kacG.ToString(CultureInfo.InvariantCulture));
                text.Add("kaCb " + mat.kacB.ToString(CultureInfo.InvariantCulture));
                text.Add("g " + mat.g.ToString(CultureInfo.InvariantCulture));
                text.Add("n " + mat.n.ToString(CultureInfo.InvariantCulture));
            }

            File.SaveFileLines(file, text);
        }
    }

    class Part
    {
        public List<int> triangles;

        public Part(List<int> triangles)
        {
            this.triangles = new List<int>(triangles);
        }
    }
}
