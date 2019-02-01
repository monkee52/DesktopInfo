﻿using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;

namespace DesktopInfo {
    public class InfoItems {
        private static IDictionary<string, InfoItemGetValueHandler> items = new Dictionary<string, InfoItemGetValueHandler>() {
            ["Uptime"] = () => TimeSpan.FromMilliseconds(NativeMethods.GetTickCount64()).ToString("d\\.hh\\:mm\\:ss"),
            ["Image Date"] = () => {
                using (UserPrincipal identity = UserPrincipal.Current) {
                    return identity.LastPasswordSet?.ToString("d");
                }
            },
            ["Username"] = () => WindowsIdentity.GetCurrent().Name,
            ["IP Addresses"] = () => {
                // Use WMI to get each connected interface's index
                IEnumerable<uint> interfaceIndices = new ManagementObjectSearcher(
                    "root\\StandardCimv2",
                    "SELECT InterfaceIndex FROM MSFT_NetAdapter WHERE MediaConnectState = 1"
                ).Get().OfType<ManagementObject>().Select(i => (uint)i.Properties["InterfaceIndex"].Value);
                    
                // Use WMI to get each connected interface's IPv4 Address
                IEnumerable<string> ipAddresses = interfaceIndices.SelectMany(ifIndex => new ManagementObjectSearcher(
                    "root\\StandardCimv2",
                    String.Format("SELECT IPAddress FROM MSFT_NetIPAddress WHERE InterfaceIndex = {0} AND AddressFamily = 2", ifIndex)
                ).Get().OfType<ManagementObject>().Select(i => i.Properties["IPAddress"].Value.ToString()));

                return String.Join(Environment.NewLine, ipAddresses);
            },
            ["Volumes"] = () => {
                // Use WMI to get all fixed volumes, ordered by drive letter
                IEnumerable<Volume> volumes = new ManagementObjectSearcher(
                    "root\\cimv2",
                    "SELECT DriveLetter, Capacity, FreeSpace, FileSystem FROM Win32_Volume WHERE DriveType = 2 OR DriveType = 3"
                ).Get().OfType<ManagementObject>().Where(v => v.Properties["DriveLetter"].Value != null && v.Properties["FileSystem"].Value != null).Select(v => new Volume(v)).OrderBy(v => v.DriveLetter);

                // Format each volume as a string '{driveLetter} {freeSpace} Free; {totalSpace} Total; {percentUsed} Used; {fileSystem}'
                IEnumerable<string> volumeStrings = volumes.Select(v => String.Format("{0} {1} Free; {2} Total; {3:f2}% Used; {4}",
                    v.DriveLetter,
                    Utilities.FormatFileSize(v.FreeSpace),
                    Utilities.FormatFileSize(v.TotalSpace),
                    (1.0d - v.FreeSpace / (double)v.TotalSpace) * 100d,
                    v.FileSystem
                ));

                return String.Join(Environment.NewLine, volumeStrings);
            }
        };

        private InfoItemFactory createInfoListItem;
        
        public InfoItem GetByName(string name) {
            if (name == null || !items.ContainsKey(name)) {
                return null;
            }

            return this.createInfoListItem(name, items[name]);
        }

        // Allow extensibility?
        public static void Add(string name, InfoItemGetValueHandler getValueHandler) {
            items[name] = getValueHandler;
        }

        public InfoItems(InfoItemFactory createInfoListItem) {
            this.createInfoListItem = createInfoListItem;
        }
    }
}
