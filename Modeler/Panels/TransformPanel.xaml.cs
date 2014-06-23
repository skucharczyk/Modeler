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
using Modeler.Transformations;
using Modeler;


namespace Modeler.Panels
{
    /// <summary>
    /// Interaction logic for TransformPanel.xaml
    /// </summary>
    public partial class TransformPanel : UserControl
    {
        float x1=0, x2=0, x3=1, y1=0, y2=0, y3=1, z1=0, z2=0, z3=1;

        public TransformPanel()
        {
            InitializeComponent();
            translateX_box.Text = x1.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            translateY_box.Text = y1.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            translateZ_box.Text = z1.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            rotateX_box.Text = x2.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            rotateY_box.Text = y2.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            rotateZ_box.Text = z2.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            scaleX_box.Text = x3.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            scaleY_box.Text = y3.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            scaleZ_box.Text = z3.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
        }

        private void translateX_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (translateX_box.Text == null || translateX_box.Text == "") translateX_box.Text = "0";
                x1 = (float)Double.Parse(translateX_box.Text, System.Globalization.CultureInfo.InvariantCulture);
                translateX_box.SelectionStart = translateX_box.Text.Length;
                translateX_box.Text = x1.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                translateX_box.Text = x1.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void translateY_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (translateY_box.Text == null || translateY_box.Text == "") translateY_box.Text = "0";
                y1 = (float)Double.Parse(translateY_box.Text, System.Globalization.CultureInfo.InvariantCulture);
                translateY_box.SelectionStart = translateY_box.Text.Length;
                translateY_box.Text = y1.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                translateY_box.Text = y1.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void translateZ_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (translateZ_box.Text == null || translateZ_box.Text == "") translateZ_box.Text = "0";
                z1 = (float)Double.Parse(translateZ_box.Text, System.Globalization.CultureInfo.InvariantCulture);
                translateZ_box.SelectionStart = translateZ_box.Text.Length;
                translateZ_box.Text = z1.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                translateZ_box.Text = z1.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void rotateX_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (rotateX_box.Text == null || rotateX_box.Text == "") rotateX_box.Text = "0";
                x2 = (float)Double.Parse(rotateX_box.Text, System.Globalization.CultureInfo.InvariantCulture);
                rotateX_box.SelectionStart = rotateX_box.Text.Length;
                rotateX_box.Text = x2.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                rotateX_box.Text = x2.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void roateteY_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (rotateY_box.Text == null || rotateY_box.Text == "") rotateY_box.Text = "0";
                y2 = (float)Double.Parse(rotateY_box.Text, System.Globalization.CultureInfo.InvariantCulture);
                rotateY_box.SelectionStart = rotateY_box.Text.Length;
                rotateY_box.Text = y2.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                rotateY_box.Text = y2.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void rotateZ_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (rotateZ_box.Text == null || rotateZ_box.Text == "") rotateZ_box.Text = "0";
                z2 = (float)Double.Parse(rotateZ_box.Text, System.Globalization.CultureInfo.InvariantCulture);
                rotateZ_box.SelectionStart = rotateZ_box.Text.Length;
                rotateZ_box.Text = z2.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                rotateZ_box.Text = z2.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void scaleX_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (scaleX_box.Text == null || scaleX_box.Text == "") scaleX_box.Text = "1";
                x3 = (float)Double.Parse(scaleX_box.Text, System.Globalization.CultureInfo.InvariantCulture);
                scaleX_box.SelectionStart = scaleX_box.Text.Length;
            }
            catch (Exception)
            {
                scaleX_box.Text = x3.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            }
            if (Double.Parse(scaleX_box.Text.Replace(".", ",")) < 0.0001) x3 = (float)0.0001;
            scaleX_box.Text = x3.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
        }

        private void scaleY_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (scaleY_box.Text == null || scaleY_box.Text == "") scaleY_box.Text = "1";
                y3 = (float)Double.Parse(scaleY_box.Text, System.Globalization.CultureInfo.InvariantCulture);
                scaleY_box.SelectionStart = scaleY_box.Text.Length;
            }
            catch (Exception)
            {
                scaleY_box.Text = y3.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            }
            if (Double.Parse(scaleY_box.Text.Replace(".", ",")) < 0.0001) y3 = (float)0.0001;
            scaleY_box.Text = y3.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
        }

        private void scaleZ_box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (scaleZ_box.Text == null || scaleZ_box.Text == "") scaleZ_box.Text = "1";
                z3 = (float)Double.Parse(scaleZ_box.Text, System.Globalization.CultureInfo.InvariantCulture);
                scaleZ_box.SelectionStart = scaleZ_box.Text.Length;
            }
            catch (Exception)
            {
                scaleZ_box.Text = z3.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            } 
            if (Double.Parse(scaleZ_box.Text.Replace(".", ",")) < 0.0001) z3 = (float)0.0001;
            scaleZ_box.Text = z3.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
        }

        private void button1_Clicked(object sender, RoutedEventArgs e)
        {
            DependencyObject depObj = this.Parent;

            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } 
            while (depObj.GetType() != typeof(MainWindow));

            MainWindow parent = (MainWindow)depObj;
            parent.transformPanelButtonClick(x1, y1, z1, x2, y2, z2, x3, y3, z3);
        }
    }
}
