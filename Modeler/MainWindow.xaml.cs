using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms;
using Modeler.DialogBoxes;
using Modeler.Undo;
using SlimDX;
using Modeler.Data.Scene;
using Modeler.Data;
using Modeler.Data.Elements;
using Modeler.Data.Shapes;
using Modeler.Graphics;
using Modeler.Data.Galleries;
using Modeler.Data.Surfaces;
using Modeler.Data.Light;
using System.Globalization;
using ContextMenu = System.Windows.Forms.ContextMenu;
using Cursor = System.Windows.Input.Cursor;
using System.Windows.Media;
using DataObject = System.Windows.DataObject;

namespace Modeler
{
    public enum ViewportOrientation { None = -1, Perspective = 3, Front = 0, Side = 1, Top = 2 };

    public enum ContextItem
    {
        Grab = 0,
        Rotate = 1,
        Scale = 2,
        ScaleDimension = 3
    };

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Renderer renderer;
        private ViewportOrientation maxViewport;
        private const int xOffset = 16;
        private const int yOffset = 86;
        private const float undefined = -36000;
        private Scene currScene;
        private BezierSurface bezierSurface;

        private Undo.UndoStack undo = new Undo.UndoStack();
        private Undo.BezierUndoStack bezierUndo = new BezierUndoStack();

        private ShapeGallery _shapesGallery;
        private PreparedObjectsGallery _elementsGallery;
        private SurfaceGallery _surfaceGallery;
        private LightGallery _lightGallery;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.ContextMenu bezierContextMenu;

        private float dragX, dragY, directionX, directionY, angle;
        private int mRCx, mRCy;  // mouse right button click position
        private Vector3D center;

        private Point mousePos;
        private bool mouseDownHandled;
        private ViewportOrientation shiftingViewport;
        private ViewportOrientation shiftingBezierViewport;
        private static Vector3D trash;

        private CopyPaste.CopyPaste copyPaste;

        static System.Diagnostics.PerformanceCounter pc = new System.Diagnostics.PerformanceCounter("Memory", "Available Bytes");

        private const String bezierMessage = "Powierzchnia o podanej nazwie istnieje już w galerii. Akceptacja bez \nzmiany nazwy " +
                               " spowoduje nadpisanie powierzchni.";
        private const String materialMessage = "Materiał o podanej nazwie istnieje już w galerii. Akceptacja bez \nzmiany nazwy " +
                               " spowoduje nadpisanie materiału.";
        private const String lightMessage = "Światło o podanej nazwie istnieje już w galerii. Akceptacja bez \nzmiany nazwy " +
                               " spowoduje nadpisanie światła.";
        private const String lightSceneMessage = "Światło o podanej nazwie istnieje na scenie. Akceptacja bez \nzmiany nazwy " +
                               " spowoduje nadpisanie światła.";
        private const String elementMessage = "Obiekt o podanej nazwie istnieje już w galerii. Akceptacja bez\nzmiany nazwy " +
                               " spowoduje nadpisanie obiektu.";
        private const String materialSceneMessage = "Materiał o podanej nazwie istnieje już w scenie. Akceptacja bez \nzmiany nazwy " +
                               " spowoduje nadpisanie materiału.";

