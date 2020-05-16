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


        /// <summary>
        /// 获取数据库连接状态
        /// </summary>
        /// <returns></returns>
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
                string str_cols = @"SELECT n.nspname, c.relname, a.attname, t.typname, a.attlen, a.atttypmod, a.attnotnull, a.atthasdef, a.attnum,col_description(a.attrelid,a.attnum) as description
                FROM pg_attribute a
                LEFT JOIN pg_type t ON t.oid = a.atttypid
                LEFT JOIN pg_class c ON c.oid = a.attrelid
                LEFT JOIN pg_namespace n ON c.relnamespace = n.oid
                WHERE c.relkind in ('r','v') AND a.attnum > 0 AND n.nspname = 'public' AND c.relname = '" + table_name + @"' and  a.attname not like '%dropped%' ORDER BY a.attnum ";
                NpgsqlDataAdapter adCols = new NpgsqlDataAdapter(str_cols, conn);
                if (dsTables.Tables["t_cols"] != null) dsTables.Tables["t_cols"].Clear();
                adCols.Fill(dsTables, "t_cols");
                CreateModel(table_name, table_comment, dsTables);
                CreateDAL(table_name, table_comment, dsTables);
                CreateBLL(table_name, table_comment, dsTables);
                CreateHanderAdmin(table_name, table_comment, dsTables);
            }
        }

        string cur_dir = System.Environment.CurrentDirectory;

        #region 创建三层

        /// <summary>
        /// 创建Model层
        /// </summary>
        /// <param name="table_name">表名称</param>
        /// <param name="table_comment">表说明</param>
        /// <param name="ds"></param>
        private void CreateModel(string table_name, string table_comment, DataSet ds)
        {
            Directory.CreateDirectory(cur_dir + "/Model");
            StringBuilder sb = new StringBuilder();
            sb.Append(@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{");
            sb.Append("\r\n" + @"    /// <summary>");
            sb.Append("\r\n" + @"    /// " + table_comment + "");
            sb.Append("\r\n" + @"    /// <summary>");
            sb.Append("\r\n" + @"    public class " + table_name + @"
    {");
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
                        case "numeric":
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
            File.WriteAllText(cur_dir + "/Model/" + table_name + ".cs", sb.ToString());
        }

        /// <summary>
        /// 创建DAL层
        /// </summary>
        /// <param name="table_name">表名称</param>
        /// <param name="table_comment">表说明</param>
        /// <param name="ds"></param>
        private void CreateDAL(string table_name, string table_comment, DataSet ds)
        {
            //创建BDHelper
            DbHelperPostgreSQL dbPGHelp = new DbHelperPostgreSQL(this.ConnectString);
            DirectoryInfo dir = Directory.CreateDirectory(cur_dir + "/DAL");

            //表字段集合
            string colNames = "";
            string coltNames = "TT.R,";
            string typeNames = "";
            for (int i = 0; i < ds.Tables["t_cols"].Rows.Count; i++)
            {
                colNames += ds.Tables["t_cols"].Rows[i]["attname"].ToString() + ",";
                coltNames += "TT." + ds.Tables["t_cols"].Rows[i]["attname"].ToString() + ",";
                typeNames += ds.Tables["t_cols"].Rows[0]["typname"].ToString() + ",";
            }
            colNames = colNames.Substring(0, colNames.LastIndexOf(","));
            coltNames = coltNames.Substring(0, coltNames.LastIndexOf(","));
            typeNames = typeNames.Substring(0, typeNames.LastIndexOf(","));

            //查询主键
            string keyColName = "";
            string keyTypeName = "";

            string strSql = "select pg_constraint.conname as pk_name,pg_attribute.attname as colname,pg_type.typname as typename from pg_constraint inner join pg_class on pg_constraint.conrelid = pg_class.oid inner join pg_attribute on pg_attribute.attrelid = pg_class.oid and pg_attribute.attnum = pg_constraint.conkey[1] inner join pg_type on pg_type.oid = pg_attribute.atttypid where pg_class.relname = '" + table_name + "' and pg_constraint.contype='p'";
            DataSet dsKey = dbPGHelp.Query(strSql.ToString());
            if (dsKey != null && dsKey.Tables[0].Rows.Count > 0)
            {
                keyColName = dsKey.Tables[0].Rows[0]["colname"].ToString(); ;
                keyTypeName = dsKey.Tables[0].Rows[0]["typename"].ToString();
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model;
using System.Collections;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using DBUtility;

namespace DAL
{");
            sb.Append("\r\n" + @"    public class " + table_name);
            sb.Append("\r\n" + @"    {
        /// <summary>
        /// 是否存在该记录
        /// </summary>");
            sb.Append("\r\n        public static bool Exists(" + GetCSharpType(keyTypeName) + " " + keyColName + ")");
            sb.Append("\r\n        {");
            sb.Append("\r\n            StringBuilder strSql = new StringBuilder();");
            sb.Append("\r\n            strSql.Append(\"select count(1) from " + table_name + "\");");
            sb.Append("\r\n            strSql.Append(\" where " + keyColName + "=@" + keyColName + "\");");
            sb.Append("\r\n            NpgsqlParameter[] parameters = {");
            sb.Append("\r\n			   new NpgsqlParameter(\"@" + keyColName + "\", NpgsqlDbType." + GetNpgsqlType(keyTypeName) + ")};");
            sb.Append("\r\n            parameters[0].Value = " + keyColName + ";");
            sb.Append("\r\n            return DbHelperPostgreSQL.Exists(strSql.ToString(), parameters);");
            sb.Append("\r\n        }");

            sb.Append("\r\n" + @"
        /// <summary>
        /// 增加一条数据
        /// </summary>");

            sb.Append("\r\n" + @"        public static bool Add(Model." + table_name + @" model)
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
                sb.Append("\r\n            parameters[" + (i) + "].Value = model." + ds.Tables["t_cols"].Rows[i]["attname"] + ";");
            }
            sb.Append("\r\n" + @"            int rows = DbHelperPostgreSQL.ExecuteSql(strSql.ToString(), parameters);
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
        /// 更新一条数据
        /// </summary>
");
            sb.Append(@"        public static bool Update(Model." + table_name + @" model)
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
            sb.Append("\r\n            strSql.Append(\" where " + keyColName + "=@" + keyColName + "\");");
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
            sb.Append("\r\n" + @"            int rows = DbHelperPostgreSQL.ExecuteSql(strSql.ToString(), parameters);
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
        public static bool Delete(" + GetCSharpType(keyTypeName) + " " + keyColName + @")
        {
            StringBuilder strSql = new StringBuilder();");
            sb.Append("\r\n            strSql.Append(\"delete from " + table_name + " \");");
            sb.Append("\r\n            strSql.Append(\" where " + keyColName + "=@" + keyColName + "\");");
            sb.Append("\r\n            NpgsqlParameter[] parameters = {");
            sb.Append("\r\n					new NpgsqlParameter(\"@" + keyColName + "\", NpgsqlDbType." + GetNpgsqlType(keyTypeName) + ")};");
            sb.Append("\r\n" + @"            parameters[0].Value = " + keyColName + @";
            int rows = DbHelperPostgreSQL.ExecuteSql(strSql.ToString(), parameters);
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
            sb.Append("\r\n        public static Model." + table_name + " GetModel(" + GetCSharpType(keyTypeName) + " " + keyColName + ")");
            sb.Append("\r\n" + @"        {
            StringBuilder strSql = new StringBuilder();
");
            sb.Append("\r\n            strSql.Append(\"select  " + colNames + " from " + table_name + "\");");
            sb.Append("\r\n            strSql.Append(\" where " + keyColName + "=@" + keyColName + "\");");
            sb.Append("\r\n            NpgsqlParameter[] parameters = {");
            sb.Append("\r\n					new NpgsqlParameter(\"@" + keyColName + "\", NpgsqlDbType." + GetNpgsqlType(keyTypeName) + ")};");
            sb.Append("\r\n            parameters[0].Value = " + keyColName + ";");
            sb.Append("\r\n            Model." + table_name + " model = new Model." + table_name + "();");
            sb.Append("\r\n" + @"            DataSet ds = DbHelperPostgreSQL.Query(strSql.ToString(), parameters);
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
        public static DataSet GetList(string strWhere)
        {
            StringBuilder strSql = new StringBuilder();
");
            sb.Append("\r\n            strSql.Append(\"select " + colNames + " \");");
            sb.Append("\r\n            strSql.Append(\" FROM " + table_name + "\");");
            sb.Append("\r\n" + "            if (strWhere.Trim() != \"\")");
            sb.Append("\r\n			{");
            sb.Append("\r\n                strSql.Append(\" where \" + strWhere);");
            sb.Append("\r\n" + @"            }
            return DbHelperPostgreSQL.Query(strSql.ToString());
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public static DataSet GetAllList()
        {
            StringBuilder strSql = new StringBuilder();
");
            sb.Append("\r\n            strSql.Append(\"select " + colNames + " \");");
            sb.Append("\r\n            strSql.Append(\" FROM " + table_name + "\");");
            sb.Append("\r\n" + @"            return DbHelperPostgreSQL.Query(strSql.ToString());
        }
");
            sb.Append(@"		
		/// <summary>
		/// 分页获取数据列表
		/// </summary>
		 public static DataSet GetListByPage(string strWhere, string orderby, int startIndex, int endIndex)
		{
			StringBuilder strSql = new StringBuilder();");
            sb.Append("\r\n	        strSql.Append(\"select " + coltNames + " from ( \");");
            sb.Append("\r\n			strSql.Append(\" select ROW_NUMBER() OVER (\");");
            sb.Append("\r\n			if (!string.IsNullOrEmpty(orderby.Trim()))");
            sb.Append("\r\n			{");
            sb.Append("\r\n				strSql.Append(\"order by T.\" + orderby);");
            sb.Append("\r\n			}");
            sb.Append("\r\n			strSql.Append(\")AS R, T.*  from " + table_name + " T \");");
            sb.Append("\r\n			if (!string.IsNullOrEmpty(strWhere.Trim()))");
            sb.Append("\r\n			{");
            sb.Append("\r\n				strSql.Append(\" where \" + strWhere);");
            sb.Append("\r\n			}");
            sb.Append("\r\n			strSql.Append(\" ) TT\");");
            sb.Append("\r\n			strSql.AppendFormat(\" where TT.R between {0} and {1}\", startIndex, endIndex);");
            sb.Append("\r\n" + @"            return DbHelperPostgreSQL.Query(strSql.ToString());
       }
");

            sb.Append(@"		
		/// <summary>
		/// 获取记录总数
		/// </summary>
		public static int GetRecordCount(string strWhere)
		{
			StringBuilder strSql = new StringBuilder();");
            sb.Append("\r\n			strSql.Append(\"select count(1) FROM " + table_name + " \");");
            sb.Append("\r\n			if(strWhere.Trim() != \"\")");
            sb.Append("\r\n			{");
            sb.Append("\r\n				strSql.Append(\" where \"+strWhere);");
            sb.Append("\r\n			}");
            sb.Append("\r\n" + @"            object obj = DbHelperPostgreSQL.GetSingle(strSql.ToString());
            if (obj == null)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(obj);
            }
");
            sb.Append("\r\n" + @"		}
	}
}
");

            File.WriteAllText(dir.FullName + "/" + table_name + ".cs", sb.ToString());
        }

        /// <summary>
        /// 创建BLL层
        /// </summary>
        /// <param name="table_name">表名称</param>
        /// <param name="table_comment">表说明</param>
        /// <param name="ds"></param>
        private void CreateBLL(string table_name, string table_comment, DataSet ds)
        {
            //创建BDHelper
            DbHelperPostgreSQL dbPGHelp = new DbHelperPostgreSQL(this.ConnectString);
            DirectoryInfo dir = Directory.CreateDirectory(cur_dir + "/BLL");

            //查询主键
            string keyColName = "";
            string keyTypeName = "";

            string strSql = "select pg_constraint.conname as pk_name,pg_attribute.attname as colname,pg_type.typname as typename from pg_constraint inner join pg_class on pg_constraint.conrelid = pg_class.oid inner join pg_attribute on pg_attribute.attrelid = pg_class.oid and pg_attribute.attnum = pg_constraint.conkey[1] inner join pg_type on pg_type.oid = pg_attribute.atttypid where pg_class.relname = '" + table_name + "' and pg_constraint.contype='p'";
            DataSet dsKey = dbPGHelp.Query(strSql.ToString());
            if (dsKey != null && dsKey.Tables[0].Rows.Count > 0)
            {
                keyColName = dsKey.Tables[0].Rows[0]["colname"].ToString(); ;
                keyTypeName = dsKey.Tables[0].Rows[0]["typename"].ToString();
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(@"using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Model;
using DAL;
namespace BLL
{");
            sb.Append("\r\n    //" + table_comment + "");
            sb.Append("\r\n    public class " + table_name + "");
            sb.Append("\r\n    {");
            sb.Append("\r\n       ");
            sb.Append("\r\n        public " + table_name + "()");
            sb.Append("\r\n" + @"        { }

        /// <summary>
        /// 是否存在该记录
        /// </summary>
        public static bool Exists(" + GetCSharpType(keyTypeName) + " " + keyColName + @")
        {
            return DAL." + table_name + ".Exists(" + keyColName + @");
        }

        /// <summary>
        /// 增加一条数据
        /// </summary>
");
            sb.Append("        public static bool Add(Model." + table_name + " model)");
            sb.Append("\r\n" + @"        {");
            sb.Append("\r\n            return DAL." + table_name + ".Add(model);");
            sb.Append("" + @"
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
");
            sb.Append("        public static bool Update(Model." + table_name + " model)");
            sb.Append("\r\n" + @"        {");
            sb.Append("\r\n            return DAL." + table_name + ".Update(model);");
            sb.Append("" + @"
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
");
            sb.Append("        public static bool Delete(" + GetCSharpType(keyTypeName) + " " + keyColName + ")");
            sb.Append("\r\n        {");
            sb.Append("\r\n            return DAL." + table_name + ".Delete(" + keyColName + ");");
            sb.Append("\r\n" + @"        }

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
");
            sb.Append("        public static Model." + table_name + " GetModel(" + GetCSharpType(keyTypeName) + " " + keyColName + ")");
            sb.Append("\r\n        {");
            sb.Append("\r\n            return DAL." + table_name + ".GetModel(" + keyColName + ");");
            sb.Append("\r\n" + @"        }");
            sb.Append("\r\n" + @"

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public static DataSet GetList(string strWhere)
        {");
            sb.Append("\r\n            return DAL." + table_name + ".GetList(strWhere);");
            sb.Append("" + @"
        }

        /// <summary>
        /// 获得数据总数
        /// </summary>
        public static int GetRecordCount(string strWhere)
        {");
            sb.Append("\r\n            return DAL." + table_name + ".GetRecordCount(strWhere);");
            sb.Append("" + @"
        }

        /// <summary>
        /// 分页获取数据列表
        /// </summary>
        public static DataSet GetListByPage(string strWhere, string orderby, int startIndex, int endIndex, out int recordcount)
        {");
            sb.Append("\r\n            recordcount = DAL." + table_name + ".GetRecordCount(strWhere);");
            sb.Append("\r\n            return DAL." + table_name + ".GetListByPage(strWhere, orderby, startIndex, endIndex);");
            sb.Append("" + @"
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
");
            sb.Append("        public static List<Model." + table_name + "> GetModelList(string strWhere)");
            sb.Append("\r\n" + @"        {");
            sb.Append("\r\n            DataSet ds = DAL." + table_name + ".GetList(strWhere);");
            sb.Append("\r\n            return DataTableToList(ds.Tables[0]);");
            sb.Append("" + @"
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
");
            sb.Append("        public static List<Model." + table_name + "> DataTableToList(DataTable dt)");
            sb.Append("\r\n        {");
            sb.Append("\r\n            List<Model." + table_name + "> modelList = new List<Model." + table_name + ">();");
            sb.Append("\r\n" + @"            int rowsCount = dt.Rows.Count;
            if (rowsCount > 0)
            {");
            sb.Append("\r\n                Model." + table_name + " model;");
            sb.Append("\r\n" + @"                for (int n = 0; n < rowsCount; n++)
                {");
            sb.Append("\r\n                    model = new Model." + table_name + "();");
            for (int i = 0; i < ds.Tables["t_cols"].Rows.Count; i++)
            {
                sb.Append("\r\n                    if (dt.Rows[n][\"" + ds.Tables["t_cols"].Rows[i]["attname"] + "\"].ToString() != \"\")");
                sb.Append("\r\n                    {");
                sb.Append("\r\n                        model." + ds.Tables["t_cols"].Rows[i]["attname"] + " = Convert." + GetConvertType(ds.Tables["t_cols"].Rows[i]["typname"].ToString()) + "(dt.Rows[n][\"" + ds.Tables["t_cols"].Rows[i]["attname"] + "\"].ToString());");
                sb.Append("\r\n                    }");
            }
            sb.Append("\r\n" + @"                    modelList.Add(model);
                }
            }
            return modelList;");
            sb.Append("\r\n" + @"        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public static DataSet GetAllList()
        {");
            sb.Append("\r\n            return DAL." + table_name + ".GetAllList();");
            sb.Append("" + @" 
        }

    }
}");
            File.WriteAllText(dir.FullName + "/" + table_name + ".cs", sb.ToString());

        }

        /// <summary>
        /// 创建HanderAdmin层
        /// </summary>
        /// <param name="table_name">表名称</param>
        /// <param name="table_comment">表说明</param>
        /// <param name="ds"></param>
        private void CreateHanderAdmin(string table_name, string table_comment, DataSet ds)
        {
            //创建BDHelper
            DbHelperPostgreSQL dbPGHelp = new DbHelperPostgreSQL(this.ConnectString);
            DirectoryInfo dir = Directory.CreateDirectory(cur_dir + "/HandAdmin");

            //查询主键
            //string keyColName = "";
            //string keyTypeName = "";

            //string strSql = "select pg_constraint.conname as pk_name,pg_attribute.attname as colname,pg_type.typname as typename from pg_constraint inner join pg_class on pg_constraint.conrelid = pg_class.oid inner join pg_attribute on pg_attribute.attrelid = pg_class.oid and pg_attribute.attnum = pg_constraint.conkey[1] inner join pg_type on pg_type.oid = pg_attribute.atttypid where pg_class.relname = '" + table_name + "' and pg_constraint.contype='p'";
            //DataSet dsKey = dbPGHelp.Query(strSql.ToString());
            //if (dsKey != null && dsKey.Tables[0].Rows.Count > 0)
            //{
            //    keyColName = dsKey.Tables[0].Rows[0]["colname"].ToString(); ;
            //    keyTypeName = dsKey.Tables[0].Rows[0]["typename"].ToString();
            //}

            HanderAdmin ha = new HanderAdmin(dir.FullName.ToString());
            ha.CreateHanderAdmin(table_name, table_comment, ds);
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
                case "numeric":
                case "int2":
                    return "Integer";
                case "double":
                    return "double";
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
                case "numeric":
                case "int2":
                    return "int";
                case "double":
                    return "double";
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
                case "numeric":
                case "int2":
                    return "ToInt32";
                case "double":
                    return "double";
                default:
                    return "";
            }
        }

        #endregion

    }
}
