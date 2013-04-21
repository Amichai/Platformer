using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Platformer {
    public class Perspective {
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get ; private set; }

        public int UniverseWidth { get; private set; }
        public int UniverseHeight { get; private set; }

        public void SetScreenSize(int w, int h) {
            this.ScreenWidth = w;
            this.ScreenHeight = h;
        }

        public void SetUniverseSize(int w, int h) {
            this.UniverseWidth = w;
            this.UniverseHeight = h;
        }

        private int xoffset { get; set; }
        private int yoffset { get; set; }

        internal double ScreenX(double x) {
            return x + xoffset;
        }

        internal double ScreenY(double y) {
            return y + yoffset;
        }

        internal bool IsOffTheBoard(Sprite s) {
            return (s.Right < 0 || s.Top > UniverseHeight || s.Left > UniverseWidth || s.Bottom < 0);
        }

        internal XElement Serialize() {
            XElement root = new XElement("Perspective");
            root.Add(new XAttribute("ScreenWidth", this.ScreenWidth));
            root.Add(new XAttribute("ScreenHeight", this.ScreenHeight));
            root.Add(new XAttribute("UniverseWidth", this.UniverseWidth));
            root.Add(new XAttribute("UniverseHeight", this.UniverseHeight));
            root.Add(new XAttribute("xOffset", this.xoffset));
            root.Add(new XAttribute("yOffset", this.yoffset));
            return root;
        }

        internal static Perspective Deserialize(XElement root) {
            Perspective p = new Perspective();
            p.ScreenWidth = int.Parse(root.Attribute("ScreenWidth").Value);
            p.ScreenHeight = int.Parse(root.Attribute("ScreenHeight").Value);
            p.UniverseWidth = int.Parse(root.Attribute("UniverseWidth").Value);
            p.UniverseHeight = int.Parse(root.Attribute("UniverseHeight").Value);
            p.xoffset = int.Parse(root.Attribute("xOffset").Value);
            p.yoffset = int.Parse(root.Attribute("yOffset").Value);
            return p;
        }
    }
}
