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

namespace Modeler.Panels
{
    /// <summary>
    /// Interaction logic for CameraPanel.xaml
    /// </summary>
    public partial class CameraPanel : UserControl
    {
        int resX = 0, resY = 0, activeCam = 0;
        float posX = 0, posY = 0, posZ = 0, lookAtX = 0, lookAtY = 0, lookAtZ = 0, fovAngle = 0, rotateAngle = 0;

        public CameraPanel()
        {
            InitializeComponent();
            comboBox1.Items.Add("Kamera 1");
        }
        
        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            activeCam = comboBox1.SelectedIndex;
            
            if (activeCam > -1)
            {
                DependencyObject depObj = this.Parent;
                do
                {
                    depObj = LogicalTreeHelper.GetParent(depObj);
                } while (depObj.GetType() != typeof(MainWindow));
                MainWindow parent = (MainWindow)depObj;
            
                float[] cameraParam = new float[10];
                cameraParam = parent.cameraPanelActiveCamera(activeCam);
                resX = (int)cameraParam[0];
                textBox11.Text = resX.ToString();
                resY = (int)cameraParam[1];
                textBox12.Text = resY.ToString();
                posX = cameraParam[2];
                textBox4.Text = posX.ToString();
                posY = cameraParam[3];
                textBox5.Text = posY.ToString();
                posZ = cameraParam[4];
                textBox1.Text = posZ.ToString();
                lookAtX = cameraParam[5];
                textBox2.Text = lookAtX.ToString();
                lookAtY = cameraParam[6];
                textBox3.Text = lookAtY.ToString();
                lookAtZ = cameraParam[7];
                textBox6.Text = lookAtZ.ToString();
                rotateAngle = cameraParam[8];
                textBox7.Text = rotateAngle.ToString();
                slider1.Value = Double.Parse(textBox7.Text.Replace(".", ","));
                fovAngle = cameraParam[9];
                textBox8.Text = fovAngle.ToString();
                slider2.Value = Double.Parse(textBox8.Text.Replace(".", ","));
                parent.cameraPanelParChange(activeCam, resX, resY, posX, posY, posZ, lookAtX, lookAtY, lookAtZ, rotateAngle, fovAngle);
            }
        }

        private void textBox11_TextChanged(object sender, TextChangedEventArgs e)//res x
        {
            try
            {
                if (textBox11.Text == null || textBox11.Text == "") textBox11.Text = "0";
                resX = (int)Double.Parse(textBox11.Text);
                textBox11.SelectionStart = textBox11.Text.Length;
            }
            catch (Exception)
            {
                textBox11.Text = resX.ToString();
            }
        }

        private void textBox12_TextChanged(object sender, TextChangedEventArgs e)//res y
        {
            try
            {
                if (textBox12.Text == null || textBox12.Text == "") textBox12.Text = "0";
                resY = (int)Double.Parse(textBox12.Text);
                textBox12.SelectionStart = textBox12.Text.Length;
            }
            catch (Exception)
            {
                textBox12.Text = resY.ToString();
            }
        }

        private void textBox4_TextChanged(object sender, TextChangedEventArgs e)//camera center x
        {
            try
            {
                if (textBox4.Text == null || textBox4.Text == "") textBox4.Text = "0";
                posX = (float)Double.Parse(textBox4.Text);
                textBox4.SelectionStart = textBox4.Text.Length;
            }
            catch (Exception)
            {
                textBox4.Text = posX.ToString();
            }
        }

