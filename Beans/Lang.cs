using System.Collections.Generic;
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

        public static Dictionary<string, string> LangNameDic(string key) {
            var lang = Application.Current.TryFindResource(key);
            string[] langStringList = lang as string[];
            Dictionary<string, string> langNameDic = new Dictionary<string, string>();
            foreach(string langString in langStringList) {
                string[] langArray = langString.Split('=');
                langNameDic.Add(langArray[0], langArray[1]);
            }
            return langNameDic;
        }
    }
}
