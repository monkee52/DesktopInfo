using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesktopInfo {
    public static class Utilities {
        /// <summary>
        /// Byte size formatter
        /// </summary>
        /// <param name="size">The size, in bytes</param>
        /// <param name="isMetric">Whether the exponent calculation should use 1000 as the base, instead of 1024</param>
        /// <returns>A formatted string for the input byte size.</returns>
        public static string FormatFileSize(long size, bool isMetric = true) {
            if (size == 0) {
                return "0 B";
            }

            int baseVal = 1024;
            string[] suffixes = new[] { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };

            if (isMetric) {
                baseVal = 1000;
                suffixes = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            }

            int exponent = (int)(Math.Log(Math.Abs(size)) / Math.Log(baseVal));

            return String.Format("{0:f2} {1}", size / Math.Pow(baseVal, exponent), suffixes[exponent]);
        }
    }
}
