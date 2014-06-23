using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using Modeler.FileSystem;
using System.Globalization;

namespace Modeler.Data.Scene
{
    public enum Light_Type { Point, Spot, Goniometric }

    public class Light_
    {
        public String name
        { get; set; }
        public Light_Type type;
        public bool enabled;
        public float colorR, colorG, colorB;
        public float power;
        public Vector3 position;
        public Vector3 direction;
        public float innerAngle;
        public float outerAngle;
        public SortedList<float, float> goniometric;

        public Light_()
        {
            name = "default";
            type = Light_Type.Point;
            enabled = true;
            colorR = 1;
            colorG = 1;
            colorB = 1;
            power = 20;
            position = new Vector3(0, 0, 0);
            direction = new Vector3(1, 0, 0);
            innerAngle = 30;
            outerAngle = 60;
            goniometric = new SortedList<float, float>{{0, 1}, {180, 1}};
        }

        public Light_(String name, Light_Type type, bool enabled, float r,
                        float g, float b, float power, Vector3 position)
        {
            this.name = name;
            this.type = type;
            this.enabled = enabled;
            colorR = r;
            colorG = g;
            colorB = b;
            this.power = power;
            this.position = position;
            direction = new Vector3(1, 0, 0);
            innerAngle = -1;
            outerAngle = -1;
            goniometric = new SortedList<float, float> {{0, 1}, {180, 1}};
        }

        public Light_(String name, Light_Type type, bool enabled, float r,
                        float g, float b, float power, Vector3 position,
                        Vector3 direction, float innerAngle, float outerAngle,
                        SortedList<float, float> goniometric)
        {
            this.name = name;
            this.type = type;
            this.enabled = enabled;
            colorR = r;
            colorG = g;
            colorB = b;
            this.power = power;
            this.position = position;
            this.direction = direction;
            this.innerAngle = innerAngle;
            this.outerAngle = outerAngle;
            this.goniometric = goniometric;
        }

        public Light_(Light_ copy)
        {
            this.name = copy.name;
            this.type = copy.type;
            this.enabled = copy.enabled;
            colorR = copy.colorR;
            colorG = copy.colorG;
            colorB = copy.colorB;
            this.power = copy.power;
            position = new Vector3(copy.position.X, copy.position.Y, copy.position.Z);
            direction = new Vector3(copy.direction.X, copy.direction.Y, copy.direction.Z);
            innerAngle = copy.innerAngle;
            outerAngle = copy.outerAngle;
            goniometric = copy.goniometric;
        }

        public static List<Light_> LoadLights(string file)
        {
            List<Light_> lights = new List<Light_>();

            try
            {
                List<string> text = File.ReadFileLines(file);
                int pointer = 0;

                string lightsLabel = File.GetAttribute(text[pointer], 0);
                if(lightsLabel != "lights_count")
                {
                    return null;
                }
                uint lightsNum = uint.Parse(File.GetAttribute(text[pointer++], 1));

                for(int i = 0; i < lightsNum; ++i)
                {
                    string[] lightName = File.GetAttributes(text[pointer]);
                    if(lightName[0] != "light_name")
                    {
                        return null;
                    }
                    string name = name = File.CutFirstString(text[pointer]);
                    ++pointer;

                    string enabledLabel = File.GetAttribute(text[pointer], 0);
                    if(enabledLabel != "enabled")
                    {
                        return null;
                    }
                    bool enabled = int.Parse(File.GetAttribute(text[pointer++], 1)) == 1 ? true : false;

                    string typeLabel = File.GetAttribute(text[pointer], 0);
                    if(typeLabel != "light_type")
                    {
                        return null;
                    }
                    Light_Type type = Light_Type.Point;
                    switch(File.GetAttribute(text[pointer++], 1))
                    {
                        case "point":
                            type = Light_Type.Point;
                            break;

                        case "spot":
                            type = Light_Type.Spot;
                            break;

                        case "goniometric":
                            type = Light_Type.Goniometric;
                            break;
                    }

                    string colorLabel = File.GetAttribute(text[pointer], 0);
                    if(colorLabel != "rgb")
                    {
                        return null;
                    }
                    float colorR = float.Parse(File.GetAttribute(text[pointer], 1), CultureInfo.InvariantCulture);
                    float colorG = float.Parse(File.GetAttribute(text[pointer], 2), CultureInfo.InvariantCulture);
                    float colorB = float.Parse(File.GetAttribute(text[pointer++], 3), CultureInfo.InvariantCulture);

                    string powerLabel = File.GetAttribute(text[pointer], 0);
                    if(powerLabel != "power")
                    {
                        return null;
                    }
                    float power = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);

                    string posLabel = File.GetAttribute(text[pointer], 0);
                    if(posLabel != "pos")
                    {
                        return null;
                    }
                    Vector3 pos = new Vector3(float.Parse(File.GetAttribute(text[pointer], 1), CultureInfo.InvariantCulture),
                        float.Parse(File.GetAttribute(text[pointer], 2), CultureInfo.InvariantCulture),
                        float.Parse(File.GetAttribute(text[pointer++], 3), CultureInfo.InvariantCulture));

                    string dirLabel = File.GetAttribute(text[pointer], 0);
                    if(dirLabel != "dir")
                    {
                        return null;
                    }
                    Vector3 dir = new Vector3(float.Parse(File.GetAttribute(text[pointer], 1), CultureInfo.InvariantCulture),
                        float.Parse(File.GetAttribute(text[pointer], 2), CultureInfo.InvariantCulture),
                        float.Parse(File.GetAttribute(text[pointer++], 3), CultureInfo.InvariantCulture));

                    string innerAngleLabel = File.GetAttribute(text[pointer], 0);
                    if(innerAngleLabel != "inner_angle")
                    {
                        return null;
                    }
                    float innerAngle = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);
                    string outerAngleLabel = File.GetAttribute(text[pointer], 0);
                    if(outerAngleLabel != "outer_angle")
                    {
                        return null;
                    }
                    float outerAngle = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);

