
using PropertyChanged;
using System;

namespace AddWaterMark.DataBase {
    /// <summary>
    /// 快捷对象公共的属性
    /// </summary>
    /// 
    [Serializable]
    [AddINotifyPropertyChangedInterface]
    public abstract class TableData {
        // section
        [DoNotNotify]
        [TableParam(true, "id", "INTEGER", true)]
        public long? Id { get; set; }

    }
}
