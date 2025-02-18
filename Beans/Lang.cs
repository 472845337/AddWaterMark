using PropertyChanged;
using System.Collections.Generic;
using System.Windows;

namespace AddWaterMark.Beans {
    [AddINotifyPropertyChangedInterface]
    class Lang {
        public string Name { get; set; }
        public string Value { get; set; }

        public static string Find(string key) {
            return Application.Current.TryFindResource(key) as string;
        }

        public static bool ExistLang(ICollection<Lang> langs, string langValue) {
            bool result = false;
            foreach(Lang lang in langs) {
                if (lang.Value.Equals(langValue)) {
                    result = true;
                    break;
                }
            }
            return result;
        }
    }
}
