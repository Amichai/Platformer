using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Platformer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        GameInstance Instance;
        ControlPanel cp;
        public MainWindow() {
            Instance = GameInstance.Inst;
            Instance.PropertyChanged += instance_PropertyChanged;
            InitializeComponent();
            this.Instance.SpritesChanged += AllSprites_CollectionChanged;
            cp = new ControlPanel();
            cp.Show();
            Instance.Reset();
            Instance.TimeStarted += Instance_TimeStarted;
            cp.NewGameLoaded += cp_NewGameLoaded;

        }

        void cp_NewGameLoaded(object sender, EventArgs e) {
            this.Width = Instance.BoardWidth;
            this.Height = Instance.BoardHeight;
            Instance.Reset();
        }

        void Instance_TimeStarted(object sender, EventArgs e) {
            this.Focus();
        }

        void AllSprites_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) {
                Sprite a = this.Instance.NewestSprite();
                var newRectangle = new Rectangle() {
                    Width = a.Width,
                    Height = a.Height,
                    Fill = a.Brush,
                    Tag = a
                };
                Canvas.SetLeft(newRectangle, a.State.X);
                Canvas.SetLeft(newRectangle, a.State.Y);
                newRectangle.MouseRightButtonDown += newRectangle_MouseRightButtonDown;
                this.canvas.Children.Add(newRectangle);
                if (this.cp != null) {
                    this.cp.livingSprites.ItemsSource = this.Instance.GetAllSprites().ToList();
                }
            } else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove) {
                var idx = e.OldStartingIndex;
                Dispatcher.Invoke((Action)(() => 
                    {
                        this.canvas.Children.RemoveAt(idx);
                        if (this.cp != null) {
                            this.cp.livingSprites.ItemsSource = this.Instance.GetAllSprites().ToList();
                        }
                    }));

            } else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset) {
                this.canvas.Children.Clear();
                if (this.cp != null) {
                    this.cp.livingSprites.ItemsSource = this.Instance.GetAllSprites().ToList();
                }
            }
        }

        void newRectangle_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            int index = this.canvas.Children.IndexOf((UIElement)sender);
            this.Instance.RemoveSprite(index);
        }

        private void updateTheUI() {
            foreach (Rectangle a in this.canvas.Children) {
                a.Height = (a.Tag as Sprite).Height;
                a.Width = (a.Tag as Sprite).Width;
                Canvas.SetLeft(a, (a.Tag as Sprite).State.X);
                Canvas.SetTop(a, (a.Tag as Sprite).State.Y);
                //Canvas.SetLeft(a, (a.Tag as Sprite).ScreenY());
                //Canvas.SetTop(a, (a.Tag as Sprite).ScreenX());
            }
        }

        void instance_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "BackgroundBrush") {
                this.canvas.Background = Instance.BackgroundBrush;
            } else if (e.PropertyName == "NewFrame") {
                Dispatcher.Invoke((Action)(() => updateTheUI()));
            }
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            foreach (var a in cp.allSprites.SelectedItems) {
                var newSprite = ((Sprite)a).Clone();
                newSprite.State.X = Mouse.GetPosition(this.canvas).X;
                newSprite.State.Y = Mouse.GetPosition(this.canvas).Y;
                this.Instance.AddNewSprite(newSprite, true);
            }
        }

        public static new void Close(){
            GameInstance.Inst.Close();
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            this.cp.Close();
            this.Instance.Close();
        }

        private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e) {
            this.Instance.SetBoardSize(e.NewSize.Width, e.NewSize.Height);
        }

        private void Window_KeyDown_1(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Up:
                    this.Instance.Up();
                    break;
                case Key.Down:
                    this.Instance.Down();
                    break;
                case Key.Right:
                    this.Instance.Right();
                    break;
                case Key.Left:
                    this.Instance.Left();
                    break;
                case Key.Escape:
                    cp.Close();
                    MainWindow.Close();
                    break;
            }
        }
    }
}
