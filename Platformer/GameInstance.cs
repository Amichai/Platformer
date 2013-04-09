using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;

namespace Platformer {
    ///Manages assets, sprites, boards, physics, etc
    ///
    public class GameInstance : INotifyPropertyChanged {
        private GameInstance() {
            this.BoardWidth = 1000;
            this.BoardHeight = 1000;
            this.allSprites = new ObservableCollection<Sprite>();
            this.allSprites.CollectionChanged += allSprites_CollectionChanged;
            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();
            ts = new ThreadStart(time);
            this.physicsThread = new Thread(ts);
            this.physicsThread.Start();
            this.Name = "NoName";
            this.initialSprites = new List<Sprite>();
            this.GamePerspective = new Perspective();
        }

        public TimeSpan GameTime {
            get { return _GameTime; }
            set {
                if (_GameTime != value) {
                    _GameTime = value;
                    OnPropertyChanged(GameTimePropertyName);
                }
            }
        }
        private TimeSpan _GameTime;
        public const string GameTimePropertyName = "GameTime";

        void allSprites_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            var eh = SpritesChanged;
            if (eh != null) {
                eh(sender, e);
            }
        }

        public void RemoveSprite(int idx) {
            lock (this.allSprites) {
                this.allSprites.RemoveAt(idx);
            }
        }

        public Sprite NewestSprite() {
            return this.allSprites.Last();
        }

        List<Sprite> initialSprites;

        public static GameInstance Inst = new GameInstance();

        public void Close() {
            this.physicsThread.Abort();
        }

        public void AddNewSprite(Sprite s, bool initialSprite = false) {
            if (initialSprite) {
                this.initialSprites.Add(s.Clone());
            }
            lock (this.allSprites) {
                this.allSprites.Add(s);
                if (s.Name == "Avatar") {
                    this.mainCharacter = s;
                }
            }
        }

        public void Reset() {
            initializeBackground();
            this.stopwatch.Reset();
            lock (allSprites) {
                this.allSprites.Clear();
                foreach (var s in initialSprites) {
                    AddNewSprite(Sprite.Deserialize(s.Serialize()));
                }
            }
            ///Set the background
            ///place the objects, 
            ///Run the timeline + physics
        }

        Thread physicsThread;
        ThreadStart ts;

        public double BoardWidth { get; set; }
        public double BoardHeight { get; set; }

        Perspective GamePerspective { get; set; }

        private bool isOffTheBoard(Sprite s) {
            return (s.Right < 0 || s.Top > BoardHeight || s.Left > BoardWidth || s.Bottom < 0);
        }

        public enum CollisionType { right, left, top, bottom, overlap, none }

        private double collionIn1D(double s1a, double s1b, double s2a, double s2b) {
            Dictionary<string, double> horizEdges = new Dictionary<string, double>() {
                {"l1", s1a},
                {"r1", s1b},
                {"l2", s2a},
                {"r2", s2b},
            };
            var sorted = horizEdges.OrderBy(i => i.Value);
            if (sorted.ElementAt(0).Key.Last() == sorted.ElementAt(1).Key.Last()) {
                return 0;
            } else {
                return sorted.ElementAt(2).Value - sorted.ElementAt(1).Value;
            }
        }

        private CollisionType collision(Sprite s1, Sprite s2) {
            ///Sort four edges and test for arrangement
            var horizCollision = collionIn1D(s1.Left, s1.Right, s2.Left, s2.Right);
            var vertCollision = collionIn1D(s1.Top, s1.Bottom, s2.Top, s2.Bottom);

            if (horizCollision == 0 || vertCollision == 0) {
                return CollisionType.none;
            }

            if (vertCollision < horizCollision) {
                //vert collision
                if (s1.Top < s2.Top) {
                    return CollisionType.bottom;
                } else {
                    return CollisionType.top;
                }

            }
            if (horizCollision < vertCollision) {
                //horiz collision
                if (s1.Left < s2.Left) {
                    return CollisionType.right;
                } else {
                    return CollisionType.left;
                }
            }
            throw new Exception();
        }

        private void spriteEvents(Sprite s) {
            if (isOffTheBoard(s)) {
                s.RaiseOffTheBoard();
            }

            ///Test for collision
            if (s.IsSolid) {
                foreach (var s2 in this.allSprites) {
                    if (s.IsSolid && s != s2) {
                        var obst = collision(s, s2);
                        switch (obst) {
                            case CollisionType.none:
                                s2.State.ClearObstructions();
                                break;
                            case CollisionType.overlap:
                                s.State.Freeze();
                                s2.State.Freeze();
                                break;
                            default:
                                s.State.AddObstruction(obst);
                                break;
                        }
                    }
                }
            }
        }

