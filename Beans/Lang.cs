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

        public static List<Lang> FindLangList() {
            var langArray = Application.Current.TryFindResource("LangArray");
            string[] langStringList = langArray as string[];
            List<Lang> langList = new List<Lang>();
            foreach (string langString in langStringList) {
                string[] nameValueArray = langString.Split('=');
                langList.Add(new Lang { Name = nameValueArray[1], Value = nameValueArray[0] });
            }
            return langList;
        }

        public static Dictionary<string, string> LangNameDic() {
            Dictionary<string, string> langNameDic = new Dictionary<string, string>();
            foreach (Lang lang in FindLangList()) {
                langNameDic.Add(lang.Value, lang.Name);
            }
            return langNameDic;
        }
    }
}
