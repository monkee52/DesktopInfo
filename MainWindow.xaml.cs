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
    public delegate string InfoListItemGetValueHandler();
    public delegate InfoListItem InfoListItemFactory(string name, InfoListItemGetValueHandler getValue);

    public class InfoListItem : INotifyPropertyChanged
    {
        public string Name { get; private set; }

        private InfoListItemGetValueHandler getValue;

        public string Value => this.getValue();

        public event PropertyChangedEventHandler PropertyChanged;

        internal InfoListItem(MainWindow window, string name, InfoListItemGetValueHandler getValue) {
            this.Name = name;
            this.getValue = getValue;

            window.Refresh += (sender, e) => {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Value)));
            };
        }
    }

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
        private void MoveUnder() {
            // Pop-under desktop icons?
            IntPtr progman = NativeMethods.FindWindow("ProgMan", null);
            IntPtr result = IntPtr.Zero;

            // Sent WM_SPAWN_WORKER to ProgMan
            NativeMethods.SendMessageTimeout(progman, 0x052C, new IntPtr(0), IntPtr.Zero, NativeMethods.SendMessageTimeoutFlags.Normal, 1000, out result);

            IntPtr workerW = IntPtr.Zero;

            // We enumerate all Windows, until we find one, that has the SHELLDLL_DefView
            // as a child.
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

            this.Width = Screen.PrimaryScreen.WorkingArea.Width;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Left = Screen.PrimaryScreen.WorkingArea.Left;
            this.Top = Screen.PrimaryScreen.WorkingArea.Top;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // Init data
            this.CreateInfoList(InfoProperties.CreateInfoItems((name, getValue) => new InfoListItem(this, name, getValue)));

            this.DoRefresh();
            this.MoveUnder();

            // Auto-update
            Timer t1 = new Timer() {
                Interval = Properties.Settings.Default.UpdateInterval
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
        private void CreateInfoList(IList<InfoListItem> infos) {
            int row = this.mainGrid.RowDefinitions.Count;

            foreach (InfoListItem item in infos) {
                bool isSpacer = item == null;

                this.mainGrid.RowDefinitions.Add(new RowDefinition() {
                    Height = isSpacer ? new GridLength(15) : GridLength.Auto
                });

                if (!isSpacer) {
                    // Create name text block
                    TextBlock tbName = new TextBlock {
                        VerticalAlignment = VerticalAlignment.Top,
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                        FontFamily = new FontFamily("Segoe UI"),
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        TextAlignment = TextAlignment.Right,
                        Margin = new Thickness(0, 2, 2, 2),

                        Text = item.Name + ":"
                    };

                    tbName.SetValue(Grid.RowProperty, row);
                    tbName.SetValue(Grid.ColumnProperty, 0);

                    mainGrid.Children.Add(tbName);

                    // Create value text block
                    TextBlock tbValue = new TextBlock() {
                        VerticalAlignment = VerticalAlignment.Top,
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                        FontFamily = new FontFamily("Segoe UI"),
                        FontSize = 14,
                        TextAlignment = TextAlignment.Left,
                        Margin = new Thickness(2, 2, 0, 2)
                    };

                    tbValue.SetValue(Grid.RowProperty, row);
                    tbValue.SetValue(Grid.ColumnProperty, 1);

                    mainGrid.Children.Add(tbValue);

                    // Assign factory to textblock
                    System.Windows.Data.Binding binding = new System.Windows.Data.Binding("Value");

                    binding.Source = item;
                    binding.Mode = System.Windows.Data.BindingMode.OneWay;
                    binding.UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged;

                    tbValue.SetBinding(TextBlock.TextProperty, binding);
                }

                row++;
            }
        }

        /// <summary>
        /// Populates grid rows
        /// </summary>
        public void DoRefresh() {
            this.lblComputerName.Text = Environment.MachineName;

            this.Refresh?.Invoke(this, new EventArgs());
        }
    }
}
