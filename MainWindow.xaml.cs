using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace DesktopInfo {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public event EventHandler Refresh;

        public MainWindow() {
            InitializeComponent();
        }

        /// <summary>
        /// Moves window between the desktop background and desktop icons
        /// </summary>
        // Based off of https://github.com/HatsuneMiku3939/3939LiveWallpaer/blob/master/3939LiveWallpaper/Program.cs
        private void MoveUnder() {
            // Pop-under desktop icons?
            IntPtr progman = NativeMethods.FindWindow("ProgMan", null);
            IntPtr result = IntPtr.Zero;

            // Sent WM_SPAWN_WORKER to ProgMan
            NativeMethods.SendMessageTimeout(progman, 0x052C, new IntPtr(0), IntPtr.Zero, NativeMethods.SendMessageTimeoutFlags.Normal, 1000, out result);

            IntPtr workerW = IntPtr.Zero;

            // We enumerate all Windows, until we find one, that has the SHELLDLL_DefView as a child.
            // If we found that window, we take its next sibling and assign it to workerw.
            NativeMethods.EnumWindows(new NativeMethods.EnumWindowsProc((topHandle, topParamHandle) => {
                IntPtr p = NativeMethods.FindWindowEx(topHandle, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);

                if (p != IntPtr.Zero) {
                    workerW = NativeMethods.FindWindowEx(IntPtr.Zero, topHandle, "WorkerW", IntPtr.Zero);
                }

                return true;
            }), IntPtr.Zero);

            WindowInteropHelper helper = new WindowInteropHelper(this);

            NativeMethods.SetParent(helper.Handle, workerW);

            // Internally, DesktopInfo takes up the whole of the primary screen's desktop.
            // XAML is used to position the semi-transparent grid
            this.Width = Screen.PrimaryScreen.WorkingArea.Width;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Left = Screen.PrimaryScreen.WorkingArea.Left;
            this.Top = Screen.PrimaryScreen.WorkingArea.Top;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // TODO: Move to settings
            //IList<Tuple<string, string, string>> settingStrings = new List<Tuple<string, string, string>>() {
            //    new Tuple<string, string, string>("helpPhone", "Help Phone", "#PHONENUMBER")
            //};

            //foreach (Tuple<string, string, string> settingString in settingStrings) {
            //    InfoProviders.Add(new InfoProvider(settingString.Item1, settingString.Item2, settingString.Item3));
            //}

            //IList<Tuple<string, string, string>> environmentStrings = new List<Tuple<string, string, string>>() {
            //    new Tuple<string, string, string>("path", "Path VAR", "PATH")
            //};

            //foreach (Tuple<string, string, string> environmentString in environmentStrings) {
            //    InfoProviders.Add(new PolledInfoProvider(environmentString.Item1, environmentString.Item2, () => Environment.GetEnvironmentVariable(environmentString.Item3)));
            //}

            IList<IInfoProvider> infoProviders = new[] {
                "uptime",
                "imageDate",
                null,
                "username",
                null,
                "ipAddresses",
                "volumes",
            }.Select(x => InfoProviders.GetByName(x)).ToList();

            // Init data
            this.CreateInfoItemsView(infoProviders);

            this.DoRefresh();
            this.MoveUnder();

            // Auto-update info items
            Timer t1 = new Timer() {
                Interval = 5000 // TODO: Settings
            };

            t1.Tick += (object sender2, EventArgs e2) => {
                this.DoRefresh();
            };

            t1.Start();
        }

        /// <summary>
        /// Helper method to generate the grid rows needed.
        /// Also creates a tuple list to hold the value holders and the value factories
        /// </summary>
        /// <param name="infos"></param>
        private void CreateInfoItemsView(IEnumerable<IInfoProvider> infos) {
            int row = this.mainGrid.RowDefinitions.Count;

            foreach (IInfoProvider item in infos) {
                bool isSpacer = item == null;

                this.mainGrid.RowDefinitions.Add(new RowDefinition() {
                    Height = isSpacer ? new GridLength(15) : GridLength.Auto
                });

                if (!isSpacer) {
                    // Attach polled providers to instance
                    if (item is PolledInfoProvider) {
                        ((PolledInfoProvider)item).AttachWindow(this);
                    }

                    // Create name text block
                    TextBlock tbName = new TextBlock {
                        Style = (Style)this.FindResource("InfoLabelStyle"),
                        Text = item.Label + ":"
                    };

                    tbName.SetValue(Grid.RowProperty, row);
                    tbName.SetValue(Grid.ColumnProperty, 0);

                    mainGrid.Children.Add(tbName);

                    // Create value text block
                    TextBlock tbValue = new TextBlock() {
                        Style = (Style)this.FindResource("InfoValueStyle")
                    };

                    tbValue.SetValue(Grid.RowProperty, row);
                    tbValue.SetValue(Grid.ColumnProperty, 1);

                    mainGrid.Children.Add(tbValue);

                    // Assign factory to textblock, using data binding
                    System.Windows.Data.Binding binding = new System.Windows.Data.Binding("Value") {
                        Source = item,
                        Mode = System.Windows.Data.BindingMode.OneWay,
                        UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
                    };

                    tbValue.SetBinding(TextBlock.TextProperty, binding);
                }

                row++;
            }
        }

        /// <summary>
        /// Forces a refresh of all the info items displayed
        /// </summary>
        public void DoRefresh() {
            this.lblComputerName.Text = Environment.MachineName;

            this.Refresh?.Invoke(this, new EventArgs());
        }
    }
}