        private void textBox5_TextChanged(object sender, TextChangedEventArgs e)//camera center y
        {
            try
            {
                if (textBox5.Text == null || textBox5.Text == "") textBox5.Text = "0";
                posY = (float)Double.Parse(textBox5.Text);
                textBox5.SelectionStart = textBox5.Text.Length;
            }
            catch (Exception)
            {
                textBox5.Text = posY.ToString();
            }
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)//camera center z
        {
            try
            {
                if (textBox1.Text == null || textBox1.Text == "") textBox1.Text = "0";
                posZ = (float)Double.Parse(textBox1.Text);
                textBox1.SelectionStart = textBox1.Text.Length;
            }
            catch (Exception)
            {
                textBox1.Text = posZ.ToString();
            }
        }

        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)//camera look at x
        {
            try
            {
                if (textBox2.Text == null || textBox2.Text == "") textBox2.Text = "0";
                lookAtX = (float)Double.Parse(textBox2.Text);
                textBox2.SelectionStart = textBox2.Text.Length;
            }
            catch (Exception)
            {
                textBox2.Text = lookAtX.ToString();
            }
        }

        private void textBox3_TextChanged(object sender, TextChangedEventArgs e)//camera look at y
        {
            try
            {
                if (textBox3.Text == null || textBox3.Text == "") textBox3.Text = "0";
                lookAtY = (float)Double.Parse(textBox3.Text);
                textBox3.SelectionStart = textBox3.Text.Length;
            }
            catch (Exception)
            {
                textBox3.Text = lookAtY.ToString();
            }
        }

        private void textBox6_TextChanged(object sender, TextChangedEventArgs e)//camera look at z
        {
            try
            {
                if (textBox6.Text == null || textBox6.Text == "") textBox6.Text = "0";
                lookAtZ = (float)Double.Parse(textBox6.Text);
                textBox6.SelectionStart = textBox6.Text.Length;
            }
            catch (Exception)
            {
                textBox6.Text = lookAtZ.ToString();
            }
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)//camera rotate angle
        {
            if (textBox7 != null)
                textBox7.Text = slider1.Value.ToString();

            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;
            
            parent.cameraPanelParChange(activeCam, resX, resY, posX, posY, posZ, lookAtX, lookAtY, lookAtZ, rotateAngle, fovAngle);
        }

        private void textBox7_TextChanged(object sender, TextChangedEventArgs e)//camera rotate angle
        {
            try
            {
                if (textBox7.Text == null || textBox7.Text == "") textBox7.Text = "0";
                if (Double.Parse(textBox7.Text.Replace(".", ",")) > 180) textBox7.Text = "180";
                if (Double.Parse(textBox7.Text.Replace(".", ",")) < -180) textBox7.Text = "-180";
                rotateAngle = (float)Double.Parse(textBox7.Text);
                slider1.Value = Double.Parse(textBox7.Text.Replace(".", ","));
                textBox7.SelectionStart = textBox7.Text.Length;
            }
            catch (Exception)
            {
                textBox7.Text = rotateAngle.ToString();
            }
        }

        private void slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (textBox8 != null)
                textBox8.Text = slider2.Value.ToString();

            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;

            parent.cameraPanelParChange(activeCam, resX, resY, posX, posY, posZ, lookAtX, lookAtY, lookAtZ, rotateAngle, fovAngle);
        }

        private void textBox8_TextChanged(object sender, TextChangedEventArgs e)//kąt rozwarcia kamery
        {
            try
            {
                if (textBox8.Text == null || textBox8.Text == "") textBox8.Text = "40";
                if (Double.Parse(textBox8.Text.Replace(".", ",")) >= 180) textBox8.Text = "179.9999";
                if (Double.Parse(textBox8.Text.Replace(".", ",")) <= 0) textBox8.Text = "0.0001";
                slider2.Value = Double.Parse(textBox8.Text.Replace(".", ","));
                fovAngle = (float)Double.Parse(textBox8.Text);
                textBox8.SelectionStart = textBox8.Text.Length;
            }
            catch (Exception)
            {
                textBox8.Text = fovAngle.ToString();
            }
        }

        private void comboBox1_LostFocus(object sender, RoutedEventArgs e)
        {
            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;
            parent.cameraPanelParChange(activeCam, resX, resY, posX, posY, posZ, lookAtX, lookAtY, lookAtZ, rotateAngle, fovAngle);
        }

        private void textBox11_LostFocus(object sender, RoutedEventArgs e)//res x
        {
            textBox11.Text = resX.ToString();
            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;
            parent.cameraPanelParChange(activeCam, resX, resY, posX, posY, posZ, lookAtX, lookAtY, lookAtZ, rotateAngle, fovAngle);
        }

        private void textBox12_LostFocus(object sender, RoutedEventArgs e)//res y
        {
            textBox12.Text = resY.ToString();
            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;
            parent.cameraPanelParChange(activeCam, resX, resY, posX, posY, posZ, lookAtX, lookAtY, lookAtZ, rotateAngle, fovAngle);
        }

        private void textBox4_LostFocus(object sender, RoutedEventArgs e)// camera position x
        {
            textBox4.Text = posX.ToString();
            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;
            parent.cameraPanelParChange(activeCam, resX, resY, posX, posY, posZ, lookAtX, lookAtY, lookAtZ, rotateAngle, fovAngle);
        }

        private void textBox5_LostFocus(object sender, RoutedEventArgs e)//camera position y
        {
            textBox5.Text = posY.ToString();
            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;
            parent.cameraPanelParChange(activeCam, resX, resY, posX, posY, posZ, lookAtX, lookAtY, lookAtZ, rotateAngle, fovAngle);
        }

        private void textBox1_LostFocus(object sender, RoutedEventArgs e)//camera position z
        {
            textBox1.Text = posZ.ToString();
            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;
            parent.cameraPanelParChange(activeCam, resX, resY, posX, posY, posZ, lookAtX, lookAtY, lookAtZ, rotateAngle, fovAngle);
        }

        private void textBox2_LostFocus(object sender, RoutedEventArgs e)//look at x
        {
            textBox2.Text = lookAtX.ToString();
            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;
            parent.cameraPanelParChange(activeCam, resX, resY, posX, posY, posZ, lookAtX, lookAtY, lookAtZ, rotateAngle, fovAngle);
        }

        private void textBox3_LostFocus(object sender, RoutedEventArgs e)//look at y
        {
            textBox3.Text = lookAtY.ToString();
            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;
            parent.cameraPanelParChange(activeCam, resX, resY, posX, posY, posZ, lookAtX, lookAtY, lookAtZ, rotateAngle, fovAngle);
        }

        private void textBox6_LostFocus(object sender, RoutedEventArgs e)//look at z
        {
            textBox6.Text = lookAtZ.ToString();
            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;
            parent.cameraPanelParChange(activeCam, resX, resY, posX, posY, posZ, lookAtX, lookAtY, lookAtZ, rotateAngle, fovAngle);
        }

        private void textBox7_LostFocus(object sender, RoutedEventArgs e)//rotate
        {
            textBox7.Text = rotateAngle.ToString();
            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;
            parent.cameraPanelParChange(activeCam, resX, resY, posX, posY, posZ, lookAtX, lookAtY, lookAtZ, rotateAngle, fovAngle);
        }

        private void textBox8_LostFocus(object sender, RoutedEventArgs e)// kąt rozwarcia
        {
            textBox8.Text = fovAngle.ToString();
            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;
            parent.cameraPanelParChange(activeCam, resX, resY, posX, posY, posZ, lookAtX, lookAtY, lookAtZ, rotateAngle, fovAngle);
        }

        private void Grid_GotFocus(object sender, RoutedEventArgs e)//aktualizuje pozycje i punkt patrzenia kamery w przypadku modyfikacji interaktywnych w podgladzie sceny
        {
            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;

            comboBox1.SelectedIndex = parent.getActiveCam();  //ustawia kamere przy pierwszym odpaleniu panelu kamery, później nic nie zmienia 
        }

        public void newSceneLoaded()
        {
            comboBox1.SelectedIndex = -1;
            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;

            comboBox1.Items.Clear();
            for (int i = 0; i < parent.getCams(); i++)
            {
                comboBox1.Items.Add("Kamera " + (i + 1));
            }
            comboBox1.SelectedIndex = parent.getActiveCam();
        }

        public void cameraMoved()
        {
            DependencyObject depObj = this.Parent;
            do
            {
                depObj = LogicalTreeHelper.GetParent(depObj);
            } while (depObj.GetType() != typeof(MainWindow));
            MainWindow parent = (MainWindow)depObj;
            float[] tmp = new float[10];
            tmp = parent.cameraPanelActiveCamera(activeCam);

            resX = (int) tmp[0];
            textBox11.Text = resX.ToString();
            resY = (int) tmp[1];
            textBox12.Text = resY.ToString();
            posX = tmp[2];
            textBox4.Text = posX.ToString();
            posY = tmp[3];
            textBox5.Text = posY.ToString();
            posZ = tmp[4];
            textBox1.Text = posZ.ToString();
            lookAtX = tmp[5];
            textBox2.Text = lookAtX.ToString();
            lookAtY = tmp[6];
            textBox3.Text = lookAtY.ToString();
            lookAtZ = tmp[7];
            textBox6.Text = lookAtZ.ToString();
            fovAngle = tmp[9];
            textBox8.Text = fovAngle.ToString();
        }

        public void AngleChange(float newAngle)
        {
            slider2.Value = newAngle;
        }

    }
}
