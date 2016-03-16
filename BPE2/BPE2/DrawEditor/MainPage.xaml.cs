using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using static System.Byte;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DrawEditor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private InkManager _myInkManager = new InkManager();
        private string _drawingTool;
        private double _x1, _x2, _y1, _y2, _strokeThickness = 1;
        private Line _newLine;
        private Ellipse _newEllipse;
        private Point _startPoint, _previousContactPoint, _currentContactPoint;
        private Polyline _pencil;
        private Rectangle _newRectangle;
        private Color _borderColor, _fillColor;
        private Windows.UI.Xaml.Shapes.Path _currentPath;
        private uint _penId, _touchId;
        public MainPage()
        {
            this.InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Init(ref Display, ref SizeComboBox, ref ColourStroke,ref ColourFill);
        }

        private async void New_Click(object sender, RoutedEventArgs e)
        {
            //if (await Confirm("Create New Drawing?", "Draw Editor", "Yes", "No"))
            //{
            //    Display.InkPresenter.StrokeContainer.Clear();
            //}
        }

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    FileOpenPicker picker = new FileOpenPicker();
            //    picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            //    picker.FileTypeFilter.Add(".drw");
            //    StorageFile file = await picker.PickSingleFileAsync();
            //    using (IInputStream stream = await file.OpenSequentialReadAsync())
            //    {
            //        await Display.InkPresenter.StrokeContainer.LoadAsync(stream);
            //    }
            //}
            //catch
            //{

            //}
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                
            }
            catch
            {
                
            }
            //try
            //{
            //    var picker = new FileSavePicker {SuggestedStartLocation = PickerLocationId.DocumentsLibrary};
            //    picker.FileTypeChoices.Add("Drawing", new List<string>() { ".drw" });
            //    picker.DefaultFileExtension = ".drw";
            //    picker.SuggestedFileName = "Drawing";
            //    var file = await picker.PickSaveFileAsync();
            //    if (file == null) return;
            //    CachedFileManager.DeferUpdates(file);
            //    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            //    {
            //        await Display.InkPresenter.StrokeContainer.SaveAsync(stream);
            //    }
            //    var status = await CachedFileManager.CompleteUpdatesAsync(file);

            //}
            //catch
            //{

            //}
        }
        public static async Task<IRandomAccessStream> RenderToRandomAccessStream(UIElement element)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(element);

            var pixelBuffer = await rtb.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();

            // Useful for rendering in the correct DPI
            var displayInformation = DisplayInformation.GetForCurrentView();

            var stream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                 BitmapAlphaMode.Premultiplied,
                                 (uint)rtb.PixelWidth,
                                 (uint)rtb.PixelHeight,
                                 displayInformation.RawDpiX,
                                 displayInformation.RawDpiY,
                                 pixels);

            await encoder.FlushAsync();
            stream.Seek(0);

            return stream;
        }

        private void Colour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Display == null) return;
            var selectedColour = ((ComboBoxItem)((ComboBox)sender).SelectedItem)?.Tag.ToString();
            _borderColor = StringToColour(selectedColour);
            //var attributes = Display.InkPresenter.CopyDefaultDrawingAttributes();
            //attributes.Color = stringToColour(selectedColour);
            //Display.InkPresenter.UpdateDefaultDrawingAttributes(attributes);
        }

        private void Size_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Display != null)
            {
                string selectedSize = ((ComboBoxItem)((ComboBox)sender).SelectedItem)?.Tag.ToString();
                _strokeThickness = double.Parse(selectedSize);
                //InkDrawingAttributes attributes = Display.InkPresenter.CopyDefaultDrawingAttributes();
                //attributes.Size = new Size(int.Parse(selectedSize), int.Parse(selectedSize));
                //Display.InkPresenter.UpdateDefaultDrawingAttributes(attributes);
            }
        }

        private void ColourFill_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Display == null) return;
            var comboBoxItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            if (comboBoxItem == null) return;
            var selectedColour = comboBoxItem.Tag.ToString();
            _fillColor = StringToColour(selectedColour);
        }

        public string ColourToString(Color value)
        {
            return $"{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}";
        }

        private static Color StringToColour(string value)
        {
            return Color.FromArgb(a: Parse(value.Substring(0, 2), 
                NumberStyles.HexNumber),
                r: Parse(value.Substring(2, 2), NumberStyles.HexNumber),
                g: Parse(value.Substring(4, 2), NumberStyles.HexNumber),
                b: Parse(value.Substring(6, 2), NumberStyles.HexNumber));
        }

        public async Task<bool> Confirm(string content, string title, string ok, string cancel)
        {
            var result = false;
            var dialog = new MessageDialog(content, title);
            dialog.Commands.Add(new UICommand(ok, cmd => result = true));
            dialog.Commands.Add(new UICommand(cancel, cmd => result = false));
            await dialog.ShowAsync();
            return result;
        }

        public void Init(ref Canvas display, ref ComboBox size, ref ComboBox colour, ref ComboBox fillColour)
        {
            //var selectedSize = ((ComboBoxItem)size.SelectedItem)?.Tag.ToString();
            //var selectedColour = ((ComboBoxItem)colour.SelectedItem)?.Tag.ToString();
            _borderColor = StringToColour(((ComboBoxItem)colour.SelectedItem)?.Tag.ToString());
            _fillColor = StringToColour(((ComboBoxItem)fillColour.SelectedItem)?.Tag.ToString());
            _strokeThickness = double.Parse(((ComboBoxItem)size.SelectedItem)?.Tag.ToString());
            //var attributes = new InkDrawingAttributes
            //{
            //    Color = stringToColour(selectedColour),
            //    Size = new Size(int.Parse(selectedSize), int.Parse(selectedSize)),
            //    IgnorePressure = false,
            //    FitToCurve = true
            //};
            //display.InkPresenter.UpdateDefaultDrawingAttributes(attributes);
            //display.InkPresenter.InputDeviceTypes =
            //   CoreInputDeviceTypes.Mouse |
            //   CoreInputDeviceTypes.Pen |
            //   CoreInputDeviceTypes.Touch;
        }

        public void Colour(ref InkCanvas display, ref ComboBox colour)
        {
            if (display == null) return;
            var selectedColour = ((ComboBoxItem)colour.SelectedItem)?.Tag.ToString();
            var attributes = display.InkPresenter.CopyDefaultDrawingAttributes();
            attributes.Color = StringToColour(selectedColour);
            display.InkPresenter.UpdateDefaultDrawingAttributes(attributes);
        }

        

        private void canvas_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }

        public void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = _drawingTool != "Eraser" ? new CoreCursor(CoreCursorType.Cross, 1) : new CoreCursor(CoreCursorType.UniversalNo, 1);

            switch (_drawingTool)
            {
                case "Line":
                {
                    _newLine = new Line
                    {
                        X1 = e.GetCurrentPoint(Display).Position.X,
                        Y1 = e.GetCurrentPoint(Display).Position.Y
                    };
                    _newLine.X2 = _newLine.X1 + 1;
                    _newLine.Y2 = _newLine.Y1 + 1;
                    _newLine.StrokeThickness = _strokeThickness;
                    _newLine.Stroke = new SolidColorBrush(_borderColor);
                    Display.Children.Add(_newLine);
                }
                    break;
                case "Pencil":
                {

                        //var myDrawingAttributes = new InkDrawingAttributes
                        //{
                        //    Size = new Size(_strokeThickness, _strokeThickness),
                        //    Color = _borderColor,
                        //    FitToCurve = true
                        //};
                        //_myInkManager.SetDefaultDrawingAttributes(myDrawingAttributes);

                        //_previousContactPoint = e.GetCurrentPoint(Display).Position;
                        ////PointerDeviceType pointerDevType = e.Pointer.PointerDeviceType;  to identify the pointer device
                        //if (e.GetCurrentPoint(Display).Properties.IsLeftButtonPressed)
                        //{
                        //    // Pass the pointer information to the InkManager.
                        //    _myInkManager.ProcessPointerDown(e.GetCurrentPoint(Display));
                        //    _penId = e.GetCurrentPoint(Display).PointerId;
                        //    e.Handled = true;
                        //}

                        /*Path*/
                        //_currentPath = new Windows.UI.Xaml.Shapes.Path
                        //{
                        //    Data = new PathGeometry
                        //    {
                        //        Figures = { new PathFigure
                        //            {
                        //                StartPoint = e.GetCurrentPoint((UIElement)sender).Position,
                        //                Segments = { new PolyBezierSegment() }
                        //            }}
                        //    },
                        //    Stroke = new SolidColorBrush(_borderColor),
                        //    StrokeThickness = _strokeThickness
                        //};
                        //Display.Children.Add(_currentPath);
                    _pencil = new Polyline
                    {
                        Stroke = new SolidColorBrush(_borderColor),
                        StrokeThickness = _strokeThickness,
                        StrokeMiterLimit = 0,
                        FillRule = FillRule.Nonzero
                    };
                    _pencil.Points?.Add(e.GetCurrentPoint((UIElement) sender).Position);
                    Display.Children.Add(_pencil);



                }
                    break;
                case "Rectangle":
                {
                    _newRectangle = new Rectangle();
                    _x1 = e.GetCurrentPoint(Display).Position.X;
                    _y1 = e.GetCurrentPoint(Display).Position.Y;
                    _x2 = _x1;
                    _y2 = _y1;
                    _newRectangle.Width = _x2 - _x1;
                    _newRectangle.Height = _y2 - _y1;
                    _newRectangle.StrokeThickness = _strokeThickness;
                    _newRectangle.Stroke = new SolidColorBrush(_borderColor);
                    _newRectangle.Fill = new SolidColorBrush(_fillColor);
                    Display.Children.Add(_newRectangle);
                }
                    break;
                case "Ellipse":
                {
                    _x1 = e.GetCurrentPoint(Display).Position.X;
                    _y1 = e.GetCurrentPoint(Display).Position.Y;
                    _x2 = _x1;
                    _y2 = _y1;
                    _newEllipse = new Ellipse
                    {
                        Width = _x2 - _x1,
                        Height = _y2 - _y1,
                        StrokeThickness = _strokeThickness,
                        Stroke = new SolidColorBrush(_borderColor),
                        Fill = new SolidColorBrush(_fillColor)
                    };
                    Display.Children.Add(_newEllipse);
                }
                    break;
                case "Eraser":
                {
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.UniversalNo, 1);
                    _startPoint = e.GetCurrentPoint(Display).Position;
                    _pencil = new Polyline
                    {
                        Stroke = Display.Background,
                        StrokeThickness = this._strokeThickness,
                        StrokeMiterLimit = 0,
                        FillRule = FillRule.Nonzero
                    };
                    Display.Children.Add(_pencil);
                }
                    break;
            }
        }

        private void canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = _drawingTool != "Eraser" ? 
                new CoreCursor(CoreCursorType.Cross, 1) :
                new CoreCursor(CoreCursorType.UniversalNo, 1);

            switch (_drawingTool)
            {
                case "Pencil":
                    {
                        /* Old Code
                        if (e.GetCurrentPoint(canvas).Properties.IsLeftButtonPressed == true)
                        {
                            if (StartPoint != e.GetCurrentPoint(canvas).Position)
                            {
                                Pencil.Points.Add(e.GetCurrentPoint(canvas).Position);
                            }
                        }
                        */

                        //if (e.Pointer.PointerId == _penId || e.Pointer.PointerId == _touchId)
                        //{
                        //    // Distance() is an application-defined function that tests
                        //    // whether the pointer has moved far enough to justify 
                        //    // drawing a new line.
                        //    _currentContactPoint = e.GetCurrentPoint(Display).Position;
                        //    _x1 = _previousContactPoint.X;
                        //    _y1 = _previousContactPoint.Y;
                        //    _x2 = _currentContactPoint.X;
                        //    _y2 = _currentContactPoint.Y;

                        //    if (Distance(_x1, _y1, _x2, _y2) > 2.0)
                        //    {
                        //        var line = new Line()
                        //        {
                        //            X1 = _x1,
                        //            Y1 = _y1,
                        //            X2 = _x2,
                        //            Y2 = _y2,
                        //            StrokeThickness = _strokeThickness,
                        //            Stroke = new SolidColorBrush(_borderColor)
                        //        };

                        //        _previousContactPoint = _currentContactPoint;
                        //        Display.Children.Add(line);
                        //        _myInkManager.ProcessPointerUpdate(e.GetCurrentPoint(Display));
                        //    }
                        //}

                        /*Path*/
                        //if (_currentPath == null) return;

                        //var pls = (PolyBezierSegment)((PathGeometry)_currentPath.Data).Figures.Last().Segments.Last();
                        //pls.Points.Add(e.GetCurrentPoint((UIElement)sender).Position);
                        if (e.GetCurrentPoint(Display).Properties.IsLeftButtonPressed)
                        {
                            _pencil.Points?.Add(e.GetCurrentPoint((UIElement)sender).Position);
                        }
                    }
                    break;

                case "Line":
                    {
                        if (e.GetCurrentPoint(Display).Properties.IsLeftButtonPressed)
                        {
                            _newLine.X2 = e.GetCurrentPoint(Display).Position.X;
                            _newLine.Y2 = e.GetCurrentPoint(Display).Position.Y;
                        }
                    }
                    break;

                case "Rectangle":
                    {
                        if (e.GetCurrentPoint(Display).Properties.IsLeftButtonPressed)
                        {
                            _x2 = e.GetCurrentPoint(Display).Position.X;
                            _y2 = e.GetCurrentPoint(Display).Position.Y;
                            if ((_x2 - _x1) > 0 && (_y2 - _y1) > 0)
                                _newRectangle.Margin = new Thickness(_x1, _y1, _x2, _y2);
                            else if ((_x2 - _x1) < 0 && (_y2 - _y1) > 0)
                                _newRectangle.Margin = new Thickness(_x2, _y1, _x1, _y2);
                            else if ((_y2 - _y1) < 0 && (_x2 - _x1) > 0)
                                _newRectangle.Margin = new Thickness(_x1, _y2, _x2, _y1);
                            else if ((_x2 - _x1) < 0 && (_y2 - _y1) < 0)
                                _newRectangle.Margin = new Thickness(_x2, _y2, _x1, _y1);
                            _newRectangle.Width = Math.Abs(_x2 - _x1);
                            _newRectangle.Height = Math.Abs(_y2 - _y1);
                        }
                    }
                    break;

                case "Ellipse":
                    {
                        if (e.GetCurrentPoint(Display).Properties.IsLeftButtonPressed
                            &&_newEllipse!=null)
                        {
                            _x2 = e.GetCurrentPoint(Display).Position.X;
                            _y2 = e.GetCurrentPoint(Display).Position.Y;
                            if ((_x2 - _x1) > 0 && (_y2 - _y1) > 0)
                                _newEllipse.Margin = new Thickness(_x1, _y1, _x2, _y2);
                            else if ((_x2 - _x1) < 0&& (_y2 - _y1) > 0)
                                _newEllipse.Margin = new Thickness(_x2, _y1, _x1, _y2);
                            else if ((_y2 - _y1) < 0 && (_x2 - _x1) > 0)
                                _newEllipse.Margin = new Thickness(_x1, _y2, _x2, _y1);
                            else if ((_x2 - _x1) < 0 && (_y2 - _y1) < 0)
                                _newEllipse.Margin = new Thickness(_x2, _y2, _x1, _y1);
                            _newEllipse.Width = Math.Abs(_x2 - _x1);
                            _newEllipse.Height = Math.Abs(_y2 - _y1);
                        }
                    }
                    break;

                case "Eraser":
                    {
                        if (e.GetCurrentPoint(Display).Properties.IsLeftButtonPressed)
                        {
                            if (_startPoint != e.GetCurrentPoint(Display).Position
                                &&e.GetCurrentPoint(Display).Position.X>_strokeThickness
                                && e.GetCurrentPoint(Display).Position.Y > _strokeThickness)
                            {
                                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.UniversalNo, 1);
                                _pencil.Points?.Add(e.GetCurrentPoint(Display).Position);
                            }
                        }
                    }
                    break;
            }
        }

        private static double Distance(double x1, double y1, double x2, double y2)
        {
            var d = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            return d;
        }

        public void canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId == _penId || e.Pointer.PointerId == _touchId)
                _myInkManager.ProcessPointerUp(e.GetCurrentPoint(Display));

            _currentPath = null;
            _touchId = 0;
            _penId = 0;
            e.Handled = true;
            _pencil = null;
            _newLine = null;
            _newRectangle = null;
            _newEllipse = null;
        }
        private void Tools_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var obj = sender as GridView;
            if (obj != null) _drawingTool = ((TextBlock) obj.SelectedItem)?.Tag.ToString();
        }
    }
}

