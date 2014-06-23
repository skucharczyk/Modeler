using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Modeler.Data.Scene;
using System.IO;
using Modeler.Graphics;
using System.Drawing.Imaging;

namespace Modeler.Panels
{
    /// <summary>
    /// Interaction logic for LightsPanel.xaml
    /// </summary>
    public partial class LightsPanel : UserControl
    {
        private Light_ light;

        public LightsPanel()
        {
            InitializeComponent();
            SetLight(new Light_());
        }

        public void SetLight(Light_ lgt)
        {
            light = lgt;
            goniometry.Goniometry = light.goniometric;

            colorRed_slider.Value = light.colorR;
            colorGreen_slider.Value = light.colorG;
            colorBlue_slider.Value = light.colorB;

            flux_slider.Value = light.power;

            innerAngle_slider.Value = light.innerAngle;
            outerAngle_slider.Value = light.outerAngle;

            name_box.Text = light.name;

            lgt.position.X = 0;
            lgt.position.Y = 0;
            lgt.position.Z = 0;

            UpdateElements();
            RenderLight();
        }

        private void lightPreview_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Object") || e.Data.GetDataPresent("Light"))
            {
                if (e.Data.GetData("Light") is Light_)
                {
                    Light_ lightData = (Light_)e.Data.GetData("Light");
                    Light_ lightPrevious = (Light_)e.Data.GetData("LightPrevious");
                    e.Data.SetData("Object", e.Data.GetData("Light"));
                    lightData.position = lightPrevious.position;
                    lightData.direction = lightPrevious.direction;
                }
                object data = e.Data.GetData("Object");
                Console.WriteLine(data.GetType());
                if (data is Modeler.Data.Light.LightObj)
                {
                    light = new Light_(((Modeler.Data.Light.LightObj)data).Light);
                    //material.colorR = material.colorR * 255;
                    //material.colorG = material.colorG * 255;
                    //material.colorB = material.colorB * 255;
                    SetLight(light);
                    //SetSliders();
                    //name_box.Text = material.name;
                }
                else if (data is Light_)
                {
                    light = new Light_(((Light_)data));
                    SetLight(light);
                    //SetSliders();
                    //name_box.Text = material.name;
                }
            }
        }

        public void RenderLight()
        {
            MemoryStream ms = new MemoryStream();
            LightRaytracer.Render(light).Save(ms, ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            lightPreview.Source = bi;
        }

        private void clolorRed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            colorRed_box.Text = colorRed_slider.Value.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            light.colorR = (float)colorRed_slider.Value;

            RenderLight();
        }

        private void colorGreen_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            colorGreen_box.Text = colorGreen_slider.Value.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            light.colorG = (float)colorGreen_slider.Value;

