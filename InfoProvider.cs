using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DesktopInfo {
    public class InfoProvider : IInfoProvider {
        public string Name { get; protected set; }
        public string Label { get; protected set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _value;
        public virtual string Value {
            get {
                return this._value;
            }
            set {
                if (value != this._value) {
                    this._value = value;

                    this.InvokePropertyChanged();
                }
            }
        }

        protected InfoProvider() {

        }

        public InfoProvider(string name, string label, string value) {
            this.Name = name;
            this.Label = label;
            this.Value = value;
        }

        protected void InvokePropertyChanged() {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Value)));
        }
    }
}
