using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Platformer {
    public class Perspective {
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }

        public int UniverseWidth { get; set; }
        public int UniverseHeight { get; set; }

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
            throw new NotImplementedException();
        }

        internal static Perspective Deserialize(XElement a) {
            throw new NotImplementedException();
        }
    }
}