        private bool canDrop = false;
        private bool emptyNodesRemoved = false;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
            } 
            catch (Exception e)
            { 
            }


            bezierSurface = new BezierSurface("new surface", "");
            bezierSurface.Triangulate(0.5f);

            renderer = new Renderer(Views.Handle, ViewsBezier.Handle);
            maxViewport = ViewportOrientation.None;
            mouseDownHandled = false;
            shiftingViewport = ViewportOrientation.None;
            shiftingBezierViewport = ViewportOrientation.None;
            //currScene = Scene.GetExampleScene();
            currScene = new Scene();

            // Tworzenie kolekcji obiektów i dodawanie jej do kontrolki ItemsControl
            // galerii ksztaltow.
            _shapesGallery = new ShapeGallery();
            // Wczytywanie powierzchni beziera
            ksztalty_ListView.ItemsSource = _shapesGallery;

            // Tworzenie kolekcji gotowych obiektów i dodawanie jej do kontrolki ItemsControl
            // galerii gotowych obiektów.
            _elementsGallery = new PreparedObjectsGallery();
            gotowe_ListView.ItemsSource = _elementsGallery;

            _surfaceGallery = new SurfaceGallery();
            materialy_ListView.ItemsSource = _surfaceGallery;

            _lightGallery = new LightGallery();
            swiatla_ListView.ItemsSource = _lightGallery;

            //Menu kontekstowe
            contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.MenuItems.Add("Przenieś", contextMenuClick);
            contextMenu.MenuItems.Add("Obróć", contextMenuClick);
            contextMenu.MenuItems.Add("Skaluj", contextMenuClick);
            contextMenu.MenuItems.Add("Skaluj wzdłuż osi", contextMenuClick);
            contextMenu.MenuItems.Add("Powiększ widok", contextViewport);
            contextMenu.MenuItems.Add("Zapisz gotowy element", contextSave);
            System.Windows.Forms.MenuItem[] subMenu = new System.Windows.Forms.MenuItem[4];
            System.Windows.Forms.MenuItem subLeft = new System.Windows.Forms.MenuItem("Lewej", subLeftClick);
            System.Windows.Forms.MenuItem subRight = new System.Windows.Forms.MenuItem("Prawej", subRightClick);
            System.Windows.Forms.MenuItem subUp = new System.Windows.Forms.MenuItem("Góry", subUpClick);
            System.Windows.Forms.MenuItem subDown = new System.Windows.Forms.MenuItem("Dołu", subDownClick);
            subMenu[0] = subLeft;
            subMenu[1] = subRight;
            subMenu[2] = subUp;
            subMenu[3] = subDown;
            contextMenu.MenuItems.Add("Dosuń do", subMenu);
            contextMenu.MenuItems.Add("Kopiuj", contextCopy);
            contextMenu.MenuItems.Add("Wklej", contextPaste);
            contextMenu.MenuItems.Add("Usuń", contextDelete);
            contextMenu.MenuItems[0].Checked = true;
            Views.ContextMenu = contextMenu;

            //Menu kontekstowe dla płatów beziera
            bezierContextMenu = new ContextMenu();
            bezierContextMenu.MenuItems.Add("Zresetuj punkty", bezierContextMenuClick);
            bezierContextMenu.MenuItems.Add("Nowa powierzchnia", bezierContextMenuNewSurface);
            ViewsBezier.ContextMenu = bezierContextMenu;

            copyPaste = new CopyPaste.CopyPaste();
        }

        ViewportInfo GetViewBezierCoords()
        {
            return new ViewportInfo(ViewsBezier.Size.Width, ViewsBezier.Size.Height, new int[] { 0 }, new int[] { 0 },
                new int[] { ViewsBezier.Size.Width }, new int[] { ViewsBezier.Size.Height });
        }

        ViewportInfo GetViewCoords(int selectedTab)
        {
            ViewportInfo viewInfo = new ViewportInfo();

            if (selectedTab == 0)
            {
                viewInfo.resX = Views.Size.Width;
                viewInfo.resY = Views.Size.Height;
            }
            else
            {
                viewInfo.resX = ViewsBezier.Size.Width;
                viewInfo.resY = ViewsBezier.Size.Height;
            }

            int panelX = 0;
            int panelY = 0;

            viewInfo.posX = new int[4];
            viewInfo.posY = new int[4];
            viewInfo.sizeX = new int[4];
            viewInfo.sizeY = new int[4];

            if(maxViewport == ViewportOrientation.None || selectedTab != 0)
            {
                int elemWidth;
                int elemHeight;
                if (selectedTab == 0)
                {
                    elemWidth = (int)(Views.Size.Width / 2) - 1;
                    elemHeight = (int)(Views.Size.Height / 2) - 1;
                }
                else
                {
                    elemWidth = (int)(ViewsBezier.Size.Width / 2) - 1;
                    elemHeight = (int)(ViewsBezier.Size.Height / 2) - 1;
                }


                viewInfo.posX[0] = panelX;
                viewInfo.posY[0] = panelY;
                viewInfo.sizeX[0] = elemWidth;
                viewInfo.sizeY[0] = elemHeight;

                viewInfo.posX[1] = panelX + elemWidth + 2;
                viewInfo.posY[1] = panelY;
                viewInfo.sizeX[1] = viewInfo.resX - viewInfo.posX[1];
                viewInfo.sizeY[1] = elemHeight;

                viewInfo.posX[2] = panelX;
                viewInfo.posY[2] = panelY + elemHeight + 2;
                viewInfo.sizeX[2] = elemWidth;
                viewInfo.sizeY[2] = viewInfo.resY - viewInfo.posY[2];

                viewInfo.posX[3] = panelX + elemWidth + 2;
                viewInfo.posY[3] = panelY + elemHeight + 2;
                viewInfo.sizeX[3] = viewInfo.resX - viewInfo.posX[3];
                viewInfo.sizeY[3] = viewInfo.resY - viewInfo.posY[3];
            }
            else
            {
                for(int i = 0; i < 4; ++i)
                {
                    viewInfo.posX[i] = viewInfo.posY[i] = viewInfo.sizeX[i] = viewInfo.sizeY[i] = 0;
                }
                if (selectedTab == 0)
                {
                    viewInfo.sizeX[(int)maxViewport] = Views.Size.Width;
                    viewInfo.sizeY[(int)maxViewport] = Views.Size.Height;
                }
                else
                {
                    viewInfo.sizeX[(int)maxViewport] = ViewsBezier.Size.Width;
                    viewInfo.sizeY[(int)maxViewport] = ViewsBezier.Size.Height;
                }
            }

            return viewInfo;
        }

        public ViewportOrientation GetViewportType(int x, int y)
        {
            ViewportInfo views = GetViewCoords(tabWidok.SelectedIndex);

            int rect = -1;

            for(int i = 0; i < 4 && rect == -1; ++i)
            {
                if(views.posX[i] <= x && x < views.posX[i] + views.sizeX[i] && views.posY[i] <= y && y < views.posY[i] + views.sizeY[i])
                {
                    rect = i;
                }
            }

            ViewportOrientation viewport = ViewportOrientation.None;
            switch(rect)
            {
                case 0:
                    viewport = ViewportOrientation.Front;
                    break;

                case 1:
                    viewport = ViewportOrientation.Side;
                    break;

                case 2:
                    viewport = ViewportOrientation.Top;
                    break;

                case 3:
                    viewport = ViewportOrientation.Perspective;
                    break;

                case -1:
                    viewport = ViewportOrientation.None;
                    break;

            }

            return viewport;
        }

        private bool GetCtrlPressed()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl);
        }

        private bool GetAltPressed()
        {
            return Keyboard.IsKeyDown(Key.LeftAlt);
        }

        private bool GetShiftPressed()
        {
            return Keyboard.IsKeyDown(Key.LeftShift);
        }

        private void subLeftClick(object sender, System.EventArgs e)
        {
            undo.Save(currScene);

            ViewportOrientation viewport = GetViewportType(mRCx, mRCy);
            switch(viewport)
            {
                case ViewportOrientation.Front:
                    //if (DosuwanieCB.IsChecked == true)
                        Transformations.Intersection.Slide(currScene, "xyl");
                   // else
                        //Transformations.Intersection.SlideEZver(currScene, "xyl");
                    //Console.WriteLine("Dosuwanie w lewo : lewy dolny - xyl");
                    break;

                case ViewportOrientation.Side:
                    //if (DosuwanieCB.IsChecked == true)
                        Transformations.Intersection.Slide(currScene, "zxf");
                    //else
                        //Transformations.Intersection.SlideEZver(currScene, "zxf");
                    //Console.WriteLine("Dosuwanie w lewo : prawy dolny - zxf");
                    break;

                case ViewportOrientation.Top:
                    //if(DosuwanieCB.IsChecked==true)
                        Transformations.Intersection.Slide(currScene, "xyl");
                    //else
                        //Transformations.Intersection.SlideEZver(currScene, "xyl");
                    //Console.WriteLine("Dosuwanie w lewo : prawy górny - xyl");
                    break;

                case ViewportOrientation.Perspective:
                    //Console.WriteLine("Brak opcji dosuwania w tym widoku. Dosuwanie do lewej.");
                    break;

                default:
                    break;
            }
            currScene.selectedHierObj = null;
            RenderViews();
        }

        private void subRightClick(object sender, System.EventArgs e)
        {
            undo.Save(currScene);

            ViewportOrientation viewport = GetViewportType(mRCx, mRCy);
            switch(viewport)
            {
                case ViewportOrientation.Front:
                    //if (DosuwanieCB.IsChecked == true)
                        Transformations.Intersection.Slide(currScene, "xyr");
                    //else
                        //Transformations.Intersection.SlideEZver(currScene, "xyr");
                    //Console.WriteLine("Dosuwanie w prawo: lewy dolny - xyr");
                    break;

                case ViewportOrientation.Side:
                    //if (DosuwanieCB.IsChecked == true)
                        Transformations.Intersection.Slide(currScene, "zxb");
                    //else
                       // Transformations.Intersection.SlideEZver(currScene, "zxb");
                    //Console.WriteLine("Dosuwanie w prawo : prawy dolny - zxb");
                    break;

                case ViewportOrientation.Top:
                    //if (DosuwanieCB.IsChecked == true)
                        Transformations.Intersection.Slide(currScene, "xyr");
                    //else
                        //Transformations.Intersection.SlideEZver(currScene, "xyr");
                    //Console.WriteLine("Dosuwanie w prawo : prawy górny - xyr");
                    break;

                case ViewportOrientation.Perspective:
                    //.WriteLine("Brak opcji dosuwania w tym widoku. Dosuwanie do prawej.");
                    break;

                default:
                    break;
            }
            currScene.selectedHierObj = null;
            RenderViews();
        }

        private void subUpClick(object sender, System.EventArgs e)
        {
            undo.Save(currScene);

            ViewportOrientation viewport = GetViewportType(mRCx, mRCy);
            switch(viewport)
            {
                case ViewportOrientation.Front:
                    //if (DosuwanieCB.IsChecked == true)
                        Transformations.Intersection.Slide(currScene, "yzu");
                    //else
                        //Transformations.Intersection.SlideEZver(currScene, "yzu");
                    //Console.WriteLine("Dosuwanie do góry : lewy dolny - yzu");
                    break;

                case ViewportOrientation.Side:
                    //if (DosuwanieCB.IsChecked == true)
                        Transformations.Intersection.Slide(currScene, "yzu");
                    //else
                        //Transformations.Intersection.SlideEZver(currScene, "yzu");
                    //Console.WriteLine("Dosuwanie do góry : prawy dolny - yzu");
                    break;

                case ViewportOrientation.Top:
                    //if (DosuwanieCB.IsChecked == true)
                        Transformations.Intersection.Slide(currScene, "zxb");
                    //else
                        //Transformations.Intersection.SlideEZver(currScene, "zxb");
                    //Console.WriteLine("Dosuwanie do góry : prawy górny - zxb");
                    break;

                case ViewportOrientation.Perspective:
                    //Console.WriteLine("Brak opcji dosuwania w tym widoku. Dosuwanie do góry.");
                    break;

                default:
                    break;
            }
            currScene.selectedHierObj = null;
            RenderViews();
        }

        private void subDownClick(object sender, System.EventArgs e)
        {
            undo.Save(currScene);

            ViewportOrientation viewport = GetViewportType(mRCx, mRCy);
            switch(viewport)
            {
                case ViewportOrientation.Front:
                    //if (DosuwanieCB.IsChecked == true)
                        Transformations.Intersection.Slide(currScene, "yzd");
                    //else
                       // Transformations.Intersection.SlideEZver(currScene, "yzd");
                    //Console.WriteLine("Dosuwanie do dołu : lewy dolny - yzd");
                    break;

                case ViewportOrientation.Side:
                    //if (DosuwanieCB.IsChecked == true)
                        Transformations.Intersection.Slide(currScene, "yzd");
                    //else
                        //Transformations.Intersection.SlideEZver(currScene, "yzd");
                    //Console.WriteLine("Dosuwanie do dołu : prawy dolny - yzd");
                    break;

                case ViewportOrientation.Top:
                    //if (DosuwanieCB.IsChecked == true)
                        Transformations.Intersection.Slide(currScene, "zxf");
                    //else
                        //Transformations.Intersection.SlideEZver(currScene, "zxf");
                    //Console.WriteLine("Dosuwanie do dołu : prawy górny - zxf");
                    break;

                case ViewportOrientation.Perspective:
                    //Console.WriteLine("Brak opcji dosuwania w tym widoku. Dosuwanie w dół.");
                    break;

                default:
                    break;
            }
            currScene.selectedHierObj = null;
            RenderViews();
        }

        private void contextSave(object sender, System.EventArgs e)
        {
            if (currScene.selTriangles.Count > 0)
            {
                Scene scene = currScene.SceneFromSelection(out trash);
                System.Drawing.Image sceneImg = renderer.GetSceneImage(scene);
                PreparedElement elem = new PreparedElement("", "", scene);
                bool replace = false;

                Modeler.DialogBoxes.NameDialog newName = new NameDialog();
                bool res = (bool)newName.Show("Podaj nazwę obiektu", "");
                if (!res)
                    return;
                elem.Name = newName.Result;

                while (_elementsGallery.GetNames().Contains(elem.Name, StringComparer.OrdinalIgnoreCase) && !replace)
                {
                    Modeler.DialogBoxes.NameDialog dialog = new NameDialog();
                    dialog.Owner = this;

                    bool result = (bool)dialog.Show(elementMessage, elem.Name);
                    string name = dialog.Result;
                    if (!result)
                        return;
                    if (elem.Name == name)
                        replace = true;
                    elem.Name = name;
                    dialog.Close();
                }

                if (replace)
                {
                    int idx = _elementsGallery.GetNameIdx(elem.Name);
                    _elementsGallery.RemoveAt(idx);
                    _elementsGallery.SaveObjectToGallery(elem, sceneImg);
                    _elementsGallery.Insert(idx, elem);
                }
                else
                {
                    
                    _elementsGallery.SaveObjectToGallery(elem, sceneImg);
                    _elementsGallery.Add(elem);
                }
            }
        }

        private void contextCopy(object sender, System.EventArgs e)
        {
            Copy(this, new RoutedEventArgs());
        }

        private void contextPaste(object sender, System.EventArgs e)
        {
            Paste(this, new RoutedEventArgs());
        }

        private void contextDelete(object sender, System.EventArgs e)
        {
            Delete(this, new RoutedEventArgs());
        }

        private void contextMenuClick(object sender, System.EventArgs e)
        {
            for (int i = 0; i < 4; i++) contextMenu.MenuItems[i].Checked = false;
            ((System.Windows.Forms.MenuItem)sender).Checked = true;

            if (contextMenu.MenuItems[0].Checked == true)
            {
                modeTBG1.Text = "Przenoszenie";
                modeTBG2.Text = "Przenoszenie";
                modeTBG3.Text = "Przenoszenie";
                modeTBG4.Text = "Przenoszenie";
                ChangeContextSelection((ContextItem)0);
            }
            else if (contextMenu.MenuItems[1].Checked == true)
            {
                modeTBG1.Text = "Obracanie";
                modeTBG2.Text = "Obracanie";
                modeTBG3.Text = "Obracanie";
                modeTBG4.Text = "Obracanie";
                ChangeContextSelection((ContextItem)1);
            }
            else if (contextMenu.MenuItems[2].Checked == true)
            {
                modeTBG1.Text = "Skalowanie";
                modeTBG2.Text = "Skalowanie";
                modeTBG3.Text = "Skalowanie";
                modeTBG4.Text = "Skalowanie";
                ChangeContextSelection((ContextItem)2);
            }
            else if (contextMenu.MenuItems[3].Checked == true)
            {
                modeTBG1.Text = "Skalowanie wzdłuż osi";
                modeTBG2.Text = "Skalowanie wzdłuż osi";
                modeTBG3.Text = "Skalowanie wzdłuż osi";
                modeTBG4.Text = "Skalowanie wzdłuż osi";
                ChangeContextSelection((ContextItem)3);
            } 
        }

        private void bezierContextMenuClick(object sender, System.EventArgs e)
        {
            bezierSurface.ResetControlPoints();
            RenderBezier();
        }

        private void bezierContextMenuNewSurface(object sender, System.EventArgs e)
        {
            bezierSurface = new BezierSurface("new surface", "");
            bezierSurface.Triangulate((float)triang_Slider.Value);
            bezierUndo = new BezierUndoStack();
            RenderBezier();
        }

        private void contextViewport(object sender, System.EventArgs e)
        {
            if (maxViewport == ViewportOrientation.None)
            {
                ViewportOrientation rect = GetViewportType(mRCx, mRCy);

                if (rect != ViewportOrientation.None)
                {
                    maxViewport = rect;
                    contextMenu.MenuItems[4].Text = "Zmniejsz widok";

                    RenderViews();
                }
            }
            else
            {
                maxViewport = ViewportOrientation.None;
                contextMenu.MenuItems[4].Text = "Powiększ widok";

                RenderViews();
            }
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            if (currScene.modified)
            {
                MessageBoxResult ifSave = System.Windows.MessageBox.Show("Czy chesz zapisać bieżącą scenę ?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ifSave == MessageBoxResult.Yes)
                {
                    SaveFileAs(sender, e);
                }
            }

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "Pliki sceny |*.scn|Wszystkie pliki |*.*";
            //dlg.InitialDirectory = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                undo = null;
                undo = new UndoStack();
                Scene fileScene = Scene.ReadSceneFromFile(dlg.FileName);
                if (fileScene != null)
                {
                    currScene = fileScene;

                    Renderer.RecalculateData(currScene);

                    cameraPan.newSceneLoaded();
                    cameraPan.comboBox1.SelectedIndex = currScene.activeCamera;

                    RenderViews();
                }
                else
                {
                    System.Windows.MessageBox.Show("Wybrany plik ma niepoprawny format.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                trNumTBG1.Text = currScene.triangles.Count().ToString();
                trNumTBG2.Text = currScene.triangles.Count().ToString();
                trNumTBG3.Text = currScene.triangles.Count().ToString();
                trNumTBG4.Text = currScene.triangles.Count().ToString();
            }
            currScene.selectedHierObj = null;
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            if (currScene.filePath != null)
            {
                currScene.SaveSceneFile(currScene.filePath);
            }
            else
            {
                SaveFileAs(sender, e);
            }
        }

        private void SaveFileAs(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.Filter = "Pliki sceny |*.scn|Wszystkie pliki |*.*";
            //dlg.InitialDirectory = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                currScene.SaveSceneFile(dlg.FileName);
            }
        }

        public static long GetFreeMemory()
        {
            long freeMemory = Convert.ToInt64(pc.NextValue());
            return freeMemory;
        }

        private void RenderViews()
        {
            renderer.RenderViews(GetViewCoords(tabWidok.SelectedIndex), currScene);
        }

        private void RenderBezier()
        {
            //bezierSurface.Triangulate(0.8f);
            renderer.RenderBezier(GetViewCoords(tabWidok.SelectedIndex), bezierSurface);
        }

        private void Wireframe(object sender, RoutedEventArgs e)
        {
            renderer.ChangeWireframe();

            RenderViews();
            RenderBezier();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Views_Paint(object sender, PaintEventArgs e)
        {
            RenderViews();
        }

        private void ViewsBezier_Paint(object sender, PaintEventArgs e)
        {
            RenderBezier();
        }

        private void Views_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int sumSelected = currScene.selTriangles.Count + currScene.selLights.Count + currScene.selCams.Count;

            if (e.Button == MouseButtons.Left && (contextMenu.MenuItems[0].Checked == false || (sumSelected > 1 && mouseDownHandled == false)))
            {
                ViewportInfo views = GetViewCoords(tabWidok.SelectedIndex);

                int x = e.X;
                int y = e.Y;

                ViewportOrientation viewport = GetViewportType(x, y);

                if(viewport != ViewportOrientation.None)
                {
                    ViewportType viewportType = viewport == ViewportOrientation.Perspective ? ViewportType.Perspective : ViewportType.Orto;
                    int rect = (int)viewport;
                    int orthoRect = rect == 3 ? 0 : rect;

                    SelectingElems.SelectElems(currScene, renderer.GetCamsPoints(), renderer.GetLightsPoints(), viewportType, new System.Drawing.Point(x - views.posX[rect], y - views.posY[rect]),
                        new System.Drawing.Point(views.sizeX[rect], views.sizeY[rect]), new Vector2(renderer.OrthoWidth[orthoRect], (float)views.sizeY[rect] / views.sizeX[rect] * renderer.OrthoWidth[orthoRect]),
                        renderer.OrthoPos[orthoRect], renderer.OrthoLookAt[orthoRect], GetCtrlPressed());

                    Renderer.RecalculateData(currScene);
                    RenderViews();
                    currScene.selectedHierObj = null;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                ViewportInfo views = GetViewCoords(tabWidok.SelectedIndex);

                mRCx = e.X;
                mRCy = e.Y;

                //Console.WriteLine("x " + mRCx + " y " + mRCy);
            }

            mouseDownHandled = false;            
        }

        private void widok_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = System.Windows.DragDropEffects.Copy | System.Windows.DragDropEffects.Move;
            ViewportInfo views = GetViewCoords(tabWidok.SelectedIndex);

            float x = (float)(e.GetPosition(this).X - dragX);
            float y = (float)(e.GetPosition(this).Y - dragY);

            // Przesuwanie
            if (e.Data.GetData("Viewport") != null)
            {    
                if (tabWidok.SelectedIndex == 1)
                {
                    if (e.Data.GetData("Viewport") != null)
                    {
                        if (e.Data.GetData("Viewport").Equals("front"))
                        {
                            // Lewy dolny panel dziala na x y, a więc jest to !!!PRZOD!!!
                            float factor = (renderer.BezierOrthoWidth[0] / views.sizeX[0]);
                            bezierSurface.translateSelectedPoint(x * factor, -y * factor, 0);
                        }
                        if (e.Data.GetData("Viewport").Equals("top"))
                        {
                            // Prawy górny panel działa na x z, a więc jest to !!!GORA!!!
                            float factor = (renderer.BezierOrthoWidth[2] / views.sizeX[2]);
                            bezierSurface.translateSelectedPoint(x * factor, 0, y * factor);
                        }
                        if (e.Data.GetData("Viewport").Equals("side"))
                        {
                            // Prawy dolny panel działa na y z, a wiec jest to !!!BOK!!!
                            float factor = (renderer.BezierOrthoWidth[1] / views.sizeX[1]);
                            bezierSurface.translateSelectedPoint(0, -y * factor, -x * factor);
                        }
                    }

                    RenderBezier();
                }

                // Przesuwanie płaszczyzn obcinających

                else if (e.Data.GetData("Plane") != null)
                {
                    int xx = (int)e.GetPosition(this).X - xOffset;
                    int yy = (int)e.GetPosition(this).Y - yOffset;

                    ViewportOrientation viewport = GetViewportType(xx, yy);

                    if (viewport != ViewportOrientation.None && viewport != ViewportOrientation.Perspective)
                    {
                        float factor = (renderer.OrthoWidth[(int) viewport]/views.sizeX[(int) viewport]);

                        if ((string) e.Data.GetData("Plane") == "YMIN")
                        {
                            renderer.ShiftClipPlane(ClipPlaneType.YMIN, -y*factor);
                        }
                        else if ((string) e.Data.GetData("Plane") == "YPLUS")
                        {
                            renderer.ShiftClipPlane(ClipPlaneType.YPLUS, -y*factor);
                        }
                        else if ((string) e.Data.GetData("Plane") == "XMIN")
                        {
                            renderer.ShiftClipPlane(ClipPlaneType.XMIN, x*factor);
                        }
                        else if ((string) e.Data.GetData("Plane") == "XPLUS")
                        {
                            renderer.ShiftClipPlane(ClipPlaneType.XPLUS, x*factor);
                        }
                        if ((string) e.Data.GetData("Plane") == "ZPLUS" && (string) e.Data.GetData("Viewport") == "side")
                        {
                            renderer.ShiftClipPlane(ClipPlaneType.ZPLUS, -x*factor);
                        }
                        if ((string) e.Data.GetData("Plane") == "ZMIN" && (string) e.Data.GetData("Viewport") == "side")
                        {
                            renderer.ShiftClipPlane(ClipPlaneType.ZMIN, -x*factor);
                        }
                        if ((string) e.Data.GetData("Plane") == "ZPLUS" && (string) e.Data.GetData("Viewport") == "top")
                        {
                            renderer.ShiftClipPlane(ClipPlaneType.ZPLUS, y*factor);
                        }
                        if ((string) e.Data.GetData("Plane") == "ZMIN" && (string) e.Data.GetData("Viewport") == "top")
                        {
                            renderer.ShiftClipPlane(ClipPlaneType.ZMIN, y*factor);
                        }

                        RenderViews();
                    }
                }

                // Przesuwanie punktów kamery i światła stożkowego i goniometrycznego

                else if(e.Data.GetData("Point") != null && SelectingElems.pointFound != -1 && !GetAltPressed())
                {
                    mousePos.X += (int)(e.GetPosition(this).X - dragX);
                    mousePos.Y += (int)(e.GetPosition(this).Y - dragY);

                    int viewport = -1;
                    switch((string)e.Data.GetData("Viewport"))
                    {
                        case "front":
                            viewport = 0;
                            break;

                        case "side":
                            viewport = 1;
                            break;

                        case "top":
                            viewport = 2;
                            break;
                    }

                    int posX = (int)mousePos.X - views.posX[viewport];
                    int posY = (int)mousePos.Y - views.posY[viewport];

                    Vector3 outCamPos, outSurfPos;

                    SelectingElems.CalcOrthoCoords(new System.Drawing.Point(posX, posY), new System.Drawing.Point(views.sizeX[viewport], views.sizeY[viewport]),
                        new Vector2(renderer.OrthoWidth[viewport], views.sizeY[viewport] * renderer.OrthoWidth[viewport] / views.sizeX[viewport]),
                        renderer.OrthoPos[viewport], renderer.OrthoLookAt[viewport], out outCamPos, out outSurfPos);

                    if(x != 0 || y != 0)
                    {
                        switch((string)e.Data.GetData("Viewport"))
                        {
                            case "front":
                                currScene.MoveLightCamPoints((int)e.Data.GetData("Point"), new Vector3(outSurfPos.X, outSurfPos.Y, 0), viewport, renderer.orthoWidth[0]);
                                break;

                            case "side":
                                currScene.MoveLightCamPoints((int)e.Data.GetData("Point"), new Vector3(0, outSurfPos.Y, outSurfPos.Z), viewport, renderer.orthoWidth[1]);
                                break;

                            case "top":
                                currScene.MoveLightCamPoints((int)e.Data.GetData("Point"), new Vector3(outSurfPos.X, 0, outSurfPos.Z), viewport, renderer.orthoWidth[2]);
                                break;
                        }
                    }
                    cameraPan.cameraMoved();

                    RenderViews();
                }
                
                // Przesuwanie
                else
                {
                    if (contextMenu.MenuItems[0].Checked)
                    {
                        if (e.Data.GetData("Viewport").Equals("front") && !GetAltPressed())
                        {
                            // Lewy dolny panel dziala na x y, a więc jest to !!!PRZOD!!!
                            float factor = (renderer.OrthoWidth[0] / views.sizeX[0]);
                            Transformations.Transformations.Translate(currScene, x * factor, -y * factor, 0);
                            //if(currScene.selTriangles.Count > 0)
                            //{
                            //    sceneChange = true;
                            //}
                            //Console.WriteLine("x " + (x * factor) + " y " + (-y * factor) + " z " + 0);
                        }
                        if (e.Data.GetData("Viewport").Equals("top") && !GetAltPressed())
                        {
                            // Prawy górny panel działa na x z, a więc jest to !!!GORA!!!
                            float factor = (renderer.OrthoWidth[2] / views.sizeX[2]);
                            Transformations.Transformations.Translate(currScene, x * factor, 0, y * factor);
                            //if(currScene.selTriangles.Count > 0)
                            //{
                            //    sceneChange = true;
                            //}
                            ////Console.WriteLine("x " + (x * factor) + " y " + 0 + " z " + (y * factor));
                        }
                        if (e.Data.GetData("Viewport").Equals("side") && !GetAltPressed())
                        {
                            // Prawy dolny panel działa na y z, a wiec jest to !!!BOK!!!
                            float factor = (renderer.OrthoWidth[1] / views.sizeX[1]);
                            Transformations.Transformations.Translate(currScene, 0, -y * factor, -x * factor);
                            //if(currScene.selTriangles.Count > 0)
                            //{
                            //    sceneChange = true;
                            //}
                            //Console.WriteLine("x " + 0 + " y " + (-y * factor) + " z " + (-x * factor));
                        }

                        if (currScene.cameraMoved)
                        {
                            cameraPan.cameraMoved();
                            currScene.cameraMoved = false;
                            //sceneChange = true;
                        }
                    }
                    // Obracanie
                    else if (contextMenu.MenuItems[1].Checked)
                    {
                        float factor = (renderer.OrthoWidth[1] / views.sizeX[1]);
                        directionX = directionX + x * factor;
                        directionY = directionY + y * factor;
                        if (currScene.selTriangles.Count > 0 || currScene.selLights.Count > 1)
                        {
                            if (angle == undefined)
                            {
                                HashSet<uint> uniqueVertices = new HashSet<uint>();
                                foreach (HierarchyMesh obj in currScene.selTriangles)
                                {
                                    //uint tmp;
                                    foreach (uint triagleIdx in obj.triangles)
                                    {
                                        //tmp = currScene.triangles[(int)triagleIdx].p1;
                                        //if (!uniqueVertices.Contains(tmp))
                                        uniqueVertices.Add(currScene.triangles[(int)triagleIdx].p1);
                                        //tmp = currScene.triangles[(int)triagleIdx].p2;
                                        //if (!uniqueVertices.Contains(tmp))
                                        uniqueVertices.Add(currScene.triangles[(int)triagleIdx].p2);
                                        //tmp = currScene.triangles[(int)triagleIdx].p3;
                                        //if (!uniqueVertices.Contains(tmp))
                                        uniqueVertices.Add(currScene.triangles[(int)triagleIdx].p3);
                                    }
                                }



                                //Wyznaczenie środka
                                center = new Vector3D(0, 0, 0);
                                int count = uniqueVertices.Count + currScene.selLights.Count;
                                foreach (int vertIdx in uniqueVertices)
                                {
                                    center.x = center.x + currScene.points[vertIdx].x / count;
                                    center.y = center.y + currScene.points[vertIdx].y / count;
                                    center.z = center.z + currScene.points[vertIdx].z / count;
                                }
                                foreach (int lightIdx in currScene.selLights)
                                {
                                    center.x = center.x + currScene.lights[lightIdx].position.X / count;
                                    center.y = center.y + currScene.lights[lightIdx].position.Y / count;
                                    center.z = center.z + currScene.lights[lightIdx].position.Z / count;
                                }
                            }

                            if (e.Data.GetData("Viewport").Equals("side"))
                            {
                                float newAngle = (float)Math.Atan2(-directionX - center.z + renderer.OrthoPos[1].Z, -directionY - center.y + renderer.OrthoPos[1].Y);
                                if (angle != undefined) Transformations.Transformations.RotateOX(currScene, newAngle - angle);
                                angle = newAngle;
                                //if(currScene.selTriangles.Count > 0)
                                //{
                                //    sceneChange = true;
                                //}
                            }
                            if (e.Data.GetData("Viewport").Equals("top"))
                            {
                                float newAngle = (float)Math.Atan2(directionX - center.x + renderer.OrthoPos[2].X, directionY - center.z + renderer.OrthoPos[2].Z);
                                if (angle != undefined) Transformations.Transformations.RotateOY(currScene, newAngle - angle);
                                angle = newAngle;
                                //if(currScene.selTriangles.Count > 0)
                                //{
                                //    sceneChange = true;
                                //}
                            }
                            if (e.Data.GetData("Viewport").Equals("front"))
                            {
                                float newAngle = (float)Math.Atan2(directionX - center.x + renderer.OrthoPos[0].X, -directionY - center.y + renderer.OrthoPos[0].Y);
                                if (angle != undefined) Transformations.Transformations.RotateOZ(currScene, newAngle - angle);
                                angle = newAngle;
                                //if(currScene.selTriangles.Count > 0)
                                //{
                                //    sceneChange = true;
                                //}
                            }
                        }
                        else if (currScene.selLights.Count == 1)
                        {
                            

                            if (e.Data.GetData("Viewport").Equals("front"))
                            {
                                float vectorLength = (float)Math.Sqrt(
                                    currScene.lights[currScene.selLights[0]].direction.X
                                    * currScene.lights[currScene.selLights[0]].direction.X
                                    + currScene.lights[currScene.selLights[0]].direction.Y
                                    * currScene.lights[currScene.selLights[0]].direction.Y);
                                currScene.lights[currScene.selLights[0]].direction.X = directionX - currScene.lights[currScene.selLights[0]].position.X + renderer.OrthoPos[0].X;
                                currScene.lights[currScene.selLights[0]].direction.Y = -directionY - currScene.lights[currScene.selLights[0]].position.Y + renderer.OrthoPos[0].Y;
                                float newVectorLength = (float)Math.Sqrt(
                                    currScene.lights[currScene.selLights[0]].direction.X
                                    * currScene.lights[currScene.selLights[0]].direction.X
                                    + currScene.lights[currScene.selLights[0]].direction.Y
                                    * currScene.lights[currScene.selLights[0]].direction.Y);
                                currScene.lights[currScene.selLights[0]].direction.X =
                                    currScene.lights[currScene.selLights[0]].direction.X * vectorLength / newVectorLength;
                                currScene.lights[currScene.selLights[0]].direction.Y =
                                    currScene.lights[currScene.selLights[0]].direction.Y * vectorLength / newVectorLength;

                                //if(currScene.selLights.Count > 0)
                                //{
                                //    sceneChange = true;
                                //}
                            }
                            if (e.Data.GetData("Viewport").Equals("side"))
                            {
                                float vectorLength = (float)Math.Sqrt(
                                    currScene.lights[currScene.selLights[0]].direction.Z
                                    * currScene.lights[currScene.selLights[0]].direction.Z
                                    + currScene.lights[currScene.selLights[0]].direction.Y
                                    * currScene.lights[currScene.selLights[0]].direction.Y);
                                currScene.lights[currScene.selLights[0]].direction.Z = -directionX - currScene.lights[currScene.selLights[0]].position.Z + renderer.OrthoPos[1].Z;
                                currScene.lights[currScene.selLights[0]].direction.Y = -directionY - currScene.lights[currScene.selLights[0]].position.Y + renderer.OrthoPos[1].Y;
                                float newVectorLength = (float)Math.Sqrt(
                                    currScene.lights[currScene.selLights[0]].direction.Z
                                    * currScene.lights[currScene.selLights[0]].direction.Z
                                    + currScene.lights[currScene.selLights[0]].direction.Y
                                    * currScene.lights[currScene.selLights[0]].direction.Y);
                                currScene.lights[currScene.selLights[0]].direction.Z =
                                    currScene.lights[currScene.selLights[0]].direction.Z * vectorLength / newVectorLength;
                                currScene.lights[currScene.selLights[0]].direction.Y =
                                    currScene.lights[currScene.selLights[0]].direction.Y * vectorLength / newVectorLength;

                                //if(currScene.selLights.Count > 0)
                                //{
                                //    sceneChange = true;
                                //}
                            }
                            if (e.Data.GetData("Viewport").Equals("top"))
                            {
                                float vectorLength = (float)Math.Sqrt(
                                    currScene.lights[currScene.selLights[0]].direction.X
                                    * currScene.lights[currScene.selLights[0]].direction.X
                                    + currScene.lights[currScene.selLights[0]].direction.Z
                                    * currScene.lights[currScene.selLights[0]].direction.Z);
                                currScene.lights[currScene.selLights[0]].direction.X = directionX - currScene.lights[currScene.selLights[0]].position.X + renderer.OrthoPos[2].X;
                                currScene.lights[currScene.selLights[0]].direction.Z = directionY - currScene.lights[currScene.selLights[0]].position.Z + renderer.OrthoPos[2].Z;
                                float newVectorLength = (float)Math.Sqrt(
                                    currScene.lights[currScene.selLights[0]].direction.X
                                    * currScene.lights[currScene.selLights[0]].direction.X
                                    + currScene.lights[currScene.selLights[0]].direction.Z
                                    * currScene.lights[currScene.selLights[0]].direction.Z);
                                currScene.lights[currScene.selLights[0]].direction.X =
                                    currScene.lights[currScene.selLights[0]].direction.X * vectorLength / newVectorLength;
                                currScene.lights[currScene.selLights[0]].direction.Z =
                                    currScene.lights[currScene.selLights[0]].direction.Z * vectorLength / newVectorLength;

                                //if(currScene.selLights.Count > 0)
                                //{
                                //    sceneChange = true;
                                //}
                            }
                        }
                    }
                    // Skalowanie
                    else if (contextMenu.MenuItems[2].Checked)
                    {
                        if (e.Data.GetData("Viewport").Equals("front"))
                        {
                            Transformations.Transformations.Scale(currScene, x, x, x);
                            //if(currScene.selTriangles.Count > 0)
                            //{
                            //    sceneChange = true;
                            //}
                        }
                        if (e.Data.GetData("Viewport").Equals("top"))
                        {
                            Transformations.Transformations.Scale(currScene, x, x, x);
                            //if(currScene.selTriangles.Count > 0)
                            //{
                            //    sceneChange = true;
                            //}
                        }
                        if (e.Data.GetData("Viewport").Equals("side"))
                        {
                            Transformations.Transformations.Scale(currScene, x, x, x);
                            //if(currScene.selTriangles.Count > 0)
                            //{
                            //    sceneChange = true;
                            //}
                        }
                    }
                    // Skalowanie wzdłuż osi
                    else if (contextMenu.MenuItems[3].Checked)
                    {
                        if (e.Data.GetData("Viewport").Equals("front"))
                        {
                            Transformations.Transformations.Scale(currScene, x, y, 0);
                            //if(currScene.selTriangles.Count > 0)
                            //{
                            //    sceneChange = true;
                            //}
                        }
                        if (e.Data.GetData("Viewport").Equals("top"))
                        {
                            Transformations.Transformations.Scale(currScene, x, 0, y);
                            //if(currScene.selTriangles.Count > 0)
                            //{
                            //    sceneChange = true;
                            //}
                        }
                        if (e.Data.GetData("Viewport").Equals("side"))
                        {
                            Transformations.Transformations.Scale(currScene, 0, y, x);
                            //if(currScene.selTriangles.Count > 0)
                            //{
                            //    sceneChange = true;
                            //}
                        }
                    }
                    RenderViews();
                }
            }

            dragX = (float)e.GetPosition(this).X;
            dragY = (float)e.GetPosition(this).Y;
        }


        ////////////////////////////////////////////////////////////////////////////////////
        // Sekcja odpowiadająca za interakcje DRAG
        private void ksztalt_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && ksztalty_ListView.SelectedIndex > -1)
            {
                System.Windows.DataObject dataObject = new System.Windows.DataObject();
                dataObject.SetData("Object", _shapesGallery.ElementAt(ksztalty_ListView.SelectedIndex));
                DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Copy);
                //Console.WriteLine(_shapesCol.ElementAt(ksztalty_ListView.SelectedIndex).ToString());
            }
        }

        private void gotowe_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && gotowe_ListView.SelectedIndex > -1)
            {
                System.Windows.DataObject dataObject = new System.Windows.DataObject();
                dataObject.SetData("Object", _elementsGallery.ElementAt(gotowe_ListView.SelectedIndex));
                DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Copy);
                //Console.WriteLine(_elementsCol.ElementAt(gotowe_ListView.SelectedIndex).ToString());
            }
            currScene.selectedHierObj = null;
        }

        private void materialy_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && materialy_ListView.SelectedIndex > -1)
            {
                System.Windows.DataObject dataObject = new System.Windows.DataObject();
                dataObject.SetData("Object", _surfaceGallery.ElementAt(materialy_ListView.SelectedIndex));
                DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Copy);
                //Console.WriteLine(_elementsCol.ElementAt(gotowe_ListView.SelectedIndex).ToString());
            }
            currScene.selectedHierObj = null;
        }

        private void swiatla_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && swiatla_ListView.SelectedIndex > -1)
            {
                System.Windows.DataObject dataObject = new System.Windows.DataObject();
                dataObject.SetData("Object", _lightGallery.ElementAt(swiatla_ListView.SelectedIndex));
                DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Copy);
                //Console.WriteLine(_elementsCol.ElementAt(gotowe_ListView.SelectedIndex).ToString());
            }
            currScene.selectedHierObj = null;
        }

        // Koniec sekcji
        ////////////////////////////////////////////////////////////////////////////////////

        private void objectToScene_Drop(object sender, System.Windows.DragEventArgs e)
        {
            // Wciąganie obiektów do sceny
            if (tabWidok.SelectedIndex == 0)
            {
                if (e.Data.GetDataPresent("Object"))
                {
                    object data = e.Data.GetData("Object");
                    //Console.WriteLine(data.GetType());
                    //ViewportInfo coords = GetViewCoords(tabWidok.SelectedIndex);
                    Vector3 translation;
                    if (data is Shape_)
                    {
                        Shape_ shape = (Shape_)data;
                        undo.Save(currScene);
                        // przekształcenie współrzędnych
                        float x = (float)e.GetPosition(this).X - xOffset;
                        float y = (float)e.GetPosition(this).Y - yOffset;

                        translation = CalculateTranslation(x, y);
                        currScene.AddObject(shape.Triangulate((float)triang_Slider.Value), shape.Name, translation);
                        currScene.hierarchyChange = false;
                        initializeTreeView();

                        //sceneChange = true;
                    }
                    else if (data is PreparedElement)
                    {
                        undo.Save(currScene);
                        PreparedElement element = (PreparedElement)data;

                        // przekształcenie współrzędnych
                        float x = (float)e.GetPosition(this).X - xOffset;
                        float y = (float)e.GetPosition(this).Y - yOffset;

                        translation = CalculateTranslation(x, y);
                        //for (int i = 0; i < element.Scene.cams.Count; i++)
                        //{
                        //    cameraPan.comboBox1.Items.Add("Kamera " + (currScene.cams.Count() + 1 + i));
                        //}
                        currScene.AddPreparedElement(element, translation);
                        currScene.hierarchyChange = false;
                        initializeTreeView();

                        //sceneChange = true;
                    }
                    else if (data is Surface)
                    {
                        Surface surface = (Surface)data;
                        undo.Save(currScene);
                        bool zazn = true;

                        if (currScene.selTriangles.Count == 0)
                        {
                            zazn = false;
                            ViewportInfo views = GetViewCoords(tabWidok.SelectedIndex);
                            int x = (int)e.GetPosition(this).X - xOffset;
                            int y = (int)e.GetPosition(this).Y - yOffset;

                            ViewportOrientation viewport = GetViewportType(x, y);

                            if (viewport != ViewportOrientation.None)
                            {
                                ViewportType viewportType = viewport == ViewportOrientation.Perspective ? ViewportType.Perspective : ViewportType.Orto;
                                int rect = (int)viewport;
                                int orthoRect = rect == 3 ? 0 : rect;

                                SelectingElems.SelectElems(currScene, renderer.GetCamsPoints(), renderer.GetLightsPoints(), viewportType, new System.Drawing.Point(x - views.posX[rect], y - views.posY[rect]),
                                    new System.Drawing.Point(views.sizeX[rect], views.sizeY[rect]), new Vector2(renderer.OrthoWidth[orthoRect], (float)views.sizeY[rect] / views.sizeX[rect] * renderer.OrthoWidth[orthoRect]),
                                    renderer.OrthoPos[orthoRect], renderer.OrthoLookAt[orthoRect], false);
                            }
                        }

                        if (String.Compare(surface.Material.Name, "default", true) == 0)
                        {
                            Modeler.DialogBoxes.NameDialog newName = new NameDialog();
                            bool res = (bool)newName.Show("Podaj nową nazwę materiału", "");
                            if (!res)
                                return;
                            surface.Material.name = newName.Result;

                        }
                        bool replace = false;
                        while (currScene.ContainsMaterialName(surface.Material.Name) && !replace)
                        {
                            Modeler.DialogBoxes.NameDialog dialog = new NameDialog();
                            dialog.Owner = this;

                            bool result = (bool)dialog.Show(materialSceneMessage, surface.Material.Name);
                            string name = dialog.Result;
                            if (!result)
                                return;
                            if (surface.Material.Name == name)
                                replace = true;
                            surface.Material.name = name;
                            dialog.Close();
                        }

                        currScene.AddMaterial(surface.Material);
                        currScene.hierarchyChange = false;

                        if (!zazn) currScene.ClearSelectedTriangles();
                        //Console.WriteLine("Przypisanie atrybutów powierzchniowych");

                        //sceneChange = true;
                    }
                    else if (data is Material_)
                    {
                        Material_ material = new Material_((Material_)data);
                        material.colorR = material.colorR / 255;
                        material.colorG = material.colorG / 255;
                        material.colorB = material.colorB / 255;
                        undo.Save(currScene);
                        bool zazn = true;

                        if (currScene.selTriangles.Count == 0)
                        {
                            zazn = false;
                            ViewportInfo views = GetViewCoords(tabWidok.SelectedIndex);
                            int x = (int)e.GetPosition(this).X - xOffset;
                            int y = (int)e.GetPosition(this).Y - yOffset;

                            ViewportOrientation viewport = GetViewportType(x, y);

                            if (viewport != ViewportOrientation.None)
                            {
                                ViewportType viewportType = viewport == ViewportOrientation.Perspective ? ViewportType.Perspective : ViewportType.Orto;
                                int rect = (int)viewport;
                                int orthoRect = rect == 3 ? 0 : rect;

                                SelectingElems.SelectElems(currScene, renderer.GetCamsPoints(), renderer.GetLightsPoints(), viewportType, new System.Drawing.Point(x - views.posX[rect], y - views.posY[rect]),
                                    new System.Drawing.Point(views.sizeX[rect], views.sizeY[rect]), new Vector2(renderer.OrthoWidth[orthoRect], (float)views.sizeY[rect] / views.sizeX[rect] * renderer.OrthoWidth[orthoRect]),
                                    renderer.OrthoPos[orthoRect], renderer.OrthoLookAt[orthoRect], false);
                            }
                        }

                        if (String.Compare(material.Name, "default", true) == 0)
                        {
                            Modeler.DialogBoxes.NameDialog newName = new NameDialog();
                            bool res = (bool)newName.Show("Podaj nową nazwę materiału", "");
                            if (!res)
                                return;
                            material.name = newName.Result;

                        }
                        bool replace = false;
                        while (currScene.ContainsMaterialName(material.Name) && !replace)
                        {
                            Modeler.DialogBoxes.NameDialog dialog = new NameDialog();
                            dialog.Owner = this;

                            bool result = (bool)dialog.Show(materialSceneMessage, material.Name);
                            string name = dialog.Result;
                            if (!result)
                                return;
                            if (material.Name == name)
                                replace = true;
                            material.name = name;
                            dialog.Close();
                        }

                        currScene.AddMaterial(material);

                        if (!zazn) currScene.ClearSelectedTriangles();
                        //Console.WriteLine("Przypisanie atrybutów powierzchniowych");

                        //sceneChange = true;
                    }
                    else if (data is LightObj)
                    {
                        if (currScene.selLights.Count > 0)  //objętnie ile ich jest zaznaczonych zmienia tylko 1 sprawdzająć nazwe
                        {
                            undo.Save(currScene);
                            Light_ sceneLight = currScene.lights[currScene.selLights[0]];
                            Light_ dataLight = ((LightObj)data).Light;
                            sceneLight.colorR = dataLight.colorR;
                            sceneLight.colorG = dataLight.colorG;
                            sceneLight.colorB = dataLight.colorB;
                            sceneLight.enabled = dataLight.enabled;
                            sceneLight.goniometric = dataLight.goniometric;
                            sceneLight.innerAngle = dataLight.innerAngle;
                            sceneLight.name = dataLight.name;
                            sceneLight.outerAngle = dataLight.outerAngle;
                            sceneLight.power = dataLight.power;
                            sceneLight.type = dataLight.type;

                            //sceneChange = true;
                        }
                        else
                        {
                            LightObj light = (LightObj)data;
                            undo.Save(currScene);
                            Light_ sceneLight = new Light_(light.Light);

                            // przekształcenie współrzędnych
                            float x = (float)e.GetPosition(this).X - xOffset;
                            float y = (float)e.GetPosition(this).Y - yOffset;

                            translation = CalculateTranslation(x, y);

                            currScene.AddLight(sceneLight, translation); 
                            currScene.hierarchyChange = false;
                            initializeTreeView();

                            //sceneChange = true;
                        }
                    }

                    else if (data is Light_)
                    {
                        if (currScene.selLights.Count > 0)
                        {
                            undo.Save(currScene);
                            Light_ sceneLight = currScene.lights[currScene.selLights[0]];
                            Light_ dataLight = (Light_)data;
                            sceneLight.colorR = dataLight.colorR;
                            sceneLight.colorG = dataLight.colorG;
                            sceneLight.colorB = dataLight.colorB;
                            sceneLight.enabled = dataLight.enabled;
                            sceneLight.goniometric = dataLight.goniometric;
                            sceneLight.innerAngle = dataLight.innerAngle;
                            sceneLight.name = dataLight.name;
                            sceneLight.outerAngle = dataLight.outerAngle;
                            sceneLight.power = dataLight.power;
                            sceneLight.type = dataLight.type;

                            //currScene.hierarchyChange = true;
                            currScene.hierarchyChange = false;
                            initializeTreeView();

                            //sceneChange = true;
                        }
                        else
                        {
                            Light_ light = (Light_)data;
                            undo.Save(currScene);
                            Light_ sceneLight = new Light_(light);

                            // przekształcenie współrzędnych
                            float x = (float)e.GetPosition(this).X - xOffset;
                            float y = (float)e.GetPosition(this).Y - yOffset;

                            translation = CalculateTranslation(x, y);

                            currScene.AddLight(sceneLight, translation);
                            //TO DO sprawdzenie czy nazwa jest unikatowa
                            //currScene.hierarchyChange = true;
                            currScene.hierarchyChange = false;
                            initializeTreeView();

                            //sceneChange = true;
                        }

                    }
                    else if (data is Camera)
                    {
                        Camera cam = (Camera)data;
                        undo.Save(currScene);

                        float x = (float)e.GetPosition(this).X - xOffset;
                        float y = (float)e.GetPosition(this).Y - yOffset;

                        translation = CalculateTranslation(x, y);

                        currScene.AddCamera(cam, translation);
                        cameraPan.comboBox1.Items.Add("Kamera " + (currScene.cams.Count()));
                        cameraPan.comboBox1.SelectedIndex = currScene.activeCamera;

                        //sceneChange = true;
                    }
                }
            } 
            // Wciąganie obiektów do edytora powierzchni beziera.
            else if (tabWidok.SelectedIndex == 1)
            {
                if (e.Data.GetDataPresent("Object"))
                {
                    object data = e.Data.GetData("Object");

                    if (data is BezierSurface)
                    {
                        bezierSurface = new BezierSurface((BezierSurface)data);
                        bezierSurface.Triangulate((float)triang_Slider.Value);
                        RenderBezier();
                    }
                }
            }

            e.Handled = true;

            trNumTBG1.Text = currScene.triangles.Count().ToString();
            trNumTBG2.Text = currScene.triangles.Count().ToString();
            trNumTBG3.Text = currScene.triangles.Count().ToString();
            trNumTBG4.Text = currScene.triangles.Count().ToString();

            Renderer.RecalculateData(currScene);

            RenderViews();
            currScene.selectedHierObj = null;
        }

        private void triang_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (triangValue_TextBox != null)
                triangValue_TextBox.Text = triang_Slider.Value.ToString("F4", CultureInfo.InvariantCulture);
        }

        private void triangValue_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(triangValue_TextBox.Text)) triangValue_TextBox.Text = "0.0001";
                if (Double.Parse(triangValue_TextBox.Text.Replace(".", ",")) > 1) triangValue_TextBox.Text = "1";
                triang_Slider.Value = Double.Parse(triangValue_TextBox.Text.Replace(".", ","));
                //string s = triangValue_TextBox.Text;
                triangValue_TextBox.SelectionStart = triangValue_TextBox.Text.Length;
                //triangValue_TextBox.SelectionStart = triangValue_TextBox.Text.Length - s.Replace("0", "").Length + 1;
            }
            catch (Exception)
            {
                triangValue_TextBox.Text = triang_Slider.Value.ToString();
            }
        }

        private void Views_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Views.Focus();
            mouseDownHandled = false;

            if (!GetShiftPressed() && e.Button == MouseButtons.Left)
            {
                if (currScene.selCams.Count > 0 || currScene.selLights.Count > 0 || currScene.selTriangles.Count > 0)
                {
                    System.Windows.DataObject dataObject = new System.Windows.DataObject();
                    ViewportInfo coords = GetViewCoords(tabWidok.SelectedIndex);
                    undo.Save(currScene);

                    ViewportOrientation viewport = GetViewportType(e.X, e.Y);
                    switch(viewport)
                    {
                        case ViewportOrientation.Front:
                            dataObject.SetData("Viewport", "front");
                            break;

                        case ViewportOrientation.Side:
                            dataObject.SetData("Viewport", "side");
                            break;

                        case ViewportOrientation.Top:
                            dataObject.SetData("Viewport", "top");
                            break;

                        case ViewportOrientation.Perspective:
                            dataObject.SetData("Viewport", "perspective");
                            break;

                        default:
                            break;
                    }

                    if (currScene.selLights.Count == 1)
                    {
                        dataObject.SetData("Light", currScene.lights[currScene.selLights[0]]);
                        dataObject.SetData("LightPrevious", new Light_(currScene.lights[currScene.selLights[0]]));
                    }

                    dragX = (float)e.X + xOffset;
                    dragY = (float)e.Y + yOffset;
                    if (contextMenu.MenuItems[1].Checked == true)
                    {
                        directionX = 0;
                        directionY = 0;
                        angle = undefined;
                        int rect = (int)viewport;
                        ViewportInfo views = GetViewCoords(tabWidok.SelectedIndex);
                        dragX = (int)(views.posX[rect] + views.sizeX[rect] / 2.0f) + xOffset;
                        dragY = (int)(views.posY[rect] + views.sizeY[rect] / 2.0f) + yOffset;
                    }
                    DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Move);
                    currScene.selectedHierObj = null;
                    RenderViews();
                }
                //else
                //{
                //    ViewportInfo views = GetViewCoords(tabWidok.SelectedIndex);
                //    int x = (int)e.X;
                //    int y = (int)e.Y;

                //    ViewportOrientation viewport = GetViewportType(x, y);

                //    if(viewport != ViewportOrientation.None)
                //    {
                //        ViewportType viewportType = viewport == ViewportOrientation.Perspective ? ViewportType.Perspective : ViewportType.Orto;
                //        int rect = (int)viewport;
                //        int orthoRect = rect == 3 ? 0 : rect;

                //        SelectingElems.SelectElems(currScene, renderer.GetCamsPoints(), renderer.GetLightsPoints(), viewportType, new System.Drawing.Point(x - views.posX[rect], y - views.posY[rect]),
                //            new System.Drawing.Point(views.sizeX[rect], views.sizeY[rect]), new Vector2(renderer.OrthoWidth[orthoRect], (float)views.sizeY[rect] / views.sizeX[rect] * renderer.OrthoWidth[orthoRect]),
                //            renderer.OrthoPos[orthoRect], renderer.OrthoLookAt[orthoRect], false);
                //    }


                //    for (int i = 0; i < currScene.parts.Count; i++)
                //    {
                //        foreach (HierarchyMesh obj in currScene.selTriangles)
                //            if (currScene.parts[i].triangles.Contains((int)obj.triangles[0]))
                //            {
                //                System.Windows.DataObject dataObject = new System.Windows.DataObject();
                //                Material_ material = null;
                //                foreach (Material_ m in currScene.materials)
                //                {
                //                    if (m.name == currScene.materialAssign[i])
                //                    {
                //                        material = new Material_(m);
                //                        material.colorR = material.colorR * 255;
                //                        material.colorG = material.colorG * 255;
                //                        material.colorB = material.colorB * 255;
                //                    }
                //                }
                //                dataObject.SetData("Object", material);
                //                currScene.selTriangles.Clear();
                //                DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Move);
                //                i = Int32.MaxValue - 1;
                //                break;
                //            }
                //    }

                //    currScene.ClearSelectedTriangles();
                //}
            }
            //Console.WriteLine(e.Button + " " + GetShiftPressed());
            else if(e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && GetShiftPressed()))
            {
                ViewportInfo views = GetViewCoords(tabWidok.SelectedIndex);
                int x = (int)e.X;
                int y = (int)e.Y;

                ViewportOrientation viewport = GetViewportType(x, y);
                if(shiftingViewport == ViewportOrientation.None && viewport != ViewportOrientation.None && viewport != ViewportOrientation.Perspective)
                {
                    shiftingViewport = viewport;
                }

                if(shiftingViewport != ViewportOrientation.None)
                {
                    renderer.MoveOrtho((int)shiftingViewport, (int)-(e.X - mousePos.X), (int)-(e.Y - mousePos.Y),
                        views.sizeX[(int)shiftingViewport], views.sizeY[(int)shiftingViewport]);

                    RenderViews();
                }
            }
            else
            {
                shiftingViewport = ViewportOrientation.None;
            }

            mousePos.X = e.X;
            mousePos.Y = e.Y;
        }

        private void Views_Wheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int x = (int)e.X;
            int y = (int)e.Y;

            ViewportOrientation viewport = GetViewportType(x, y);

            if(viewport != ViewportOrientation.None && viewport != ViewportOrientation.Perspective)
            {
                renderer.ScaleOrtho((int)viewport, e.Delta * SystemInformation.MouseWheelScrollLines / 120, GetViewCoords(tabWidok.SelectedIndex), x, y);

                RenderViews();
            }
            else if (viewport == ViewportOrientation.Perspective)
            {
                currScene.ChangeCameraAngle(-e.Delta * SystemInformation.MouseWheelScrollLines);
                cameraPan.cameraMoved();
                RenderViews();
            }
        }

        private void NowyB_Click(object sender, RoutedEventArgs e)
        {
            if(currScene.modified == true)
            {
                MessageBoxResult ifSave = System.Windows.MessageBox.Show("Czy chesz zapisać bieżącą scenę ?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if(ifSave == MessageBoxResult.Yes)
                {
                    SaveFileAs(sender, e);
                }
            }

            //SurfaceRenderer.Render(new Material_("mat1", 0.6f, 0.95f, 0.5f, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 100, 1, 1, 1));
            currScene = null;
            undo = null;
            undo = new UndoStack();
            currScene = new Scene();
            trNumTBG1.Text = currScene.triangles.Count().ToString();
            trNumTBG2.Text = currScene.triangles.Count().ToString();
            trNumTBG3.Text = currScene.triangles.Count().ToString();
            trNumTBG4.Text = currScene.triangles.Count().ToString();

            Renderer.RecalculateData(currScene);
            cameraPan.newSceneLoaded();
            cameraPan.comboBox1.SelectedIndex = currScene.activeCamera;
            currScene.selectedHierObj = null;

            //sceneChange = false;
            RenderViews();
        }

        private void undoClick(object sender, RoutedEventArgs e)
        {
            if (tabWidok.SelectedIndex == 0)
            {
                currScene = undo.Undo(currScene);
                //cameraPan.newSceneLoaded();
                Renderer.RecalculateData(currScene);
                RenderViews();
            } else if (tabWidok.SelectedIndex == 1)
            {
                bezierSurface = bezierUndo.Undo(bezierSurface);
                //bezierSurface.Triangulate((float)triang_Slider.Value);
                RenderBezier();
            }
            currScene.selectedHierObj = null;
            trNumTBG1.Text = currScene.triangles.Count().ToString();
            trNumTBG2.Text = currScene.triangles.Count().ToString();
            trNumTBG3.Text = currScene.triangles.Count().ToString();
            trNumTBG4.Text = currScene.triangles.Count().ToString();

            //sceneChange = true;
        }

        private void redoClick(object sender, RoutedEventArgs e)
        {
            if (tabWidok.SelectedIndex == 0)
            {
                currScene = undo.Redo(currScene);
                Renderer.RecalculateData(currScene);
                RenderViews();
            }
            else
            {
                bezierSurface = bezierUndo.Redo(bezierSurface);
                RenderBezier();
            }
            currScene.selectedHierObj = null;
            trNumTBG1.Text = currScene.triangles.Count().ToString();
            trNumTBG2.Text = currScene.triangles.Count().ToString();
            trNumTBG3.Text = currScene.triangles.Count().ToString();
            trNumTBG4.Text = currScene.triangles.Count().ToString();

            //sceneChange = true;
        }

        private void materialy_ListView_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Object"))
            {
                object data = e.Data.GetData("Object");
                //Console.WriteLine(data.GetType());
                if (data is Material_)
                {
                    Material_ material = new Material_((Material_)data);
                    Graphics.SurfaceRaytracer.Render(material);
                    material.colorR = material.colorR / 255;
                    material.colorG = material.colorG / 255;
                    material.colorB = material.colorB / 255;
                    bool replace = false;

                    if (String.Compare(material.Name, "default", true) == 0) 
                    {
                        Modeler.DialogBoxes.NameDialog newName = new NameDialog();
                        bool res = (bool) newName.Show("Podaj nową nazwę materiału", "");
                        if (!res)
                            return;
                        material.name = newName.Result;

                    }
                    while (_surfaceGallery.GetNames().Contains(material.Name, StringComparer.OrdinalIgnoreCase) && !replace)
                    {
                        Modeler.DialogBoxes.NameDialog dialog = new NameDialog();
                        dialog.Owner = this;

                        bool result = (bool)dialog.Show(materialMessage, material.Name);
                        string name = dialog.Result;
                        if (!result)
                            return;
                        if (material.Name == name)
                            replace = true;
                        material.name = name;
                        dialog.Close();
                    }
                    if (replace)
                    {
                        int idx = _surfaceGallery.GetNameIndex(material.name);
                        _surfaceGallery.RemoveAt(idx);
                        Surface surface = _surfaceGallery.SaveToGallery(material);
                        _surfaceGallery.Insert(idx, surface);
                    }
                    else
                    {
                        Surface surface = _surfaceGallery.SaveToGallery(material);
                        _surfaceGallery.Add(surface);
                    }
                }
            }
        }

        private void swiatloGaleriaTab_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Object") || e.Data.GetDataPresent("Light"))
            {
                object data;
                if (e.Data.GetData("Object") is Light_)
                {
                    data = e.Data.GetData("Object");
                }
                else
                {
                    data = e.Data.GetData("Light");
                }
                //Console.WriteLine(data.GetType());
                if (data is Light_)
                {
                    Light_ light = new Light_((Light_)data);
                    Graphics.LightRaytracer.Render(light);
                    bool replace = false;

                    if (String.Compare(light.name, "Default", true) == 0)
                    {
                        Modeler.DialogBoxes.NameDialog newName = new NameDialog();
                        bool res = (bool)newName.Show("Podaj nową nazwę światła", "");
                        if (!res)
                            return;
                        light.name = newName.Result;

                    }
                    while (_lightGallery.GetNames().Contains(light.name, StringComparer.OrdinalIgnoreCase) && !replace)
                    {
                        Modeler.DialogBoxes.NameDialog dialog = new NameDialog();
                        dialog.Owner = this;

                        bool result = (bool)dialog.Show(lightMessage, light.name);
                        string name = dialog.Result;
                        if (!result)
                            return;
                        if (light.name == name)
                            replace = true;
                        light.name = name;
                        dialog.Close();
                    }
                    if (replace)
                    {
                        int idx = _lightGallery.GetNameIndex(light.name);
                        _lightGallery.RemoveAt(idx);
                        LightObj lgtObj = _lightGallery.SaveLightToGallery(light);
                        _lightGallery.Insert(idx, lgtObj);
                    }
                    else
                    {
                        LightObj lgtObj = _lightGallery.SaveLightToGallery(light);
                        _lightGallery.Add(lgtObj);
                    }
                }
            }
        }

        //Metoda wywoływana przez kliknięcie kliknięcie guzika w panelu transformacji
        public void transformPanelButtonClick(float transx, float transy, float transz,
                                                float rotatex, float rotatey, float rotatez,
                                                float scalex, float scaley, float scalez)
        {
            undo.Save(currScene);
            Transformations.Transformations.Translate(currScene, transx, transy, transz);
            Transformations.Transformations.Rotate(currScene, rotatex, rotatey, rotatez);
            Transformations.Transformations.ScalePar(currScene, scalex, scaley, scalez);

            //sceneChange = true;

            RenderViews();
        }

        // Pomocnicze metody do obsługi panelu kamer

        public int getCams()
        {
            return currScene.cams.Count;
        }

        public int getActiveCam()
        {
            return currScene.activeCamera;
        }

        public void cameraPanelParChange( int camIndex, int resX, int resY, float positionX, float positionY, float positionZ,
                                            float lookAtX, float lookAtY, float lookAtZ, float rotateAngle, float fovAngle) //zczytuje paraetry kamery z panelu
        {
            undo.Save(currScene);
            //Console.WriteLine(positionX+" "+positionY+" "+positionZ+" "+lookAtX+" "+lookAtY+" "+lookAtZ+" "+fovAngle);
            
            //if (camIndex > -1 && camIndex < currScene.cams.Count())
            {
                currScene.activeCamera = camIndex;
                currScene.cams.ElementAt(currScene.activeCamera).resolutionX = resX;
                currScene.cams.ElementAt(currScene.activeCamera).resolutionY = resY;
                currScene.cams.ElementAt(currScene.activeCamera).position.X = positionX;
                currScene.cams.ElementAt(currScene.activeCamera).position.Y = positionY;
                currScene.cams.ElementAt(currScene.activeCamera).position.Z = positionZ;
                currScene.cams.ElementAt(currScene.activeCamera).lookAt.X = lookAtX;
                currScene.cams.ElementAt(currScene.activeCamera).lookAt.Y = lookAtY;
                currScene.cams.ElementAt(currScene.activeCamera).lookAt.Z = lookAtZ;
                currScene.cams.ElementAt(currScene.activeCamera).fovAngle = fovAngle;
                currScene.cams.ElementAt(currScene.activeCamera).rotateAngle = rotateAngle;

                //sceneChange = true;
                RenderViews();
            }
            
        }

        public float[] cameraPanelActiveCamera(int cameraIndex) //ustawia aktywną kamerę oraz pobiera jej dane
        {
            currScene.activeCamera = cameraIndex;

            float[] tmp = new float[] { currScene.cams.ElementAt(currScene.activeCamera).resolutionX,
                                        currScene.cams.ElementAt(currScene.activeCamera).resolutionY,
                                        currScene.cams.ElementAt(currScene.activeCamera).position.X,
                                        currScene.cams.ElementAt(currScene.activeCamera).position.Y,
                                        currScene.cams.ElementAt(currScene.activeCamera).position.Z,
                                        currScene.cams.ElementAt(currScene.activeCamera).lookAt.X,
                                        currScene.cams.ElementAt(currScene.activeCamera).lookAt.Y,
                                        currScene.cams.ElementAt(currScene.activeCamera).lookAt.Z,
                                        currScene.cams.ElementAt(currScene.activeCamera).rotateAngle,
                                        currScene.cams.ElementAt(currScene.activeCamera).fovAngle};
            return tmp;
        }

        // Koniec pomocniczych metod do panelu kamer

        private void RemoveSurface(object sender, RoutedEventArgs e)
        {
            _surfaceGallery.RemoveAt(materialy_ListView.SelectedIndex);
        }

        private void RemoveLight(object sender, RoutedEventArgs e)
        {
            _lightGallery.RemoveAt(swiatla_ListView.SelectedIndex);
        }

        private void ViewsBezier_Wheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int x = (int)e.X;
            int y = (int)e.Y;

            ViewportOrientation viewport = GetViewportType(x, y);

            if (viewport != ViewportOrientation.None && viewport != ViewportOrientation.Perspective)
            {
                renderer.ScaleBezierOrtho((int)viewport, e.Delta * SystemInformation.MouseWheelScrollLines / 120, GetViewCoords(tabWidok.SelectedIndex), x, y);
                RenderBezier();
            }
            else
            {
                //Console.WriteLine(e.Delta * SystemInformation.MouseWheelScrollLines / 120);
                renderer.ScaleBezierPersp(e.Delta * SystemInformation.MouseWheelScrollLines / -120);
                RenderBezier();
            }
        }

        //private void ViewsBezier_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    ViewportInfo views = GetViewCoords();
        //    int x = (int)e.X;
        //    int y = (int)e.Y;

        //    ViewportOrientation viewport = GetViewportType(x, y);

        //    if (viewport != ViewportOrientation.None)
        //    {
        //        ViewportType viewportType = viewport == ViewportOrientation.Perspective ? ViewportType.Perspective : ViewportType.Orto;
        //        int rect = (int)viewport;
        //        int orthoRect = rect == 3 ? 0 : rect;

        //        SelectingElems.SelectBezierControlPoint(bezierSurface, renderer.bezierCam, viewportType, new System.Drawing.Point(x - views.posX[rect], y - views.posY[rect]),
        //                new System.Drawing.Point(views.sizeX[rect], views.sizeY[rect]), new Vector2(renderer.BezierOrthoWidth[orthoRect],
        //                (float)views.sizeY[rect] / views.sizeX[rect] * renderer.BezierOrthoWidth[orthoRect]), 
        //                renderer.BezierOrthoPos[orthoRect], renderer.BezierOrthoLookAt[orthoRect]);

        //        RenderBezier();
        //    }

        //}

        private void ViewsBezier_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ViewsBezier.Focus();

            if (e.Button == MouseButtons.Left && !GetShiftPressed())
            {
                if (bezierSurface.selectedPointIdx != -1)
                {
                    System.Windows.DataObject dataObject = new System.Windows.DataObject();
                    ViewportInfo coords = GetViewCoords(tabWidok.SelectedIndex);
                    undo.Save(currScene);

                    ViewportOrientation viewport = GetViewportType(e.X, e.Y);
                    switch (viewport)
                    {
                        case ViewportOrientation.Front:
                            dataObject.SetData("Viewport", "front");
                            break;

                        case ViewportOrientation.Side:
                            dataObject.SetData("Viewport", "side");
                            break;

                        case ViewportOrientation.Top:
                            dataObject.SetData("Viewport", "top");
                            break;

                        case ViewportOrientation.Perspective:
                            dataObject.SetData("Viewport", "perspective");
                            break;

                        default:
                            break;
                    }

                    dragX = (float)e.X + xOffset;
                    dragY = (float)e.Y + yOffset;
                    DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Move);
                }
            }
            else if (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Left && GetShiftPressed())
            {
                ViewportInfo views = GetViewCoords(tabWidok.SelectedIndex);
                int x = (int)e.X;
                int y = (int)e.Y;

                ViewportOrientation viewport = GetViewportType(x, y);
                if (shiftingViewport == ViewportOrientation.None && viewport != ViewportOrientation.None && viewport != ViewportOrientation.Perspective)
                {
                    shiftingViewport = viewport;
                }

                if (shiftingViewport != ViewportOrientation.None)
                {
                    renderer.MoveBezierOrtho((int)shiftingViewport, (int)-(e.X - mousePos.X), (int)-(e.Y - mousePos.Y),
                        views.sizeX[(int)shiftingViewport], views.sizeY[(int)shiftingViewport]);

                    RenderBezier();
                }
            }
            else
            {
                shiftingViewport = ViewportOrientation.None;
            }
            mousePos.X = e.X;
            mousePos.Y = e.Y;
        }

        private void bezierTab_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            //e.Effects = System.Windows.DragDropEffects.Copy | System.Windows.DragDropEffects.Move;
            //ViewportInfo views = GetViewCoords(tabWidok.SelectedIndex);

            //float x = (float)(e.GetPosition(this).X - dragX);
            //float y = (float)(e.GetPosition(this).Y - dragY);

            //// Przesuwanie
            //if (e.Data.GetData("Viewport") != null)
            //{
            //    if (e.Data.GetData("Viewport").Equals("front"))
            //    {
            //        // Lewy dolny panel dziala na x y, a więc jest to !!!PRZOD!!!
            //        float factor = (renderer.BezierOrthoWidth[0] / views.sizeX[0]);
            //        bezierSurface.translateSelectedPoint(x * factor, -y * factor, 0);
            //    }
            //    if (e.Data.GetData("Viewport").Equals("top"))
            //    {
            //        // Prawy górny panel działa na x z, a więc jest to !!!GORA!!!
            //        float factor = (renderer.BezierOrthoWidth[2] / views.sizeX[2]);
            //        bezierSurface.translateSelectedPoint(x * factor, 0, y * factor);
            //    }
            //    if (e.Data.GetData("Viewport").Equals("side"))
            //    {
            //        // Prawy dolny panel działa na y z, a wiec jest to !!!BOK!!!
            //        float factor = (renderer.BezierOrthoWidth[1] / views.sizeX[1]);
            //        bezierSurface.translateSelectedPoint(0, -y * factor, -x * factor);
            //    }
            //}

            //RenderBezier();

            //dragX = (float)e.GetPosition(this).X;
            //dragY = (float)e.GetPosition(this).Y;
        }

        private void ViewsBezier_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !GetShiftPressed())
            {
                ViewportInfo views = GetViewCoords(tabWidok.SelectedIndex);
                int x = (int)e.X;
                int y = (int)e.Y;

                ViewportOrientation viewport = GetViewportType(x, y);

                if (viewport != ViewportOrientation.None)
                {
                    ViewportType viewportType = viewport == ViewportOrientation.Perspective ? ViewportType.Perspective : ViewportType.Orto;
                    int rect = (int)viewport;
                    int orthoRect = rect == 3 ? 0 : rect;

                    SelectingElems.SelectBezierControlPoint(bezierSurface, renderer.bezierCam, viewportType, new System.Drawing.Point(x - views.posX[rect], y - views.posY[rect]),
                            new System.Drawing.Point(views.sizeX[rect], views.sizeY[rect]), new Vector2(renderer.BezierOrthoWidth[orthoRect],
                            (float)views.sizeY[rect] / views.sizeX[rect] * renderer.BezierOrthoWidth[orthoRect]),
                            renderer.BezierOrthoPos[orthoRect], renderer.BezierOrthoLookAt[orthoRect]);
                    RenderBezier();
                }

                if (bezierSurface.selectedPointIdx != -1)
                {
                    System.Windows.DataObject dataObject = new System.Windows.DataObject();
                    ViewportInfo coords = GetViewCoords(tabWidok.SelectedIndex);

                    //ViewportOrientation viewport = GetViewportType(e.X, e.Y);
                    switch (viewport)
                    {
                        case ViewportOrientation.Front:
                            dataObject.SetData("Viewport", "front");
                            break;

                        case ViewportOrientation.Side:
                            dataObject.SetData("Viewport", "side");
                            break;

                        case ViewportOrientation.Top:
                            dataObject.SetData("Viewport", "top");
                            break;

                        case ViewportOrientation.Perspective:
                            dataObject.SetData("Viewport", "perspective");
                            break;

                        default:
                            break;
                    }

                    dragX = (float)e.X + xOffset;
                    dragY = (float)e.Y + yOffset;
                    bezierUndo.Save(bezierSurface);
                    DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Move);
                    RenderBezier();
                }
                else
                {
                    System.Windows.DataObject dataObject = new System.Windows.DataObject();
                    dataObject.SetData("Bezier", bezierSurface);
                    DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Copy);
                }
            }
        }

        private void Views_MouseEnter(object sender, EventArgs e)
        {
            Views.Focus();
        }

        private void ViewsBezier_MouseEnter(object sender, EventArgs e)
        {
            ViewsBezier.Focus();
        }

        private void Views_MouseLeave(object sender, EventArgs e)
        {
            tabWidok.Focus();
        }

        private void ViewsBezier_MouseLeave(object sender, EventArgs e)
        {
            tabWidok.Focus();
        }

        /// <summary>
        /// Metoda odpowiadająca za przesuwanie płaszczyzn obcinających i zaznaczanie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Views_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ClipPlaneType planeType = ClipPlaneType.NONE;

            if (Renderer.Clipping && e.Button == MouseButtons.Left && !GetShiftPressed())
            {
                ViewportInfo views = GetViewCoords(tabWidok.SelectedIndex);
                int x = (int)e.X;
                int y = (int)e.Y;

                ViewportOrientation viewport = GetViewportType(x, y);

                if (viewport != ViewportOrientation.None)
                {
                    ViewportType viewportType = viewport == ViewportOrientation.Perspective
                                                    ? ViewportType.Perspective
                                                    : ViewportType.Orto;
                    int rect = (int)viewport;
                    int orthoRect = rect == 3 ? 0 : rect;

                    planeType = SelectingElems.SelectClippingPlane(Renderer.clipVertices, Renderer.clipIndices, viewportType,
                                                       new System.Drawing.Point(x - views.posX[rect],
                                                                                y - views.posY[rect]),
                                                       new System.Drawing.Point(views.sizeX[rect], views.sizeY[rect]),
                                                       new Vector2(renderer.OrthoWidth[orthoRect],
                                                                   (float)views.sizeY[rect] / views.sizeX[rect] *
                                                                   renderer.OrthoWidth[orthoRect]),
                                                       renderer.OrthoPos[orthoRect], renderer.OrthoLookAt[orthoRect],
                                                       viewport);

                    //Console.WriteLine(planeType.ToString());

                    if(planeType != ClipPlaneType.NONE)
                    {
                        System.Windows.DataObject dataObject = new System.Windows.DataObject();

                        switch(viewport)
                        {
                            case ViewportOrientation.Front:
                                dataObject.SetData("Viewport", "front");
                                break;

                            case ViewportOrientation.Side:
                                dataObject.SetData("Viewport", "side");
                                break;

                            case ViewportOrientation.Top:
                                dataObject.SetData("Viewport", "top");
                                break;

                            case ViewportOrientation.Perspective:
                                dataObject.SetData("Viewport", "perspective");
                                break;

                            default:
                                break;
                        }
                        dragX = (float)e.X + xOffset;
                        dragY = (float)e.Y + yOffset;
                        dataObject.SetData("Plane", planeType.ToString());
                        DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Copy);
                    }
                }
            }

            int sumSelected = currScene.selTriangles.Count + currScene.selLights.Count + currScene.selCams.Count;

            if(e.Button == MouseButtons.Left && planeType == ClipPlaneType.NONE && contextMenu.MenuItems[0].Checked == true && sumSelected <= 1 && !GetShiftPressed())
            {
                mouseDownHandled = true;

                ViewportInfo views = GetViewCoords(tabWidok.SelectedIndex);
                int x = (int)e.X;
                int y = (int)e.Y;

                ViewportOrientation viewport = GetViewportType(x, y);

                if(viewport != ViewportOrientation.None)
                {
                    ViewportType viewportType = viewport == ViewportOrientation.Perspective
                                                    ? ViewportType.Perspective
                                                    : ViewportType.Orto;
                    int rect = (int)viewport;
                    int orthoRect = rect == 3 ? 0 : rect;

                    SelectingElems.SelectElems(currScene, renderer.GetCamsPoints(), renderer.GetLightsPoints(), viewportType, new System.Drawing.Point(x - views.posX[rect], y - views.posY[rect]),
                        new System.Drawing.Point(views.sizeX[rect], views.sizeY[rect]), new Vector2(renderer.OrthoWidth[orthoRect], (float)views.sizeY[rect] / views.sizeX[rect] * renderer.OrthoWidth[orthoRect]),
                        renderer.OrthoPos[orthoRect], renderer.OrthoLookAt[orthoRect], GetCtrlPressed());

                    rect = (int)viewport;
                    orthoRect = rect == 3 ? 0 : rect;

                    System.Windows.DataObject dataObject = new System.Windows.DataObject();

                    switch(viewport)
                    {
                        case ViewportOrientation.Front:
                            dataObject.SetData("Viewport", "front");
                            break;

                        case ViewportOrientation.Side:
                            dataObject.SetData("Viewport", "side");
                            break;

                        case ViewportOrientation.Top:
                            dataObject.SetData("Viewport", "top");
                            break;

                        case ViewportOrientation.Perspective:
                            dataObject.SetData("Viewport", "perspective");
                            break;

                        default:
                            break;
                    }

                    dataObject.SetData("Point", SelectingElems.pointFound);

                    Renderer.RecalculateData(currScene);
                    if (GetAltPressed())
                    {
                        for (int i = 0; i < currScene.parts.Count; i++)
                        {
                            foreach (HierarchyMesh obj in currScene.selTriangles)
                            {
                                if (currScene.parts[i].triangles.Contains((int) obj.triangles[0]))
                                {
                                    Material_ material = null;
                                    foreach (Material_ m in currScene.materials)
                                    {
                                        if (m.name == currScene.materialAssign[i])
                                        {
                                            material = new Material_(m);
                                            material.colorR = material.colorR*255;
                                            material.colorG = material.colorG*255;
                                            material.colorB = material.colorB*255;
                                        }
                                    }
                                    dataObject.SetData("Object", material);
                                    i = Int32.MaxValue - 1;
                                    break;
                                }
                            }
                        }
                    }

                    if(currScene.selLights.Count == 1 && GetAltPressed())
                    {
                        dataObject.SetData("Light", currScene.lights[currScene.selLights[0]]);
                        dataObject.SetData("LightPrevious", new Light_(currScene.lights[currScene.selLights[0]]));
                    }

                    if (currScene.selTriangles.Count > 0 && GetAltPressed())
                    {
                        System.Windows.DataObject do1 = new DataObject();
                        do1.SetData("Prepared", "");
                        DragDrop.DoDragDrop(this, do1, System.Windows.DragDropEffects.Copy);
                    }

                    dragX = (float)e.X + xOffset;
                    dragY = (float)e.Y + yOffset;
                    undo.Save(currScene);
                    DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Copy);

                    RenderViews();
                }
            }
        }

        private void Camera_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.DataObject dataObject = new System.Windows.DataObject();
                dataObject.SetData("Object", new Camera());
                DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Copy);
            }
        }

        private Vector3 CalculateTranslation(float x, float y)
        {
            Vector3 translation = new Vector3(0, 0, 0);

            ViewportOrientation viewport = GetViewportType((int)x, (int)y);
            ViewportInfo coords = GetViewCoords(tabWidok.SelectedIndex);

            switch (viewport)
            {
                case ViewportOrientation.Perspective:
                    Vector3 outCamPos, outSurfPos;
                    SelectingElems.CalcPerspCoords(new System.Drawing.Point((int)x - coords.posX[3], (int)y - coords.posY[3]),
                        new System.Drawing.Point(coords.sizeX[3], coords.sizeY[3]), currScene.cams[currScene.activeCamera].fovAngle,
                        currScene.cams[currScene.activeCamera].rotateAngle, currScene.cams[currScene.activeCamera].position,
                        currScene.cams[currScene.activeCamera].lookAt, out outCamPos, out outSurfPos);

                    Vector3 dir = Vector3.Normalize(outSurfPos - outCamPos);

                    translation = outCamPos + (15 - 0.083f * currScene.cams[currScene.activeCamera].fovAngle) * dir;
                    break;

                case ViewportOrientation.Front:
                    translation.X = (x - (coords.posX[0] + coords.sizeX[0] / 2)) / coords.sizeX[0] * renderer.OrthoWidth[0] + renderer.OrthoLookAt[0].X;
                    translation.Z = renderer.OrthoLookAt[1].Z;
                    translation.Y = -(y - (coords.posY[0] + coords.sizeY[0] / 2)) / coords.sizeY[0] * (coords.sizeY[0] * renderer.OrthoWidth[0] / coords.sizeX[0])
                        + renderer.OrthoLookAt[0].Y;
                    break;

                case ViewportOrientation.Side:
                    translation.Z = -(x - (coords.posX[1] + coords.sizeX[1] / 2)) / coords.sizeX[1] * renderer.OrthoWidth[1] + renderer.OrthoLookAt[1].Z;
                    translation.X = renderer.OrthoLookAt[0].X;
                    translation.Y = -(y - (coords.posY[1] + coords.sizeY[1] / 2)) / coords.sizeY[1] * (coords.sizeY[1] * renderer.OrthoWidth[1] / coords.sizeX[1])
                        + renderer.OrthoLookAt[1].Y;
                    break;

                case ViewportOrientation.Top:
                    translation.X = (x - (coords.posX[2] + coords.sizeX[2] / 2)) / coords.sizeX[2] * renderer.OrthoWidth[2] + renderer.OrthoLookAt[2].X;
                    translation.Y = renderer.OrthoLookAt[0].Y;
                    translation.Z = (y - (coords.posY[2] + coords.sizeY[2] / 2)) / coords.sizeY[2] * (coords.sizeY[2] * renderer.OrthoWidth[2] / coords.sizeX[2])
                        + renderer.OrthoLookAt[2].Z;
                    break;

                default:
                    break;
            }

            return translation;
        }

        private void Copy(object sender, RoutedEventArgs e)
        {
            copyPaste.CopySelection(currScene);
            //sceneChange = true;
        }

        private void Paste(object sender, RoutedEventArgs e)
        {
            undo.Save(currScene);
            //float x = (float)System.Windows.Forms.Cursor.Position.X; //(float)Mouse.GetPosition(this).X;
            //float y = (float)System.Windows.Forms.Cursor.Position.Y; //(float)Mouse.GetPosition(this).Y;
            Vector3 translation = renderer.OrthoLookAt[0];
            translation.Z = renderer.OrthoLookAt[1].Z;

            if (Mouse.GetPosition(this).X < 0 && Mouse.GetPosition(this).Y < 0 || this.WindowState == WindowState.Maximized)
            {
                float x = (float)(System.Windows.Forms.Control.MousePosition.X + Mouse.GetPosition(this).X) - xOffset; //(float)Mouse.GetPosition(this).X;
                float y = (float)(System.Windows.Forms.Control.MousePosition.Y + Mouse.GetPosition(this).Y) - yOffset; //(float)Mouse.GetPosition(this).Y;

                //Console.WriteLine(x + " " + y + " " + Mouse.GetPosition(this).X + " " + Mouse.GetPosition(this).Y);

                if (!(x < 0 || y < 0))
                    translation = CalculateTranslation(x, y);

            }
            //int camsBeforePaste = currScene.cams.Count;

            ViewportOrientation viewport = GetViewportType((int)mousePos.X, (int)mousePos.Y);
            copyPaste.Paste(currScene, translation, viewport);
            //for (int i = 0; i < currScene.cams.Count - camsBeforePaste; i++)
            //{
            //    cameraPan.comboBox1.Items.Add("Kamera " + (camsBeforePaste+1+i));
            //}
            trNumTBG1.Text = currScene.triangles.Count().ToString();
            trNumTBG2.Text = currScene.triangles.Count().ToString();
            trNumTBG3.Text = currScene.triangles.Count().ToString();
            trNumTBG4.Text = currScene.triangles.Count().ToString();

            initializeTreeView();
            currScene.selectedHierObj = null;
            Renderer.RecalculateData(currScene);

            //sceneChange = true;
            RenderViews();
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            undo.Save(currScene);
            copyPaste.CopySelection(currScene);
            currScene.DeleteSelected();
            Renderer.RecalculateNormals(currScene);
            Renderer.RecalculateData(currScene);

            trNumTBG1.Text = currScene.triangles.Count().ToString();
            trNumTBG2.Text = currScene.triangles.Count().ToString();
            trNumTBG3.Text = currScene.triangles.Count().ToString();
            trNumTBG4.Text = currScene.triangles.Count().ToString();

            //Console.WriteLine("usuwnaie main window");

            if (currScene.cameraRemoved)
            {
                //Console.WriteLine("kamer w scenie " + currScene.cams.Count());
                //Console.WriteLine("Aktywna: " + currScene.activeCamera);

                int tmp = currScene.activeCamera;
                cameraPan.comboBox1.SelectedIndex = 0;
                cameraPan.comboBox1.Items.RemoveAt(currScene.cams.Count());
                cameraPan.comboBox1.SelectedIndex = tmp;
                currScene.cameraRemoved = false;
            }
            initializeTreeView();
            currScene.selectedHierObj = null;

            //sceneChange = true;
            RenderViews();
        }

        //Panel hierarchii oraz operacje z nim związane
        //Metody pomocnicze

        private void initializeTreeView()
        {
            treeView1.Items.Clear();
            TreeViewItem root = new TreeViewItem();
            root.Header = "Scena";
            root.IsSelected = false;
            root.AllowDrop = true;

            //Console.WriteLine("ładowanie widoku hierarchii");
            List<HierarchyObject> treeList = new List<HierarchyObject>();
            foreach (HierarchyObject ho in currScene.hierarchy.objects)
            {
                //Console.WriteLine(ho.GetType());
                if (ho is HierarchyMesh)
                {
                    addTreeViewMesh((HierarchyMesh)ho, root);
                }
                else if (ho is HierarchyLight)
                {
                    addTreeViewLight((HierarchyLight)ho, root);
                }
                else //hierarchyNode
                {
                    // Wyłączona hierarchia
                    //if (!(ho is HierarchyLight))
                    {
                        HierarchyNode hn = (HierarchyNode)ho;
                        if (hn.hObjects.Count == 0) //jeśli w między czasie zostały przeniesione/usunięte niższe warstwy
                        {
                            treeList.Add(ho);
                        }
                        else
                        {
                            addTreeViewNode((HierarchyNode)ho, root);
                        }
                    }
                }
            }
            foreach (HierarchyObject ho in treeList)
            {
                currScene.hierarchy.objects.Remove(ho);
            }
            treeView1.Items.Add(root);

            List<TreeViewItem> emptyNode = new List<TreeViewItem>();
            List<TreeViewItem> emptyNodeParent = new List<TreeViewItem>();

            emptyNodesRemoved = true;
            while (emptyNodesRemoved)
            {
                emptyNodesRemoved = false;
                removeEmtpyNodes((TreeViewItem)treeView1.Items.GetItemAt(0), emptyNode, emptyNodeParent);
                for (int i = 0; i < emptyNodeParent.Count; i++)
                {
                    emptyNodeParent.ElementAt(i).Items.Remove(emptyNode.ElementAt(i));
                }

                emptyNode.Clear();
                emptyNodeParent.Clear();
            }
            refreshHierarchy();
        }

        private void addTreeViewNode(HierarchyNode hobj, TreeViewItem parent)
        {
            TreeViewItem node = new TreeViewItem();
            node.Header = hobj.ToString();
            node.IsSelected = false;
            List<HierarchyObject> treeList = new List<HierarchyObject>();
            foreach (HierarchyObject ho in hobj.hObjects)
            {
                if (ho is HierarchyMesh)
                    addTreeViewMesh((HierarchyMesh)ho, node);
                else if (ho is HierarchyLight)
                    addTreeViewLight((HierarchyLight)ho, node);
                else //hierarchyNode
                {
                    HierarchyNode hn = (HierarchyNode)ho;
                    if (hn.hObjects.Count == 0) //jeśli w między czasie zostały przeniesione/usunięte niższe warstwy
                    {
                        //hobj.hObjects.Remove(ho);
                        treeList.Add(ho);
                    }
                    else
                    {
                        addTreeViewNode((HierarchyNode)ho, node);
                    }
                }
            }
            foreach (HierarchyObject ho in treeList)
            {
                hobj.hObjects.Remove(ho);
            }

            parent.Items.Add(node);
        }

        private void addTreeViewMesh(HierarchyMesh hobj, TreeViewItem parent)
        {
            TreeViewItem child = new TreeViewItem();
            child.Header = hobj.ToString();
            child.Name = "mesh"+"_"+hobj.triangles.ElementAt(0).ToString(); //identyfikator mesha
            child.IsSelected = false;
            parent.Items.Add(child);
        }

        private void addTreeViewLight(HierarchyLight hobj, TreeViewItem parent)
        {
            TreeViewItem child = new TreeViewItem();
            child.Header = hobj.name;
            child.Name = ("light"+"_" + (hobj.lightIndex)); //identyfikacja kamery po indeksie
            child.IsSelected = false;
            parent.Items.Add(child);
        }

        //private void selectingFromHierarchy(TreeViewItem node, bool selAll)  //warto dodać listę wszystkich meshy i świateł żeby ich nie szukać za każdym razem
        //{

        //    if (node.HasItems && !node.IsSelected && !selAll) // jesteśmy w węźle, przechodzimy w głąb bez zaznaczania
        //    {
        //        //Console.WriteLine("wchodzimy głębiej, zaznaczenia nie znaleziono");
        //        foreach (TreeViewItem tvi in node.Items)
        //        {
        //            selectingFromHierarchy(tvi, false);
        //        }
        //    }
        //    else if (node.HasItems && !node.IsSelected && selAll || node.HasItems && node.IsSelected) // jesteśmy w węźle, przechodzimy w głąb i zaznaczamy wszystko
        //    {
        //        //Console.WriteLine("wchodzimy głebiej zaznaczając wszystkie meshe, bo: jesteśmy w zaznaczonym/ weszliśmy już");
        //        foreach (TreeViewItem tvi in node.Items)
        //        {
        //            selectingFromHierarchy(tvi, true);
        //        }
        //    }
        //    else if (!node.IsSelected && selAll || node.IsSelected) //mesh(lub light), którego dodajemy do zaznaczonych
        //    {
        //        char[] delimiterChars = { '_' };
        //        string[] words = node.Name.Split(delimiterChars);
        //        if (words[0].Equals("mesh"))
        //            selectMesh(Convert.ToInt32(words[1]), currScene.hierarchy.objects);
        //        else
        //        {
        //            Console.WriteLine("zaznaczamy swiatlo " + words[0].ToString() + "   " + words[1].ToString());
        //            //selectLight(node.Name, lights);
        //            currScene.selLights.Add(Convert.ToInt32(words[1]));
        //        }
        //    }
        //}

        private void selectingFromHierarchy(TreeViewItem node, HierarchyObject ho, bool selAll, bool found)  //warto dodać listę wszystkich meshy i świateł żeby ich nie szukać za każdym razem
        {

            if (node.HasItems && !node.IsSelected && !selAll) // jesteśmy w węźle, przechodzimy w głąb bez zaznaczania
            {
                if (ho is HierarchyNode)
                {
                    //HierarchyNode hn = new HierarchyNode(ho.name);
                    HierarchyNode hn = (HierarchyNode)ho;
                    //Console.WriteLine("ho "+hn.hObjects.Count + " tvi  " + node.Items.Count);
                    //Console.WriteLine("wchodzimy głębiej, zaznaczenia nie znaleziono");
                    //foreach (TreeViewItem tvi in node.Items)
                    for (int i = 0; i < node.Items.Count; i++)
                    {
                        selectingFromHierarchy((TreeViewItem)node.Items.GetItemAt(i), (HierarchyObject)(hn.hObjects.ElementAt(i)), false, found);
                    }
                }
                //else
                    //Console.WriteLine("hierarchie sie nie pokrywaja1");
            }
            else if (node.HasItems && !node.IsSelected && selAll)// || node.HasItems && node.IsSelected) // jesteśmy w węźle, przechodzimy w głąb i zaznaczamy wszystko
            {
                if (ho is HierarchyNode)
                {
                    //Console.WriteLine("asdaasfa");
                    //HierarchyNode hn = new HierarchyNode(ho.name);
                    HierarchyNode hn = (HierarchyNode)ho;
                    //Console.WriteLine("wchodzimy głebiej zaznaczając wszystkie meshe, bo: jesteśmy w zaznaczonym/ weszliśmy już");
                    for (int i = 0; i < node.Items.Count; i++)
                    {
                        selectingFromHierarchy((TreeViewItem) node.Items.GetItemAt(i),
                                               (HierarchyObject) (hn.hObjects.ElementAt(i)), true, found);
                    }
                }
                //else
                    //Console.WriteLine("hierarchie sie nie pokrywaja2");
            }
            else if (node.HasItems && node.HasItems && node.IsSelected) // jesteśmy w węźle zaznaczonym, przechodzimy w głąb i zaznaczamy wszystko
            {
                if (ho is HierarchyNode)
                {
                    //Console.WriteLine("asdasfdqwfdq");
                    //HierarchyNode hn = new HierarchyNode(ho.name);
                    HierarchyNode hn = (HierarchyNode)ho;
                    if (!found)
                    {
                        currScene.selectedHierObj = (HierarchyNode) ho;
                        found = true;
                    }
                    //currScene.addWithHierarchy = true;

                    for (int i = 0; i < node.Items.Count; i++)
                    {
                        selectingFromHierarchy((TreeViewItem)node.Items.GetItemAt(i), (HierarchyObject)(hn.hObjects.ElementAt(i)), true, found);
                    }
                }
                //else
                    //Console.WriteLine("hierarchie sie nie pokrywaja3");
            }

            else if (!node.IsSelected && selAll || node.IsSelected) //mesh(lub light), którego dodajemy do zaznaczonych
            {
                char[] delimiterChars = { '_' };
                string[] words = node.Name.Split(delimiterChars);
                if (words[0].Equals("mesh"))
                    selectMesh(Convert.ToInt32(words[1]), currScene.hierarchy.objects);
                else
                {
                    //Console.WriteLine("zaznaczamy swiatlo " + words[0].ToString() + "   " + words[1].ToString());
                    //selectLight(node.Name, lights);
                    currScene.selLights.Add(Convert.ToInt32(words[1]));
                }
            }
        }

        private void selectMesh(int firstTriangleIndex, List<HierarchyObject> listHObj)
        {
            foreach (HierarchyObject ho in listHObj)
            {
                if (ho is HierarchyMesh)//jeśli jest to mesh
                {
                    HierarchyMesh hm = (HierarchyMesh)ho;
                    if ((int)hm.triangles.ElementAt(0) == firstTriangleIndex)  //jeśli jest to mesh którego szukamy
                    {
                        currScene.selTriangles.Add(hm);
                        break;
                    }
                }
                else if (ho is HierarchyNode)//jeśli jest to node
                {
                    HierarchyNode hn = (HierarchyNode)ho;
                    selectMesh(firstTriangleIndex, hn.hObjects);
                }
            }
        }

        //private void selectLight(String lightName, List<Light_> lights)
        //{
        //    foreach (Light_ hl in lights)
        //    {
        //        if (lightName.Equals(hl.name))
        //        {
        //            currScene.selLights.Add(lights.IndexOf(hl));
        //            break;
        //        }
        //    }
        //}

        private void deleteMesh(int firstTriangleIndex, List<HierarchyObject> listHObj)
        {
            List<HierarchyObject> treeList = new List<HierarchyObject>();
            foreach (HierarchyObject ho in listHObj)
            {
                if (ho is HierarchyMesh)//jeśli jest to mesh
                {
                    HierarchyMesh hm = (HierarchyMesh)ho;
                    if ((int)hm.triangles.ElementAt(0) == firstTriangleIndex)  //jeśli jest to mesh którego szukamy
                    {
                        //listHObj.Remove(ho);
                        treeList.Add(ho);
                    }
                }
                else if (ho is HierarchyNode)  //jeśli jest to node
                {
                    HierarchyNode hn = (HierarchyNode)ho;
                    deleteMesh(firstTriangleIndex, hn.hObjects);
                }
            }
            foreach (HierarchyObject ho in treeList)
            {
                listHObj.Remove(ho);
            }
        }

        private void deleteLight(int lightId, List<HierarchyObject> listHObj)
        {
            List<HierarchyObject> treeList = new List<HierarchyObject>();
            foreach (HierarchyObject ho in listHObj)
            {
                if (ho is HierarchyLight)//jeśli jest to light
                {
                    HierarchyLight hl = (HierarchyLight)ho;
                    if (hl.lightIndex== lightId)  //jeśli jest to swiatlo, które szukamy
                    {
                        //listHObj.Remove(ho);
                        treeList.Add(ho);
                    }
                }
                else if (ho is HierarchyNode)  //jeśli jest to node
                {
                    HierarchyNode hn = (HierarchyNode)ho;
                    deleteLight(lightId, hn.hObjects);
                }
            }
            foreach (HierarchyObject ho in treeList)
            {
                listHObj.Remove(ho);
            }
        }
        
        private void moveNode(TreeViewItem node, TreeViewItem movedItem, List<TreeViewItem> movedParent,
                                 TreeViewItem dropOnItem, List<TreeViewItem> dropOnParent)
        {
            foreach (TreeViewItem tvi in node.Items)
            {
                if (tvi.Equals(movedItem))// && !dropOnItem.Equals(tvi))
                {
                    movedParent.Add(node);
                    //if (tvi.HasItems && dropOnParent.Count == 0) //jeśli jeszcze nie znaleźliśmy parenta dropOn itema to go i tak nie moze byc nizej
                    //moveNode(tvi, movedItem, movedParent, movedGrandParent, dropOnItem, dropOnParent, granPaFound);
                    //Console.WriteLine("ustalamy parenta przenoszonego ");
                }
                //else if (tvi.Equals(movedItem) && dropOnItem.Equals(tvi))
                //{
                //    Console.WriteLine("No drop");
                //}
                else if (dropOnItem.Equals(tvi))// && !tvi.Equals(movedItem) )
                {
                    //dropOnParent.Clear();
                    dropOnParent.Add(node);
                    //Console.WriteLine("ustalamy parenta dropOn");
                    if (tvi.HasItems && movedParent.Count == 0) //jeśli jeszcze nie znaleźliśmy parenta wybranego itema
                        moveNode(tvi, movedItem, movedParent, dropOnItem, dropOnParent);
                }
                else if (tvi.HasItems)
                {
                    moveNode(tvi, movedItem, movedParent, dropOnItem, dropOnParent);
                }
            }

            //sceneChange = true;
        }

        private void allowHierarchyDrop(TreeViewItem node, TreeViewItem dropSel, TreeViewItem dropOn, bool selFound)
        {
            foreach (TreeViewItem tvi in node.Items)
            {
                if (tvi.Equals(dropOn) && !selFound) // nie natknęliśmy się wcześniej na dropSel
                {
                    canDrop = true;
                    //Console.WriteLine("can drop");
                }
                else if (tvi.Equals(dropOn) && selFound)//dropOn jest potomkiem dropSel
                {
                    canDrop = false;
                    //Console.WriteLine("cant drop");
                }
                else if (tvi.Equals(dropSel)) // natknelismy sie na dropsel, musimy sprawdzic czy dropOn jest jego potomkiem
                {
                    //schodzimy nizej
                    bool selFound1 = true;
                    allowHierarchyDrop(tvi, dropSel, dropOn, selFound1);
                }
                else //nie jestesmy ani w dropSel ani dropOn
                    //if(selFound)//jeśli mamy dropSel i to nie jest dropOn - schodzimy niżej
                    if (tvi.HasItems)  //jeśli nie jestśmy w liściach - schodzimy niżej
                        allowHierarchyDrop(tvi, dropSel, dropOn, selFound);
            }
        }

        private void removeEmtpyNodes(TreeViewItem node, List<TreeViewItem> emptyNode, List<TreeViewItem> emptyNodeParent)
        {
            List<HierarchyLight> hLights = new List<HierarchyLight>();
            hLights = currScene.hierarchy.GetAllLights();

            foreach (TreeViewItem tvi in node.Items)
            {                
                char[] delimiterChars = { '_' };
                string[] words = tvi.Name.Split(delimiterChars);
                //Console.WriteLine("node name: "+tvi.Name);
                //Console.WriteLine("words 0: " + words[0]);
                //Console.WriteLine("words[0] " + words[0]);
                if (!tvi.HasItems && !words[0].Equals("mesh") && !words[0].Equals("light"))//tvi.Name.Equals(null))
                {
                    //Console.WriteLine("usuwamy pustego noda");
                    emptyNode.Insert(0, tvi);
                    emptyNodeParent.Insert(0, node);
                    emptyNodesRemoved = true;
                }
                else if (tvi.HasItems)
                {
                    removeEmtpyNodes(tvi, emptyNode, emptyNodeParent);
                }
            }
        }

        private void refreshHierarchy() //po modyfikacjach w panelu hierarchii "odświeżamy" hierarchię sceny
        {
            Hierarchy hierarchy = new Hierarchy();

            if (((TreeViewItem)treeView1.Items.GetItemAt(0)).HasItems)
            {
                List<HierarchyMesh> meshes = currScene.hierarchy.GetAllMeshes();
                List<HierarchyLight> lights = currScene.hierarchy.GetAllLights();
                foreach (TreeViewItem tvi in ((TreeViewItem)treeView1.Items.GetItemAt(0)).Items)
                {
                    if (tvi.HasItems)
                    {
                        HierarchyNode hn = new HierarchyNode(tvi.Header.ToString());
                        buildHierarchy(hn, tvi, meshes, lights);
                        hierarchy.objects.Add(hn);
                        //Console.WriteLine("budujemy hierarchie - node :" + hn.name.ToString());
                    }
                    else 
                    {
                        char[] delimiterChars = { '_' };
                        string[] words = tvi.Name.Split(delimiterChars);
                        if (words[0].Equals("mesh"))
                        {
                            HierarchyMesh hm = new HierarchyMesh(tvi.Header.ToString());
                            foreach (HierarchyMesh hmesh in meshes)
                            {
                                if (Convert.ToInt32(words[1]) == (int)hmesh.triangles.ElementAt(0))
                                {
                                    hm = hmesh;
                                    hm.name = tvi.Header.ToString();
                                    break;
                                }
                            }
                            //Console.WriteLine("budujemy hierarchie - mesh :" + hm.name.ToString());
                            hierarchy.objects.Add(hm);
                        }
                        else //if(words[0].Equals("light"));
                        {
                            HierarchyLight hl = new HierarchyLight(tvi.Header.ToString(), Convert.ToInt32(words[1]));
                            hl.name = tvi.Header.ToString();
                            hierarchy.objects.Add(hl);
                        }
                    }
                }
            }
            currScene.hierarchy = hierarchy;
        }

        private void buildHierarchy(HierarchyNode nodeParent, TreeViewItem treeItem, 
                                    List<HierarchyMesh> meshes, List<HierarchyLight> lights)
        {
            foreach (TreeViewItem tvi in treeItem.Items)
            {
                if (tvi.HasItems)
                {
                    HierarchyNode hn = new HierarchyNode(tvi.Header.ToString());
                    //Console.WriteLine("nazwa nodea " + tvi.Header.ToString());
                    buildHierarchy(hn, tvi, meshes, lights);
                    //Console.WriteLine("budujemy hierarchie - node :" + hn.name.ToString());
                    nodeParent.hObjects.Add(hn);
                }
                else //mesh || light
                {
                    char[] delimiterChars = { '_' };
                    string[] words = tvi.Name.Split(delimiterChars);
                    if (words[0].Equals("mesh"))  //mesh
                    {
                        HierarchyMesh hm = new HierarchyMesh(tvi.Header.ToString());
                        foreach (HierarchyMesh hmesh in meshes)
                        {
                            if (Convert.ToInt32(words[1]) == (int)hmesh.triangles.ElementAt(0))
                            {
                                hm = hmesh;
                                hm.name = tvi.Header.ToString();
                                break;
                            }
                        }
                        //Console.WriteLine("budujemy hierarchie - mesh :" + hm.name.ToString());
                        nodeParent.hObjects.Add(hm);
                    }
                    else //if(words[0].Equals("light"));
                    {

                        HierarchyLight hl = new HierarchyLight(tvi.Name.ToString(), (Convert.ToInt32(words[1])));
                        hl.name = tvi.Header.ToString();
                        nodeParent.hObjects.Add(hl);
                    }
                }
            }
        }

        //Zdarzenia - panel hierarchii

        private void treeView1_GotFocus(object sender, RoutedEventArgs e)
        {
            if (currScene.hierarchyChange)
            {
                currScene.hierarchyChange = false;
                initializeTreeView();
            }

            foreach (TreeViewItem tvi in treeView1.Items) //w zasadzie to tylko dla roota
            {
                //Console.WriteLine("Sprawdzamy czy cos sie zaznaczylo");
                currScene.selTriangles.Clear();
                currScene.selLights.Clear();
                //List<HierarchyLight> lights = currScene.hierarchy.GetAllLights();
                //if (tvi.IsSelected && tvi.HasItems)
                //    selectingFromHierarchy(tvi, true);
                //else if (tvi.HasItems)
                //    selectingFromHierarchy(tvi, false);
                if (!((TreeViewItem)treeView1.Items.GetItemAt(0)).IsSelected) //jeżeli root nie jest wybrany
                {
                    for (int i = 0; i < ((TreeViewItem)treeView1.Items.GetItemAt(0)).Items.Count; i++)
                    {
                        if (((TreeViewItem)(((TreeViewItem)treeView1.Items.GetItemAt(0)).Items.GetItemAt(i))).IsSelected)
                            selectingFromHierarchy((TreeViewItem)(((TreeViewItem)treeView1.Items.GetItemAt(0)).Items.GetItemAt(i)), (HierarchyObject)(currScene.hierarchy.objects.ElementAt(i)), true, false);
                        else
                            selectingFromHierarchy((TreeViewItem)(((TreeViewItem)treeView1.Items.GetItemAt(0)).Items.GetItemAt(i)), (HierarchyObject)(currScene.hierarchy.objects.ElementAt(i)), false, false);
                    }
                }
                else //jeżeli mamy zaznaczoną całą scenę
                {
                    for (int i = 0; i < ((TreeViewItem)treeView1.Items.GetItemAt(0)).Items.Count; i++)
                    {
                            selectingFromHierarchy((TreeViewItem)(((TreeViewItem)treeView1.Items.GetItemAt(0)).Items.GetItemAt(i)), (HierarchyObject)(currScene.hierarchy.objects.ElementAt(i)), true, false);                        
                    }
                }
            }

            Renderer.RecalculateData(currScene);
            RenderViews();
        }

        private void treeView1_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent("treeItem"))
            {
                FrameworkElement frameworkElement = e.OriginalSource as FrameworkElement;
                while (VisualTreeHelper.GetParent(frameworkElement).GetType() != typeof(TreeViewItem))
                {
                    frameworkElement = VisualTreeHelper.GetParent(frameworkElement) as FrameworkElement;
                }
                TreeViewItem dropOn = ((TreeViewItem)VisualTreeHelper.GetParent(frameworkElement));
                TreeViewItem dropSel = (TreeViewItem)treeView1.SelectedItem;
                bool tmp = false;
                allowHierarchyDrop((TreeViewItem)treeView1.Items.GetItemAt(0), dropSel, dropOn, tmp);

                if (!dropOn.Equals(dropSel) && canDrop)
                {
                    List<TreeViewItem> parentDropOn = new List<TreeViewItem>();
                    List<TreeViewItem> parentDropSel = new List<TreeViewItem>();

                    foreach (TreeViewItem tvi in treeView1.Items)
                    {
                        moveNode(tvi, dropSel, parentDropSel, dropOn, parentDropOn);
                    }

                    parentDropSel.ElementAt(0).Items.Remove(dropSel); //usuwamy połączenie z rodzicem przenoszonego itema

                    if (dropOn.Items.Count == 0 && !dropOn.Equals(treeView1.Items.GetItemAt(0)))  //jeśli przenosimy na liść(mesh/light) dodajemy nowy węzeł do którego zamieszczamy oba itemy
                    {
                        parentDropOn.ElementAt(0).Items.Remove(dropOn);
                        TreeViewItem newNode = new TreeViewItem();
                        newNode.Items.Add(dropOn);
                        newNode.Items.Add(dropSel);
                        newNode.Header = "Węzeł";
                        parentDropOn.ElementAt(0).Items.Add(newNode);

                        Modeler.DialogBoxes.NameDialog newName = new NameDialog();
                        string nodeName = "Węzeł";
                        bool res = (bool)newName.Show("Podaj nazwę obiektu", nodeName);
                        if (!res)
                            return;
                        newNode.Header = newName.Result;
                    }
                    else
                    {
                        dropOn.Items.Add(dropSel);
                    }

                    parentDropSel.Clear();
                    parentDropOn.Clear();

                    List<TreeViewItem> emptyNode = new List<TreeViewItem>();
                    List<TreeViewItem> emptyNodeParent = new List<TreeViewItem>();

                    emptyNodesRemoved = true;
                    while (emptyNodesRemoved)
                    {
                        emptyNodesRemoved = false;
                        removeEmtpyNodes((TreeViewItem)treeView1.Items.GetItemAt(0), emptyNode, emptyNodeParent);
                        for (int i = 0; i < emptyNodeParent.Count; i++)
                        {
                            emptyNodeParent.ElementAt(i).Items.Remove(emptyNode.ElementAt(i));
                        }

                        emptyNode.Clear();
                        emptyNodeParent.Clear();
                    }
                    currScene.selectedHierObj = null;
                    refreshHierarchy();
                }

                //sceneChange = true;
            }
        }

        private void treeView1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && treeView1.SelectedItem != null && treeView1.SelectedItem != treeView1.Items.GetItemAt(0))
            {
                System.Windows.DataObject dataObject = new System.Windows.DataObject();
                dataObject.SetData("treeItem", treeView1.SelectedItem);
                DragDrop.DoDragDrop(this, dataObject, System.Windows.DragDropEffects.Move);
            }
        }

        private void treeView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            bool flag = true;
            FrameworkElement frameworkElement = e.OriginalSource as FrameworkElement;
            while (VisualTreeHelper.GetParent(frameworkElement).GetType() != typeof(TreeViewItem))
            {
                if (frameworkElement.GetType() == typeof(System.Windows.Controls.ScrollViewer))
                {
                    flag = false;
                    break;
                }

                //Console.WriteLine("parent ; " + (VisualTreeHelper.GetParent(frameworkElement).GetType()).ToString());
                frameworkElement = VisualTreeHelper.GetParent(frameworkElement) as FrameworkElement;
            }

            if (flag)
            {
                TreeViewItem doubleClicked = ((TreeViewItem)VisualTreeHelper.GetParent(frameworkElement));

                if (doubleClicked.Equals(treeView1.SelectedItem) && !doubleClicked.Equals(treeView1.Items.GetItemAt(0)))
                {
                    //Console.WriteLine("Zmiana nazwy");
                    Modeler.DialogBoxes.NameDialog newName = new NameDialog();
                    bool res = (bool)newName.Show("Podaj nową nazwę dla zaznaczonego obiektu w panelu hierarchii ", "");
                    if (!res)
                        return;
                    doubleClicked.Header = newName.Result;

                    //sceneChange = true;
                }
                
                //TO DO sprawdzanie czy światło ma unikalną nazwę

                refreshHierarchy();
            }
        }

        private void GrupujB_Click(object sender, RoutedEventArgs e) //pozmienia hierarchię a później stworzy widok od nowa
        {
            if (currScene.selTriangles.Count > 1 && currScene.selLights.Count ==0)  //zaznaczone same meshe
            {
                HierarchyNode node = new HierarchyNode("Węzeł");
                Modeler.DialogBoxes.NameDialog newName = new NameDialog();
                string nodeName = "Węzeł";
                bool res = (bool)newName.Show("Podaj nazwę obiektu", nodeName);
                if (!res)
                    return;
                node.name = newName.Result;

                foreach (HierarchyMesh mesh in currScene.selTriangles)
                {
                    node.hObjects.Add(mesh);
                    deleteMesh((int)mesh.triangles.ElementAt(0), currScene.hierarchy.objects);
                }
                currScene.hierarchy.objects.Add(node);
                currScene.hierarchyChange = false;
                initializeTreeView();

                //sceneChange = true;
            }
            else if (currScene.selTriangles.Count == 0 && currScene.selLights.Count > 0) //zaznaczone same światła
            {
                HierarchyNode node = new HierarchyNode("Węzeł");
                List<HierarchyLight> lights = currScene.hierarchy.GetAllLights();
                
                foreach (int lightIndex in currScene.selLights)
                {
                    node.hObjects.Add(lights.ElementAt(lightIndex));
                    deleteLight(lightIndex, currScene.hierarchy.objects);
                }
                currScene.hierarchy.objects.Add(node);
                currScene.hierarchyChange = false;
                initializeTreeView();

                //sceneChange = true;
            }
            else //zaznaczone światła i meshe
            {
                HierarchyNode node = new HierarchyNode("Węzeł");
                List<HierarchyLight> lights = currScene.hierarchy.GetAllLights();

                foreach (HierarchyMesh mesh in currScene.selTriangles)
                {
                    node.hObjects.Add(mesh);
                    deleteMesh((int)mesh.triangles.ElementAt(0), currScene.hierarchy.objects);
                }
                foreach (int lightIndex in currScene.selLights)
                {
                    node.hObjects.Add(lights.ElementAt(lightIndex));
                    deleteLight(lightIndex, currScene.hierarchy.objects);
                }
                currScene.hierarchy.objects.Add(node);
                currScene.hierarchyChange = false;
                initializeTreeView();

                //sceneChange = true;
            }
            currScene.selectedHierObj = null;
        }

        private void DzielB_Click(object sender, RoutedEventArgs e) //jeżeli jakiś node w hierarchii jest zaznaczony, to wszystkie jego dzieci idą do pierwszego pokolenia
        {

            TreeViewItem dropOn = (TreeViewItem)treeView1.Items.GetItemAt(0);//((TreeViewItem)VisualTreeHelper.GetParent(frameworkElement));
            TreeViewItem dropSel = (TreeViewItem)treeView1.SelectedItem;
            if (!dropOn.Equals(dropSel) && dropSel is TreeViewItem)
            {
                List<TreeViewItem> parentDropOn = new List<TreeViewItem>();//nie uzywany
                List<TreeViewItem> parentDropSel = new List<TreeViewItem>();

                if (!dropSel.HasItems)//jeśli rozdzielamy mesha - to po prostu go przenosimy do dzieci roota
                {
                    //foreach (TreeViewItem tvi in treeView1.Items)
                    //{
                    //moveNode(tvi, dropSel, parentDropSel, dropOn, parentDropOn);
                    moveNode((TreeViewItem)treeView1.Items.GetItemAt(0), dropSel, parentDropSel, dropOn, parentDropOn);
                    //}

                    parentDropSel.ElementAt(0).Items.Remove(dropSel); //usuwamy połączenie z rodzicem przenoszonego itema

                    dropOn.Items.Add(dropSel);

                    parentDropSel.Clear();
                    parentDropOn.Clear();

                    List<TreeViewItem> emptyNode = new List<TreeViewItem>();
                    List<TreeViewItem> emptyNodeParent = new List<TreeViewItem>();

                    emptyNodesRemoved = true;
                    while (emptyNodesRemoved)
                    {
                        emptyNodesRemoved = false;
                        removeEmtpyNodes((TreeViewItem)treeView1.Items.GetItemAt(0), emptyNode, emptyNodeParent);
                        for (int i = 0; i < emptyNodeParent.Count; i++)
                        {
                            emptyNodeParent.ElementAt(i).Items.Remove(emptyNode.ElementAt(i));
                        }

                        emptyNode.Clear();
                        emptyNodeParent.Clear();
                    }
                }
                else //rozdzielamy node - przenosimy wszystkie jego dzieci do dzieci roota
                {
                    List<TreeViewItem> children = new List<TreeViewItem>();
                    foreach (TreeViewItem child in dropSel.Items)
                    {
                        children.Add(child);
                        //foreach (TreeViewItem tvi in treeView1.Items)
                        {
                            //moveNode(tvi, child, parentDropSel, dropOn, parentDropOn);
                            moveNode((TreeViewItem)treeView1.Items.GetItemAt(0), child, parentDropSel, dropOn, parentDropOn);
                        }
                    }

                    foreach (TreeViewItem child in children)
                    {
                        parentDropSel.ElementAt(0).Items.Remove(child); //usuwamy połączenie z rodzicem przenoszonego itema

                        dropOn.Items.Add(child);
                    }

                    parentDropSel.Clear();
                    parentDropOn.Clear();

                    List<TreeViewItem> emptyNode = new List<TreeViewItem>();
                    List<TreeViewItem> emptyNodeParent = new List<TreeViewItem>();

                    emptyNodesRemoved = true;
                    while (emptyNodesRemoved)
                    {
                        emptyNodesRemoved = false;
                        removeEmtpyNodes((TreeViewItem)treeView1.Items.GetItemAt(0), emptyNode, emptyNodeParent);
                        for (int i = 0; i < emptyNodeParent.Count; i++)
                        {
                            emptyNodeParent.ElementAt(i).Items.Remove(emptyNode.ElementAt(i));
                        }

                        emptyNode.Clear();
                        emptyNodeParent.Clear();
                    }
                }

                //sceneChange = true;

                refreshHierarchy();
            }
            currScene.selectedHierObj = null;
        }
        private void ZapiszGotowy_Click(object sender, RoutedEventArgs e)
        {
            contextSave(sender, e);
        }

        //koniec panelu hierarchii

        private void RemoveObject(object sender, RoutedEventArgs e)
        {
            _elementsGallery.RemoveAt(gotowe_ListView.SelectedIndex);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) //event przy zamykaniu aplikacji
        {
            if(currScene.modified == true)
            {
                MessageBoxResult ifSave = System.Windows.MessageBox.Show("Czy chesz zapisać bieżącą scenę ?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if(ifSave == MessageBoxResult.Yes)
                {
                    SaveFileAs(new object(), new RoutedEventArgs());
                }
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)  //menu- plik- zamknij
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void ClippingCB_Click(object sender, RoutedEventArgs e)
        {
            //ClippingCB.IsChecked = !ClippingCB.IsChecked;
            renderer.SetClipping((bool)ClippingCB.IsChecked);
            RenderViews();
        }

        private void podstawoweTab_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Bezier"))
            {
                //Modeler.DialogBoxes.BezierNameDialog dialog = new BezierNameDialog();
                object data = e.Data.GetData("Bezier");
                //Console.WriteLine(data.GetType());
                bool replace = false;
                if (data is BezierSurface)
                {
                    BezierSurface bezier = new BezierSurface((BezierSurface)data);
                    //bezier.Name = "Def";
                    if (String.Compare(bezier.Name, "default", true) == 0 || 
                        String.Compare(bezier.Name, "new surface", true) == 0)
                    {
                        Modeler.DialogBoxes.NameDialog newName = new NameDialog();
                        bool res = (bool)newName.Show("Podaj nową nazwę powierzchni", "");
                        if (!res)
                            return;
                        bezier.Name = newName.Result;

                    }
                    while (_shapesGallery.GetNames().Contains(bezier.Name, StringComparer.OrdinalIgnoreCase) && !replace)
                    {
                        bool result;
                        Modeler.DialogBoxes.NameDialog dialog = new NameDialog();
                        string name;
                        if (ShapeGallery.ReservedNames.Contains(bezier.Name, StringComparer.OrdinalIgnoreCase))
                        {
                            dialog.Owner = this;
                            result = (bool)dialog.Show("Nazwa powierzchni jest zarezerwowana dla podstawowego kształtu.", bezier.Name);
                            name = dialog.Result;
                            if (!result)
                                return;
                            bezier.Name = name;
                            dialog.Close();
                        }
                        else
                        {
                            dialog.Owner = this;
                            result = (bool) dialog.Show(bezierMessage, bezier.Name);
                            name = dialog.Result;
                            if (!result)
                                return;
                            if (bezier.Name == name)
                                replace = true;
                            bezier.Name = name;
                            dialog.Close();
                        }
                    }

                    bezier.Triangulate(1);
                    if (replace)
                    {
                        int idx = _shapesGallery.GetNameIndex(bezier.Name);
                        _shapesGallery.RemoveAt(idx);
                        _shapesGallery.SaveBezierToGallery(bezier, renderer.GetBezierImage(bezier));
                        _shapesGallery.Insert(idx, bezier);
                    }
                    else
                    {
                        _shapesGallery.SaveBezierToGallery(bezier, renderer.GetBezierImage(bezier));
                        _shapesGallery.Add(bezier);                        
                    }
                }
            }
        }

        private void ShapesDelete(object sender, RoutedEventArgs e)
        {
            _shapesGallery.DeleteBezierAt(ksztalty_ListView.SelectedIndex);
        }

        private void gotowe_ListView_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent("treeItem"))// && !((TreeViewItem)treeView1.Items.GetItemAt(0)).IsSelected)
            {
                currScene.selectedHierObj = null;
                currScene.selLights.Clear();
                currScene.selTriangles.Clear();
                
                for (int i = 0; i < ((TreeViewItem)treeView1.Items.GetItemAt(0)).Items.Count; i++)
                {
                    if (((TreeViewItem)(((TreeViewItem)treeView1.Items.GetItemAt(0)).Items.GetItemAt(i))).IsSelected)
                        selectingFromHierarchy((TreeViewItem)(((TreeViewItem)treeView1.Items.GetItemAt(0)).Items.GetItemAt(i)), (HierarchyObject)(currScene.hierarchy.objects.ElementAt(i)), true, false);
                    else
                        selectingFromHierarchy((TreeViewItem)(((TreeViewItem)treeView1.Items.GetItemAt(0)).Items.GetItemAt(i)), (HierarchyObject)(currScene.hierarchy.objects.ElementAt(i)), false, false);
                }

                contextSave(sender, e);

                currScene.selectedHierObj = null;
            }
            else if (e.Data.GetDataPresent("Prepared"))
            {
                contextSave(sender, e);
                e.Handled = true;
            }
        }

        private void RenderButton_Click(object sender, RoutedEventArgs e)
        {
            string processPath;
            string tmpFilePath;
            string currDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
#if FINAL
            processPath = currDirectory + "\\Raytracer.exe";
            tmpFilePath = currDirectory + "\\raytracing_tmp\\";
#elif DEBUG
            processPath = "..\\..\\..\\Debug\\Raytracer.exe";
            tmpFilePath = "..\\..\\raytracing_tmp\\";
#else
            processPath = "..\\..\\..\\Release\\Raytracer.exe";
            tmpFilePath = "..\\..\\raytracing_tmp\\";
#endif
            string tmpFileName = "raytracing_tmp" + ".scn";
            currScene.SaveSceneFile(tmpFilePath+tmpFileName);
            System.Diagnostics.Process.Start(processPath, tmpFilePath + tmpFileName);
        }

        private void Views_KeyPress(object sender, KeyPressEventArgs e)
        {
            ViewportInfo info = GetViewCoords(tabWidok.SelectedIndex);
            switch (e.KeyChar)
            {
                    // Plus
                case (char)43:
                    renderer.ScaleOrtho(0, 5, GetViewCoords(tabWidok.SelectedIndex), info.sizeX[0]/2, info.sizeY[0]/2);
                    RenderViews();
                    break;
                    // Minu
                case (char)45:
                    renderer.ScaleOrtho(0, -5, GetViewCoords(tabWidok.SelectedIndex), info.sizeX[0] / 2, info.sizeY[0] / 2);
                    RenderViews();
                    break;
                case 'g':
                    ChangeContextSelection(ContextItem.Grab);
                    break;
                case 'r':
                    ChangeContextSelection(ContextItem.Rotate);
                    break;
                case 's':
                    ChangeContextSelection(ContextItem.Scale);
                    break;
                case 'd':
                    ChangeContextSelection(ContextItem.ScaleDimension);
                    break;
            }
        }

        private void ChangeContextSelection(ContextItem item)
        {

            for (int i = 0; i < contextMenu.MenuItems.Count; i++)
            {
                contextMenu.MenuItems[i].Checked = false;
            }
            for (int i = 0; i < workMode.Items.Count; i++)
            {
                ((System.Windows.Controls.MenuItem)workMode.Items[i]).IsChecked = false;
            }
            ((System.Windows.Controls.MenuItem)workMode.Items[(int)item]).IsChecked = true;
            contextMenu.MenuItems[(int)item].Checked = true;
            if (contextMenu.MenuItems[0].Checked == true)
            {
                modeTBG1.Text = "Przenoszenie";
                modeTBG2.Text = "Przenoszenie";
                modeTBG3.Text = "Przenoszenie";
                modeTBG4.Text = "Przenoszenie";
            }
            else if (contextMenu.MenuItems[1].Checked == true)
            {
                modeTBG1.Text = "Obracanie";
                modeTBG2.Text = "Obracanie";
                modeTBG3.Text = "Obracanie";
                modeTBG4.Text = "Obracanie";
            }
            else if (contextMenu.MenuItems[2].Checked == true)
            {
                modeTBG1.Text = "Skalowanie";
                modeTBG2.Text = "Skalowanie";
                modeTBG3.Text = "Skalowanie";
                modeTBG4.Text = "Skalowanie";
            }
            else if (contextMenu.MenuItems[3].Checked == true)
            {
                modeTBG1.Text = "Skalowanie wzdłuż osi";
                modeTBG2.Text = "Skalowanie wzdłuż osi";
                modeTBG3.Text = "Skalowanie wzdłuż osi";
                modeTBG4.Text = "Skalowanie wzdłuż osi";
            } 
        }

        private void ViewsBezier_KeyPress(object sender, KeyPressEventArgs e)
        {
            ViewportInfo info = GetViewCoords(tabWidok.SelectedIndex);
            switch (e.KeyChar)
            {
                // Plus
                case (char)43:
                    renderer.ScaleBezierOrtho(0, 5, GetViewCoords(tabWidok.SelectedIndex), info.sizeX[0] / 2, info.sizeY[0] / 2);
                    RenderBezier();
                    break;
                // Minu
                case (char)45:
                    renderer.ScaleBezierOrtho(0, -5, GetViewCoords(tabWidok.SelectedIndex), info.sizeX[0] / 2, info.sizeY[0] / 2);
                    RenderBezier();
                    break;
            }
        }

        private void Grab_Click(object sender, RoutedEventArgs e)
        {
            ChangeContextSelection(ContextItem.Grab);
        }

        private void Rotate_Click(object sender, RoutedEventArgs e)
        {
            ChangeContextSelection(ContextItem.Rotate);
        }

        private void Scale_Click(object sender, RoutedEventArgs e)
        {
            ChangeContextSelection(ContextItem.Scale);
        }

        private void ScaleDimension_Click(object sender, RoutedEventArgs e)
        {
            ChangeContextSelection(ContextItem.ScaleDimension);
        }

        //private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        DragDrop.DoDragDrop(this, sender, DragDropEffects.Move);
        //        Console.WriteLine(e.Source);
        //    }
        //}
    }
}