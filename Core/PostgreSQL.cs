using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using System.Data;
using System.IO;

namespace Core
{
    public class PostgreSQL : DBModel
    {
        public PostgreSQL(string connectString)
        {
            this.ConnectString = connectString;
        }

        public override bool GetStatue()
        {
            NpgsqlConnection connection = new NpgsqlConnection(this.ConnectString);
            try
            {
                connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
        }


        /// <summary>
        /// 开始生成三层任务
        /// </summary>
        public override void BeginTask()
        {
            //获取所有数据表
            string str_tableNames = "select relname as tablename, cast(obj_description(relfilenode,'pg_class') as varchar) as comment from pg_class a left join  pg_namespace  b on a.relnamespace =b.oid where b.nspname = 'public' and a .reltype>0 and relkind ='r' order by a.relname";
            NpgsqlConnection conn = new NpgsqlConnection(this.ConnectString);
            NpgsqlDataAdapter adTables = new NpgsqlDataAdapter(str_tableNames, conn);
            DataSet dsTables = new System.Data.DataSet();
            adTables.Fill(dsTables, "t_tables");

            for (int i = 0; i < dsTables.Tables["t_tables"].Rows.Count; i++)
            {
                string table_name = dsTables.Tables["t_tables"].Rows[i]["tablename"].ToString();
                string table_comment = dsTables.Tables["t_tables"].Rows[i]["comment"].ToString();
                string str_cols = @"SELECT n.nspname, c.relname, a.attname, t.typname, a.attlen, a.atttypmod, a.attnotnull, a.atthasdef, a.attnum,d.description
                FROM pg_attribute a
                LEFT JOIN pg_type t ON t.oid = a.atttypid
                LEFT JOIN pg_class c ON c.oid = a.attrelid
                LEFT JOIN pg_namespace n ON c.relnamespace = n.oid
				LEFT JOIN pg_description d ON d.objsubid=a.attnum
                WHERE c.relkind in ('r','v') AND a.attnum > 0 AND n.nspname = 'public' AND c.relname = '" + table_name + @"'ORDER BY a.attnum ";
                NpgsqlDataAdapter adCols = new NpgsqlDataAdapter(str_cols, conn);
                if (dsTables.Tables["t_cols"] != null) dsTables.Tables["t_cols"].Clear();
                adCols.Fill(dsTables, "t_cols");
                CreateModel(table_name, table_comment, dsTables);
                CreateDAL(table_name, table_comment, dsTables);
            }
        }

        string cur_dir = System.Environment.CurrentDirectory;

        #region 创建三层

        /// <summary>
        /// 创建Models层
        /// </summary>
        /// <param name="table_name">表名称</param>
        /// <param name="table_comment">表说明</param>
        /// <param name="ds"></param>
        private void CreateModel(string table_name, string table_comment, DataSet ds)
        {
            Directory.CreateDirectory(cur_dir + "/Models");
            StringBuilder sb = new StringBuilder();
            sb.Append(@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models
{");
            sb.Append("\r\n" + @"    /// <summary>");
            sb.Append("\r\n" + @"    /// " + table_comment + "");
            sb.Append("\r\n" + @"    /// <summary>");
            sb.Append("\r\n" + @"    public class " + table_name + @"
    {");
            sb.Append("\r\n");
            for (int i = 0; i < ds.Tables["t_cols"].Rows.Count; i++)
            {
                string col_name = ds.Tables["t_cols"].Rows[i]["attname"].ToString();
                string col_type = ds.Tables["t_cols"].Rows[i]["typname"].ToString();
                string col_describe = ds.Tables["t_cols"].Rows[i]["description"].ToString();
                if (col_type != "")
                {
                    sb.Append("\r\n" + @"        /// <summary>");
                    sb.Append("\r\n" + @"        /// " + col_describe + "");
                    sb.Append("\r\n" + @"        /// <summary>");
                    sb.Append("\r\n");
                    sb.Append(@"        public ");
                    switch (col_type)
                    {
                        case "int8":
                            sb.Append("long " + col_name + " { set; get; }\r\n");
                            break;
                        case "varchar":
                        case "bpchar":
                            sb.Append("string " + col_name + " { set; get; }\r\n");
                            break;
                        case "timestamp":
                            sb.Append("DateTime " + col_name + " { set; get; }\r\n");
                            break;
                        case "int4":
                        case "int2":
                            sb.Append("int " + col_name + " { set; get; }\r\n");
                            break;
                        default:
                            break;
                    }
                }
            }
            sb.Append(@"    }
}");
            File.WriteAllText(cur_dir + "/Models/" + table_name + ".cs", sb.ToString());
        }

        /// <summary>
        /// 创建DAL层
        /// </summary>
        /// <param name="table_name">表名称</param>
        /// <param name="table_comment">表说明</param>
        /// <param name="ds"></param>
        private void CreateDAL(string table_name, string table_comment, DataSet ds)
        {
            DirectoryInfo di = Directory.CreateDirectory(cur_dir + "/DAL");
            string key_col_name = ds.Tables["t_cols"].Rows[0]["attname"].ToString();
            string key_col_type = ds.Tables["t_cols"].Rows[0]["typname"].ToString();
            StringBuilder sb = new StringBuilder();
            sb.Append(@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Models;
using System.Collections;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using NpgsqlDB;

namespace DAL
{");
            sb.Append("\r\n" + @"    public class " + table_name);
            sb.Append("\r\n" + @"    {
        /// <summary>
        /// 是否存在该记录
        /// </summary>");
            sb.Append("\r\n        public bool Exists(" + GetCSharpType(key_col_type) + " " + key_col_name + ")");
            sb.Append("\r\n        {");
            sb.Append("\r\n            StringBuilder strSql = new StringBuilder();");
            sb.Append("\r\n            strSql.Append(\"select count(1) from " + table_name + "\");");
            sb.Append("\r\n            strSql.Append(\" where " + key_col_name + "=@" + key_col_name + "\");");
            sb.Append("\r\n            NpgsqlParameter[] parameters = {");
            sb.Append("\r\n			   new NpgsqlParameter(\"@" + key_col_name + "\", NpgsqlDbType." + GetNpgsqlType(key_col_type) + ")};");
            sb.Append("\r\n            parameters[0].Value = " + key_col_name + ";");
            sb.Append("\r\n            return DbHelperSQL.Exists(strSql.ToString(), parameters);");
            sb.Append("\r\n        }");

            sb.Append("\r\n" + @"
        /// <summary>
        /// 增加一条数据
        /// </summary>");

            sb.Append("\r\n" + @"        public long Add(Model." + table_name + @" model)
        {");
            sb.Append("\r\n" + @"            StringBuilder strSql = new StringBuilder();");
            sb.Append("\r\n            strSql.Append(\"insert into " + table_name + "(\");");
            sb.Append("\r\n            strSql.Append(@\"");
            for (int i = 0; i < ds.Tables["t_cols"].Rows.Count; i++)
            {
                //如果第一列为自增长
                if (i == 0)
                {
                    if (ds.Tables["t_cols"].Rows[i]["atthasdef"].ToString().Trim() == "True")//有默认值，视为自增长ID
                    {
                        continue;
                    }
                }
                if (i == ds.Tables["t_cols"].Rows.Count - 1)
                {
                    sb.Append(ds.Tables["t_cols"].Rows[i]["attname"]);
                }
                else
                {
                    sb.Append(ds.Tables["t_cols"].Rows[i]["attname"] + ",");
                }
            }
            sb.Append(")\");");
            sb.Append("\r\n            strSql.Append(\" values (\");");
            sb.Append("\r\n            strSql.Append(@\"");
            for (int i = 0; i < ds.Tables["t_cols"].Rows.Count; i++)
            {
                //如果第一列为自增长
                if (i == 0)
                {
                    if (ds.Tables["t_cols"].Rows[i]["atthasdef"].ToString().Trim() == "True")//有默认值，视为自增长ID
                    {
                        continue;
                    }
                }
                if (i == ds.Tables["t_cols"].Rows.Count - 1)
                {
                    sb.Append("@" + ds.Tables["t_cols"].Rows[i]["attname"]);
                }
                else
                {
                    sb.Append("@" + ds.Tables["t_cols"].Rows[i]["attname"] + ",");
                }
            }
            sb.Append(")\");");
            sb.Append("\r\n            NpgsqlParameter[] parameters = {");
            for (int i = 0; i < ds.Tables["t_cols"].Rows.Count; i++)
            {
                //如果第一列为自增长
                if (i == 0)
                {
                    if (ds.Tables["t_cols"].Rows[i]["atthasdef"].ToString().Trim() == "True")//有默认值，视为自增长ID
                    {
                        continue;
                    }
                }
                if (i == ds.Tables["t_cols"].Rows.Count - 1)
                {
                    sb.Append("\r\n" + "					new NpgsqlParameter(\"@" + ds.Tables["t_cols"].Rows[i]["attname"] + "\",NpgsqlDbType." + GetNpgsqlType(ds.Tables["t_cols"].Rows[i]["typname"].ToString()) + ")};");
                }
                else
                {
                    sb.Append("\r\n" + "					new NpgsqlParameter(\"@" + ds.Tables["t_cols"].Rows[i]["attname"] + "\",NpgsqlDbType." + GetNpgsqlType(ds.Tables["t_cols"].Rows[i]["typname"].ToString()) + "),");
                }
            }
            for (int i = 0; i < ds.Tables["t_cols"].Rows.Count; i++)
            {
                //如果第一列为自增长
                if (i == 0)
                {
                    if (ds.Tables["t_cols"].Rows[i]["atthasdef"].ToString().Trim() == "True")//有默认值，视为自增长ID
                    {
                        continue;
                    }
                }
                sb.Append("\r\n            parameters[" + (i - 1) + "].Value = model." + ds.Tables["t_cols"].Rows[i]["attname"] + ";");
            }
            sb.Append("\r\n            StringBuilder sb = new StringBuilder();");
            sb.Append("\r\n            sb.Append(\"select max(" + key_col_name + ") from " + table_name + "\");");
            sb.Append("\r\n" + @"            Dictionary<string, NpgsqlParameter[]> ht = new Dictionary<string, NpgsqlParameter[]>();
            ht.Add(strSql.ToString(), parameters);
            ht.Add(sb.ToString(), null);
            object obj = DbHelperSQL.ExecuteSqlTranRet(ht);
            if (obj == null)
            {
                return -1;
            }
            else
            {
                return Convert.ToInt64(obj);
            }
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
");
            sb.Append(@"        public bool Update(Model." + table_name + @" model)
        {
            StringBuilder strSql = new StringBuilder();
");
            sb.Append("\r\n            strSql.Append(\"update " + table_name + " set \");");
            for (int i = 1; i < ds.Tables["t_cols"].Rows.Count; i++)
            {
                if (i == ds.Tables["t_cols"].Rows.Count - 1)
                {
                    sb.Append("\r\n            strSql.Append(\"" + ds.Tables["t_cols"].Rows[i]["attname"] + "=@" + ds.Tables["t_cols"].Rows[i]["attname"] + "\");");
                }
                else
                {
                    sb.Append("\r\n            strSql.Append(\"" + ds.Tables["t_cols"].Rows[i]["attname"] + "=@" + ds.Tables["t_cols"].Rows[i]["attname"] + ",\");");
                }
            }
            sb.Append("\r\n            strSql.Append(\" where " + key_col_name + "=@" + key_col_name + "\");");
            sb.Append("\r\n            NpgsqlParameter[] parameters = {");
            for (int i = 0; i < ds.Tables["t_cols"].Rows.Count; i++)
            {
                if (i == ds.Tables["t_cols"].Rows.Count - 1)
                {
                    sb.Append("\r\n" + "					new NpgsqlParameter(\"@" + ds.Tables["t_cols"].Rows[i]["attname"] + "\",NpgsqlDbType." + GetNpgsqlType(ds.Tables["t_cols"].Rows[i]["typname"].ToString()) + ")};");
                }
                else
                {
                    sb.Append("\r\n" + "					new NpgsqlParameter(\"@" + ds.Tables["t_cols"].Rows[i]["attname"] + "\",NpgsqlDbType." + GetNpgsqlType(ds.Tables["t_cols"].Rows[i]["typname"].ToString()) + "),");
                }
            }
            for (int i = 0; i < ds.Tables["t_cols"].Rows.Count; i++)
            {
                sb.Append("\r\n            parameters[" + i + "].Value = model." + ds.Tables["t_cols"].Rows[i]["attname"] + ";");
            }
            sb.Append("\r\n" + @"            int rows=DbHelperSQL.ExecuteSql(strSql.ToString(), parameters);
            if (rows > 0)
			{
				return true;
			}
			else
			{
				return false;
			}
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public bool Delete(" + GetCSharpType(key_col_type) + " " + key_col_name + @")
        {
            StringBuilder strSql = new StringBuilder();");
            sb.Append("\r\n            strSql.Append(\"delete from " + table_name + " \");");
            sb.Append("\r\n            strSql.Append(\" where " + key_col_name + "=@" + key_col_name + "\");");
            sb.Append("\r\n            NpgsqlParameter[] parameters = {");
            sb.Append("\r\n					new NpgsqlParameter(\"@" + key_col_name + "\", NpgsqlDbType." + GetNpgsqlType(key_col_type) + ")};");
            sb.Append("\r\n" + @"            parameters[0].Value = " + key_col_name + @";
            int rows=DbHelperSQL.ExecuteSql(strSql.ToString(), parameters);
            if (rows > 0)
			{
				return true;
			}
			else
			{
				return false;
			}
        }

        /// <summary>
        /// 得到一个对象实体
        /// </summary>");
            sb.Append("\r\n        public Model." + table_name + " GetModel(" + GetCSharpType(key_col_type) + " " + key_col_name + ")");
            sb.Append("\r\n" + @"        {
            StringBuilder strSql = new StringBuilder();
");
            sb.Append("\r\n            strSql.Append(\"select  * from " + table_name + "\");");
            sb.Append("\r\n            strSql.Append(\" where " + key_col_name + "=@" + key_col_name + "\");");
            sb.Append("\r\n            NpgsqlParameter[] parameters = {");
            sb.Append("\r\n					new NpgsqlParameter(\"@" + key_col_name + "\", NpgsqlDbType." + GetNpgsqlType(key_col_type) + ")};");
            sb.Append("\r\n            parameters[0].Value = " + key_col_name + ";");
            sb.Append("\r\n            Model." + table_name + " model = new Model." + table_name + "();");
            sb.Append("\r\n" + @"            DataSet ds = DbHelperSQL.Query(strSql.ToString(), parameters);
            if (ds.Tables[0].Rows.Count > 0)
            {");
            for (int i = 0; i < ds.Tables["t_cols"].Rows.Count; i++)
            {
                sb.Append("\r\n                if (ds.Tables[0].Rows[0][\"" + ds.Tables["t_cols"].Rows[i]["attname"] + "\"].ToString() != \"\")");
                sb.Append("\r\n                {");
                sb.Append("\r\n                    model." + ds.Tables["t_cols"].Rows[i]["attname"] + " = Convert." + GetConvertType(ds.Tables["t_cols"].Rows[i]["typname"].ToString()) + "( ds.Tables[0].Rows[0][\"" + ds.Tables["t_cols"].Rows[i]["attname"] + "\"].ToString());");
                sb.Append("\r\n                }");
            }
            sb.Append("\r\n" + @"                return model;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public DataSet GetList(string strWhere)
        {
            StringBuilder strSql = new StringBuilder();
");
            sb.Append("\r\n            strSql.Append(\"select * \");");
            sb.Append("\r\n            strSql.Append(\" FROM " + table_name + "\");");
            sb.Append("\r\n" + "            if (strWhere.Trim() != \"\")");
            sb.Append("{");
            sb.Append("\r\n                strSql.Append(\" where \" + strWhere);");
            sb.Append("\r\n" + @"            }
            return DbHelperSQL.Query(strSql.ToString());
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public DataSet GetAllList()
        {
            StringBuilder strSql = new StringBuilder();
");
            sb.Append("\r\n            strSql.Append(\"select * \");");
            sb.Append("\r\n            strSql.Append(\" FROM " + table_name + "\");");
            sb.Append("\r\n" + @"            return DbHelperSQL.Query(strSql.ToString());
        }
");

            sb.Append(@"		
		/// <summary>
		/// 获得前几行数据
		/// </summary>
		public DataSet GetList(int Top,string strWhere,string filedOrder)
		{
			StringBuilder strSql = new StringBuilder();");
            sb.Append("\r\n			strSql.Append(\"select \");");
            sb.Append("\r\n" + @"			if(Top > 0)
			{");
            sb.Append("\r\n				strSql.Append(\" top \"+Top.ToString());");
            sb.Append("\r\n			}");
            sb.Append("\r\n			strSql.Append(\" * \");");
            sb.Append("\r\n			strSql.Append(\" FROM " + table_name + " \");");
            sb.Append("\r\n			if(strWhere.Trim() != \"\")");
            sb.Append("\r\n			{");
            sb.Append("\r\n				strSql.Append(\" where \"+strWhere);");
            sb.Append("\r\n			}");
            sb.Append("\r\n			strSql.Append(\" order by \" + filedOrder);");
            sb.Append("\r\n" + @"            return DbHelperSQL.Query(strSql.ToString());");
            sb.Append("\r\n" + @"		}
	}
}
");

            File.WriteAllText(di.FullName + "/" + table_name + ".cs", sb.ToString());
        }

        private string GetNpgsqlType(string dbType)
        {
            switch (dbType)
            {
                case "int8":
                    return "Bigint";
                case "varchar":
                case "bpchar":
                    return "Varchar";
                case "timestamp":
                    return "TimestampTZ";
                case "int4":
                case "int2":
                    return "Integer";
                default:
                    return "";
            }
        }

        private string GetCSharpType(string dbType)
        {
            switch (dbType)
            {
                case "int8":
                    return "long";
                case "varchar":
                case "bpchar":
                    return "string";
                case "timestamp":
                    return "DateTime";
                case "int4":
                case "int2":
                    return "int";
                default:
                    return "";
            }
        }

        private string GetConvertType(string dbType)
        {
            switch (dbType)
            {
                case "int8":
                    return "ToInt64";
                case "varchar":
                case "bpchar":
                    return "ToString";
                case "timestamp":
                    return "ToDateTime";
                case "int4":
                case "int2":
                    return "ToInt32";
                default:
                    return "";
            }
        }

        #endregion



    }
}
