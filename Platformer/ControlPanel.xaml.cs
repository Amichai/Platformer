using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Platformer {
    /// <summary>
    /// Interaction logic for ControlPanel.xaml
    /// </summary>
    public partial class ControlPanel : Window {
        public ControlPanel() {
            InitializeComponent();
            this.boardStatePanel.DataContext = GameInstance.Inst;

            List<Sprite> allSprites = new List<Sprite>();
            var state1 = new Position() { Ay = 0, Ax = 0};
            var s1 = new Sprite(state1) {
                Name = "Red Brick",
                Brush = Brushes.Red,
                IsSolid = true
            };

            var state2 = new Position() { Ay = .0001, Ax = .000012 };
            var s2 = new Sprite(state2) {
                Name = "Wall",
                Brush = Brushes.Black
            };

            var state3 = new Position() { Ay = .0001, Ax = .000012 };
            var s3 = new Sprite(state3) {
                Name = "Avatar",
                Brush = Brushes.Green,
                Width = 10,
                Height = 30
            };

            //var s3 = new Sprite();

            allSprites.Add(s1);
            allSprites.Add(s2);
            allSprites.Add(s3);
            this.allSprites.ItemsSource = allSprites;
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastFilepath)) {
                openFile(Properties.Settings.Default.LastFilepath);
            }
            ///Create a list of sprites, bind these sprites to the control panel
            ///Allow point and click addition to the window
        }

        private void Reset_Click(object sender, RoutedEventArgs e) {
            GameInstance.Inst.Reset();
        }

        private void Start_Click(object sender, RoutedEventArgs e) {
            GameInstance.Inst.Start();
            
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            GameInstance.Inst.Serialize().Save(@"C:\Platformer\savedGame.xml");
        }

        public event EventHandler NewGameLoaded;

        private void OnNewGameLoaded() {
            var eh = NewGameLoaded;
            if (eh != null) {
                eh(this, new EventArgs());
            }
        }

        private void openFile(string filepath) {
            GameInstance.Inst.Deserialize(filepath);
            OnNewGameLoaded();
        }

        private void Open_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            var filepath = ofd.FileName;
            Properties.Settings.Default.LastFilepath = filepath;
            Properties.Settings.Default.Save();
            openFile(filepath);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            MainWindow.Close();
        }
    }
}
