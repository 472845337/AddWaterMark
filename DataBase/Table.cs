using System;

namespace AddWaterMark.DataBase {
    /// <summary>
    /// 数据表基础对象
    /// </summary>
    internal class Table : Attribute {

        public Table(string tableName) {
            TableName = tableName;
        }

        public string TableName { get; }
    }
}
