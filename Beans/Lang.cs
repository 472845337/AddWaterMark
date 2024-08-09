using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PropertyChanged;

namespace AddWaterMark.Beans {
    [AddINotifyPropertyChangedInterface]
    class Lang {
        public string Name { get; set; }
        public string Value { get; set; }

        public static string Find(string key) {
            return Application.Current.TryFindResource(key) as string;
        }
    }
}
