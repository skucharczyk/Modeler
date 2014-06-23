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
using System.IO;
using System.Globalization;
using Modeler.Data.Scene;
using Modeler.Graphics;
using System.Drawing.Imaging;
using Modeler.Data.Surfaces;

namespace Modeler.Panels
{
    /// <summary>
    /// Interaction logic for AtributesPanel.xaml
    /// </summary>
    public partial class AtributesPanel : UserControl
    {
        private Material_ material;
        private bool setsliders = false;

        public AtributesPanel()
        {
            InitializeComponent();
            InitializeMaterial();
        }

        private void InitializeMaterial()
        {
            material = new Material_("Default", 159, 182, 205, 
                0.8f, 0.8f, 0.8f, 
                0.1f, 0.1f, 0.1f,
                0.0f, 0.0f, 0.0f, 
                0.0f, 0.0f, 0.0f, 
                500, 1);
            SetSliders();
            RenderMaterial();
        }

        private void SetSliders()
        {
            setsliders = true;

            colorRed_slider.Value = (int)material.colorR;
            colorBlue_slider.Value = (int)material.colorB;
            colorGreen_slider.Value = (int)material.colorG;

            diffuseRed_slider.Value = material.kdcR;
            diffuseGreen_slider.Value = material.kdcG;
            diffuseBlue_slider.Value = material.kdcB;

            specularRed_slider.Value = material.kscR;
            specularGreen_slider.Value = material.kscG;
            specularBlue_slider.Value = material.kscB;

            transparentRed_slider.Value = material.krcR;
            transparentGreen_slider.Value = material.krcG;
            transparentBlue_slider.Value = material.krcB;

            transparentRed_box.Text = material.krcR.ToString("F4", CultureInfo.InvariantCulture);
            transparentGreen_box.Text = material.krcG.ToString("F4", CultureInfo.InvariantCulture);
            transparentBlue_box.Text = material.krcB.ToString("F4", CultureInfo.InvariantCulture);

            ambientRed_slider.Value = material.kacR;
            ambientGreen_slider.Value = material.kacG;
            ambientBlue_slider.Value = material.kacB;

            ambientRed_box.Text = material.kacR.ToString("F4", CultureInfo.InvariantCulture);
            ambientGreen_box.Text = material.kacG.ToString("F4", CultureInfo.InvariantCulture);
            ambientBlue_box.Text = material.kacB.ToString("F4", CultureInfo.InvariantCulture);

            eta_slider.Value = material.n;

            g_slider.Value = material.g;
            setsliders = false;
        }

        public void SetMaterial(Material_ material)
        {
            this.material = material;
            SetSliders();
        }

        public Material_ GetMaterial()
        {
            return material;
        }

