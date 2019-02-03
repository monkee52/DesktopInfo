using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DesktopInfo {
    public interface IInfoProvider : INotifyPropertyChanged {
        string Name { get; }
        string Label { get; }
        string Value { get; }
    }
}
