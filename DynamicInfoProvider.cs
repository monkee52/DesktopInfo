using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesktopInfo {
    public class DynamicInfoProvider : InfoProvider {

        public DynamicInfoProvider(string name, string label, GetValueHandler getValueHandler) {
            this.Name = name;
            this.Label = label;

            this.Value = getValueHandler();
        }
    }
}
