using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer {
    public static class Util {
        public static double Sqrd(this double val) {
            return Math.Pow(val, 2);
        }
        public static double Sqrd(this int val) {
            return Math.Pow(val, 2);
        }

        public static int? IndexAt<T>(this IEnumerable<T> list, T element) {
            int i = 0;
            foreach (var item in list)
	        {
		        if(item.Equals(element)){
                    return i;
                }
                i++;
	        }
            return null;
        }
    }
}