        private void RenderMaterial() 
        {
            MemoryStream ms = new MemoryStream();
            SurfaceRaytracer.Render(material).Save(ms, ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            materialPreview.Source = bi;
            //ms.Dispose();
        }

        private void clolorRed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            colorRed_box.Text = colorRed_slider.Value.ToString();
            material.colorR = (int) colorRed_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void colorGreen_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            colorGreen_box.Text = colorGreen_slider.Value.ToString();
            material.colorG = (int)colorGreen_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void colorBlue_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            colorBlue_box.Text = colorBlue_slider.Value.ToString();
            material.colorB = (int)colorBlue_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void diffuseRed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            diffuseRed_box.Text = diffuseRed_slider.Value.ToString("F4", CultureInfo.InvariantCulture);
            material.kdcR = (float)diffuseRed_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void diffuseGreen_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            diffuseGreen_box.Text = diffuseGreen_slider.Value.ToString("F4", CultureInfo.InvariantCulture);
            material.kdcG = (float)diffuseGreen_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void diffuseBlue_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            diffuseBlue_box.Text = diffuseBlue_slider.Value.ToString("F4", CultureInfo.InvariantCulture);
            material.kdcB = (float)diffuseBlue_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void specularRed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            specularRed_box.Text = specularRed_slider.Value.ToString("F4", CultureInfo.InvariantCulture);
            material.kscR = (float)specularRed_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void specularGreen_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            specularGreen_box.Text = specularGreen_slider.Value.ToString("F4", CultureInfo.InvariantCulture);
            material.kscG = (float)specularGreen_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void specularBlue_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            specularBlue_box.Text = specularBlue_slider.Value.ToString("F4", CultureInfo.InvariantCulture);
            material.kscB = (float)specularBlue_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void transparentRed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            transparentRed_box.Text = transparentRed_slider.Value.ToString("F4", CultureInfo.InvariantCulture); ;
            material.krcR = (float)transparentRed_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void transparentGreen_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            transparentGreen_box.Text = transparentGreen_slider.Value.ToString("F4", CultureInfo.InvariantCulture);
            material.krcG = (float)transparentGreen_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void transparentBlue_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            transparentBlue_box.Text = transparentBlue_slider.Value.ToString("F4", CultureInfo.InvariantCulture);
            material.krcB = (float)transparentBlue_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void eta_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            eta_box.Text = eta_slider.Value.ToString("F4", CultureInfo.InvariantCulture);
            material.n = (float)eta_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void g_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            g_box.Text = g_slider.Value.ToString();
            material.g = (float)g_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void materialPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.DataObject dataObject = new System.Windows.DataObject();
                dataObject.SetData("Object", material);
                DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Copy);
            }
        }

        private void colorRed_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (colorRed_box.Text == null || colorRed_box.Text == "") colorRed_box.Text = "0";
                if (Double.Parse(colorRed_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 255) colorRed_box.Text = "255";
                colorRed_slider.Value = Double.Parse(colorRed_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
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
                if (Double.Parse(colorGreen_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 255) colorGreen_box.Text = "255";
                colorGreen_slider.Value = Double.Parse(colorGreen_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
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
                if (Double.Parse(colorBlue_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 255) colorBlue_box.Text = "255";
                colorBlue_slider.Value = Double.Parse(colorBlue_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                colorBlue_box.SelectionStart = colorBlue_box.Text.Length;
            }
            catch (Exception)
            {
                colorBlue_box.Text = colorBlue_slider.Value.ToString();
            }
        }

        private void diffuseRed_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (diffuseRed_box.Text == null || diffuseRed_box.Text == "") diffuseRed_box.Text = "0";
                if (Double.Parse(diffuseRed_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 1) diffuseRed_box.Text = "1";
                diffuseRed_slider.Value = Double.Parse(diffuseRed_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                diffuseRed_box.SelectionStart = diffuseRed_box.Text.Length;
            }
            catch (Exception)
            {
                diffuseRed_box.Text = diffuseRed_slider.Value.ToString();
            }
        }

        private void diffuseGreen_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (diffuseGreen_box.Text == null || diffuseGreen_box.Text == "") diffuseGreen_box.Text = "0";
                if (Double.Parse(diffuseGreen_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 1) diffuseGreen_box.Text = "1";
                diffuseGreen_slider.Value = Double.Parse(diffuseGreen_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                diffuseGreen_box.SelectionStart = diffuseGreen_box.Text.Length;
            }
            catch (Exception)
            {
                diffuseGreen_box.Text = diffuseGreen_slider.Value.ToString();
            }
        }

        private void diffuseBlue_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (diffuseBlue_box.Text == null || diffuseBlue_box.Text == "") diffuseBlue_box.Text = "0";
                if (Double.Parse(diffuseBlue_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 1) diffuseBlue_box.Text = "1";
                diffuseBlue_slider.Value = Double.Parse(diffuseBlue_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                diffuseBlue_box.SelectionStart = diffuseBlue_box.Text.Length;
            }
            catch (Exception)
            {
                diffuseBlue_box.Text = diffuseBlue_slider.Value.ToString();
            }
        }

        private void specularRed_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (specularRed_box.Text == null || specularRed_box.Text == "") specularRed_box.Text = "0";
                if (Double.Parse(specularRed_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 1) specularRed_box.Text = "1";
                specularRed_slider.Value = Double.Parse(specularRed_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                specularRed_box.SelectionStart = specularRed_box.Text.Length;
            }
            catch (Exception)
            {
                specularRed_box.Text = specularRed_slider.Value.ToString();
            }
        }

        private void specularGreen_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (specularGreen_box.Text == null || specularGreen_box.Text == "") specularGreen_box.Text = "0";
                if (Double.Parse(specularGreen_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 1) specularGreen_box.Text = "1";
                specularGreen_slider.Value = Double.Parse(specularGreen_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                specularGreen_box.SelectionStart = specularGreen_box.Text.Length;
            }
            catch (Exception)
            {
                specularGreen_box.Text = specularGreen_slider.Value.ToString();
            }
        }

        private void specularBlue_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (specularBlue_box.Text == null || specularBlue_box.Text == "") specularBlue_box.Text = "0";
                if (Double.Parse(specularBlue_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 1) specularBlue_box.Text = "1";
                specularBlue_slider.Value = Double.Parse(specularBlue_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                specularBlue_box.SelectionStart = specularBlue_box.Text.Length;
            }
            catch (Exception)
            {
                specularBlue_box.Text = specularBlue_slider.Value.ToString();
            }
        }

        private void transparentRed_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (transparentRed_box.Text == null || transparentRed_box.Text == "") transparentRed_box.Text = "0";
                if (Double.Parse(transparentRed_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 1) transparentRed_box.Text = "1";
                transparentRed_slider.Value = Double.Parse(transparentRed_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                transparentRed_box.SelectionStart = transparentRed_box.Text.Length;
            }
            catch (Exception)
            {
                transparentRed_box.Text = transparentRed_slider.Value.ToString();
            }
        }

        private void transparentGreen_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (transparentGreen_box.Text == null || transparentGreen_box.Text == "") transparentGreen_box.Text = "0";
                if (Double.Parse(transparentGreen_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 1) transparentGreen_box.Text = "1";
                transparentGreen_slider.Value = Double.Parse(transparentGreen_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                transparentGreen_box.SelectionStart = transparentGreen_box.Text.Length;
            }
            catch (Exception)
            {
                transparentGreen_box.Text = transparentGreen_slider.Value.ToString();
            }
        }

        private void transparentBlue_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (transparentBlue_box.Text == null || transparentBlue_box.Text == "") transparentBlue_box.Text = "0";
                if (Double.Parse(transparentBlue_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 1) transparentBlue_box.Text = "1";
                transparentBlue_slider.Value = Double.Parse(transparentBlue_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                transparentBlue_box.SelectionStart = transparentBlue_box.Text.Length;
            }
            catch (Exception)
            {
                transparentBlue_box.Text = transparentBlue_slider.Value.ToString();
            }
        }

        private void g_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (g_box.Text == null || g_box.Text == "") g_box.Text = "0";
                if (Double.Parse(g_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 1000) g_box.Text = "1000";
                g_slider.Value = Double.Parse(g_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                g_box.SelectionStart = g_box.Text.Length;
            }
            catch (Exception)
            {
                g_box.Text = g_slider.Value.ToString();
            }
        }

        private void eta_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Double.Parse(eta_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 10) eta_box.Text = "10";
                eta_slider.Value = Double.Parse(eta_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                eta_box.SelectionStart = eta_box.Text.Length;
            }
            catch (Exception ex)
            {
                eta_box.Text = eta_slider.Value.ToString();
            }
        }

        private void name_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (material != null) material.name = name_box.Text;
        }


        private void ambientRed_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ambientRed_box.Text == null || ambientRed_box.Text == "") ambientRed_box.Text = "0";
                if (Double.Parse(ambientRed_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 1) ambientRed_box.Text = "1";
                ambientRed_slider.Value = Double.Parse(ambientRed_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                ambientRed_box.SelectionStart = ambientRed_box.Text.Length;
            }
            catch (Exception)
            {
                ambientRed_box.Text = ambientRed_slider.Value.ToString();
            }
        }

        private void ambientGreen_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ambientGreen_box.Text == null || ambientGreen_box.Text == "") ambientGreen_box.Text = "0";
                if (Double.Parse(ambientGreen_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 1) ambientGreen_box.Text = "1";
                ambientGreen_slider.Value = Double.Parse(ambientGreen_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                ambientGreen_box.SelectionStart = ambientGreen_box.Text.Length;
            }
            catch (Exception)
            {
                ambientGreen_box.Text = ambientGreen_slider.Value.ToString();
            }
        }

        private void ambientBlue_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ambientBlue_box.Text == null || ambientBlue_box.Text == "") ambientBlue_box.Text = "0";
                if (Double.Parse(ambientBlue_box.Text.Replace(",", "."), CultureInfo.InvariantCulture) > 1) ambientBlue_box.Text = "1";
                ambientBlue_slider.Value = Double.Parse(ambientBlue_box.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                ambientBlue_box.SelectionStart = ambientBlue_box.Text.Length;
            }
            catch (Exception)
            {
                ambientBlue_box.Text = ambientBlue_slider.Value.ToString();
            }
        }

        private void ambientRed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ambientRed_box.Text = ambientRed_slider.Value.ToString("F4", CultureInfo.InvariantCulture); ;
            material.kacR = (float)ambientRed_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void ambientGreen_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ambientGreen_box.Text = ambientGreen_slider.Value.ToString("F4", CultureInfo.InvariantCulture);
            material.kacG = (float)ambientGreen_slider.Value;

            if (!setsliders) RenderMaterial();
        }

        private void ambientBlue_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ambientBlue_box.Text = ambientBlue_slider.Value.ToString("F4", CultureInfo.InvariantCulture);
            material.kacB = (float)ambientBlue_slider.Value;

            if (!setsliders) RenderMaterial();
        }


        private void materialPreview_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Object"))
            {
                object data = e.Data.GetData("Object");
                Console.WriteLine(data.GetType());
                if (data is Surface)
                {
                    material = new Material_(((Surface)data).Material);
                    material.colorR = material.colorR * 255;
                    material.colorG = material.colorG * 255;
                    material.colorB = material.colorB * 255;
                    RenderMaterial();
                    SetSliders();
                    name_box.Text = material.name;
                }
                else if (data is Material_)
                {
                    material = new Material_(((Material_)data));
                    RenderMaterial();
                    SetSliders();
                    name_box.Text = material.name;
                }
            }
        }
    }
}
