using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

//sources:
//https://wpf.2000things.com/2011/07/11/339-wrapping-a-buttons-text-content-to-multiple-lines/
//https://social.msdn.microsoft.com/Forums/vstudio/en-US/828a21f9-db01-4f9a-8461-b9a585820b50/how-to-draw-an-ellipse-inside-a-canvas-at-specific-coordinates?forum=wpf
//https://stackoverflow.com/questions/3237597/add-ellipse-position-within-canvas
//https://stackoverflow.com/questions/11013316/get-the-height-width-of-window-wpf
//https://stackoverflow.com/questions/6084398/pick-a-random-brush
//https://stackoverflow.com/questions/23117878/change-cursor-to-hand-when-i-hover-over-a-button
//https://stackoverflow.com/questions/6059894/how-draw-rectangle-in-wpf
//https://stackoverflow.com/questions/5971300/programmatically-changing-button-icon-in-wpf/5973592#5973592
//https://stackoverflow.com/questions/8881865/saving-a-wpf-canvas-as-an-image
//https://stackoverflow.com/questions/10315188/open-file-dialog-and-select-a-file-using-wpf-controls-and-c-sharp
//https://stackoverflow.com/questions/4325453/can-we-concat-two-properties-in-data-binding
//https://stackoverflow.com/questions/36744395/cannot-be-negative-and-avoid-letter-inputs-on-textbox
//https://stackoverflow.com/questions/29263904/wpf-combobox-as-system-windows-media-colors
//https://stackoverflow.com/questions/272633/add-spaces-before-capital-letters
//https://stackoverflow.com/questions/6763032/how-to-pick-a-background-color-depending-on-font-color-to-have-proper-contrast
//
//
//
//
//
//

