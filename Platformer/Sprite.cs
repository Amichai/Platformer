using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;

namespace Platformer {
    public class Sprite {
        public Sprite(Position state) {
            this.lastUpdate = null;
            this.State = state;
            this.Width = 60;
            this.Height = 20;
            this.IsSolid = true;
            this.DestroyOffscreen = true;
            this.DeleteMe = false;
        }

        public double ScreenX() {
            return GameInstance.Inst.GamePerspective.ScreenX(this.State.X);
        }

        public double ScreenY() {
            return GameInstance.Inst.GamePerspective.ScreenY(this.State.Y);
        }

        public double Left {
            get {
                return State.X;
            }
        }

        public double Right {
            get {
                return State.X + Width;
            }
        }

        public double Top {
            get {
                return State.Y;
            }
        }

        public double Bottom {
            get {
                return State.Y + Height;
            }
        }

        public bool DestroyOffscreen { get; set; }
        public bool IsSolid { get; set; }

        public Brush Brush { get; set; }

        public Position State { get; set; }
        private TimeSpan? lastUpdate;

        public string Name { get; set; }

        /// <summary>
        /// Return true if you want to update the UI
        /// </summary>
        public bool Update(TimeSpan currentTime) {
            if (lastUpdate == null) {
                lastUpdate = currentTime;
            }
            var elapsed = currentTime - lastUpdate.Value;
            State.Update(elapsed);
            lastUpdate = currentTime;
            ///Update the collision manager

//            CollisionManager[this].Test(State);

            ///Set of sprites to check against
            ///Test for collisions  
            ///Handle collision

            ///This means we should update the UI
            return true;
            //Check for collisions, events, etc
        }

        public double Width { get; set; }

        public double Height { get; set; }

        public Sprite Clone() {
            return Sprite.Deserialize(this.Serialize());
        }

        public bool DeleteMe { get; set; }

        internal void RaiseOffTheBoard() {
            if (this.DestroyOffscreen) {
                this.DeleteMe = true;
            }
        }

        public XElement Serialize() {
            XElement root = new XElement("Sprite");
            root.Add(new XAttribute("Name", this.Name));
            root.Add(new XAttribute("Width", this.Width));
            root.Add(new XAttribute("Height", this.Height));
            root.Add(new XAttribute("Brush", this.Brush));
            root.Add(new XAttribute("DestroyOffScreen", this.DestroyOffscreen));
            root.Add(new XAttribute("IsSolid", this.IsSolid));
            root.Add(this.State.Serialize());
            return root;
        }

        public static Sprite Deserialize(XElement root) {
            var state = Position.Deserialize(root.Elements().First());
            Sprite spr = new Sprite(state);
            spr.Name = (string)root.Attribute("Name");
            spr.Width = double.Parse((string)root.Attribute("Width"));
            spr.Height = double.Parse((string)root.Attribute("Height"));
            spr.Brush = new BrushConverter().ConvertFromString((string)root.Attribute("Brush")) as Brush;
            spr.DestroyOffscreen = bool.Parse((string)root.Attribute("DestroyOffScreen"));
            spr.IsSolid = bool.Parse((string)root.Attribute("IsSolid"));
            return spr;
        }
    }
}
