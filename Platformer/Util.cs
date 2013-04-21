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

        public static HashSet<T> RemoveAll<T>(this HashSet<T> list, IEnumerable<T> remove) {
            foreach (var a in remove) {
                list.Remove(a);
            }
            return list;
        }

        public static HashSet<T> AddRange<T>(this HashSet<T> list, IEnumerable<T> toAdd) {
            foreach (var a in toAdd) {
                list.Add(a);
            }
            return list;
        }

        public static HashSet<T> Overlap<T>(this IEnumerable<T> a, IEnumerable<T> b) {
            IEnumerable<T> smaller, larger;
            if (a.Count() < b.Count()) {
                smaller = a;
                larger = b;
            } else {
                smaller = b;
                larger = a;
            }
            HashSet<T> toReturn = new HashSet<T>();
            foreach (var e in smaller) {
                if (larger.Contains(e)) {
                    toReturn.Add(e);
                }
            }

            return toReturn;
        }

        public static HashSet<T> MyUnion<T>(this HashSet<T> a, List<T> b) {
            IEnumerable<T> smaller, larger;
            if (a.Count() < b.Count()) {
                smaller = a;
                larger = b;
            } else {
                smaller = b;
                larger = a;
            }
            HashSet<T> toReturn = new HashSet<T>();
            foreach (var e in smaller) {
                if (!toReturn.Contains(e)) {
                    toReturn.Add(e);
                }
            }
            return toReturn;
        }

        public static int Round(this double val){
            return (int)Math.Round(val);
        }
    }
}
