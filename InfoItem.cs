using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DesktopInfo {
    public delegate string InfoItemGetValueHandler();
    public delegate InfoItem InfoItemFactory(string name, InfoItemGetValueHandler getValue);

    public class InfoItem : INotifyPropertyChanged {
        public string Name { get; private set; }

        private InfoItemGetValueHandler getValue;

        public string Value => this.getValue();

        public event PropertyChangedEventHandler PropertyChanged;

        protected internal InfoItem(MainWindow window, string name, InfoItemGetValueHandler getValue) {
            this.Name = name;
            this.getValue = getValue;

            // Propogate window's refresh down to each info item, and then trigger the DataBindings as a result
            window.Refresh += (sender, e) => {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Value)));
            };
        }
    }
}
