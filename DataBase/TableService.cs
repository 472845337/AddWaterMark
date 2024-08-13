
using AddWaterMark.Config;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Reflection;
using System.Text;

namespace AddWaterMark.DataBase {
    /// <summary>
    /// 操作http发送数据
    /// </summary>
    public class TableService<T> where T : TableData {
        private static readonly SqLiteHelper SqlLiteHelper = SqLiteFactory.GetSqLiteHelper(Configs.AppStartPath + DbConstants.DbName, DbConstants.DbPassword);

        public TableService() {
            InitTable();
        }

        private void InitTable() {
            string tableName = GetTableName();
            // 判断表是否存在
            if (!SqlLiteHelper.TableExists(tableName)) {
                // 创建表
                CreateTable();
            } else {
                // SQLLite数据库添加新增的列
                List<string> addColumnList = new List<string>();
                List<string> nameList = GetTableParam();
                // 获取自己和父类的属性
                var infos = typeof(T).GetProperties();
                foreach (var info in infos) {
                    var tableParam = GetAttributeByProperty<TableParam>(info);
                    if (null == tableParam) {
                        continue;
                    }
                    if (!nameList.Contains(tableParam.param)) {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(tableParam.param).Append(" ").Append(tableParam.type);
                        if (tableParam.isKey) {
                            sb.Append(" PRIMARY KEY");
                        }
                        if (tableParam.autoIncrement) {
                            sb.Append(" AUTOINCREMENT");
                        }
                        addColumnList.Add(sb.ToString());
                    }
                }
                if (addColumnList.Count > 0) {
                    foreach (string addColumn in addColumnList) {
                        SqlLiteHelper.ExecuteNonQuery($"ALTER TABLE {tableName} ADD COLUMN {addColumn}", null);
                    }
                }
            }
        }

        /// <summary>
        /// 插入数据并返回ID
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public void Insert(T t) {
            long id = SqlLiteHelper.InsertData(GetTableName(), GetParams(t, true));
            t.Id = id;
        }

        /// <summary>
        /// 清除所有数据
        /// </summary>
        internal void Clear() {
            // 清空所有的数据
            SqlLiteHelper.ExecuteNonQuery($"DELETE FROM {GetTableName()}", null);
        }

        /// <summary>
        /// 删除某个数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Delete(long? id) {
            if (null == id) {
                return 0;
            } else {
                return SqlLiteHelper.ExecuteNonQuery($"DELETE FROM {GetTableName()} where id=@id", new[] { new SQLiteParameter("id", id) });
            }
        }