            RenderLight();
        }

        private void colorBlue_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            colorBlue_box.Text = colorBlue_slider.Value.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            light.colorB = (float)colorBlue_slider.Value;

            RenderLight();
        }

        private void colorRed_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (colorRed_box.Text == null || colorRed_box.Text == "") colorRed_box.Text = "0";
                if (Double.Parse(colorRed_box.Text.Replace(".", ",")) > 1) colorRed_box.Text = "1";
                colorRed_slider.Value = Double.Parse(colorRed_box.Text.Replace(".", ","));
                colorRed_box.SelectionStart = colorRed_box.Text.Length;
            }
            catch (Exception)
            {
                colorRed_box.Text = colorRed_slider.Value.ToString();
            }
        }

        private void colorGreen_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (colorGreen_box.Text == null || colorGreen_box.Text == "") colorGreen_box.Text = "0";
                if (Double.Parse(colorGreen_box.Text.Replace(".", ",")) > 1) colorGreen_box.Text = "1";
                colorGreen_slider.Value = Double.Parse(colorGreen_box.Text.Replace(".", ","));
                colorGreen_box.SelectionStart = colorGreen_box.Text.Length;
            }
            catch (Exception)
            {
                colorGreen_box.Text = colorGreen_slider.Value.ToString();
            }
        }

        private void colorBlue_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (colorBlue_box.Text == null || colorBlue_box.Text == "") colorBlue_box.Text = "0";
                if (Double.Parse(colorBlue_box.Text.Replace(".", ",")) > 1) colorBlue_box.Text = "1";
                colorBlue_slider.Value = Double.Parse(colorBlue_box.Text.Replace(".", ","));
                colorBlue_box.SelectionStart = colorBlue_box.Text.Length;
            }
            catch (Exception)
            {
                colorBlue_box.Text = colorBlue_slider.Value.ToString();
            }
        }

        private void flux_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            flux_box.Text = flux_slider.Value.ToString();
            light.power = (float) flux_slider.Value;

            RenderLight();
        }

        private void flux_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (flux_box.Text == null || flux_box.Text == "") flux_box.Text = "0";
                if (Double.Parse(flux_box.Text.Replace(".", ",")) > 100) flux_box.Text = "100";
                flux_slider.Value = Double.Parse(flux_box.Text.Replace(".", ","));
                flux_box.SelectionStart = flux_box.Text.Length;
            }
            catch (Exception)
            {
                flux_box.Text = flux_slider.Value.ToString();
            }
        }

        private void innerAngle_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            innerAngle_box.Text = innerAngle_slider.Value.ToString();
            light.innerAngle = (float)innerAngle_slider.Value;

            RenderLight();
        }

        private void innerAngle_box_LostFocus(object sender, RoutedEventArgs e)
        {

            try
            {
                if (innerAngle_box.Text == null || innerAngle_box.Text == "") innerAngle_box.Text = "0";
                if (Double.Parse(innerAngle_box.Text.Replace(".", ",")) > 180) innerAngle_box.Text = "180";
                innerAngle_slider.Value = Double.Parse(innerAngle_box.Text.Replace(".", ","));
                innerAngle_box.SelectionStart = innerAngle_box.Text.Length;
            }
            catch (Exception)
            {
                innerAngle_box.Text = innerAngle_slider.Value.ToString();
            }
        }

        private void lightType_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Console.WriteLine(lightType_combo.SelectedItem + " " + lightType_combo.SelectedValue + " " + lightType_combo.SelectedIndex);
            //light.type = (Light_Type)Enum.Parse(typeof(Light_Type), lightType_combo.SelectedItem.ToString());
            
            if (light != null)
            {
                light.type = (Light_Type)lightType_combo.SelectedIndex;
                RenderLight();
                UpdateElements();
            }
        }

        private void outerAngle_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            outerAngle_box.Text = outerAngle_slider.Value.ToString();
            light.outerAngle = (float)outerAngle_slider.Value;

            RenderLight();
        }

        private void outerAngle_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (outerAngle_box.Text == null || outerAngle_box.Text == "") outerAngle_box.Text = "0";
                if (Double.Parse(outerAngle_box.Text.Replace(".", ",")) > 180) outerAngle_box.Text = "180";
                outerAngle_slider.Value = Double.Parse(outerAngle_box.Text.Replace(".", ","));
                outerAngle_box.SelectionStart = outerAngle_box.Text.Length;
            }
            catch (Exception)
            {
                outerAngle_box.Text = outerAngle_slider.Value.ToString();
            }
        }

        private void lightPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.DataObject dataObject = new System.Windows.DataObject();
                dataObject.SetData("Object", light);
                DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Copy);
                //Console.WriteLine(_elementsCol.ElementAt(gotowe_ListView.SelectedIndex).ToString());
            }
        }

        private void UpdateElements()
        {
            switch (light.type)
            {
                case Light_Type.Point:
                    lightType_combo.SelectedIndex = (int)Light_Type.Point;
                    goniometry.Visibility = Visibility.Hidden;
                    innerAngle_box.Visibility = Visibility.Hidden;
                    innerAngle_label.Visibility = Visibility.Hidden;
                    innerAngle_slider.Visibility = Visibility.Hidden;
                    outerAngle_box.Visibility = Visibility.Hidden;
                    outerAngle_label.Visibility = Visibility.Hidden;
                    outerAngle_slider.Visibility = Visibility.Hidden;
                    break;
                case Light_Type.Spot:
                    lightType_combo.SelectedIndex = (int)Light_Type.Spot;
                    goniometry.Visibility = Visibility.Hidden;
                    innerAngle_box.Visibility = Visibility.Visible;
                    innerAngle_label.Visibility = Visibility.Visible;
                    innerAngle_slider.Visibility = Visibility.Visible;
                    outerAngle_box.Visibility = Visibility.Visible;
                    outerAngle_label.Visibility = Visibility.Visible;
                    outerAngle_slider.Visibility = Visibility.Visible;
                    break;
                case Light_Type.Goniometric:
                    lightType_combo.SelectedIndex = (int)Light_Type.Goniometric;
                    goniometry.Visibility = Visibility.Visible;
                    innerAngle_box.Visibility = Visibility.Hidden;
                    innerAngle_label.Visibility = Visibility.Hidden;
                    innerAngle_slider.Visibility = Visibility.Hidden;
                    outerAngle_box.Visibility = Visibility.Hidden;
                    outerAngle_label.Visibility = Visibility.Hidden;
                    outerAngle_slider.Visibility = Visibility.Hidden;
                    innerAngle_slider.Value = 3;
                    outerAngle_slider.Value = 3;
                    break;
                default:
                    break;
            }
        }

        private void name_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (light != null) light.name = name_box.Text;
        }
    }
}