                    SortedList<float, float> goniometric = new SortedList<float, float>();

                    string gonioNumLabel = File.GetAttribute(text[pointer], 0);
                    if(gonioNumLabel != "gonio_count")
                    {
                        return null;
                    }
                    uint gonioNum = uint.Parse(File.GetAttribute(text[pointer++], 1));

                    for(int j = 0; j < gonioNum; ++j)
                    {
                        float gonioIndex = float.Parse(File.GetAttribute(text[pointer], 0), CultureInfo.InvariantCulture);
                        float gonioValue = float.Parse(File.GetAttribute(text[pointer++], 1), CultureInfo.InvariantCulture);

                        goniometric.Add(gonioIndex, gonioValue);
                    }

                    lights.Add(new Light_(name, type, enabled, colorR, colorG, colorB, power, pos));

                    lights[lights.Count - 1].direction = new Vector3(dir.X, dir.Y, dir.Z);
                    lights[lights.Count - 1].innerAngle = innerAngle;
                    lights[lights.Count - 1].outerAngle = outerAngle;
                    lights[lights.Count - 1].goniometric = goniometric;
                }
            }
            catch(Exception)
            {
                return null;
            }

            return lights;
        }

        public static void SaveLights(List<Light_> lights, string file)
        {
            List<string> text = new List<string>();

            text.Add("");
            text.Add("// światła");

            text.Add("lights_count " + lights.Count.ToString());

            foreach(Light_ light in lights)
            {
                text.Add("");

                text.Add("light_name " + light.name);
                text.Add("enabled " + (light.enabled == true ? "1" : "0"));
                text.Add("light_type " + light.type.ToString().ToLowerInvariant());
                text.Add("rgb " + light.colorR.ToString(CultureInfo.InvariantCulture) + " " + light.colorG.ToString(CultureInfo.InvariantCulture) + " " + light.colorB.ToString(CultureInfo.InvariantCulture));
                text.Add("power " + light.power);
                text.Add("pos " + light.position.X.ToString(CultureInfo.InvariantCulture) + " " + light.position.Y.ToString(CultureInfo.InvariantCulture) + " " + light.position.Z.ToString(CultureInfo.InvariantCulture));
                text.Add("dir " + light.direction.X.ToString(CultureInfo.InvariantCulture) + " " + light.direction.Y.ToString(CultureInfo.InvariantCulture) + " " + light.direction.Z.ToString(CultureInfo.InvariantCulture));
                text.Add("inner_angle " + light.innerAngle);
                text.Add("outer_angle " + light.outerAngle);
                text.Add("gonio_count " + light.goniometric.Count.ToString());

                for(int i = 0; i < light.goniometric.Count; ++i)
                {
                    text.Add(light.goniometric.Keys[i].ToString(CultureInfo.InvariantCulture) + " " + light.goniometric.Values[i].ToString(CultureInfo.InvariantCulture));
                }
            }

            File.SaveFileLines(file, text);
        }
    }
}
