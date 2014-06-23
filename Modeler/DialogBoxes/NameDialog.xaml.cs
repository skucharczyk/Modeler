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
using System.Windows.Shapes;

namespace Modeler.DialogBoxes
{
    /// <summary>
    /// Interaction logic for BezierNameDialog.xaml
    /// </summary>
    public partial class NameDialog : Window
    {
        private string _result;
        public string Result
        {
            get { return _result; }
            set { _result = value; }
        }

        public NameDialog()
        {
            InitializeComponent();
            //Message.Text = "Powierzchnia o podanej nazwie istnieje już w galerii. Akceptacja bez \nzmiany nazwy " +
            //                  " spowoduje nadpisanie powierzchni.";
            Message.IsReadOnly = true;
        }

        public bool? Show(String message, String name)
        {
            // NOTE: Message and Image are fields created in the XAML markup
            //BezierNameDialog msgBox = new BezierNameDialog();
            Message.Text = message;
            this.Name.Text = name;
            Name.Focus();
            return this.ShowDialog();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Result = Name.Text;
            this.DialogResult = true;
        }

        private void KeyUpEv(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
            }
        }
    }
}