namespace WPFLab2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point startPoint;
        private Rectangle rect;
        private Ellipse ellipse;
        private bool skipTextValidation = false;
        int highestZIndex;
        bool shapeMouseDownTrigerred=false;
        Stack<string> selectedShapes = new Stack<string>();
        List<int> selectedShapesList = new List<int>();
        public IEnumerable<KeyValuePair<string, Color>> NamedColors { get; private set; }

        bool captured = false;
        List<double> x_shape = new List<double>();
        List<double> x_canvas = new List<double>();
        List<double> y_shape = new List<double>();
        List<double> y_canvas = new List<double>();


        List<UIElement> source = new List<UIElement>();
        int amount=0;



      //  private bool _isMoving;
       // private Point? _buttonPosition;
      //  private double deltaX;
       // private double deltaY;
       // private TranslateTransform _currentTT;

        public MainWindow()
        {
            InitializeComponent();
            this.NamedColors = this.GetColors();
            Func<KeyValuePair<string,Color>,bool> predicate = ad=> { return (ad.Key != "Transparent"); };
            NamedColors= NamedColors.TakeWhile(predicate);



            highestZIndex = Canvas.Children.Count;
            this.DataContext = this;
            IsAnyShapeSelected();
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            for (int i = 0; i < 4; i++)
            {
                if (rand.Next() % 2 == 0)
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Height = rand.Next() % 200 + 10;
                    ellipse.Width = rand.Next() % 200 + 10;
                    ellipse.Fill = PickBrush();
                    // ellipse.
                    //ellipse.ClipToBounds = true;
                    ellipse.Tag = 0;
                    ellipse.MouseEnter += (s, e) => Mouse.OverrideCursor = Cursors.Hand;
                    ellipse.MouseLeave += (s, e) => Mouse.OverrideCursor = Cursors.Arrow;
                    ellipse.MouseRightButtonDown += Shape_MouseRightButtonDown;
                    ellipse.MouseLeftButtonDown += Shape_MouseLeftDown;//rand.Next() % ((sender as Canvas).ActualWidth - ellipse.Width), rand.Next() % ((sender as Canvas).ActualHeight - ellipse.Height)
                    ellipse.MouseMove += shape_MouseMove;
                    ellipse.MouseLeftButtonUp += shape_MouseLeftButtonUp;

                    double left = rand.Next() % ((sender as Canvas).ActualWidth - ellipse.Width);
                    double top = rand.Next() % ((sender as Canvas).ActualHeight - ellipse.Height);
                  
                    
                     Canvas.SetLeft(ellipse, left);
                    Canvas.SetTop(ellipse,top);
                    //ellipse.Margin = new Thickness(left, top, 0, 0);
                    (sender as Canvas).Children.Add(ellipse);
                }
                else
                {
                    Rectangle rectangle = new Rectangle();
                    rectangle.Height = rand.Next() % 200 + 10;
                    rectangle.Width = rand.Next() % 200 + 10;
                    rectangle.Tag = 0;
                    // rectangle.ClipToBounds = true;
                    rectangle.Fill = PickBrush();
                    rectangle.MouseEnter += (s, e) => Mouse.OverrideCursor = Cursors.Hand;
                    rectangle.MouseLeave += (s, e) => Mouse.OverrideCursor = Cursors.Arrow;
                    rectangle.MouseRightButtonDown += Shape_MouseRightButtonDown;
                    rectangle.MouseLeftButtonDown += Shape_MouseLeftDown;
                    rectangle.MouseMove += shape_MouseMove;
                    rectangle.MouseLeftButtonUp += shape_MouseLeftButtonUp;
                    double left = rand.Next() % ((sender as Canvas).ActualWidth - rectangle.Width);
                    double top = rand.Next() % ((sender as Canvas).ActualHeight - rectangle.Height);
                    
                    Canvas.SetLeft(rectangle, left);
                   Canvas.SetTop(rectangle,top);
                    //rectangle.Margin = new Thickness(left,top , 0, 0);
                    (sender as Canvas).Children.Add(rectangle);
                }
            }
        }

        private void Shape_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            if ((int)(sender as Shape).Tag == 0)
            {
                   if (selectedShapesList.Contains((sender as Shape).GetHashCode()))
                      selectedShapesList.Remove((sender as Shape).GetHashCode());
                    selectedShapesList.Add((sender as Shape).GetHashCode());
                highestZIndex++;
                Canvas.SetZIndex((sender as Shape), highestZIndex);
                for(int i = 0; i < Canvas.Children.Count; i++)
                {
                    if ((int)(Canvas.Children[i] as Shape).Tag == 2)
                        (Canvas.Children[i] as Shape).Tag = 1;

                }
                (sender as Shape).Tag = 2;
                DropShadowEffect dropShadowEffect = new DropShadowEffect();
                dropShadowEffect.BlurRadius = 50;
                dropShadowEffect.Direction = 270;
                dropShadowEffect.ShadowDepth = 0;
                dropShadowEffect.Opacity = 1;
                dropShadowEffect.Color = Colors.White;
                (sender as Shape).Effect = dropShadowEffect;
                widthTextBox.Text = (sender as Shape).Width.ToString();
                heightTextBox.Text = (sender as Shape).Height.ToString();
                if (((sender as Shape).RenderTransform as RotateTransform) != null)
                    Slider.Value = ((sender as Shape).RenderTransform as RotateTransform).Angle;
                else
                    Slider.Value = 0;

                shapeMouseDownTrigerred = true;
                int index = 0;
                //MessageBox.Show(AddSpacesToSentence(GetColorName((SolidColorBrush)(sender as Shape).Fill), true));
                IEnumerator<KeyValuePair<string, Color>> enumerator = NamedColors.GetEnumerator();
                for (int i = 0; i < NamedColors.Count<KeyValuePair<string, Color>>(); i++)
                {
                    if (enumerator.Current.Key == AddSpacesToSentence(GetColorName((SolidColorBrush)(sender as Shape).Fill), true))
                        index = i;
                    enumerator.MoveNext();
                }
                Combobox.SelectedIndex = index - 1;
            }
            else
            {
                for (int i = 0; i < Canvas.Children.Count; i++)
                {
                    if ((int)(Canvas.Children[i] as Shape).Tag == 2)
                        (Canvas.Children[i] as Shape).Tag = 1;

                }
                if (selectedShapesList.Count > 0)
                {
                   // selectedShapesList.RemoveAt(selectedShapesList.Count - 1);
                    selectedShapesList.Remove((sender as Shape).GetHashCode());


                }
                (sender as Shape).Tag = 0;
                (sender as Shape).Effect = null;
                if (selectedShapesList.Count > 0)
                {
                    
                    for (int i = 0; i < Canvas.Children.Count; i++)
                    {
                        if (Canvas.Children[i].GetHashCode() == selectedShapesList[selectedShapesList.Count - 1])
                        {
                            if (((Canvas.Children[i] as Shape).RenderTransform as RotateTransform) != null)
                                Slider.Value = ((Canvas.Children[i] as Shape).RenderTransform as RotateTransform).Angle;
                            else
                                Slider.Value = 0;
                            //selectedShapesList.Add((Canvas.Children[i] as Shape).GetHashCode());
                            (Canvas.Children[i] as Shape).Tag = 2;
                            widthTextBox.Text = (Canvas.Children[i] as Shape).Width.ToString();
                            heightTextBox.Text = (Canvas.Children[i] as Shape).Height.ToString();
                            shapeMouseDownTrigerred = true;
                            DropShadowEffect dropShadowEffect = new DropShadowEffect();
                            dropShadowEffect.BlurRadius = 50;
                            dropShadowEffect.Direction = 270;
                            dropShadowEffect.ShadowDepth = 0;
                            dropShadowEffect.Opacity = 1;
                            dropShadowEffect.Color = Colors.White;
                            (Canvas.Children[i] as Shape).Effect = dropShadowEffect;
                            int index = 0;
                            //MessageBox.Show(AddSpacesToSentence(GetColorName((SolidColorBrush)(sender as Shape).Fill), true));
                            IEnumerator<KeyValuePair<string, Color>> enumerator = NamedColors.GetEnumerator();
                            for (int j = 0; j < NamedColors.Count<KeyValuePair<string, Color>>(); j++)
                            {
                                if (enumerator.Current.Key == AddSpacesToSentence(GetColorName((SolidColorBrush)(Canvas.Children[i] as Shape).Fill), true))
                                    index = j;
                                enumerator.MoveNext();
                            }
                            Combobox.SelectedIndex = index - 1;

                        }

                    }



                }
               
            }
            IsAnyShapeSelected();

          
            //if ((int)(sender as Shape).Tag == 2 && selectedShapesList.Count == 1)
            //{
            //    selectedShapesList.RemoveAt(selectedShapesList.Count - 1);
            //    (sender as Shape).Tag = 0;
            //    (sender as Shape).Effect = null;



            //}
            //else if ((int)(sender as Shape).Tag == 2 && selectedShapesList.Count > 1)
            //{
            //    selectedShapesList.RemoveAt(selectedShapesList.Count - 1);
            //    (sender as Shape).Tag = 0;
            //    (sender as Shape).Effect = null;
            //    for (int i = 0; i < Canvas.Children.Count; i++)
            //    {
            //        if (Canvas.Children[i].GetHashCode() == selectedShapesList[selectedShapesList.Count - 1])
            //        {
            //            //selectedShapesList.Add((Canvas.Children[i] as Shape).GetHashCode());
            //            (Canvas.Children[i] as Shape).Tag = 2;
            //            widthTextBox.Text = (Canvas.Children[i] as Shape).Width.ToString();
            //            heightTextBox.Text = (Canvas.Children[i] as Shape).Height.ToString();
            //            shapeMouseDownTrigerred = true;
            //            DropShadowEffect dropShadowEffect = new DropShadowEffect();
            //            dropShadowEffect.BlurRadius = 50;
            //            dropShadowEffect.Direction = 270;
            //            dropShadowEffect.ShadowDepth = 0;
            //            dropShadowEffect.Opacity = 1;
            //            dropShadowEffect.Color = Colors.White;
            //            (Canvas.Children[i] as Shape).Effect = dropShadowEffect;

            //        }

            //    }


            //}

            //else
            //{

            //    for (int i = 0; i < Canvas.Children.Count; i++)
            //    {
            //        if ((int)(Canvas.Children[i] as Shape).Tag == 2)
            //        {
            //            (Canvas.Children[i] as Shape).Tag = 0;
            //            (Canvas.Children[i] as Shape).Effect = null;
            //        }
            //    }
            //    //    if(selectedShapes.Contains((sender as Shape).Uid))
            //    //  selectedShapes.
            //    //  selectedShapes.Push((sender as Shape).Uid);
            //    if (selectedShapesList.Contains((sender as Shape).GetHashCode()))
            //        selectedShapesList.Remove((sender as Shape).GetHashCode());
            //    selectedShapesList.Add((sender as Shape).GetHashCode());
            //    (sender as Shape).Tag = 2;
            //    widthTextBox.Text = (sender as Shape).Width.ToString();
            //    heightTextBox.Text = (sender as Shape).Height.ToString();
            //    shapeMouseDownTrigerred = true;
            //    DropShadowEffect dropShadowEffect = new DropShadowEffect();
            //    dropShadowEffect.BlurRadius = 50;
            //    dropShadowEffect.Direction = 270;
            //    dropShadowEffect.ShadowDepth = 0;
            //    dropShadowEffect.Opacity = 1;
            //    dropShadowEffect.Color = Colors.White;
            //    (sender as Shape).Effect = dropShadowEffect;
            //    //Combobox.SelectedIndex = Combobox.Items.IndexOf((sender as Shape).Fill.ToString());
            //    //for (int i = 0; i < Combobox.Items.Count; i++)
            //    //{
            //    //    if (Combobox.Items.GetItemAt(i).ToString() == (sender as Shape).Fill.ToString())
            //    //    {
            //    //        Combobox.Items.MoveCurrentTo(Combobox.Items.GetItemAt(i));
            //    //        break;
            //    //    }

            //    //}
            //    //Combobox.Items. = (sender as Shape).Fill.ToString();
            //    // AngleTextBlock.Text = (sender as Shape).GeometryTransform.
            //}

        }
        private void Ellipse_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((int)(sender as Ellipse).Tag == 0 || (int)(sender as Ellipse).Tag == 2)
            {
                highestZIndex++;
                Canvas.SetZIndex((sender as Ellipse), highestZIndex);
                (sender as Ellipse).Tag = 1;
                DropShadowEffect dropShadowEffect = new DropShadowEffect();
                dropShadowEffect.BlurRadius = 50;
                dropShadowEffect.Direction = 270;
                dropShadowEffect.ShadowDepth = 0;
                dropShadowEffect.Opacity = 1;
                dropShadowEffect.Color = Colors.White;
                (sender as Ellipse).Effect = dropShadowEffect;
                shapeMouseDownTrigerred = true;
            }
            else
            {
                (sender as Ellipse).Tag = 0;
                (sender as Ellipse).Effect = null;
            }
            IsAnyShapeSelected();
        }

        private void Rectangle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((int)(sender as Rectangle).Tag == 0|| (int)(sender as Rectangle).Tag==2)
            {
                highestZIndex++;
                Canvas.SetZIndex((sender as Rectangle), highestZIndex);
                DropShadowEffect dropShadowEffect = new DropShadowEffect();
                (sender as Rectangle).Tag = 1;
                dropShadowEffect.BlurRadius = 50;
                dropShadowEffect.Direction = 270;
                dropShadowEffect.ShadowDepth = 0;
                dropShadowEffect.Opacity = 0.5;
                dropShadowEffect.Color = Colors.White;
                (sender as Rectangle).Effect = dropShadowEffect;
                shapeMouseDownTrigerred = true;
            }
            else
            {
                (sender as Rectangle).Tag = 0;
                (sender as Rectangle).Effect = null;
            }
            IsAnyShapeSelected();
        }

        private Brush PickBrush()
        {
            Brush result = Brushes.Transparent;

            Random rnd = new Random();

            Type brushesType = typeof(Brushes);

            PropertyInfo[] properties = brushesType.GetProperties();

            int random = rnd.Next(properties.Length);
            result = (Brush)properties[random].GetValue(null, null);

            return result;
        }

        private void RandomColorsButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Canvas.Children.Count; i++)
            {
                if((int)(Canvas.Children[i] as Shape).Tag==2|| (int)(Canvas.Children[i] as Shape).Tag == 1)
                (Canvas.Children[i] as Shape).Fill = PickBrush();
            }
            
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int[] todelete = new int[Canvas.Children.Count];
            Array.Clear(todelete, 0, todelete.Length);
            for (int i = 0; i < Canvas.Children.Count; i++)
            {
                if ((int)(Canvas.Children[i] as Shape).Tag == 1|| (int)(Canvas.Children[i] as Shape).Tag==2)
                {
                    Canvas.Children.Remove(Canvas.Children[i]);
                    i = -1;
                }
            }
            IsAnyShapeSelected();
        }

        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Cross;
            shapeMouseDownTrigerred = false;
            Canvas.MouseDown += Canvas_MouseDownRect;
            Canvas.MouseMove += Canvas_MouseMoveRect;
            Canvas.MouseUp += Canvas_MouseUpRect;
        }

        private void Canvas_MouseDownRect(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(Canvas);

            rect = new Rectangle
            {
                Tag = 0,
                Fill = PickBrush()
            };
            Canvas.SetLeft(rect, startPoint.X);
            Canvas.SetTop(rect, startPoint.Y);
            rect.MouseEnter += (s, e) => Mouse.OverrideCursor = Cursors.Hand;
            rect.MouseLeave += (s, e) => Mouse.OverrideCursor = Cursors.Arrow;
            rect.MouseRightButtonDown += Shape_MouseRightButtonDown;
            rect.MouseLeftButtonDown += Shape_MouseLeftDown;

            rect.MouseMove += shape_MouseMove;
            rect.MouseLeftButtonUp += shape_MouseLeftButtonUp;






            Canvas.Children.Add(rect);
        }

        private void Canvas_MouseMoveRect(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released || rect == null)
                return;

            var pos = e.GetPosition(Canvas);

            var x = Math.Min(pos.X, startPoint.X);
            var y = Math.Min(pos.Y, startPoint.Y);

            var w = Math.Max(pos.X, startPoint.X) - x;
            var h = Math.Max(pos.Y, startPoint.Y) - y;
           
            rect.Width = w;
            rect.Height = h;

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
        }

        private void Shape_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if ((int)(sender as Shape).Tag == 0)
            {
                for (int i = 0; i < Canvas.Children.Count; i++)
                {
                    
                        (Canvas.Children[i] as Shape).Tag = 0;
                        (Canvas.Children[i] as Shape).Effect = null;
                    
                }
           
                if (selectedShapesList.Contains((sender as Shape).GetHashCode()))
                    selectedShapesList.Remove((sender as Shape).GetHashCode());
                selectedShapesList.Add((sender as Shape).GetHashCode());
                (sender as Shape).Tag = 2;
                widthTextBox.Text = (sender as Shape).Width.ToString();
                heightTextBox.Text = (sender as Shape).Height.ToString();
                shapeMouseDownTrigerred = true;
                DropShadowEffect dropShadowEffect = new DropShadowEffect();
                dropShadowEffect.BlurRadius = 50;
                dropShadowEffect.Direction = 270;
                dropShadowEffect.ShadowDepth = 0;
                dropShadowEffect.Opacity = 1;
                dropShadowEffect.Color = Colors.White;
                (sender as Shape).Effect = dropShadowEffect;



            }
           
           
            IsAnyShapeSelected();
            if (((sender as Shape).RenderTransform as RotateTransform) != null)
                Slider.Value = ((sender as Shape).RenderTransform as RotateTransform).Angle;
            else
                Slider.Value = 0;
            //var p = (sender as Shape).TranslatePoint(new Point(0, 0), Canvas);
            //double currentLeft = p.X;
            //double currentTop = p.Y;


            // Vector offset = VisualTreeHelper.GetOffset((Visual)sender);
            //  Combobox.SelectedValue = "Aqua";// AddSpacesToSentence(GetColorName((SolidColorBrush)(sender as Shape).Fill),true);
            int index = 0;
            //MessageBox.Show(AddSpacesToSentence(GetColorName((SolidColorBrush)(sender as Shape).Fill), true));
          IEnumerator<KeyValuePair<string,Color>> enumerator=NamedColors.GetEnumerator();
            for (int i = 0; i < NamedColors.Count<KeyValuePair<string, Color>>(); i++)
            {
                if (enumerator.Current.Key == AddSpacesToSentence(GetColorName((SolidColorBrush)(sender as Shape).Fill), true))
                    index = i;
                enumerator.MoveNext();
            }
            Combobox.SelectedIndex = index-1;
            for (int i = 0; i < Canvas.Children.Count; i++)
            {
                if ((int)(Canvas.Children[i] as Shape).Tag == 1 || (int)(Canvas.Children[i] as Shape).Tag == 2)
                {
                    source.Add((UIElement)(Canvas.Children[i] as Shape));
                    Mouse.Capture(source[amount]);
                    
                    x_shape.Add(Canvas.GetLeft(source[amount]));
                    x_canvas.Add(e.GetPosition(Canvas).X);
                    y_shape.Add(Canvas.GetTop(source[amount]));
                    y_canvas.Add(e.GetPosition(Canvas).Y);
                    amount++;
                }
            }
            captured = true;



            //_buttonPosition = (sender as Shape).TransformToAncestor(Canvas).Transform(new Point(0, 0));
            //var mousePosition = Mouse.GetPosition(Canvas);
            //deltaX = mousePosition.X - _buttonPosition.Value.X;
            //deltaY = mousePosition.Y - _buttonPosition.Value.Y;
            //_isMoving = true;

        }
        private string GetColorName(SolidColorBrush brush)
        {
            var results = typeof(Colors).GetProperties().Where(
             p => (Color)p.GetValue(null, null) == brush.Color).Select(p => p.Name);
            return results.Count() > 0 ? results.First() : String.Empty;
        }
        private void shape_MouseMove(object sender, MouseEventArgs e)
        {

            //if (!_isMoving) return;

            //TranslateTransform transform = new TranslateTransform();
            //transform.X = Mouse.GetPosition(Canvas).X;
            //transform.Y = Mouse.GetPosition(Canvas).Y;
            //(sender as Shape).RenderTransform = transform;


            // var mousePoint = Mouse.GetPosition(Canvas);

            //var offsetX = (_currentTT == null ? _buttonPosition.Value.X : _buttonPosition.Value.X - _currentTT.X) + deltaX - mousePoint.X;
            // var offsetY = (_currentTT == null ? _buttonPosition.Value.Y : _buttonPosition.Value.Y - _currentTT.Y) + deltaY - mousePoint.Y;

            //(sender as Shape).RenderTransform = new TranslateTransform(-offsetX, -offsetY);

            if (captured)
            {
                int index = 0;
                for (int i = 0; i <Canvas.Children.Count; i++)
                {
                    if ((int)(Canvas.Children[i] as Shape).Tag == 1 || (int)(Canvas.Children[i] as Shape).Tag == 2)
                    {
                        Mouse.OverrideCursor = Cursors.ScrollAll;
                        double x = e.GetPosition(Canvas).X;
                        double y = e.GetPosition(Canvas).Y;
                        x_shape[index] += x - x_canvas[index];

                        Canvas.SetLeft(source[index], x_shape[index]);
                        x_canvas[index] = x;
                        y_shape[index] += y - y_canvas[index];
                        Canvas.SetTop(source[index], y_shape[index]);
                        y_canvas[index] = y;
                        index++;
                    }
                }
            }
        }
        private void shape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //_currentTT = (sender as Shape).RenderTransform as TranslateTransform;
            //_isMoving = false;
            Mouse.OverrideCursor = Cursors.Arrow;
            Mouse.Capture(null);
            source.Clear();
            x_shape.Clear();
            x_canvas.Clear();
            y_shape.Clear();
            y_canvas.Clear();
            amount = 0;
            captured = false;
        }
        private void Canvas_MouseUpRect(object sender, MouseButtonEventArgs e)
        {
            rect = null;
            Mouse.OverrideCursor = Cursors.Arrow;
            Canvas.MouseDown -= Canvas_MouseDownRect;
            Canvas.MouseMove -= Canvas_MouseMoveRect;
            Canvas.MouseUp -= Canvas_MouseUpRect;
        }

        private void EllipseButton_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Cross;
            Canvas.MouseDown += Canvas_MouseDownEllipse;
            Canvas.MouseMove += Canvas_MouseMoveEllipse;
            Canvas.MouseUp += Canvas_MouseUpEllipse;
        }

        private void Canvas_MouseDownEllipse(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(Canvas);

            ellipse = new Ellipse
            {
                Tag = 0,
                Fill = PickBrush()
            };
            Canvas.SetLeft(ellipse, startPoint.X);
            Canvas.SetTop(ellipse, startPoint.Y);
            ellipse.MouseEnter += (s, e) => Mouse.OverrideCursor = Cursors.Hand;
            ellipse.MouseLeave += (s, e) => Mouse.OverrideCursor = Cursors.Arrow;
            ellipse.MouseRightButtonDown += Shape_MouseRightButtonDown;
            ellipse.MouseLeftButtonDown += Shape_MouseLeftDown;

            ellipse.MouseMove += shape_MouseMove;
            ellipse.MouseLeftButtonUp += shape_MouseLeftButtonUp;

            Canvas.Children.Add(ellipse);
            // CaptureMouse();
        }

        private void Canvas_MouseMoveEllipse(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released || ellipse == null)
                return;

            var pos = e.GetPosition(Canvas);

            var x = Math.Min(pos.X, startPoint.X);
            var y = Math.Min(pos.Y, startPoint.Y);

            var w = Math.Max(pos.X, startPoint.X) - x;
            var h = Math.Max(pos.Y, startPoint.Y) - y;

            ellipse.Width = w;
            ellipse.Height = h;

            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, y);
        }

        private void Canvas_MouseUpEllipse(object sender, MouseButtonEventArgs e)
        {
            // ReleaseMouseCapture();
            ellipse = null;
            shapeMouseDownTrigerred = false;
            Mouse.OverrideCursor = Cursors.Arrow;
            Canvas.MouseDown -= Canvas_MouseDownEllipse;
            Canvas.MouseMove -= Canvas_MouseMoveEllipse;
            Canvas.MouseUp -= Canvas_MouseUpEllipse;
        }

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            ResourceManager rm = null;
            if ((sender as Button).Content == FindResource("Play"))
            {
                (sender as Button).Content = FindResource("Stop");
                rm = new ResourceManager("WPFLab2.pl", Assembly.GetExecutingAssembly());
            }
            else
            {
                (sender as Button).Content = FindResource("Play");
                rm = new ResourceManager("WPFLab2.en", Assembly.GetExecutingAssembly());
            }
            RectangleTextBlock.Text = rm.GetString("Rectangle");
            EllipseTextBlock.Text = rm.GetString("Ellipse");
            WidthTextBlock.Text = rm.GetString("Width");
            HeightTextBlock.Text = rm.GetString("Height");
            AngleTextBlock.Text = rm.GetString("Angle");
            ColorTextBlock.Text = rm.GetString("Color");
            ExportTextBlock.Text = rm.GetString("Save as .png");
            DeleteTextBlock.Text = rm.GetString("Delete");
            RandomTextBlock.Text = rm.GetString("Random Colors");
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Files (*.png)|*.png";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)Canvas.RenderSize.Width,
  (int)Canvas.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
                rtb.Render(Canvas);

                //var crop = new CroppedBitmap(rtb, new Int32Rect(50, 50, 250, 250));

                BitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

                using (var fs = System.IO.File.OpenWrite(dlg.FileName))
                {
                    pngEncoder.Save(fs);
                }
            }
        }

        private void widthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!skipTextValidation)
            {
                int tempInt = 0;
                bool parsed = int.TryParse((sender as TextBox).Text, out tempInt);
                if (!parsed || Int32.Parse((sender as TextBox).Text) <= 0 || (sender as TextBox).Text == String.Empty)
                {
                    WidthBorder.BorderBrush = System.Windows.Media.Brushes.Red;
                    WidthBorder.BorderThickness = new Thickness(2);
                    //(sender as TextBox).BorderBrush= System.Windows.Media.Brushes.Red;
                    return;
                }
                WidthBorder.BorderThickness = new Thickness(0);
                for (int i = 0; i < Canvas.Children.Count; i++)
                {
                    if ((int)(Canvas.Children[i] as Shape).Tag == 2)
                    {
                        (Canvas.Children[i] as Shape).Width = Int32.Parse((sender as TextBox).Text);
                    }
                }
            }
        }

        private void heightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!skipTextValidation)
            {
                int tempInt = 0;
                bool parsed = int.TryParse((sender as TextBox).Text, out tempInt);
                if (!parsed || Int32.Parse((sender as TextBox).Text) <= 0 || (sender as TextBox).Text == String.Empty)
                {
                    HeightBorder.BorderBrush = System.Windows.Media.Brushes.Red;
                    HeightBorder.BorderThickness = new Thickness(2);
                    return;
                }
                HeightBorder.BorderThickness = new Thickness(0);
                for (int i = 0; i < Canvas.Children.Count; i++)
                {
                    if ((int)(Canvas.Children[i] as Shape).Tag == 2)
                    {
                        (Canvas.Children[i] as Shape).Height = Int32.Parse((sender as TextBox).Text);
                    }
                }
            }
        }
        string AddSpacesToSentence(String text, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
           // String.Format.Foreground=
            return newText.ToString();
        }
        private IEnumerable<KeyValuePair<String, Color>> GetColors()
        {
            return typeof(Colors)
                .GetProperties()
                .Where(prop =>
                    typeof(Color).IsAssignableFrom(prop.PropertyType))
                .Select(prop => 

                new KeyValuePair<String, Color>(AddSpacesToSentence(prop.Name,true), (Color)prop.GetValue(null))
                );
           
        }

        private void TextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < Canvas.Children.Count; i++)
            {
                if ((int)(Canvas.Children[i] as Shape).Tag == 2)
                {
                    (Canvas.Children[i] as Shape).Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(RemoveWhitespace((sender as TextBlock).Text)));// System.Drawing.Color.FromName((sender as TextBlock).Text));
                }
            }
            
        }
        private string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RotateTransform rotateTransform2 = new RotateTransform((sender as Slider).Value);

            for(int i = 0; i < Canvas.Children.Count; i++)
            {
                if ((int)(Canvas.Children[i] as Shape).Tag == 2)
                {
                    rotateTransform2.CenterX = (Canvas.Children[i] as Shape).Width/2;
                    rotateTransform2.CenterY =  (Canvas.Children[i] as Shape).Height/2;
                    (Canvas.Children[i] as Shape).RenderTransform = rotateTransform2;
                }
            }           
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool proceed = true;
            for (int i = 0; i < Canvas.Children.Count; i++)
            {
                if (Canvas.Children[i].IsMouseDirectlyOver)
                    proceed = false;


            }
            
            //Point p = e.GetPosition(Canvas);
            //HitTestResult result = VisualTreeHelper.HitTest(Canvas, p);
            //for(int i = 0; i < Canvas.Children.Count; i++)
            //{
            //    if (VisualTreeHelper.HitTest(Canvas.Children[i], p) != null)
            //        proceed = false;
            //    if (Canvas.Children[i] is Rectangle)
            //    {
            //        if (IsInsideRectangle(Canvas.Children[i], p))
            //            proceed = false;
            //    }
            //}           
            if (proceed)
            {               
                for (int i = 0; i < Canvas.Children.Count; i++)
                {
                    if ((int)(Canvas.Children[i] as Shape).Tag == 2 || (int)(Canvas.Children[i] as Shape).Tag == 1)
                    {
                        (Canvas.Children[i] as Shape).Tag = 0;
                        (Canvas.Children[i] as Shape).Effect = null;
                    }
                }
            }
        }
        private bool MouseInShape(Point p)
        {
            for(int i = 0; i < Canvas.Children.Count; i++)
            {
                //if(p.X)
                    //(Canvas.Children as Shape).
            }
            return false;
        }
        public bool IsInsideRectangle(UIElement element, Point _pos)
        {
            double top = Canvas.GetTop(element);
            double left = Canvas.GetLeft(element);
            double right = left + (element as System.Windows.Shapes.Rectangle).Width;
            double bottom = top + (element as System.Windows.Shapes.Rectangle).Height;
            return _pos.X > left && _pos.X < right && _pos.Y > top && _pos.Y < bottom;
        }
        private bool IsAnyShapeSelected()
        {
            bool isSelected = false;
            for(int i = 0; i < Canvas.Children.Count; i++)
            {
                if((int)(Canvas.Children[i] as Shape).Tag == 2|| (int)(Canvas.Children[i] as Shape).Tag == 1)
                {
                    isSelected = true;

                }
            }
            if (!isSelected)
            {
                skipTextValidation = true;
                widthTextBox.IsEnabled = false;
                heightTextBox.IsEnabled = false;
                DeleteButton.IsEnabled = false;
                RandomColorsButton.IsEnabled = false;
                Slider.IsEnabled = false;
                Combobox.IsEnabled = false;
                Combobox.SelectedItem = null;
                widthTextBox.Text = "";
                heightTextBox.Text = "";
                return false;
            }
            else
            {
                skipTextValidation = false;
                widthTextBox.IsEnabled = true;
                heightTextBox.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                RandomColorsButton.IsEnabled = true;
                Slider.IsEnabled = true;
                Combobox.IsEnabled = true;
                return true;
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}

//What doesn't work

//Lab:
//Mouse not followed outside of window when drawing shape
//FIXED - Objects are spawning on areas other than canvas

//Home:
//FIXED - Transparent color is present
//FIXED -Text in color choosing is only black
//fIXED - Shape's color is not showing on combobox when the shape is selected
//FIXED - color combobox text is not formatted, AquaBlue shoud be Aqua Blue
//FIXED - Clicking on Canvas doesnt work, dont know how to detect whether mouse is in Shape
//FIXED - Cant move multiple shapes


//General flaws
//magic values assigned to Tag
//Unecessary separate right mouse down fucntions for ellipse and rectangle, could just use shape