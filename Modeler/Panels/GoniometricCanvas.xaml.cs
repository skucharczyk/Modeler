using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Modeler.Panels
{
    /// <summary>
    /// Interaction logic for GoniometricCanvas.xaml
    /// </summary>
    public partial class GoniometricCanvas : UserControl
    {
        private SortedList<float, float> _goniometry;
        // Podane tutaj katy musza byc 2 razy wieksze niz te, ktore chcemy wyswietlac
        // a wiec jesli maksymalnym katem ma byc 180 musimy podac 360.
        private const float MinAngle = 0;
        private const float MaxAngle = 360;
        private const float MinY = 0;
        private const float MaxY = 1;
        private readonly Typeface _textTypeface = new Typeface("Verdana");
        private bool _initialized = false;
        private readonly Brush _backgroundBrush = Brushes.Gray;
        private readonly Pen _gridLineColor = new Pen(Brushes.Black, 0.5);
        private readonly Brush _textBrush = Brushes.White;
        private readonly Brush _pointBrush = Brushes.White;
        private const int HorizontalLines = 4;
        private const int VerticalLines = 7;
        private static float _horizontalDet;
        private static float _verticalDet;
        private static float _verticalAngleDet;
        private const int AngleAccuracy = 3;
        private const float ValueAccuracy = 0.05f;
        private const int AngleSteps = 4;
        private const float TextSize = 10f;
        // margines wyświetlany jest z dołu oraz z lewej i prawej
        private new const float Margin = 10f;
        private float rightClickX;
        private float rightClickY;
        private ContextMenu menu;

        public SortedList<float, float> Goniometry
        {
            set
            {
                _goniometry = value;
            }
        }

        public GoniometricCanvas()
        {
            InitializeComponent();
            menu = new ContextMenu();
            MenuItem item1 = new MenuItem();
            item1.Header = "Usuń";
            item1.Click += new RoutedEventHandler(DeleteClicked);
            MenuItem item2 = new MenuItem();
            item2.Click += new RoutedEventHandler(ResetClicked);
            item2.Header = "Zresetuj punkty";
            menu.Items.Add(item1);
            menu.Items.Add(item2);
            this.ContextMenu = menu;
        }

        private bool GetControlPressed()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            DrawGrid(drawingContext);
            DrawChart(drawingContext);
            UpdateParent();
        }

        private void DrawGrid(DrawingContext drawingContext)
        {
            Point p1 = new Point();
            Point p2 = new Point();
            // Inicjalizacja koloru tła
            drawingContext.DrawRectangle(_backgroundBrush, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

            if (!_initialized)
            {
                _horizontalDet = (float)((MaxY * (this.ActualHeight - Margin) - MinY * (this.ActualHeight-Margin)) / (HorizontalLines - 1));
                _verticalAngleDet = ((MaxAngle - MinAngle) / (VerticalLines-1));
                _verticalDet = (float) ((this.ActualWidth - 2*Margin)*_verticalAngleDet/(MaxAngle - MinAngle));
                _initialized = true;
            }

            // Rysowanie podzialki na x i y
            // Podzialka osi Y
            for (int i = 0; i < HorizontalLines; i++)
            {
                p1.X = Margin;
                p1.Y = (this.ActualHeight - Margin) - i * _horizontalDet;
                p2.X = this.ActualWidth - Margin;
                p2.Y = p1.Y;
                drawingContext.DrawLine(_gridLineColor, p1, p2);
            }
            // Podzialka osi X
            for (int i = 0; i < VerticalLines; i++)
            {
                p1.X = i * _verticalDet + Margin;
                p1.Y = 0;
                p2.X = p1.X;
                p2.Y = this.ActualHeight - Margin;
                drawingContext.DrawLine(_gridLineColor, p1, p2);
            }
            // Podpisywanie osi i niektorych wartosci
            FormattedText text;
            float angle = MinAngle;
            const float angleDet = (MaxAngle - MinAngle)/2/(AngleSteps-1);
            float x = Margin;
            float detX = (float) (this.ActualWidth-2*Margin)/(AngleSteps-1);
            for (int i = 0; i < AngleSteps; i++)
            {
                text = new FormattedText(angle.ToString(), System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, _textTypeface, TextSize, _textBrush);
                drawingContext.DrawText(text, new Point(x - (float)text.Width/2, this.ActualHeight-TextSize-2));
                x += detX;
                angle += angleDet;
            }
        }

        private void DrawChart(DrawingContext drawingContext)
        {
            if (_goniometry != null && _goniometry.Count > 0)
            {
                Point p1 = new Point();
                Point p2 = new Point();
                float tmp;

                float centrAngle = (MinAngle + MaxAngle) / 4;
                float range = (MaxAngle - MinAngle) / 4;
                for (int i = 0; i < _goniometry.Count - 1; i++)
                {
                    //tmp = (((maxAngle + minAngle) / 2) + Goniometry.Keys[i]) / (centrAngle);k
                    tmp = (_goniometry.Keys[i] - centrAngle) / range;
                    p1.X = (this.ActualWidth - 2*Margin)/2 + tmp*(this.ActualWidth - 2*Margin)/2;
                    p1.Y = (this.ActualHeight - Margin)*((MaxY - MinY) - _goniometry.Values[i]*(MaxY - MinY));

                    tmp = (_goniometry.Keys[i + 1] - centrAngle) / range;
                    p2.X = (this.ActualWidth - 2*Margin)/2 + tmp*(this.ActualWidth - 2*Margin)/2;
                    p2.Y = (this.ActualHeight - Margin)*((MaxY - MinY) - _goniometry.Values[i + 1]*(MaxY - MinY));

                    p1.X += Margin;
                    p2.X += Margin;

                    drawingContext.DrawLine(_gridLineColor, p1, p2);
                    drawingContext.DrawEllipse(_pointBrush, null, p1, 3, 3);
                }
                // Rysowanie ostatniego punktu
                tmp = (_goniometry.Keys[_goniometry.Count - 1] - centrAngle) / range;
                p1.X = (this.ActualWidth - 2*Margin)/2 + tmp*(this.ActualWidth - 2*Margin)/2;
                p1.Y = (this.ActualHeight - Margin) * ((MaxY - MinY) - _goniometry.Values[_goniometry.Count - 1] * (MaxY - MinY));
                p1.X += Margin;
                drawingContext.DrawEllipse(_pointBrush, null, p1, 3, 3);
            }
        }

        private void UpdateParent()
        {
            DependencyObject parentObj = VisualTreeHelper.GetParent(this);
            if (parentObj == null) return;
            while (!(parentObj is LightsPanel))
            {
                parentObj = VisualTreeHelper.GetParent(parentObj);
            }
            LightsPanel parent = (LightsPanel)parentObj;
            parent.RenderLight();
        }

        private void GoniometricCanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            float x = (float) e.GetPosition(this).X - Margin;
            float y = (float) e.GetPosition(this).Y;

            if (x<0 || x>this.ActualWidth-2*Margin || y >this.ActualHeight-Margin)
                return;

            int angle =
                (int)
                ((MaxAngle + MinAngle)/4 +
                 ((x - (this.ActualWidth - 2*Margin)/2)/(this.ActualWidth - 2*Margin)*(MaxAngle - MinAngle)/2));

            if (!GetControlPressed())
            {
                float value = MaxY - MinY - (float)(y / (this.ActualHeight - Margin)) * (MaxY - MinY);
                if (_goniometry.IndexOfKey(angle) == -1)
                    _goniometry.Add(angle, value);
                this.InvalidateVisual();
            }
            else
            {
                int idx = -1;
                for (int i = 0; i < _goniometry.Count; i++)
                {
                    if (angle > _goniometry.Keys[i] - 5 && angle < _goniometry.Keys[i] + 5)
                        idx = i;
                }
                if (idx != -1 && idx != 0 && idx != _goniometry.Count - 1)
                {
                    _goniometry.RemoveAt(idx);
                    this.InvalidateVisual();
                }
            }
        }

        public void GoniometricCanvasDragOver(object sender, DragEventArgs e)
        {
            float x = (float) e.GetPosition(this).X - Margin;
            float y = (float) e.GetPosition(this).Y;

            if (x < 0 || x > this.ActualWidth - 2 * Margin || y > this.ActualHeight - Margin)
                return;

            float newKey = 0;
            float newVal = 0;
            int idx = (int)e.Data.GetData("Point");

            newKey =
                (int)
                ((MaxAngle + MinAngle)/4 +
                 ((x - (this.ActualWidth - 2*Margin)/2)/(this.ActualWidth - 2*Margin)*(MaxAngle - MinAngle)/2));
            newVal = MaxY - MinY - (float)(y / (this.ActualHeight - Margin)) * (MaxY - MinY);

            if (idx == 0)
            {
                newKey = MinAngle/2;
            }
            else if (idx == _goniometry.Count - 1)
            {
                newKey = MaxAngle/2;
            }
            else
            {
                newKey = newKey > MaxAngle ? MaxAngle : newKey < MinAngle ? MinAngle : newKey;
            }

            newVal = newVal > MaxY ? MaxY : newVal < MinY ? MinY : newVal;

            _goniometry.RemoveAt(idx);
            if (!_goniometry.ContainsKey(newKey))
                _goniometry.Add(newKey, newVal);
            e.Data.SetData("Point", _goniometry.IndexOfKey(newKey));
            this.InvalidateVisual();

            e.Handled = true;
        }

        private void GoniometricCanvasMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            rightClickX = (float) e.GetPosition(this).X - Margin;
            rightClickY = (float) e.GetPosition(this).Y;
        }

        private void DeleteClicked(object sender, RoutedEventArgs e)
        {
            float x = rightClickX;
            float y = rightClickY;

            if (x < 0 || x > this.ActualWidth - 2 * Margin || y > this.ActualHeight - Margin)
                return;

            int angle =
                (int)
                ((MaxAngle + MinAngle) / 4 +
                 ((x - (this.ActualWidth - 2 * Margin) / 2) / (this.ActualWidth - 2 * Margin) * (MaxAngle - MinAngle) / 2));

            int idx = -1;
            for (int i = 0; i < _goniometry.Count; i++)
            {
                if (angle > _goniometry.Keys[i] - 5 && angle < _goniometry.Keys[i] + 5)
                    idx = i;
            }
            if (idx != -1 && idx != 0 && idx != _goniometry.Count - 1)
            {
                _goniometry.RemoveAt(idx);
                this.InvalidateVisual();
            }
        }

        private void ResetClicked(object sender, RoutedEventArgs e )
        {
            while (_goniometry.Count>0)
            {
                _goniometry.RemoveAt(0);
            }
            _goniometry.Add(0, 1);
            _goniometry.Add(180, 1);
            InvalidateVisual();
        }

        private void GoniometricCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!GetControlPressed())
            {
                float x = (float) e.GetPosition(this).X - Margin;
                float y = (float) e.GetPosition(this).Y;

                if (x < 0 || x > this.ActualWidth - 2*Margin || y > this.ActualHeight - Margin)
                    return;

                float minValue = (float) (MaxY - MinY - (float) (y/(this.ActualHeight - Margin))*(MaxY - MinY)) -
                                 ValueAccuracy;
                float maxValue = (float) (MaxY - MinY - (float) (y/(this.ActualHeight - Margin))*(MaxY - MinY)) +
                                 ValueAccuracy;

                DataObject dataObject = new DataObject();

                // Znalezc trafiony punkt
                // Wyslac indeks punktu do dragover
                int idx = -1;
                int angle =
                    (int)
                    ((MaxAngle + MinAngle)/4 +
                     ((x - (this.ActualWidth - 2*Margin)/2)/(this.ActualWidth - 2*Margin)*(MaxAngle - MinAngle)/2));

                for (int i = 0; i < _goniometry.Count; i++)
                {
                    if (angle > _goniometry.Keys[i] - AngleAccuracy && angle < _goniometry.Keys[i] + AngleAccuracy)
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx != -1 && _goniometry.Values[idx] > minValue && _goniometry.Values[idx] < maxValue)
                {
                    Console.WriteLine(idx);
                    dataObject.SetData("Point", idx);
                    DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Move);
                }
            }
        }
    }
}