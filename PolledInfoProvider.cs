using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DesktopInfo {
    public delegate string GetValueHandler();

    public class PolledInfoProvider : InfoProvider {
        private GetValueHandler getValue;

        public override string Value => this.getValue();

        public PolledInfoProvider(string name, string label, GetValueHandler getValueHandler) {
            this.Name = name;
            this.Label = label;

            this.getValue = getValueHandler;
        }

        private void HandleRefresh(object sender, EventArgs e) {
            this.InvokePropertyChanged();
        }

        public void AttachWindow(MainWindow window) {
            window.Refresh += this.HandleRefresh;
        }
    }
}
