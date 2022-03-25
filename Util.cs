using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyCap {

    /// <summary>
    /// Collection of static utility methods.
    /// </summary>
    static internal class Util {

        /// <summary>
        /// Determines whether any element of <paramref name="targets"/> are contained within the <paramref name="source"/>.
        /// </summary>
        /// <returns>
        /// True if <paramref name="source"/> contains any elements of <paramref name="targets"/>
        /// </returns>
        public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> targets) {
            foreach (var item in source) {
                if (targets.Contains(item)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether all elements of <paramref name="targets"/> are contained in the <paramref name="source"/>.
        /// </summary>
        /// <returns>
        /// True if <paramref name="source"/> contains all of <paramref name="targets"/>, otherwise false.
        /// </returns>
        public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> targets) {
            return !targets.Except(source).Any();
        }
    }
}
