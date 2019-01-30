using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;

namespace DesktopInfo {
    public static class InfoProperties {
        public static IList<InfoListItem> CreateInfoItems(InfoListItemFactory CreateInfoListItem) {
            return new List<InfoListItem>() {
                CreateInfoListItem("Uptime", () => TimeSpan.FromMilliseconds(NativeMethods.GetTickCount64()).ToString("d\\.hh\\:mm\\:ss")),

                CreateInfoListItem("Image Date", () => {
                    using (UserPrincipal identity = UserPrincipal.Current) {
                        return identity.LastPasswordSet?.ToString("d");
                    }
                }),

                null,

                CreateInfoListItem("Username", () => WindowsIdentity.GetCurrent().Name),

                null,

                CreateInfoListItem("IP Address", () => String.Join(Environment.NewLine, new ManagementObjectSearcher(
                        "root\\StandardCimv2",
                        "SELECT InterfaceIndex FROM MSFT_NetAdapter WHERE MediaConnectState = 1"
                    ).Get().OfType<ManagementObject>().Select(i => (uint)i.Properties["InterfaceIndex"].Value).SelectMany(ifIndex => new ManagementObjectSearcher(
                        "root\\StandardCimv2",
                        String.Format("SELECT IPAddress FROM MSFT_NetIPAddress WHERE InterfaceIndex = {0} AND AddressFamily = 2", ifIndex)
                    ).Get().OfType<ManagementObject>().Select(i => i.Properties["IPAddress"].Value.ToString()))
                )),

                CreateInfoListItem("Volumes", () => String.Join(Environment.NewLine, new ManagementObjectSearcher(
                        "root\\cimv2",
                        "SELECT DriveLetter, Capacity, FreeSpace, FileSystem FROM Win32_Volume WHERE DriveType = 2 OR DriveType = 3"
                    ).Get().OfType<ManagementObject>().Where(v => v.Properties["DriveLetter"].Value != null && v.Properties["FileSystem"].Value != null).Select(v => new {
                        DriveLetter = (string)v.Properties["DriveLetter"].Value,
                        FreeSpace = (long)(ulong)v.Properties["FreeSpace"].Value,
                        TotalSpace = (long)(ulong)v.Properties["Capacity"].Value,
                        FileSystem = (string)v.Properties["FileSystem"].Value
                    }).OrderBy(v => v.DriveLetter).Select(v => String.Format("{0} {1} Free; {2} Total; {3:f2}% Used; {4}",
                        v.DriveLetter,
                        Utilities.FormatFileSize(v.FreeSpace),
                        Utilities.FormatFileSize(v.TotalSpace),
                        (1.0d - v.FreeSpace / (double)v.TotalSpace) * 100d,
                        v.FileSystem
                    ))
                ))
            };
        }
    }
}
