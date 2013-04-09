using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Platformer {
    public class Position : INotifyPropertyChanged {
        public Position() {
            this.X = 0;
            this.Y = 0;
            this.Vx = 0;
            this.Vy = 0;
            this.Ax = 0;
            this.Ay = 0;
            this.obstructions = new HashSet<GameInstance.CollisionType>();
        }

        public void AddObstruction(GameInstance.CollisionType obst) {
            if (!this.obstructions.Contains(obst)) {
                this.obstructions.Add(obst);
            }
        }

        HashSet<GameInstance.CollisionType> obstructions;

        public Position Clone() {
            return new Position() {
                X = this.X,
                Y = this.Y,
                Ax = this.Ax,
                Ay = this.Ay,
                Vx = this.Vx,
                Vy = this.Vy,
            };
        }

        internal void Update(TimeSpan elapsed) {
            var dVx = Ax * elapsed.TotalMilliseconds;
            var dVy = Ay * elapsed.TotalMilliseconds;
            Vx += dVx;

            var dx = Vx * elapsed.TotalMilliseconds;
            var dy = Vy * elapsed.TotalMilliseconds;
            if (dx > 0 && !obstructions.Contains(GameInstance.CollisionType.right)) {
                X += dx;
            } 
            if (dx < 0 && !obstructions.Contains(GameInstance.CollisionType.left)) {
                X += dx;
            }
            
            if (obstructions.Contains(GameInstance.CollisionType.bottom)) {
                Vy = 0;
            }
            
            Vy += dVy;

            if (dy > 0 && !obstructions.Contains(GameInstance.CollisionType.bottom)) {
                Y += dy;
            }
            if (dy < 0 && !obstructions.Contains(GameInstance.CollisionType.top)) {
                Y += dy;
            }

        }

        public double Ay {
            get { return _Ay; }
            set {
                if (_Ay != value) {
                    _Ay = value;
                    OnPropertyChanged(AyPropertyName);
                }
            }
        }
        private double _Ay;
        public const string AyPropertyName = "Ay";

        public double Ax {
            get { return _Ax; }
            set {
                if (_Ax != value) {
                    _Ax = value;
                    OnPropertyChanged(AxPropertyName);
                }
            }
        }
        private double _Ax;
        public const string AxPropertyName = "Ax";

        public double Vy {
            get { return _Vy; }
            set {
                if (_Vy != value) {
                    _Vy = value;
                    OnPropertyChanged(VyPropertyName);
                }
            }
        }
        private double _Vy;
        public const string VyPropertyName = "Vy";

        public double Vx {
            get { return _Vx; }
            set {
                if (_Vx != value) {
                    _Vx = value;
                    OnPropertyChanged(VxPropertyName);
                }
            }
        }
        private double _Vx;
        public const string VxPropertyName = "Vx";

        public double Y {
            get { return _Y; }
            set {
                if (_Y != value) {
                    _Y = value;
                    OnPropertyChanged(YPropertyName);
                }
            }
        }
        private double _Y;
        public const string YPropertyName = "Y";


        public double X {
            get { return _X; }
            set {
                if (_X != value) {
                    _X = value;
                    OnPropertyChanged(XPropertyName);
                }
            }
        }
        private double _X;
        public const string XPropertyName = "X";

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name) {
            var eh = PropertyChanged;
            if (eh != null) {
                eh(this, new PropertyChangedEventArgs(name));
            }
        }

        internal void Freeze() {
            this.Vx = 0;
            this.Vy = 0;
            this.Ax = 0;
            this.Ay = 0;
        }

        internal void ClearObstructions() {
            this.obstructions.Clear();
        }

        internal bool IsObstructed(GameInstance.CollisionType obst) {
            return this.obstructions.Contains(obst);
        }

        internal XElement Serialize() {
            XElement root = new XElement("Position");
            root.Add(new XAttribute("X", this.X));
            root.Add(new XAttribute("Y", this.Y));
            root.Add(new XAttribute("Ax", this.Ax));
            root.Add(new XAttribute("Ay", this.Ay));
            root.Add(new XAttribute("Vx", this.Vx));
            root.Add(new XAttribute("Vy", this.Vy));
            return root;
        }

        internal static Position Deserialize(XElement root) {
            var pos = new Position();
            pos.X = double.Parse((string)root.Attribute("X"));
            pos.Y = double.Parse((string)root.Attribute("Y"));
            pos.Ax = double.Parse((string)root.Attribute("Ax"));
            pos.Ay = double.Parse((string)root.Attribute("Ay"));
            pos.Vx = double.Parse((string)root.Attribute("Vx"));
            pos.Vy = double.Parse((string)root.Attribute("Vy"));
            return pos;
        }

        internal bool IsObstructed() {
            return this.obstructions.Count() > 0;
        }
    }
}