        public bool TimeRunning { get; set; }

        private void time() {
            while (true) {
                ///Iterate over all the sprites
                lock (allSprites) {
                    for (int i = 0; i < this.allSprites.Count(); i++) {
                        var s = this.allSprites[i];
                        if (s.DeleteMe) {
                            this.RemoveSprite(i);
                            i--;
                        }
                        spriteEvents(s);
                        s.Update(stopwatch.Elapsed);
                    }
                }

                this.GameTime = stopwatch.Elapsed;
                ///Set the positions on the board
                ///Check the time
                ///Update positions
                ///
                OnPropertyChanged("NewFrame");
            }
        }

        private void OnPropertyChanged(string name) {
            var eh = PropertyChanged;
            if (eh != null) {
                eh(this, new PropertyChangedEventArgs(name));
            }
        }

        public Brush BackgroundBrush {
            get { return _BackgroundBrush; }
            set {
                if (_BackgroundBrush != value) {
                    _BackgroundBrush = value;
                    OnPropertyChanged(BackgroundBrushPropertyName);
                }
            }
        }
        private Brush _BackgroundBrush;
        public const string BackgroundBrushPropertyName = "BackgroundBrush";

        public string BackgroundImage {
            get { return _BackgroundImage; }
            set {
                if (_BackgroundImage != value) {
                    _BackgroundImage = value;
                    OnPropertyChanged(BackgroundImagePropertyName);
                }
            }
        }
        private string _BackgroundImage;
        public const string BackgroundImagePropertyName = "BackgroundImage";

        public Stopwatch stopwatch {
            get { return _stopwatch; }
            set {
                if (_stopwatch != value) {
                    _stopwatch = value;
                    OnPropertyChanged(stopwatchPropertyName);
                }
            }
        }
        private Stopwatch _stopwatch;
        public const string stopwatchPropertyName = "stopwatch";
        

        private void initializeBackground() {
            BackgroundImage = @"C:\Users\Amichai\Pictures\Borges.jpg";
            var bi = new BitmapImage(new Uri(BackgroundImage));
            this.BackgroundBrush = new ImageBrush(bi);
            this.BackgroundBrush = Brushes.LightGreen;

            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Sprite> allSprites;

        public event System.Collections.Specialized.NotifyCollectionChangedEventHandler  SpritesChanged;

        public void SetBoardSize(double width, double height) {
            this.BoardWidth = width;
            this.BoardHeight = height;
        }

        public event EventHandler TimeStarted;

        private void OnTimeStarted() {
            var eh = TimeStarted;
            if (eh != null) {
                eh(this, new EventArgs());
            }
        }

        internal void Start() {
            this.stopwatch.Restart();
            OnTimeStarted();
        }

        internal ObservableCollection<Sprite> GetAllSprites() {
            return this.allSprites;
        }

        Sprite mainCharacter = null;

        internal void Up() {
            if (!this.mainCharacter.State.IsObstructed(CollisionType.bottom)) {
                return;
            }
            this.mainCharacter.State.Vy = -.1;
            this.mainCharacter.State.Y -= .1;
        }

        internal void Down() {
            this.mainCharacter.State.Vy += .1;
        }

        internal void Right() {
            this.mainCharacter.State.Vx += .1;
        }

        internal void Left() {
            this.mainCharacter.State.Vx -= .1;
        }

        public string Name { get; set; }

        public XElement Serialize() {
            XElement root = new XElement("GameInstance");
            root.Add(new XAttribute("Name", this.Name));
            root.Add(new XAttribute("Width", this.BoardWidth));
            root.Add(new XAttribute("Height", this.BoardHeight));
            foreach (var a in this.GetAllSprites()) {
                root.Add(a.Serialize());
            }
            return root;
        }

        public void Deserialize(string filepath) {
            ///Background image as well please
            var root = XElement.Load(filepath);
            this.Name = (string)root.Attribute("Name");
            this.BoardWidth = double.Parse((string)root.Attribute("Width"));
            this.BoardHeight = double.Parse((string)root.Attribute("Height"));
            if (this.allSprites != null) {
                this.allSprites.Clear();
            }
            foreach (var a in root.Elements()) {
                this.AddNewSprite(Sprite.Deserialize(a));
                this.initialSprites.Add(Sprite.Deserialize(a));
            }
        }
    }
}
