using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;

namespace SomeGameWPF {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            OSM = new ObjectStateManager(backCanvas, logging);
            //OSM.Init();
        }

        ObjectStateManager OSM;
        Random rand = new Random();

        class TextBoxPart {
            public TextBox obj;
            public int shiftX;
            public int shiftY;
            public TextBoxPart(TextBox tb, int shiftX, int shiftY) {
                obj = tb;
                this.shiftY = shiftY;
                this.shiftX = shiftX;
            }
        }

        class ImagePart {
            public Image obj;
            public int shiftX;
            public int shiftY;
            public ImagePart(Image img, int shiftX, int shiftY) {
                obj = img;
                this.shiftY = shiftY;
                this.shiftX = shiftX;
            }

        }

        class Parts {
            public List<TextBoxPart> textBoxes = new List<TextBoxPart> { };
            public List<ImagePart> images = new List<ImagePart> { };
            
            public void AddTextBox(TextBox tb, int shiftX, int shiftY) {
                textBoxes.Add(new TextBoxPart(tb, shiftX, shiftY));
            }

            public void AddImage(Image img, int shiftX, int shiftY) {
                images.Add(new ImagePart(img, shiftX, shiftY));
            }

            
        }

        class Human {
            int _left;
            public int Left {
                get { return _left; }
                set {
                    _left = value;
                }
            }

            int _top;
            public int Top {
                get { return _top; }
                set {
                    _top = value;
                }
            }

            int _toLeft;
            public int ToLeft {
                get { return _toLeft; }
                set {
                    _toLeft = value;
                }
            }

            int _toTop;
            public int ToTop {
                get { return _toTop; }
                set {
                    _toTop = value;
                }
            }
            Canvas canvas;
            TextBox logging;

            Parts parts = new Parts();
            bool stop = false;

            string uid;
            TextBox tb_msg;
            Image msg;
            Random rand;
            
            public Human(Canvas canv, TextBox logg, string uid, int rand_seed = -1) {
                this.uid = uid;

                //parts.AddTextBox(new TextBox { Width = 100, Height = 20, Visibility = Visibility.Collapsed }, 0, -20);
                
                

                parts.AddImage(new Image {
                    Name = $"body_{uid}",
                    Width = 100,
                    Height = 100,
                    Source = new BitmapImage(new Uri(@"D:\PROJECTS\SomeGameWPF\SomeGameWPF\images\human.png")),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                }, 0, 0);

                tb_msg = new TextBox { Width = 100, Height = 20, Text = "Дратути!", Visibility = Visibility.Hidden };
                tb_msg.BorderThickness = new Thickness(0);
                Canvas.SetZIndex(tb_msg, 1001);
                parts.AddTextBox(tb_msg, 100, -70);

                msg = new Image {
                    Name = $"msg_{uid}",
                    Width = 150,
                    Height = 150,
                    Source = new BitmapImage(new Uri(@"D:\PROJECTS\SomeGameWPF\SomeGameWPF\images\cloud.png")),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Visibility = Visibility.Hidden,
                };
                Canvas.SetZIndex(msg, 1000);
                parts.AddImage(msg, 70, -140);

                rand = (rand_seed > 0) ? new Random(rand_seed) : new Random();
                canvas = canv;
                logging = logg;
            }

            void Say() {
                tb_msg.Dispatcher.Invoke(DispatcherPriority.Background,
                        new DispatcherOperationCallback(delegate {
                            tb_msg.Visibility = Visibility.Visible;
                            msg.Visibility = Visibility.Visible;
                            return null;
                        }), null);
                
            }

            public void Draw(int x, int y) {
                Left = x;
                Top = y;

                foreach (var item in parts.images) {
                    item.obj.Margin = new Thickness(x, y, 0, 0);
                    canvas.Children.Add(item.obj);
                    Canvas.SetLeft(item.obj, 0);
                    Canvas.SetTop(item.obj, 0);
                }

                foreach (var item in parts.textBoxes) {
                    item.obj.Margin = new Thickness(x, y, 0, 0);
                    canvas.Children.Add(item.obj);
                    Canvas.SetLeft(item.obj, 0);
                    Canvas.SetTop(item.obj, 0);
                }

             
            }
            /*
            public void SetZIndex(int val) {
                body.Dispatcher.Invoke(DispatcherPriority.Background, 
                    new DispatcherOperationCallback(delegate {
                        Canvas.SetZIndex(body, val);
                        return null;
                    }), null);
                
            }
            */
            async public void Behavior() {
                await Task.Factory.StartNew(()=>View(), TaskCreationOptions.LongRunning);
                await Task.Factory.StartNew(()=>MovingWithoutTarget(), TaskCreationOptions.LongRunning);
            }

            public void View() {

            }

            public int MoveTo(int x, int y, int speed = 0) {
                ToLeft = x;
                ToTop = y;
                TimeSpan dur = TimeSpan.FromSeconds(Math.Sqrt(Math.Pow(Left - ToLeft, 2) + Math.Pow(Top - ToTop, 2)) / 60.0d);
                ThicknessAnimation anim;
                
                foreach (ImagePart part in parts.images) {
                    part.obj.Dispatcher.Invoke(DispatcherPriority.Background,
                        new DispatcherOperationCallback(delegate {
                            part.obj.FlowDirection = (Left < ToLeft) ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
                            anim = new ThicknessAnimation();
                            if (part.obj.Name.Contains("body")) {
                                anim.Completed += SetPos;
                                if (parts.textBoxes.Count != 0) {
                                    //anim.CurrentTimeInvalidated += PrintCoordinates;
                                }
                            }
                            
                            anim.From = new Thickness(Left+part.shiftX, Top+part.shiftY, 0, 0);
                            anim.To = new Thickness(ToLeft+part.shiftX, ToTop+part.shiftY, 0, 0);
                            anim.Duration = dur;

                            part.obj.BeginAnimation(MarginProperty, anim);

                            return null;
                        }), null);
                }

                foreach (TextBoxPart part in parts.textBoxes) {
                    part.obj.Dispatcher.Invoke(DispatcherPriority.Background,
                        new DispatcherOperationCallback(delegate {
                            anim = new ThicknessAnimation();
                            anim.From = new Thickness(Left + part.shiftX, Top + part.shiftY, 0, 0);
                            anim.To = new Thickness(ToLeft + part.shiftX, ToTop + part.shiftY, 0, 0);
                            anim.Duration = dur;
                            part.obj.BeginAnimation(MarginProperty, anim);
                            return null;
                        }), null);
                }

                return (int)dur.TotalMilliseconds;
            }

            private void PrintCoordinates(object sender, EventArgs e) {
                parts.textBoxes[0].obj.Text = $"X: {(int)parts.images[0].obj.Margin.Left}, Y: {(int)parts.images[0].obj.Margin.Top}";
            }

            private void SetPos(object sender, EventArgs e) {
                Left = ToLeft;
                Top = ToTop;
            }
            
            public void MovingWithoutTarget() {
                int x = 0, y = 0, dur = 0;
                
                while (!stop) {
                    canvas.Dispatcher.Invoke(DispatcherPriority.Background, 
                        new DispatcherOperationCallback(delegate {
                            x = rand.Next(0, (int)canvas.Width - 100);
                            y = rand.Next(0, (int)canvas.Height - 100);
                            
                            return null;
                        }), null);
                    dur = MoveTo(x, y);
                    Thread.Sleep(dur+2000);
                    // Say();
                    // Thread.Sleep(dur + 4000);
                }
            }

            void add_log(string s) {
                logging.Dispatcher.Invoke(DispatcherPriority.Background,
                        new DispatcherOperationCallback(delegate {
                            logging.Text += s;
                            return null;
                        }), null);
            }


            void log_this(string s) {
                logging.Dispatcher.Invoke(DispatcherPriority.Background,
                        new DispatcherOperationCallback(delegate {
                            logging.Text = s;
                            return null;
                        }), null);
            }
        }


        class ObjectStateManager {
            Canvas canvas;
            TextBox logging;
            Random rand = new Random();
            List<Human> humans = new List<Human> { };
            List<Human> humans_z = new List<Human> { };
            List<object> objects = new List<object> { };
            MD5 md5 = MD5.Create();
            int lastUID = 0;

            public ObjectStateManager(Canvas canv, TextBox logg) {
                canvas = canv;
                logging = logg;
                
            }
                       
            public void AddHuman(int rand_seed = -1) {
                
                Human human = new Human(canvas, logging, lastUID++.ToString().PadLeft(5, '0'), rand_seed);
                humans.Add(human);
                humans.Last().Draw(rand.Next(1, 100), rand.Next(1, 100));
                humans.Last().Behavior();

            }

            public void AddHumans(int count) {
                for (int i = 0; i < count; i++) {
                    Human human = new Human(canvas, logging, lastUID++.ToString().PadLeft(5, '0'));
                    humans.Add(human);
                    humans.Last().Draw(rand.Next(1, 100), rand.Next(1, 100));
                    humans.Last().Behavior();
                }
            }
            /*
            public async void Init() {
                await Task.Factory.StartNew(() => ControlZIndex(), TaskCreationOptions.LongRunning);
            }

            void ControlZIndex() {                
                while (true) {
                    List<Human> h = humans.OrderBy(o => o.Top).ToList();
                    for (int i = 0; i < h.Count; i++)
                        h[i].SetZIndex(i * 20);
                        
                    Thread.Sleep(1);
                }
            }
            */
            // ЛОГИ

            void add_log(string s) {
                logging.Dispatcher.Invoke(DispatcherPriority.Background,
                        new DispatcherOperationCallback(delegate {
                            logging.Text += s;
                            return null;
                        }), null);
            }


            void log_this(string s) {
                logging.Dispatcher.Invoke(DispatcherPriority.Background,
                        new DispatcherOperationCallback(delegate {
                            logging.Text = s;
                            return null;
                        }), null);
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e) {
            /*Human newbie1 = new Human(backCanvas, logging);
            Human newbie2 = new Human(backCanvas, logging);
            newbie1.Draw(1, 1);
            newbie2.Draw(50, 10);
            newbie1.ReduceSize();*/
            OSM.AddHuman();
            // OSM.AddHumans(1000);
            
            
        }
    }
}