        public void Vacuum() {
            SqlLiteHelper.Vacuum();
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public int Update(T t, bool hasNull = false) {
            List<SQLiteParameter> paramList = new List<SQLiteParameter>();
            SQLiteParameter idParame = new SQLiteParameter("id", t.Id);
            paramList.Add(idParame);
            return SqlLiteHelper.Update(GetTableName(), GetParams(t, false, hasNull), "id=@id", paramList.ToArray());
        }

        /// <summary>
        /// 根据对象查询匹配的数据
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        public List<T> SelectList(T queryModel) {
            List<T> list = new List<T>();
            // sql拼装
            List<SQLiteParameter> paramList = new List<SQLiteParameter>();
            string paramSql = string.Empty;
            string whereSql = string.Empty;
            StringBuilder selectSql = new StringBuilder();
            InitSql(queryModel, ref paramSql, ref whereSql, ref paramList);
            selectSql.Append("SELECT ").Append(paramSql).Append(" FROM ").Append(GetTableName());
            if (!string.IsNullOrEmpty(whereSql)) {
                selectSql.Append(" WHERE ").Append(whereSql);
            }
            // 执行查询
            SQLiteDataReader reader = SqlLiteHelper.ExecuteReader(selectSql.ToString(), paramList.ToArray());
            if (null != reader) {
                // 数据拼装
                while (reader.Read()) {
                    T t = Activator.CreateInstance<T>();
                    PropertyInfo[] infos = typeof(T).GetProperties();
                    foreach (var info in infos) {
                        TableParam tableParam = GetAttributeByProperty<TableParam>(info);
                        if (null == tableParam) {
                            continue;
                        }
                        object obj = reader[tableParam.param];
                        if (null != obj && DBNull.Value != obj) {
                            info.SetValue(t, obj);
                        }
                    }

                    list.Add(t);
                }
            }
            return list;
        }

        private void InitSql(T t, ref string paramSql, ref string whereSql, ref List<SQLiteParameter> paramList) {
            var infos = typeof(T).GetProperties();
            paramList = new List<SQLiteParameter>();
            StringBuilder paramSb = new StringBuilder();
            StringBuilder whereSb = new StringBuilder();
            foreach (var property in infos) {
                var tableParam = GetAttributeByProperty<TableParam>(property);
                if (null == tableParam) {
                    continue;
                }
                if (null != t && null != property.GetValue(t, null)) {
                    if (whereSb.Length > 0) {
                        whereSb.Append(" AND ");
                    }
                    whereSb.Append(tableParam.param).Append("=@").Append(property.Name);

                    SQLiteParameter param = new SQLiteParameter(property.Name, property.GetValue(t, null));
                    paramList.Add(param);
                }
                if (paramSb.Length > 0) {
                    paramSb.Append(", ");
                }
                paramSb.Append(tableParam.param);
            }
            paramSql = paramSb.ToString();
            whereSql = whereSql.ToString();
        }

        /// <summary>
        /// 创建表
        /// </summary>
        private void CreateTable() {
            // 获取自己和父类的属性
            var infos = typeof(T).GetProperties();
            var insertSql = new StringBuilder("create table ").Append(GetTableName()).Append(" (");
            var sb = new StringBuilder();
            foreach (var info in infos) {
                var tableParam = GetAttributeByProperty<TableParam>(info);
                if (null == tableParam) {
                    continue;
                }
                if (sb.Length > 0) {
                    sb.Append(", ");
                }
                sb.Append(tableParam.param).Append(" ").Append(tableParam.type);
                if (tableParam.isKey) {
                    sb.Append(" PRIMARY KEY");
                }
                if (tableParam.autoIncrement) {
                    sb.Append(" AUTOINCREMENT");
                }
            }
            insertSql.Append(sb).Append(")");
            SqlLiteHelper.ExecuteNonQuery(insertSql.ToString(), null);
        }

        private List<string> GetTableParam() {
            List<string> nameList = new List<string>();
            string sql = $"PRAGMA table_info('{GetTableName()}')";
            using (SQLiteDataReader reader = SqlLiteHelper.ExecuteReader(sql, null)) {
                while (reader.Read()) {
                    nameList.Add((string)reader["name"]);
                }
            }
            return nameList;
        }

        private string GetTableName() {
            string tableName;
            var tableAttrs = typeof(T).GetCustomAttributes(typeof(Table), true);
            if (tableAttrs.Length > 0) {
                var table = (Table)tableAttrs[0];
                tableName = table.TableName;
            } else {
                throw new Exception(typeof(T) + " class not set Table attributes");
            }
            return tableName;
        }

        /// <summary>
        /// 获取属性对应的参数字典项,自动排除主键属性 比如ID
        /// </summary>
        /// <param name="t">:TableData</param>
        /// <param name="key">是否包含主键，insert填true，update填false</param>
        /// <returns></returns>
        private Dictionary<string, object> GetParams(T t, bool key, bool hasNull = false) {
            if (null == t) return null;
            var dict = new Dictionary<string, object>();
            // 遍历所有的属性
            var infos = typeof(T).GetProperties();
            foreach (PropertyInfo info in infos) {
                var tableParam = GetAttributeByProperty<TableParam>(info);
                if (null == tableParam) {
                    continue;
                }
                if (tableParam.isKey) {
                    if (!key) {
                        continue;
                    }
                }
                object value = info.GetValue(t, null);
                if (!hasNull && null == value) {
                    continue;
                }
                dict.Add(tableParam.param, value);
            }
            return dict;
        }

        private static TA GetAttributeByProperty<TA>(PropertyInfo propertyInfo) where TA : Attribute {
            var attributes = propertyInfo.GetCustomAttributes(typeof(TA), true);
            TA attribute = null;
            if (attributes.Length > 0) {
                attribute = (TA)attributes[0];
            }
            return attribute;
        }
    }
}
