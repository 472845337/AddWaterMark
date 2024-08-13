using System;

namespace AddWaterMark.DataBase {
    internal class TableParam : Attribute {
        public readonly bool isKey;
        public readonly bool autoIncrement;
        public readonly string param;
        public readonly string type;

        public TableParam(string param, string type) {
            this.param = param;
            this.type = type;
        }

        public TableParam(bool isKey, string param, string type, bool autoIncrement = false) {
            this.isKey = isKey;
            this.param = param;
            this.type = type;
            this.autoIncrement = autoIncrement;
        }
    }
}
