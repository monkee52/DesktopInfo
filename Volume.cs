using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;

namespace DesktopInfo {
    /// <summary>
    /// Helper class to represent a WMI Win32_Volume's important info
    /// </summary>
    public class Volume {
        public string DriveLetter { get; protected set; }
        public long FreeSpace { get; protected set; }
        public long TotalSpace { get; protected set; }
        public string FileSystem { get; protected set; }

        /// <summary>
        /// Create a Volume from a ManagementObject
        /// </summary>
        /// <param name="volume">A ManagementObject that represents a Win32_Volume</param>
        public Volume(ManagementObject volume) {
            // Type check
            if (volume.ClassPath.ClassName != "Win32_Volume") {
                throw new ArgumentException("Parameter " + nameof(volume) + " is not of type 'Win32_Volume'.");
            }

            // Copy and cast properties
            this.DriveLetter = (string)volume.Properties["DriveLetter"].Value;
            this.FreeSpace = (long)(ulong)volume.Properties["FreeSpace"].Value;
            this.TotalSpace = (long)(ulong)volume.Properties["Capacity"].Value;
            this.FileSystem = (string)volume.Properties["FileSystem"].Value;
        }
    }
}
