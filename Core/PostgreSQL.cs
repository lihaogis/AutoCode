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
            string str_tableNames = "select tablename from pg_tables where schemaname='public'";
            NpgsqlConnection conn = new NpgsqlConnection(this.ConnectString);
            NpgsqlDataAdapter adTables = new NpgsqlDataAdapter(str_tableNames, conn);
            DataSet dsTables = new System.Data.DataSet();
            adTables.Fill(dsTables, "t_tables");

            for (int i = 0; i < dsTables.Tables["t_tables"].Rows.Count; i++)
            {
                string table_name = dsTables.Tables["t_tables"].Rows[i]["tablename"].ToString();
                string str_cols = @"SELECT n.nspname, c.relname, a.attname, t.typname, a.attlen, a.atttypmod, a.attnotnull, a.atthasdef, a.attnum
                FROM pg_attribute a
                LEFT JOIN pg_type t ON t.oid = a.atttypid
                LEFT JOIN pg_class c ON c.oid = a.attrelid
                LEFT JOIN pg_namespace n ON c.relnamespace = n.oid
                WHERE c.relkind in ('r','v') AND a.attnum > 0 AND n.nspname = 'public' AND c.relname = '" + table_name + @"'ORDER BY a.attnum ";
                NpgsqlDataAdapter adCols = new NpgsqlDataAdapter(str_cols, conn);
                if (dsTables.Tables["t_cols"] != null) dsTables.Tables["t_cols"].Clear();
                adCols.Fill(dsTables, "t_cols");
            }
        }

        string cur_dir = System.Environment.CurrentDirectory;

        void GenModel(string table_name, DataSet ds)
        {
            Directory.CreateDirectory(cur_dir + "/Model");
            StringBuilder sb = new StringBuilder();
            sb.Append(@"using System;
                        using System.Collections.Generic;
                        using System.Linq;
                        using System.Text;

                        namespace Models
                     {");
            sb.Append("\r\n" + @"    public class " + table_name + @"
                     {");
            sb.Append("\r\n");
            for (int i = 0; i < ds.Tables["t_cols"].Rows.Count; i++)
            {
                sb.Append(@"        public ");
                string col_name = ds.Tables["t_cols"].Rows[i]["attname"].ToString();
                string col_type = ds.Tables["t_cols"].Rows[i]["typname"].ToString();
                switch (col_type)
                {
                    case "int8":
                        sb.Append("long " + col_name + " {set;get;}\r\n");
                        break;
                    case "varchar":
                    case "bpchar":
                        sb.Append("string " + col_name + " {set;get;}\r\n");
                        break;
                    case "timestamp":
                        sb.Append("DateTime " + col_name + " {set;get;}\r\n");
                        break;
                    case "int4":
                    case "int2":
                        sb.Append("int " + col_name + " {set;get;}\r\n");
                        break;
                    default:
                        break;
                }
            }
            sb.Append(@"    }
                }");
            File.WriteAllText(cur_dir + "/Model/" + table_name + ".cs", sb.ToString());
        }



    }
}
